/*************************************************************************
 *
 *   file		: Aura.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-03 04:37:17 +0100 (on, 03 feb 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1243 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Timers;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras
{
	/// <summary>
	/// An Aura is any kind of long-lasting passive effect or buff.
	/// Some can be seen as an icon below the Player's status bar.
	/// </summary>
	public class Aura : IAura, IUpdatable, IProcHandler, ITickTimer
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public static readonly Aura[] EmptyArray = new Aura[0];

		public static readonly IEnumerator<Aura> EmptyEnumerator = new AuraEnumerator();

		#region Fields
		public readonly AuraIndexId Id;
		protected internal AuraCollection m_auras;
		protected CasterInfo m_casterInfo;
		protected Spell m_spell;
		protected List<AuraEffectHandler> m_handlers;
		protected bool m_beneficial;
		/// <summary>
		/// The controlling Timer (eg a SpellChannel) or null if self-controlled
		/// </summary>
		protected ITickTimer m_controller;

		protected int m_stackCount;
		protected int m_startTime;
		protected int m_duration;
		protected int m_amplitude;
		protected int m_ticks;
		protected int m_maxTicks;
		TimerEntry m_timer;

		protected byte m_index;
		protected AuraFlags m_auraFlags;
		protected byte m_auraLevel;

		protected AuraRecord m_record;
		#endregion

		#region Creation & Init
		public Aura()
		{
		}

		/// <summary>
		/// Creates a new Aura
		/// </summary>
		/// <param name="auras"></param>
		/// <param name="casterInfo">Information about who casted</param>
		/// <param name="spell">The spell that this Aura represents</param>
		/// <param name="handlers">All handlers must have the same AuraUID</param>
		internal Aura(AuraCollection auras, CasterInfo casterInfo, Spell spell,
			List<AuraEffectHandler> handlers, byte index, bool beneficial)
		{
			m_auras = auras;

			m_spell = spell;
			m_beneficial = beneficial;
			Id = spell.GetAuraUID(beneficial);

			m_handlers = handlers;
			m_casterInfo = casterInfo;
			m_index = index;
			m_auraLevel = (byte)casterInfo.Level;

			m_stackCount = (byte)m_spell.StackCount;
			if (m_stackCount > 0 && Caster is Character)
			{
				m_stackCount = ((Character)Caster).PlayerSpells.GetModifiedInt(SpellModifierType.Charges, m_spell, m_stackCount);
			}

			SetAmplitude();
			DetermineFlags();
		}

		internal Aura(AuraCollection auras, CasterInfo caster, AuraRecord record, List<AuraEffectHandler> handlers, byte index)
		{
			m_record = record;
			m_auras = auras;

			m_spell = record.Spell;
			m_beneficial = record.IsBeneficial;
			Id = m_spell.GetAuraUID(m_beneficial);

			m_handlers = handlers;
			m_casterInfo = caster;
			m_index = index;
			m_auraLevel = (byte)record.Level;

			m_stackCount = record.StackCount;

			SetAmplitude();
			DetermineFlags();

			// figure out amplitude and duration
			m_duration = record.MillisLeft;
			SetupTimer();

			if (m_spell.IsAreaAura)
			{
				var areaAura = new AreaAura(m_auras.Owner, m_spell);
				areaAura.Start(null, false);
			}
		}

		private void SetupTimer()
		{
			if (m_amplitude == 0 && m_duration < int.MaxValue)
			{
				m_amplitude = m_duration;
			}
			if (m_amplitude > 0 && m_controller == null)
			{
				// Aura times itself
				m_timer = new TimerEntry
				{
					Action = Apply
				};
			}
		}

		private void SetAmplitude()
		{
			if (m_amplitude != 0)
				return;

			foreach (var handler in m_handlers)
			{
				// Aura has the Amplitude of the first effect with Amplitude set
				if (handler.SpellEffect.Amplitude > 0)
				{
					m_amplitude = handler.SpellEffect.Amplitude;
					break;
				}
			}
		}

		private void DetermineFlags()
		{
			m_auraFlags = m_spell.DefaultAuraFlags;

			if (m_auras.Owner.EntityId == m_casterInfo.CasterId)
			{
				m_auraFlags |= AuraFlags.TargetIsCaster;
			}

			if (m_beneficial)
			{
				m_auraFlags |= AuraFlags.Positive;
			}
			else
			{
				m_auraFlags |= AuraFlags.Negative;
			}

			if (m_spell.Durations.Min > 0)
			{
				m_auraFlags |= AuraFlags.HasDuration;
			}

			for (var i = Math.Min(m_handlers.Count - 1, 2); i >= 0; i--)
			{
				var handler = m_handlers[i];
				m_auraFlags |= (AuraFlags)(1 << handler.SpellEffect.EffectIndex);
			}

			if (m_auraFlags == 0)
			{
				m_auraFlags = AuraFlags.Effect1AppliesAura;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// The <c>AuraCollection</c> of the Unit that owns this Aura.
		/// </summary>
		public AuraCollection Auras
		{
			get { return m_auras; }
		}

		/// <summary>
		/// The Spell that belongs to this Aura
		/// </summary>
		public Spell Spell
		{
			get { return m_spell; }
		}

		/// <summary>
		/// The amount of times that this Aura has been applied
		/// </summary>
		public int StackCount
		{
			get { return m_stackCount; }
			set { m_stackCount = value; }
		}

		/// <summary>
		/// Whether this Aura is added to it's owner
		/// </summary>
		public bool IsAdded
		{
			get;
			protected internal set;
		}

		public bool CanBeCancelled
		{
			get { return m_spell != null && m_beneficial && !m_spell.Attributes.HasFlag(SpellAttributes.CannotRemove); }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsBeneficial
		{
			get { return m_beneficial; }
		}

		/// <summary>
		/// The amount of ticks left (always 0 for non-periodic auras)
		/// </summary>
		public int TicksLeft
		{
			get { return MaxTicks - Ticks; }
		}

		/// <summary>
		/// Information about the caster
		/// </summary>
		public CasterInfo CasterInfo
		{
			get { return m_casterInfo; }
		}

		/// <summary>
		/// The actual Caster (returns null if caster went offline or disappeared for some other reason)
		/// </summary>
		public Unit Caster
		{
			get
			{
				var caster = m_casterInfo.CasterUnit;
				if (caster != null && caster.ContextHandler == Owner.ContextHandler)
				{
					return caster;
				}
				return null;
			}
		}

		public Unit Owner
		{
			get { return m_auras.Owner; }
		}

		/// <summary>
		///  The amplitude between aura-ticks (only for non-passive auras which are not channeled)
		/// </summary>
		public int Amplitude
		{
			get { return m_amplitude; }
		}

		/// <summary>
		/// Whether this Aura is not visible to the client (only its effects will make him realize it)
		/// </summary>
		public bool IsVisible
		{
			get
			{
				return !m_spell.IsPassive ||
					m_spell.AttributesEx.HasFlag(SpellAttributesEx.Negative) ||
					m_casterInfo.Caster != m_auras.Owner;
			}
		}

		/// <summary>
		/// The maximum amount of Applications for this Aura
		/// </summary>
		public int MaxApplications
		{
			get { return m_spell.MaxStackCount; }
		}

		/// <summary>
		/// The position of this Aura within the client's Aura-bar (0 if not exposed to client)
		/// </summary>
		public byte Index
		{
			get { return m_index; }
			set
			{
				RemoveFromClient();
				m_index = value;
				SendToClient();
			}
		}

		/// <summary>
		/// Time that is left until this Aura disbands in millis.
		/// Auras's without timeout can't be resetted.
		/// Channeled Auras are controlled by the holding SpellChannel.
		/// Returns a negative value if Aura doesn't has a timeout (or is already expired).
		/// </summary>
		public int TimeLeft
		{
			get
			{
				if (m_controller == null)
				{
					return m_duration - (Environment.TickCount - m_startTime);
				}
				return m_controller.TimeLeft;
			}
			set
			{
				if (HasTimer)
				{
					// figure out amplitude
					m_startTime = Environment.TickCount;

					int time;
					if (m_amplitude > 0)
					{
						m_maxTicks = value / m_amplitude;
						time = value % (m_amplitude + 1);
					}
					else
					{
						time = value;
					}

					if (m_maxTicks < 1)
					{
						m_amplitude = value;
						m_maxTicks = 1;
					}

					m_ticks = 0;

					m_timer.Start(time);
				}
			}
		}

		/// <summary>
		/// Whether it is save and legal to steal this Aura (only temporary Auras that are not controlled by a channel or similar)
		/// </summary>
		public bool CanBeStolen
		{
			get { return HasTimeout && !m_spell.IsTriggeredSpell; }
		}

		public IEnumerable<AuraEffectHandler> Handlers
		{
			get { return m_handlers; }
		}

		/// <summary>
		/// The controller of this Aura which controls timing, application and removal (such as <see cref="SpellChannel">SpellChannels</see>)
		/// </summary>
		public ITickTimer Controller
		{
			get { return m_controller; }
		}

		/// <summary>
		/// Auras that are not passive and not controlled by a <c>ITickTimer</c> have their own Timers
		/// </summary>
		public bool HasTimeout
		{
			get
			{
				//return m_amplitude > 0 && !m_spell.IsPassive && m_controller == null;
				return m_spell.Durations.Min > 0 && m_controller == null;
			}
		}

		public bool HasTimer
		{
			get { return m_timer != null; }
		}

		public int Ticks
		{
			get { return m_controller == null ? m_ticks : m_controller.Ticks; }
		}

		public int MaxTicks
		{
			get { return m_controller == null ? m_maxTicks : m_controller.MaxTicks; }
		}

		public int Duration
		{
			get { return m_controller == null ? m_duration : m_controller.Duration; }
		}

		public int Until
		{
			get
			{
				if (m_spell.IsPassive)
				{
					return -1;
				}
				else if (m_controller != null)
				{
					return m_controller.Until;
				}
				return Environment.TickCount - m_startTime;
			}
		}

		public byte Level
		{
			get { return m_auraLevel; }
		}

		public AuraFlags Flags
		{
			get { return m_auraFlags; }
		}

		#endregion

		#region Start
		/// <summary>
		/// Method is called 
		/// </summary>
		/// <param name="noTimeout">Whether the Aura should always continue and never expire.</param>
		public void Start(ITickTimer controller, bool noTimeout)
		{
			m_controller = controller;

			if (noTimeout)
			{
				m_duration = -1;
			}
			else
			{
				m_duration = Spell.GetDuration(m_casterInfo, m_auras.Owner);
			}

			SetupTimer();

			Start();
		}

		public void Start()
		{
			TimeLeft = m_duration;

			// init AuraEffectHandlers
			var handlers = m_handlers;
			foreach (var handler in handlers)
			{
				handler.Init(this);
			}

			CheckActivation();

			m_auras.OnAuraChange(this);
		}

		#endregion

		#region Active

		private bool m_IsActive;

		/// <summary>
		/// Disables the Aura without removing it's effects
		/// </summary>
		public bool IsActive
		{
			get { return m_IsActive; }
			set
			{
				if (m_IsActive != value)
				{
					if (m_IsActive = value)
					{
						Enable();
						SendToClient();
					}
					else
					{
						// remove all aura-related effects
						Disable(false);
						RemoveFromClient();
					}
				}
			}
		}

		/// <summary>
		/// These checks coincide with the checks in <see cref="PlayerAuraCollection"/>
		/// </summary>
		internal void CheckActivation()
		{
			var owner = Owner as Character;
			if (owner == null ||
				!m_spell.IsPassive ||
				((!m_spell.HasItemRequirements || m_spell.CheckItemRestrictionsWithout(null, owner.Inventory) == SpellFailedReason.Ok) &&
				(m_spell.AllowedShapeshiftMask == 0 || m_spell.AllowedShapeshiftMask.HasAnyFlag(owner.ShapeshiftMask))))
			{
				IsActive = true;
			}
		}

		#endregion

		#region Apply & Stack
		/// <summary>
		/// Applies this Aura's effect to its holder
		/// </summary>
		public void Apply()
		{
			Apply(0.0f);
		}

		/// <summary>
		/// Applies one of this Aura's Ticks to its holder
		/// </summary>
		public void Apply(float timeElapsed)
		{
			m_ticks++;

			// if controlled, the Controller decides when the Aura expires
			var expired = (!m_spell.HasPeriodicAuraEffects || m_ticks >= m_maxTicks) && m_controller == null;

			if (m_IsActive)
			{
				OnApply();

				if (!expired || m_spell.HasPeriodicAuraEffects)
				{
					ApplyHandlersTick();
					if (!IsAdded)
					{
						return;
					}
				}

				if (!expired && m_timer != null)
				{
					m_timer.Start(m_amplitude);
				}
			}

			if (expired)
			{
				Remove(false);
			}
		}

		/// <summary>
		/// Removes and then re-applies all non-perodic Aura-effects
		/// </summary>
		void ApplyNonPeriodicEffects()
		{
			if (m_spell.HasNonPeriodicAuraEffects)
			{
				foreach (var handler in m_handlers)
				{
					if (!handler.SpellEffect.IsPeriodic)
					{
						handler.Apply();
					}
				}
			}
		}

		private void ApplyHandlersTick()
		{
			foreach (var handler in m_handlers)
			{
				if (m_ticks < 1 || handler.SpellEffect.IsPeriodic) // initial tick or periodic handler
				{
					handler.Apply();
				}

				if (!IsAdded)
				{
					// aura got removed by handler
					return;
				}
			}
		}

		/// <summary>
		/// Removes and then re-applies all non-perodic Aura-effects
		/// </summary>
		public void ReApplyEffects()
		{
			if (m_spell.HasNonPeriodicAuraEffects)
			{
				foreach (var handler in m_handlers)
				{
					if (!handler.SpellEffect.IsPeriodic)
					{
						handler.Remove(false);
						handler.Apply();
					}
				}
			}
		}

		/// <summary>
		/// Removes and then re-applies all non-perodic Aura-effects
		/// </summary>
		void RemoveNonPeriodicEffects()
		{
			if (m_spell.HasNonPeriodicAuraEffects)
			{
				foreach (var handler in m_handlers)
				{
					if (!handler.SpellEffect.IsPeriodic)
					{
						handler.Remove(false);
					}
				}
			}
		}

		/// <summary>
		/// Do certain special behavior everytime Aura is applied
		/// </summary>
		private void OnApply()
		{
			if (m_spell.IsFood || m_spell.IsDrink)
			{
				CasterInfo.CasterUnit.Emote(EmoteType.SimpleEat);
			}
		}

		/// <summary>
		/// Add one more application to the stack
		/// </summary>
		public void Stack(CasterInfo caster)
		{
			if (IsAdded)
			{
				// remove non-periodic effects:
				RemoveNonPeriodicEffects();

				m_casterInfo = caster;
				if (m_stackCount < m_spell.MaxStackCount)
				{
					m_stackCount++;
				}

				// re-apply non-periodic effects:
				ApplyNonPeriodicEffects();

				// reset timer:
				TimeLeft = m_spell.GetDuration(caster, m_auras.Owner);

				if (IsVisible)
				{
					AuraHandler.SendAuraUpdate(m_auras.Owner, this);
				}
			}
		}
		#endregion

		#region Remove & Cancel
		/// <summary>
		/// Cancels and removes this Aura
		/// </summary>
		public void Cancel()
		{
			Remove(true);
		}

		public bool TryRemove(bool cancelled)
		{
			if (m_spell.IsAreaAura)
			{
				// can only cancel AreaAuras if you are the one causing it or if it can time-out
				var owner = m_auras.Owner;
				if (owner.EntityId.Low == CasterInfo.CasterId || Caster == null || Caster.UnitMaster == owner)
				{
					return owner.CancelAreaAura(m_spell);
				}
			}
			else
			{
				Remove(true);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes this Aura from the player
		/// </summary>
		/// <param name="cancelled"></param>
		public void Remove(bool cancelled)
		{
			if (IsAdded)
			{
				IsAdded = false;

				var owner = m_auras.Owner;
				var caster = Caster;
				if (caster != null)
				{
					owner.Proc(ProcTriggerFlags.AuraRemoved, owner, 
						new AuraRemovedAction{Attacker = caster, Victim = owner, Aura = this}, true);
				}

				if (m_spell.IsAreaAura && owner.CancelAreaAura(m_spell))
				{
					return;
				}

				var auras = m_auras;

				IsActive = false;

				RemoveVisibleEffects(cancelled);

				auras.Cancel(this);
				if (m_controller != null)
				{
					m_controller.OnRemove(owner, this);
				}

				if (m_record != null)
				{
					m_record.DeleteLater();
					m_record = null;
				}
			}
		}

		/// <summary>
		/// Takes care of all the eye candy that is related to the removal of this Aura.
		/// </summary>
		protected void RemoveVisibleEffects(bool cancelled)
		{
			RemoveFromClient();

			var owner = m_auras.Owner;
			if (m_spell.IsFood)
			{
				owner.StandState = StandState.Stand;
			}

			if (owner.EntityId == m_casterInfo.CasterId &&
				m_spell.Attributes.HasFlag(SpellAttributes.StartCooldownAfterEffectFade))
			{
				if (owner is Character)
				{
					//SpellHandler.SendSpellCooldown(m_auras.Owner, ((Character)m_auras.Owner).Client, m_spell.Id, (ushort)m_spell.GetCooldown(m_auras.Owner));
					SpellHandler.SendCooldownUpdate((Character)owner, m_spell.SpellId);
				}
				//m_auras.Owner.Spells.AddCooldown(m_spell, null);
			}
		}

		/// <summary>
		/// Need to guarantee that all Auras that have ever been created will also be removed
		/// </summary>
		internal void Cleanup()
		{
			IsActive = false;
			if (m_record != null)
			{
				var record = m_record;
				m_record = null;
				record.Recycle();
			}
		}

		/// <summary>
		/// See IIAura.OnRemove
		/// </summary>
		public void OnRemove(Unit owner, Aura aura)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Enable & Disable
		private void Enable()
		{
			// custom prochandlers to be applied when spell is casted
			if (m_spell.IsProc && Caster != null && m_spell.ProcHandlers != null)
			{
				foreach (var templ in m_spell.ProcHandlers)
				{
					Owner.AddProcHandler(new ProcHandler(Caster, Owner, templ));
				}
			}

			if (m_spell.DoesAuraHandleProc)
			{
				// only add proc if there is not a custom handler for it
				m_auras.Owner.AddProcHandler(this);
			}

			// apply all aura-related effects
			ApplyNonPeriodicEffects();
		}

		/// <summary>
		/// Guaranteed Cleanup
		/// </summary>
		/// <param name="cancelled"></param>
		internal void Disable(bool cancelled)
		{
			// custom prochandlers to be applied when spell is casted
			if (m_spell.ProcHandlers != null && Caster != null)
			{
				foreach (var templ in m_spell.ProcHandlers)
				{
					Owner.RemoveProcHandler(templ);
				}
			}

			if (m_spell.DoesAuraHandleProc)
			{
				// TODO: This causes an issue if we deactivate an Aura while proc handlers are iterated
				m_auras.Owner.RemoveProcHandler(this);
			}

			CallAllHandlers(handler => handler.Remove(cancelled));
		}
		#endregion

		#region Send Aura information

		protected internal void SendToClient()
		{
			if (!IsVisible)
			{
				return;
			}

#if DEBUG
			//log.Info("Sending Aura: " + this);
#endif

			//AuraHandler.SendAuraInfo(m_auras.Owner, (byte)m_index, m_spell.Id, (uint)m_duration, (uint)m_duration);
			AuraHandler.SendAuraUpdate(m_auras.Owner, this);
		}

		/// <summary>
		/// Removes all of this Aura's occupied fields
		/// </summary>
		protected void RemoveFromClient()
		{
			if (!IsVisible)
				return;

			AuraHandler.SendRemoveAura(m_auras.Owner, this);
		}
		#endregion

		#region IUpdatable

		public void Update(float dt)
		{
			if (m_timer != null)
			{
				m_timer.Update(dt);
			}
		}

		#endregion

		#region Procs
		public ProcTriggerFlags ProcTriggerFlags
		{
			get { return m_spell.ProcTriggerFlags; }
		}

		/// <summary>
		/// Spell to be triggered (if any)
		/// </summary>
		public Spell ProcSpell
		{
			get { return m_spell.ProcTriggerEffects != null ? m_spell.ProcTriggerEffects[0].TriggerSpell : null; }
		}

		/// <summary>
		/// Chance to proc in %
		/// </summary>
		public uint ProcChance
		{
			get { return m_spell.ProcChance > 0 ? m_spell.ProcChance : 100; }
		}

		public int MinProcDelay
		{
			get { return m_spell.ProcDelay; }
		}

		public DateTime NextProcTime
		{
			get;
			set;
		}

		public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
		{
			var hasProcEffects = m_spell.ProcTriggerEffects != null;
			var canProc = false;

			if (hasProcEffects)
			{
				foreach (var handler in m_handlers)
				{
					if (handler.SpellEffect.IsProc)
					{
						// only trigger proc effects or all effects, if there arent any proc-specific effects
						if ((!handler.SpellEffect.HasAffectMask ||
							(action.Spell != null && action.Spell.MatchesMask(handler.SpellEffect.AffectMask))))
						{
							// only trigger if no AffectMask is set or the triggerer matches the proc mask
							canProc = true;
							break;
						}
					}
				}
			}
			else
			{
				// Simply count down stack count and remove aura eventually
				canProc = true;
			}
			if (!canProc)
			{
				return false;
			}

			if (m_spell.CanProcBeTriggeredBy(m_auras.Owner, action, active))
			{
				return true;
			}
			return false;
		}

		public void TriggerProc(Unit triggerer, IUnitAction action)
		{
			var proced = false;

			var hasProcEffects = m_spell.ProcTriggerEffects != null;
			if (hasProcEffects)
			{
				foreach (var handler in m_handlers)
				{
					if (handler.SpellEffect.IsProc)
					{
						// only trigger proc effects or all effects, if there arent any proc-specific effects
						if ((!handler.SpellEffect.HasAffectMask ||
							(action.Spell != null && action.Spell.MatchesMask(handler.SpellEffect.AffectMask))))
						{
							// only trigger if no AffectMask is set or the trigger matches the proc mask
							handler.OnProc(triggerer, action);
							proced = true;
						}
					}
				}
			}
			else
			{
				// Simply reduce stack count and remove aura eventually
				proced = true;
			}

			if (proced && m_spell.MaxStackCount > 1)
			{
				// consume a charge
				m_stackCount--;
				if (m_stackCount == 0)
				{
					Remove(false);
				}
				else
				{
					AuraHandler.SendAuraUpdate(m_auras.Owner, this);
				}
			}
		}

		public void Dispose()
		{
			Remove(false);
		}
		#endregion

		#region Persistance
		public void Save()
		{
			RealmServer.Instance.AddMessage(() => SaveNow());
		}

		internal void SaveNow()
		{
			if (m_record == null)
			{
				// We currently only support Aura saving for chars
				var owner = m_auras.Owner;
				if (!(owner is Character))
				{
					throw new InvalidOperationException(string.Format("Tried to save non-Player Aura {0} on: {1}", this, owner));
				}
				m_record = AuraRecord.ObtainAuraRecord(this);
			}
			else
			{
				m_record.SyncData(this);
			}
			m_record.Save();
		}
		#endregion

		protected delegate void HandlerDelegate(AuraEffectHandler handler);

		protected void CallAllHandlers(HandlerDelegate dlgt)
		{
			foreach (var handler in m_handlers)
			{
				dlgt(handler);
			}
		}

		public AuraEffectHandler GetHandler(AuraType type)
		{
			foreach (var handler in Handlers)
			{
				if (handler.SpellEffect.AuraType == type)
				{
					return handler;
				}
			}
			return null;
		}

		public override string ToString()
		{
			return "Aura " + m_spell + ": " + (IsBeneficial ? "Beneficial" : "Harmful") +
				" [TimeLeft: " + TimeSpan.FromMilliseconds(TimeLeft) + "]" +
				(m_controller != null ? (" Controlled by: " + m_controller) : "");
		}

		#region AuraEnumerator
		class AuraEnumerator : IEnumerator<Aura>
		{
			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
			}

			public Aura Current
			{
				get { return null; }
			}

			object IEnumerator.Current
			{
				get { return null; }
			}
		}
		#endregion
	}
}