/*************************************************************************
 *
 *   file		: SpellChannel.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-01 04:06:23 +0100 (ma, 01 feb 2010) $

 *   revision		: $Rev: 1239 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Spells;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras;
using WCell.Util;
using WCell.Util.ObjectPools;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Represents a SpellChannel during a SpellCast (basically any Spell or Action that is being performed over time).
    /// </summary>
    public partial class SpellChannel : IUpdatable, ITickTimer
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly ObjectPool<SpellChannel> SpellChannelPool = ObjectPoolMgr.CreatePool(() => new SpellChannel());

        protected int m_duration;
        protected int m_amplitude;
        protected int m_until;
        protected int m_maxTicks;
        protected int m_ticks;
        bool m_channeling;
        internal SpellCast m_cast;
        List<SpellEffectHandler> m_channelHandlers;
        //int m_handlerSequence;

        internal TimerEntry m_timer;
        List<IAura> m_auras;

        /// <summary>
        /// Can only work with unit casters
        /// </summary>
        private SpellChannel()
        {
            m_timer = new TimerEntry(Tick);
        }

        public SpellCast Cast
        {
            get { return m_cast; }
        }

        /// <summary>
        /// The total duration of this Channel
        /// </summary>
        public int Duration
        {
            get { return m_duration; }
        }

        /// <summary>
        /// Whether this SpellChannel is currently being used
        /// </summary>
        public bool IsChanneling
        {
            get { return m_channeling; }
        }

        /// <summary>
        /// The amount of milliseconds between 2 channel ticks
        /// </summary>
        public int Amplitude
        {
            get { return m_amplitude; }
        }

        public int Until
        {
            get { return m_until; }
            set
            {
                m_until = value;
                var timeLeft = TimeLeft;
                if (timeLeft < 0)
                {
                    //Cancel();
                    timeLeft = 1;
                }

                m_ticks = MathUtil.CeilingInt(timeLeft / (float)m_amplitude);
                m_timer.Start(0, timeLeft % m_amplitude);
                SpellHandler.SendChannelUpdate(m_cast, (uint)timeLeft);
            }
        }

        public int Ticks
        {
            get { return m_ticks; }
        }

        public int MaxTicks
        {
            get { return m_maxTicks; }
        }

        /// <summary>
        /// The time in milliseconds until this channel closes
        /// </summary>
        public int TimeLeft
        {
            get { return m_until - Environment.TickCount; }
        }

        /// <summary>
        /// Reduces the channel by one tick
        /// </summary>
        public void Pushback(int millis)
        {
            if (m_channeling && m_cast != null)
            {
                Until += millis;
            }
        }

        /// <summary>
        /// Opens this SpellChannel.
        /// Will be called by SpellCast class.
        /// Requires an active Caster.
        /// </summary>
        internal void Open(List<SpellEffectHandler> channelHandlers, List<IAura> auras)
        {
            if (!m_channeling && m_cast != null)
            {
                m_channeling = true;
                m_auras = auras;

                var spell = m_cast.Spell;
                var caster = m_cast.CasterUnit;

                m_duration = spell.Durations.Max;
                m_amplitude = spell.ChannelAmplitude;

                if (m_amplitude < 1)
                {
                    // only one tick
                    m_amplitude = m_duration;
                }

                caster.ChannelSpell = spell.SpellId;

                var now = Environment.TickCount;
                m_ticks = 0;
                m_maxTicks = m_duration / m_amplitude;
                m_channelHandlers = channelHandlers;

                // get duration again, this time with modifiers
                m_duration = spell.GetDuration(caster.SharedReference);
                if (m_amplitude < 1)
                {
                    // only one tick
                    m_amplitude = m_duration;
                }

                m_until = now + m_duration;
                SpellHandler.SendChannelStart(m_cast, spell.SpellId, m_duration);

                if (m_channeling)
                {
                    m_timer.Start(m_amplitude, m_amplitude);
                }
                // Send Initial Tick?
                // Keep in mind: Aura is not initialized at this point!
            }
            else
            {
                log.Warn(this + " was opened more than once or after disposal!");
            }
        }

        /// <summary>
        /// Triggers a new tick
        /// </summary>
        protected void Tick(int timeElapsed)
        {
            m_ticks++;

            var cast = m_cast;
            if (cast == null || !cast.IsCasting)
            {
                return;
            }

            var spell = cast.Spell;
            var handlers = m_channelHandlers;

            // consume power periodically
            if (spell.PowerPerSecond > 0)
            {
                var cost = spell.PowerPerSecond;
                if (m_amplitude != 1000 && m_amplitude != 0)
                {
                    cost = (int)(cost * (m_amplitude / 1000f));
                }
                var failReason = cast.ConsumePower(cost);
                if (failReason != SpellFailedReason.Ok)
                {
                    m_cast.Cancel(failReason);
                    return;
                }
            }

            // apply effects
            foreach (var handler in handlers)
            {
                handler.OnChannelTick();
                if (!m_channeling)
                {
                    // cancelling a handler might cancel the SpellChannel
                    return;
                }
            }

            // apply all Auras remove those that went inactive in the meantime
            if (m_auras != null)
            {
                m_auras.RemoveAll(aura =>
                {
                    if (aura.IsAdded)
                    {
                        aura.Apply();
                        // remove if Aura went inactive
                        return !aura.IsAdded;
                    }
                    return true;
                });
            }

            if (m_channeling)
            {
                // Ticked event
                var ticked = Ticked;
                if (ticked != null)
                {
                    ticked(this);
                }

                // Delay next tick or close
                if (m_maxTicks <= 0 || m_ticks >= m_maxTicks)
                {
                    // we are done
                    Close(false);
                    if (cast.IsCasting)
                    {
                        cast.Cleanup();
                    }
                }
                //else
                //{
                //    m_timer.Start(m_amplitude);
                //}
            }
        }

        public void OnRemove(Unit owner, Aura aura)
        {
            if (!m_channeling)
            {
                return;
            }

            if (m_cast.CasterUnit.ChannelObject == owner)
            {
                // The Aura on our target has been removed: Cancel Channel
                m_cast.Cancel(SpellFailedReason.DontReport);
            }
            else
            {
                m_auras.Remove(aura);
            }
        }

        public void Cancel()
        {
            m_cast.Cancel();
        }

        /// <summary>
        /// Will be called internally to close this Channel.
        /// Call SpellCast.Cancel to cancel channeling.
        /// </summary>
        internal void Close(bool cancelled)
        {
            if (!m_channeling)
            {
                return;
            }
            m_channeling = false;

            var caster = m_cast.CasterUnit;
            var handlers = m_channelHandlers;
            foreach (var handler in handlers)
            {
                handler.OnChannelClose(cancelled);
            }

            var auras = m_auras;
            if (auras != null)
            {
                foreach (var aura in auras)
                {
                    aura.Remove(false);
                }

                auras.Clear();
                SpellCast.AuraListPool.Recycle(auras);
                m_auras = null;
            }

            m_channelHandlers.Clear();
            SpellCast.SpellEffectHandlerListPool.Recycle(m_channelHandlers);
            m_channelHandlers = null;

            m_timer.Stop();

            if (cancelled)
            {
                SpellHandler.SendChannelUpdate(m_cast, 0);
            }

            var obj = caster.ChannelObject;
            if (obj is DynamicObject)
            {
                ((WorldObject)obj).Delete();
            }
            caster.ChannelObject = null;
            caster.ChannelSpell = 0;
        }

        #region IUpdatable

        public void Update(int dt)
        {
            if (m_timer == null)
            {
                log.Warn("SpellChannel is updated after disposal: {0}", this);
            }
            else
            {
                m_timer.Update(dt);
            }
        }

        #endregion IUpdatable

        /// <summary>
        /// Get rid of circular references
        /// </summary>
        internal void Dispose()
        {
            m_cast = null;
            //m_timer = null;

            SpellChannelPool.Recycle(this);
        }

        public override string ToString()
        {
            if (m_cast != null)
                return "SpellChannel (Caster: " + m_cast.CasterObject + ", " + m_cast.Spell + ")";

            return "SpellChannel (Inactive)";
        }
    }
}