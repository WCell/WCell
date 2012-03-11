/*************************************************************************
 *
 *   file		: AuraEffectHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-28 16:34:36 +0100 (to, 28 jan 2010) $

 *   revision		: $Rev: 1231 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras
{
    public class AuraVoidHandler : AuraEffectHandler
    {
    }

    /// <summary>
    /// An AuraEffectHandler handles the behavior of an aura
    /// </summary>
    public abstract class AuraEffectHandler
    {
        protected internal Aura m_aura;
        protected internal SpellEffect m_spellEffect;
        public int BaseEffectValue;

        /// <summary>
        /// The value of the underlying SpellEffect that was calculated when
        /// this Aura was last applied or refreshed (see <see cref="WCell.RealmServer.Spells.SpellEffect.CalcEffectValue(Unit)"/>).
        /// The value is multiplied by the StackCount of the Aura (<see cref="WCell.RealmServer.Spells.Auras.Aura.StackCount"/>).
        /// </summary>
        public int EffectValue
        {
            get
            {
                if (m_aura != null &&
                    m_aura.Spell.CanStack &&
                    m_aura.StackCount > 1)
                {
                    return BaseEffectValue * m_aura.StackCount;
                }
                return BaseEffectValue;
            }
        }

        public Unit Holder
        {
            get { return m_aura.Auras.Owner; }
        }

        protected internal void Init(Aura aura)
        {
            m_aura = aura;
        }

        /// <summary>
        /// whether this is a positive effect (by default: If they have a positive value)
        /// </summary>
        public virtual bool IsPositive
        {
            get { return EffectValue >= 0; }
        }

        private bool m_IsActivated;

        public bool IsActivated
        {
            get { return m_IsActivated; }
            internal set
            {
                if (m_IsActivated == value) return;
                if ((m_IsActivated = value))
                {
                    Apply();
                }
                else
                {
                    Remove(false);
                }
            }
        }

        /// <summary>
        /// The Aura to which this AuraEffect belongs
        /// </summary>
        public Aura Aura
        {
            get { return m_aura; }
        }

        public Unit Owner
        {
            get { return m_aura.Auras.Owner; }
        }

        /// <summary>
        /// The SpellEffect which created this AuraEffect OR:
        /// If the Aura was triggered by another Spell and the original SpellEffect had OverrideEffectValue = true,
        /// this is the SpellEffect that triggered the creation of the Aura (through TriggerSpell, ProcTriggerSpell etc).
        /// </summary>
        public SpellEffect SpellEffect
        {
            get { return m_spellEffect; }
        }

        public void UpdateEffectValue()
        {
            BaseEffectValue = m_spellEffect.CalcEffectValue(m_aura.CasterUnit);
        }

        /// <summary>
        /// Check whether this handler can be applied to the given target.
        /// m_aura, as well as some other fields are not set when this method gets called.
        /// </summary>
        protected internal virtual void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
        {
        }

        /// <summary>
        /// To be called by Aura.Apply on periodic effects
        /// </summary>
        internal void DoApply()
        {
            if (m_IsActivated && !m_spellEffect.IsPeriodic) return;
            m_IsActivated = true;
            Apply();
        }

        /// <summary>
        /// To be called by Aura.Apply on periodic effects
        /// </summary>
        internal void DoRemove(bool cancelled)
        {
            if (!m_IsActivated) return;
            m_IsActivated = false;
            Remove(cancelled);
        }

        /// <summary>
        /// Applies this EffectHandler's effect to its holder
        /// </summary>
        protected virtual void Apply()
        {
        }

        /// <summary>
        /// Removes the effect from its holder
        /// </summary>
        protected virtual void Remove(bool cancelled)
        {
        }

        /// <summary>
        /// Whether this proc handler can be triggered by the given action
        /// </summary>
        public virtual bool CanProcBeTriggeredBy(IUnitAction action)
        {
            return true;
        }

        /// <summary>
        /// Called when a matching proc event triggers this proc handler with the given
        /// triggerer and action.
        /// </summary>
        public virtual void OnProc(Unit triggerer, IUnitAction action)
        {
        }
    }
}