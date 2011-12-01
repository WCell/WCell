/*************************************************************************
 *
 *   file		: Aura.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-03 04:37:17 +0100 (on, 03 feb 2010) $
 
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
		public static readonly Aura[] EmptyArray = new Aura[0];

		public static readonly IEnumerator<Aura> EmptyEnumerator = new AuraEnumerator();

		#region Fields
		public readonly AuraIndexId Id;
		protected internal AuraCollection m_auras;
		protected ObjectReference m_CasterReference;
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
		protected int m_auraPeriod;
		protected int m_ticks;
		protected int m_maxTicks;
		TimerEntry m_timer;

		protected byte m_index;
		protected AuraFlags m_auraFlags;
		protected byte m_auraLevel;

		protected AuraRecord m_record;

		private Item m_UsedItem;

		private bool m_hasPeriodicallyUpdatedEffectHandler;
		#endregion

		#region Creation & Init
		private Aura()
		{
		}

		/// <summary>
		/// Creates a new Aura
		/// </summary>
		/// <param name="auras"></param>
		/// <param name="casterReference">Information about who casted</param>
		/// <param name="spell">The spell that this Aura represents</param>
		/// <param name="handlers">All handlers must have the same AuraUID</param>
		internal Aura(AuraCollection auras, ObjectReference casterReference, Spell spell,
			List<AuraEffectHandler> handlers, byte index, bool beneficial)
		{
			m_auras = auras;

			m_spell = spell;
			m_beneficial = beneficial;
			Id = spell.GetAuraUID(beneficial);

			m_handlers = handlers;
			m_CasterReference = casterReference;
			m_index = index;
			m_auraLevel = (byte)casterReference.Level;

			m_stackCount = (byte)m_spell.InitialStackCount;
			if (m_stackCount > 0 && casterReference.UnitMaster != null)
			{
				m_stackCount = casterReference.UnitMaster.Auras.GetModifiedInt(SpellModifierType.Charges, m_spell, m_stackCount);
			}

			SetupValues();
		}

		internal Aura(AuraCollection auras, ObjectReference caster, AuraRecord record, List<AuraEffectHandler> handlers, byte index)
		{
			m_record = record;
			m_auras = auras;

			m_spell = record.Spell;
			m_beneficial = record.IsBeneficial;
			Id = m_spell.GetAuraUID(m_beneficial);

			m_handlers = handlers;
			m_CasterReference = caster;
			m_index = index;
			m_auraLevel = (byte)record.Level;

			m_stackCount = record.StackCount;

			SetupValues();

			// figure out amplitude and duration
			m_duration = record.MillisLeft;
			SetupTimer();

			// Start is called later
		}

		/// <summary>
		/// Called after setting up the Aura and before calling Start()
		/// </summary>
		private void SetupTimer()
		{
			if (m_controller == null)
			{
				// Aura controls itself
				if ((m_auraPeriod > 0 || m_duration > 0))
				{
					// aura has timer
					m_timer = new TimerEntry
								{
									Action = Apply
								};
				}
			}
		}

		private void SetupValues()
		{
			DetermineFlags();
			m_hasPeriodicallyUpdatedEffectHandler = m_handlers.Any(handler => handler is PeriodicallyUpdatedAuraEffectHandler);

			if (m_auraPeriod != 0)
				return;

			foreach (var handler in m_handlers)
			{
				// Aura has the AuraPeriod of the first effect with AuraPeriod set
				if (handler.SpellEffect.AuraPeriod > 0)
				{
					m_auraPeriod = handler.SpellEffect.AuraPeriod;
					break;
				}
			}
		}

		private void DetermineFlags()
		{
			m_auraFlags = m_spell.DefaultAuraFlags;

			if (m_auras.Owner.EntityId == m_CasterReference.EntityId)
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
				var index = (int)handler.SpellEffect.EffectIndex;
				if (index >= 0)
				{
					m_auraFlags |= (AuraFlags)(1 << index);
				}
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

		public bool CanBeRemoved
		{
			get
			{
				return m_spell != null && m_beneficial &&
				  !m_spell.AttributesEx.HasAnyFlag(SpellAttributesEx.Negative) &&
				  !m_spell.Attributes.HasAnyFlag(SpellAttributes.CannotRemove);
			}
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
		public ObjectReference CasterReference
		{
			get { return m_CasterReference; }
		}

		/// <summary>
		/// The actual Caster (returns null if caster went offline or disappeared for some other reason)
		/// </summary>
		public Unit CasterUnit
		{
			get
			{
				var caster = m_CasterReference.UnitMaster;
				if (caster != null && caster.IsInContext)
				{
					return caster;
				}
				return null;
			}
		}
		/// <summary>
		/// The SpellCast that caused this Aura (if still present)
		/// </summary>
		public SpellCast SpellCast
		{
			get
			{
				var channel = Controller as SpellChannel;
				if (channel != null)
				{
					return channel.Cast;
				}
				else
				{
					var caster = CasterUnit;
					if (caster != null)
					{
						return caster.SpellCast;
					}
				}

				return null;
			}
		}

		public Unit Owner
		{
			get { return m_auras.Owner; }
		}

		public Item UsedItem
		{
			get
			{
				if (m_UsedItem != null && m_UsedItem.IsInWorld && m_UsedItem.IsInContext)
				{
					return m_UsedItem;
				}
				return null;
			}
			internal set { m_UsedItem = value; }
		}

		/// <summary>
		///  The amplitude between aura-ticks (only for non-passive auras which are not channeled)
		/// </summary>
		public int AuraPeriod
		{
			get { return m_auraPeriod; }
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
					m_CasterReference.Object != m_auras.Owner;
			}
		}

		/// <summary>
		/// The maximum amount of Applications for this Aura
		/// </summary>
		public int MaxApplications
		{
            get { return m_spell.SpellAuraOptions.MaxStackCount; }
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

					// normal timeout
					if (m_auraPeriod > 0)
					{
						// periodic

						// no timeout -> infinitely many ticks
						if (value <= 0)
						{
							m_maxTicks = int.MaxValue;
						}
						else
						{
							m_maxTicks = value / m_auraPeriod;

							if (m_maxTicks < 1)
							{
								m_maxTicks = 1;
							}
						}
						time = value % (m_auraPeriod + 1);
					}
					else
					{
						// modal aura (either on or off)
						time = value;
						m_maxTicks = 1;
					}

					m_ticks = 0;

					//stop timer if we set a negative duration
					if (value < 0)
					{
						m_timer.Stop();
					}
					else
						m_timer.Start(time);
				}
			}
		}

		/// <summary>
		/// Wheter this Aura can be saved
		/// </summary>
		public bool CanBeSaved
		{
			get;
			set;
		}

		/// <summary>
		/// Whether it is safe and legal to steal this Aura (only temporary Auras that are not controlled by a channel or similar)
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
				//return m_auraPeriod > 0 && !m_spell.IsPassive && m_controller == null;
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

		/// <summary>
		/// Duration in millis
		/// </summary>
		public int Duration
		{
			get { return m_controller == null ? m_duration : m_controller.Duration; }
			set { m_duration = value; m_auraFlags |= AuraFlags.HasDuration; SetupTimer(); TimeLeft = m_duration; }
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

		public bool HasPeriodicallyUpdatedEffectHandler
		{
			get { return m_hasPeriodicallyUpdatedEffectHandler; }
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
				m_duration = Spell.GetDuration(m_CasterReference, m_auras.Owner);
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

			if (m_auras.MayActivate(this))
			{
				IsActivated = true;
			}

			CanBeSaved = this != m_auras.GhostAura &&
						 !m_spell.AttributesExC.HasFlag(SpellAttributesExC.HonorlessTarget) &&
						 UsedItem == null;

			m_auras.OnAuraChange(this);

			var caster = CasterUnit;
			var owner = Owner;
			//if (caster != null)
			//{
			//    caster.Proc(ProcTriggerFlags.AuraStarted, owner,
			//        new AuraAction { Attacker = caster, Victim = owner, Aura = this }, true);
			//}
		}

		#endregion

		#region Active & Enable & Disable
		private bool m_IsActivated;

		/// <summary>
		/// Disables the Aura without removing it's effects
		/// </summary>
		public bool IsActivated
		{
			get { return m_IsActivated; }
			set
			{
				if (m_IsActivated != value)
				{
					if (m_IsActivated = value)
					{
						Activate();
					}
					else
					{
						// remove all aura-related effects
						Deactivate(false);
					}
				}
			}
		}

		private void Activate()
		{
			// custom prochandlers to be applied when spell is casted
			if (m_spell.IsProc && CasterUnit != null && m_spell.ProcHandlers != null)
			{
				foreach (var templ in m_spell.ProcHandlers)
				{
					Owner.AddProcHandler(new ProcHandler(CasterUnit, Owner, templ));
				}
			}

			if (m_spell.IsAuraProcHandler)
			{
				// only add proc if there is not a custom handler for it
				m_auras.Owner.AddProcHandler(this);
			}

			if (m_spell.IsAreaAura && Owner.EntityId == CasterReference.EntityId)
			{
				// activate AreaAura
				var aaura = m_auras.Owner.GetAreaAura(m_spell);
				if (aaura != null)
				{
					aaura.Start(m_controller, !HasTimeout);
				}
			}

			// apply all aura-related effects
			ApplyNonPeriodicEffects();
			SendToClient();
		}

		/// <summary>
		/// Called when the Aura gets deactivated
		/// </summary>
		/// <param name="cancelled"></param>
		private void Deactivate(bool cancelled)
		{
			// custom prochandlers to be applied when spell is casted
			if (m_spell.ProcHandlers != null && CasterUnit != null)
			{
				foreach (var templ in m_spell.ProcHandlers)
				{
					Owner.RemoveProcHandler(templ);
				}
			}

			if (m_spell.IsAuraProcHandler)
			{
				// TODO: This causes an issue if we deactivate an Aura while proc handlers are iterated
				m_auras.Owner.RemoveProcHandler(this);
			}
			if (m_spell.IsAreaAura && Owner.EntityId == CasterReference.EntityId)
			{
				// deactivate AreaAura
				var aaura = m_auras.Owner.GetAreaAura(m_spell);
				if (aaura != null)
				{
					aaura.IsActivated = false;
				}
			}

			CallAllHandlers(handler => handler.DoRemove(cancelled));
			RemoveFromClient();
		}
		#endregion

		#region Apply & Stack
		/// <summary>
		/// Applies this Aura's effect to its holder
		/// </summary>
		public void Apply()
		{
			Apply(0);
		}

		/// <summary>
		/// Applies one of this Aura's Ticks to its holder
		/// </summary>
		internal void Apply(int timeElapsed)
		{
			m_ticks++;

			// if controlled, the Controller decides when the Aura expires
			var expired = (!m_spell.HasPeriodicAuraEffects || m_ticks >= m_maxTicks) && m_controller == null;

			if (m_IsActivated)
			{
				OnApply();
				ApplyPeriodicEffects();

				if (!IsAdded)
				{
					return;
				}

				if (!expired && m_timer != null)
				{
					m_timer.Start(m_auraPeriod);
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
		public void ReApplyNonPeriodicEffects()
		{
			RemoveNonPeriodicEffects();

			// update effect values
			foreach (var handler in m_handlers)
			{
				handler.UpdateEffectValue();
			}

			ApplyNonPeriodicEffects();
		}

		/// <summary>
		/// Applies all non-perodic Aura-effects
		/// </summary>
		internal void ApplyNonPeriodicEffects()
		{
			if (m_spell.HasNonPeriodicAuraEffects)
			{
				foreach (var handler in Handlers)
				{
					if (!handler.SpellEffect.IsPeriodic && m_auras.MayActivate(handler))
					{
						handler.DoApply();
						if (!IsAdded)
						{
							// aura got removed by handler (maybe owner died etc)
							return;
						}
					}
				}
			}
		}

		internal void ApplyPeriodicEffects()
		{
			if (m_spell.HasPeriodicAuraEffects)
			{
				foreach (var handler in m_handlers)
				{
					if (handler.SpellEffect.IsPeriodic && m_auras.MayActivate(handler))
					{
						handler.DoApply();
						if (!IsAdded)
						{
							// aura got removed by handler (maybe owner died etc)
							return;
						}
					}
				}
			}
		}

		/// <summary>
		/// Do certain special behavior everytime an Aura is applied
		/// for very basic Aura categories.
		/// </summary>
		private void OnApply()
		{
			if (m_spell.IsFood || m_spell.IsDrink)
			{
				CasterReference.UnitMaster.Emote(EmoteType.SimpleEat);
			}
		}

		/// <summary>
		/// Refreshes this aura. 
		/// If this Aura is stackable, will also increase the StackCount by one.
		/// </summary>
		public void Refresh(ObjectReference caster)
		{
			if (IsAdded)
			{
				// remove non-periodic effects:
				RemoveNonPeriodicEffects();

				m_CasterReference = caster;

				if (m_spell.InitialStackCount > 1)
				{
					m_stackCount = (byte)m_spell.InitialStackCount;
				}
                else if (m_stackCount < m_spell.SpellAuraOptions.MaxStackCount)
				{
					m_stackCount++;
				}

				// update effect values
				foreach (var handler in m_handlers)
				{
					handler.UpdateEffectValue();
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

		/// <summary>
		/// Checks all handlers and toggles those whose requirements aren't met
		/// </summary>
		internal void ReEvaluateNonPeriodicHandlerRequirements()
		{
			if (Spell.HasNonPeriodicAuraEffects)
			{
				foreach (var handler in Handlers)
				{
					if (!handler.SpellEffect.IsPeriodic)
					{
						handler.IsActivated = m_auras.MayActivate(handler);
					}
				}
			}
		}

		public enum AuraOverrideStatus
		{
			/// <summary>
			/// Aura does not exist
			/// </summary>
			NotPresent,

			/// <summary>
			/// Aura can be overridden (if the previous Aura can be removed)
			/// </summary>
			Replace,

			/// <summary>
			/// 
			/// </summary>
			Refresh,
			Bounced
		}

		/// <summary>
		/// Stack or removes the given Aura, if possible.
		/// Returns whether the given incompatible Aura was removed or stacked.
		/// <param name="err">Ok, if stacked or no incompatible Aura was found</param>
		/// </summary>
		public AuraOverrideStatus GetOverrideStatus(ObjectReference caster, Spell spell)
		{
			if (Spell.IsPreventionDebuff)
			{
				return AuraOverrideStatus.Bounced;
			}

			if (Spell == spell)
			{
				// same spell can always be refreshed
				return AuraOverrideStatus.Refresh;
			}
			else
			{
				if (caster == CasterReference)
				{
					if (spell != Spell
						//&&
						//spell.AuraCasterGroup != null &&
						//spell.AuraCasterGroup == Spell.AuraCasterGroup &&
						//spell.AuraCasterGroup.MaxCount == 1
						)
					{
						// different spell -> needs to be overridden
						return AuraOverrideStatus.Replace;
					}
					else
					{
						// Aura can be refreshed
						return AuraOverrideStatus.Refresh;
					}
				}
				else if (!spell.CanOverride(Spell))
				{
					return AuraOverrideStatus.Bounced;
				}

				return AuraOverrideStatus.Refresh;
			}
		}
		#endregion

		#region Remove & Cancel
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
						handler.IsActivated = false;
					}
				}
			}
		}

		public bool TryRemove(bool cancelled)
		{
			if (m_spell.IsAreaAura)
			{
				// can only cancel AreaAuras if you are the one causing it or if it can time-out
				var owner = m_auras.Owner;
				if (owner.EntityId.Low == CasterReference.EntityId || CasterUnit == null || CasterUnit.UnitMaster == owner)
				{
					owner.CancelAreaAura(m_spell);
					return true;
				}
			}
			else
			{
				Remove(cancelled);
				return true;
			}
			return false;
		}

		public void Cancel()
		{
			Remove();
		}

		internal void RemoveWithoutCleanup()
		{
			if (IsAdded)
			{
				IsAdded = false;
				Deactivate(true);

				if (m_controller != null)
				{
					m_controller.OnRemove(Owner, this);
				}

				OnRemove();
			}
		}

		/// <summary>
		/// Removes this Aura from the player
		/// </summary>
		public void Remove(bool cancelled = true)
		{
			if (IsAdded)
			{
				IsAdded = false;

				var owner = m_auras.Owner;
				if (owner == null)
				{
					LogManager.GetCurrentClassLogger().Warn("Tried to remove Aura {0} but it's owner does not exist anymore.");
					return;
				}

				if (m_controller != null)
				{
					m_controller.OnRemove(owner, this);
				}

				var auras = m_auras;
				var caster = CasterUnit;

				if (caster != null)
				{
					//caster.Proc(ProcTriggerFlags.AuraRemoved, owner,
					//    new AuraAction { Attacker = caster, Victim = owner, Aura = this }, true);	
					m_spell.NotifyAuraRemoved(this);
				}

				Deactivate(cancelled);
				RemoveVisibleEffects(cancelled);

				auras.Remove(this);
				OnRemove();

				if (m_spell.IsAreaAura && owner.EntityId == CasterReference.EntityId && owner.CancelAreaAura(m_spell))
				{
					//return;
				}
			}
		}

		void OnRemove()
		{
			if (m_record != null)
			{
				m_record.DeleteLater();
				m_record = null;
			}
		}

		/// <summary>
		/// Takes care of all the eye candy that is related to the removal of this Aura.
		/// </summary>
		protected void RemoveVisibleEffects(bool cancelled)
		{
			var owner = m_auras.Owner;
			if (m_spell.IsFood)
			{
				owner.StandState = StandState.Stand;
			}

			if (owner.EntityId == m_CasterReference.EntityId &&
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
			IsActivated = false;
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

		public void Update(int dt)
		{
			if (m_hasPeriodicallyUpdatedEffectHandler)
			{
				foreach (var handler in m_handlers)
				{
					if (handler is PeriodicallyUpdatedAuraEffectHandler)
					{
						((PeriodicallyUpdatedAuraEffectHandler)handler).Update();
					}
				}
			}
			if (m_timer != null)
			{
				m_timer.Update(dt);
			}
		}

		#endregion

		#region Procs
		public ProcTriggerFlags ProcTriggerFlags
		{
            get { return m_spell.SpellAuraOptions.ProcTriggerFlags; }
		}

		public ProcHitFlags ProcHitFlags
		{
            get { return Spell.SpellAuraOptions.ProcHitFlags; }
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
            get { return m_spell.SpellAuraOptions.ProcChance > 0 ? m_spell.SpellAuraOptions.ProcChance : 100; }
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
						if (handler.CanProcBeTriggeredBy(action) &&
							handler.SpellEffect.CanProcBeTriggeredBy(action.Spell))
						{
							// only trigger if no AffectMask or spell, or the trigger spell matches the affect mask
							canProc = true;
							break;
						}
					}
				}
			}
			else if (action.Spell == null || action.Spell != Spell)
			{
				// Simply count down stack count and remove aura eventually
				canProc = true;
			}

			return canProc && m_spell.CanProcBeTriggeredBy(m_auras.Owner, action, active);
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
						if (handler.CanProcBeTriggeredBy(action) &&
							handler.SpellEffect.CanProcBeTriggeredBy(action.Spell))
						{
							// only trigger if no AffectMask or spell, or the trigger spell matches the affect mask
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

            if (proced && m_spell.SpellAuraOptions.ProcCharges > 0)
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
			RealmServer.IOQueue.AddMessage(SaveNow);
		}

		internal void SaveNow()
		{
			if (m_record == null)
			{
				var owner = m_auras.Owner;
				if (!(owner is Character))
				{
					// We currently only support Aura saving for Characters
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
				(HasTimeout ? " [TimeLeft: " + TimeSpan.FromMilliseconds(TimeLeft) + "]" : "") +
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