/*************************************************************************
 *
 *   file		: Unit.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-05-08 23:23:29 +0200 (lø, 08 maj 2010) $
 *   last author	: $LastChangedBy: XTZGZoReX $
 *   revision		: $Rev: 1290 $
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
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Chat;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Timers;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Paths;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Taxi;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Base class for Players and NPCs (also Totems and similar).
	/// 
	/// 
	/// </summary>
	public abstract partial class Unit : WorldObject, ILivingEntity, ISummoner
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Time in milliseconds between last move and when one officially stands still
		/// </summary>
		public static uint MinStandStillDelay = 400;

		public static readonly UpdateFieldCollection UpdateFieldInfos = UpdateFieldMgr.Get(ObjectTypeId.Unit);

		protected override UpdateFieldCollection _UpdateFieldInfos
		{
			get { return UpdateFieldInfos; }
		}

		#region Fields
		protected IBrain m_brain;
		protected Faction m_faction;
		protected SpellCollection m_spells;
		protected AuraCollection m_auras;
		protected int m_comboPoints;
		protected Unit m_comboTarget;
		protected ulong m_auraUpdateMask;

		/// <summary>
		/// Indicates whether regeneration of Health and Power is currently activated
		/// </summary>
		protected bool m_regenerates;

		/// <summary>
		/// Flat, school-specific PowerCostMods
		/// </summary>
		protected int[] m_schoolPowerCostMods;

		/// <summary>
		/// The time of when this Unit last moved (used for speedhack check)
		/// </summary>
		protected internal int m_lastMoveTime;

		protected Unit m_FirstAttacker;

		private Unit m_LastKiller;

		protected bool m_IsPinnedDown;

		protected internal TimerEntry m_TaxiMovementTimer;
		internal int taxiTime;

		/// <summary>
		/// The currently occupied VehicleSeat (if any)
		/// </summary>
		protected internal VehicleSeat m_vehicleSeat;
		#endregion

		protected Unit()
		{
			Type |= ObjectTypes.Unit;

			// combat
			m_isInCombat = false;
			m_attackTimer = new TimerEntry(CombatTick);

			CastSpeedFactor = 1f;

			ResetMechanicDefaults();

			m_flying = m_waterWalk = m_hovering = m_featherFalling = 0;
			m_canMove = m_canInteract = m_canHarm = m_canCastSpells = true;
		}

		/// <summary>
		/// The Unit that attacked this NPC first.
		/// </summary>
		public Unit FirstAttacker
		{
			get { return m_FirstAttacker; }
			set
			{
				if (value != null)
				{
					value = value.Master ?? value;
				}
				m_FirstAttacker = value;
				MarkUpdate(UnitFields.DYNAMIC_FLAGS);
			}
		}

		/// <summary>
		/// The Unit that last killed this guy or null, if none or gone (is not reliable over time).
		/// </summary>
		public Unit LastKiller
		{
			get
			{
				if (!m_LastKiller.IsInWorld)
				{
					m_LastKiller = null;
				}
				return m_LastKiller;
			}
			internal set { m_LastKiller = value; }
		}

		/// <summary>
		/// Whether this Unit is currently participating in PvP.
		/// That is if both participants are players and/or belong to players.
		/// </summary>
		public bool IsPvPing
		{
			get
			{
				return
					m_FirstAttacker != null &&
					IsPlayerOwned &&
					m_FirstAttacker.IsPlayerOwned;
			}
		}

		#region Misc Props

		public IBrain Brain
		{
			get { return m_brain; }
			set { m_brain = value; }
		}

		/// <summary>
		/// Whether this is a Spirit Guide/Spirit Healer.
		/// </summary>
		public bool IsSpiritHealer
		{
			get
			{
				return NPCFlags.HasFlag(NPCFlags.SpiritHealer);
			}
		}

		/// <summary>
		/// A collection of all Auras (talents/buffs/debuffs) of this Unit
		/// </summary>
		public AuraCollection Auras
		{
			get { return m_auras; }
		}

		public ulong AuraUpdateMask
		{
			get { return m_auraUpdateMask; }
			set { m_auraUpdateMask = value; }
		}


		/// <summary>
		/// Gets the chat tag for the character.
		/// </summary>
		public virtual ChatTag ChatTag
		{
			get
			{
				return ChatTag.None;
			}
		}

		public int LastMoveTime
		{
			get { return m_lastMoveTime; }
		}
		#endregion

		#region Combo Points
		/// <summary>
		/// Amount of current combo points with last combo target
		/// </summary>
		public int ComboPoints
		{
			get
			{
				return m_comboPoints;
			}
		}

		/// <summary>
		/// Current holder of combo-points for this chr
		/// </summary>
		public Unit ComboTarget
		{
			get
			{
				return m_comboTarget;
			}
			//set
			//{
			//    m_comboTarget = value;
			//    if (m_comboTarget == null)
			//    {
			//        m_comboPoints = 0;

			//    }
			//}
		}

		public void ResetComboPoints()
		{
			if (m_comboTarget != null)
			{
				ModComboState(null, 0);
			}
		}

		/// <summary>
		/// Change combo target and/or amount of combo points
		/// </summary>
		/// <returns>If there is a change</returns>
		public virtual bool ModComboState(Unit target, int amount)
		{
			if (amount != 0 || target != m_comboTarget)
			{
				if (target == null)
				{
					m_comboPoints = 0;
				}
				else
				{
					if (target == m_comboTarget)
					{
						m_comboPoints += amount;
					}
					else
					{
						m_comboPoints = amount;
					}
					m_comboPoints = MathUtil.ClampMinMax(m_comboPoints, 0, 5);
				}

				m_comboTarget = target;

				return true;
			}

			return false;
		}
		#endregion

		/// <summary>
		/// Returns one of the arbitrary modifier values
		/// </summary>
		public int GetIntMod(StatModifierInt stat)
		{
			if (IntMods != null)
			{
				return IntMods[(int)stat];
			}
			return 0;
		}

		#region Death
		public virtual bool IsAlive
		{
			get
			{
				return IsInWorld && Health > 0;
			}
		}

		public bool IsDead
		{
			get
			{
				return !IsAlive;
			}
		}

		/// <summary>
		/// Whether this is a ghost
		/// </summary>
		public bool IsGhost
		{
			get { return m_auras.GhostAura != null; }
		}

		private uint m_DeathPrevention;

		/// <summary>
		/// This is used to prevent this Unit from dying during a
		/// critical process, such as damage application.
		/// If health is at 0 this Unit won't "<see cref="Die"/>" until
		/// DeathPrevention is set to 0 again. This prevents certain problems from happening.
		/// </summary>
		protected internal uint DeathPrevention
		{
			get { return m_DeathPrevention; }
			set
			{
				if (m_DeathPrevention != value)
				{
					m_DeathPrevention = value;
					if (value == 0)
					{
						// disable death prevention
						if (Health == 0)
						{
							Die(true);
						}
					}
				}
			}
		}

		/// <summary>
		/// Different from <see cref="WCell.RealmServer.Entities.Unit.Kill()"/> which actively kills the Unit.
		/// Is called when this Unit dies, i.e. Health gets smaller than 1.
		/// </summary>
		protected void Die(bool force)
		{
			if (force || !IsAlive)
			{
				return;
			}

			// we just died
			if (!OnBeforeDeath())
			{
				return;
			}

			// set health to 0
			SetUInt32(UnitFields.HEALTH, 0);

			// send quick update
			MiscHandler.SendHealthUpdate(this, Health);

			// mark this Unit to resend DynamicFlags
			MarkUpdate(UnitFields.DYNAMIC_FLAGS);

			Dismount();

			var cast = m_spellCast;
			if (cast != null)
			{
				var spell = cast.Spell;
				if (spell != null)
				{
					m_spellCast.Cancel(SpellFailedReason.Ok);
				}
			}

			m_auras.RemoveWhere(aura => !aura.Spell.PersistsThroughDeath);

			this.UpdatePowerRegen();

			Power = 0;	// no more power

			IsInCombat = false;

			CancelTaxiFlight();

			if (m_brain != null)
			{
				m_brain.OnDeath();
			}

			OnDeath();

			// exit combat mode etc
			Target = null;
		}

		protected abstract bool OnBeforeDeath();

		protected abstract void OnDeath();

		/// <summary>
		/// Resurrects this Unit if dead
		/// </summary>
		public void Resurrect()
		{
			if (!IsAlive)
			{
				// Values according to http://www.wowwiki.com/Death
				// 50% health, 50% mana, 0 rage, 0 energy
				Health = MaxHealth / 2;

				if (PowerType == PowerType.Mana)
				{
					Power = MaxPower / 2;
				}
				else if (PowerType == PowerType.Rage)
				{
					Power = 0;
				}
				else if (PowerType == PowerType.Energy)
				{
					Power = MaxPower;
				}
			}
		}

		/// <summary>
		/// Called automatically when Unit re-gains Health.
		/// </summary>
		internal protected virtual void OnResurrect()
		{
			MiscHandler.SendHealthUpdate(this, Health);

			MarkUpdate(UnitFields.DYNAMIC_FLAGS);
		}

		/// <summary>
		/// NPCs cant spawn corpses!
		/// 
		/// Spawns this Unit's corpse at the current location
		/// </summary>
		//public virtual Corpse SpawnCorpse(bool bones, bool lootable)
		//{
		//    return new Corpse(CasterInfo, m_map, m_position, m_orientation, DisplayId, 0, 0, 0, 0, 0, 0, Gender, Race,
		//        bones ? CorpseFlags.Bones : CorpseFlags.None,
		//        lootable ? CorpseDynamicFlags.PlayerLootable : CorpseDynamicFlags.None);
		//}
		#endregion

		#region Mounting
		/// <summary>
		/// whether this Unit is sitting on a ride
		/// </summary>
		public bool IsMounted
		{
			get
			{
				return m_auras != null && m_auras.MountAura != null;
			}
		}


		public void Mount(MountId mountEntry)
		{
			NPCEntry mount;
			if (!NPCMgr.Mounts.TryGetValue(mountEntry, out mount))
			{
				log.Warn("Invalid Mount Entry-Id {0} ({1})", mountEntry, (int)mountEntry);
				return;
			}
			Mount(mount.DisplayIds.GetRandom());
		}

		public void Mount(NPCId mountId)
		{
			var mount = NPCMgr.GetEntry(mountId);
			if (mount != null)
			{
				Mount(mount.DisplayIds.GetRandom());
			}
		}

		/// <summary>
		/// Mounts the given displayId
		/// </summary>
		public virtual void Mount(uint displayId)
		{
			Dismount();
			SetUInt32(UnitFields.MOUNTDISPLAYID, displayId);
			IncMechanicCount(SpellMechanic.Mounted);

			//var evt = Mounted;
			//if (evt != null)
			//{
			//    evt(this);
			//}
		}

		/// <summary>
		/// Takes the mount off this Unit's butt (if mounted)
		/// </summary>
		public void Dismount()
		{
			if (IsUnderInfluenceOf(SpellMechanic.Mounted))
			{
				if (m_auras.MountAura != null)
				{
					m_auras.MountAura.Remove(false);
				}
				else
				{
					DoDismount();
				}
			}
		}

		/// <summary>
		/// Is called internally.
		/// <see cref="Dismount"/> 
		/// </summary>
		internal protected virtual void DoDismount()
		{
			m_auras.MountAura = null;
			SetUInt32(UnitFields.MOUNTDISPLAYID, 0);
			DecMechanicCount(SpellMechanic.Mounted);

			//var evt = Unmounted;
			//if (evt != null)
			//{
			//    evt(this);
			//}
		}
		#endregion

		#region Regeneration
		/// <summary>
		/// whether the Unit is allowed to regenerate at all.
		/// </summary>
		public bool Regenerates
		{
			get { return m_regenerates; }
			set
			{
				if (value != m_regenerates)
				{
					if (m_regenerates = value)
					{
						this.UpdatePowerRegen();
						UnitFlags2 |= UnitFlags2.RegeneratePower;
					}
					else
					{
						UnitFlags2 ^= UnitFlags2.RegeneratePower;
					}
				}
			}
		}

		public virtual bool IsRegenerating
		{
			get { return m_regenerates && IsAlive; }
		}

		/// <summary>
		/// Mana regen is in the "interrupted" state for Spell-Casters 5 seconds after a SpellCast and during SpellChanneling.
		/// See http://www.wowwiki.com/Mana_regeneration#Five-Second_Rule
		/// </summary>
		public bool IsManaRegenInterrupted
		{
			get
			{
				return PowerType == PowerType.Mana && m_spellCast != null &&
					((Environment.TickCount - m_spellCast.StartTime) < RegenerationFormulas.PowerRegenInterruptedCooldown || m_spellCast.IsChanneling);
			}
		}

		/// <summary>
		/// The real amount of Power that is added per regen-tick
		/// </summary>
		public int PowerRegenPerTickActual
		{
			get;
			internal set;
		}
		private int m_PowerRegenPerTick;

		/// <summary>
		/// The amount of Power to add per regen-tick (while not being "interrupted").
		/// Value is automatically set, depending on stats
		/// </summary>
		public int PowerRegenPerTick
		{
			get { return m_PowerRegenPerTick; }
			internal set
			{
				if (m_PowerRegenPerTick != value)
				{
					m_PowerRegenPerTick = value;
					SetFloat(UnitFields.POWER_REGEN_FLAT_MODIFIER + (int)PowerType, value);
				}
			}
		}

		public float PowerRegenPerMillis
		{
			get { return m_PowerRegenPerTick / (float)RegenerationFormulas.RegenTickDelayMillis; }
		}

		private int _manaRegenPerTickInterrupted;

		/// <summary>
		/// The amount of power to be generated during combat per regen tick (while being "interrupted")
		/// Only used for PowerType.Mana units
		/// </summary>
		public int ManaRegenPerTickInterrupted
		{
			get { return _manaRegenPerTickInterrupted; }
			internal set
			{
				if (_manaRegenPerTickInterrupted != value)
				{
					_manaRegenPerTickInterrupted = value;
					SetFloat(UnitFields.POWER_REGEN_INTERRUPTED_FLAT_MODIFIER + (int)PowerType, value);
				}
			}
		}

		/// <summary>
		/// The amount of Health to add per regen-tick while not in combat
		/// </summary>
		public int HealthRegenPerTickNoCombat
		{
			get;
			internal set;
		}

		/// <summary>
		/// The amount of Health to add per regen-tick during combat
		/// </summary>
		public int HealthRegenPerTickCombat
		{
			get;
			internal set;
		}

		/// <summary>
		/// Is called on Regeneration ticks
		/// </summary>
		protected void Regenerate(int dt)
		{
			if (!IsRegenerating)
			{
				return;
			}

			// regen Health
			var oldHealth = Health;

			int healthRegen;
			if (m_isInCombat)
			{
				healthRegen = HealthRegenPerTickCombat;
			}
			else
			{
				healthRegen = HealthRegenPerTickNoCombat;
			}

			if (healthRegen != 0)
			{
				Health = oldHealth + healthRegen;
			}

			// regen Power
			//var power = PowerRegenPerTick;

			//if (IsManaRegenInterrupted)
			//{
			//    power = MathUtil.Divide(power * ManaRegenPerTickInterrupted, 100);
			//}

			// TODO: Find out when client is in interrupted mode


			// Power is interpolated automagically
			this.UpdatePowerRegen();
			UpdatePower(dt);
		}

		public virtual int GetBasePowerRegen()
		{
			return 0;
		}
		#endregion

		#region Powers and Power costs
		/// <summary>
		/// Returns the modified power-cost needed to cast a Spell of the given DamageSchool 
		/// and the given base amount of power required
		/// </summary>
		public virtual int GetPowerCost(DamageSchool school, Spell spell, int cost)
		{
			int modifier = PowerCostModifier;
			if (m_schoolPowerCostMods != null)
			{
				modifier += m_schoolPowerCostMods[(int)school];
			}

			cost += modifier;

			cost = (int)(Math.Round(PowerCostMultiplier) * cost);
			return cost;
		}

		/// <summary>
		/// Modifies the power-cost for the given DamageSchool by value
		/// </summary>
		public void ModPowerCost(DamageSchool type, int value)
		{
			if (m_schoolPowerCostMods == null)
			{
				m_schoolPowerCostMods = new int[DamageSchoolCount];
			}
			m_schoolPowerCostMods[(int)type] += value;
		}

		/// <summary>
		/// Modifies the power-cost for all of the given DamageSchools by value
		/// </summary>
		public void ModPowerCost(uint[] schools, int value)
		{
			if (m_schoolPowerCostMods == null)
			{
				m_schoolPowerCostMods = new int[DamageSchoolCount];
			}

			foreach (var school in schools)
			{
				m_schoolPowerCostMods[school] += value;
			}
		}

		/// <summary>
		/// Modifies the power-cost for the given DamageSchool by value
		/// </summary>
		public void ModPowerCostPct(DamageSchool type, int value)
		{
			if (m_schoolPowerCostMods == null)
			{
				m_schoolPowerCostMods = new int[DamageSchoolCount];
			}
			m_schoolPowerCostMods[(int)type] += value;
		}

		/// <summary>
		/// Modifies the power-cost for all of the given DamageSchools by value
		/// </summary>
		public void ModPowerCostPct(uint[] schools, int value)
		{
			if (m_schoolPowerCostMods == null)
			{
				m_schoolPowerCostMods = new int[DamageSchoolCount];
			}

			foreach (var school in schools)
			{
				m_schoolPowerCostMods[school] += value;
			}
		}

		/// <summary>
		/// Tries to consume the given amount of Power, also considers modifiers to Power-cost.
		/// </summary>
		public bool ConsumePower(DamageSchool type, Spell spell, int neededPower)
		{
			neededPower = GetPowerCost(type, spell, neededPower);

			if (Power >= neededPower)
			{
				Power -= neededPower;
				return true;
			}

			return false;
		}
		#endregion

		#region Healing & Leeching & Burning
		public int GetHealthPercent(int value)
		{
			return (value * MaxHealth + 50) / 100;
		}

		/// <summary>
		/// Heals this unit and sends the corresponding animation (healer might be null)
		/// </summary>
		/// <param name="effect">The effect of the spell that triggered the healing (or null)</param>
		/// <param name="healer">The object that heals this Unit (or null)</param>
		/// <param name="value">The amount to be healed</param>
		public void HealPercent(int value, Unit healer = null, SpellEffect effect = null)
		{
			Heal((value * MaxHealth + 50) / 100, healer, effect);
		}

		/// <summary>
		/// Heals this unit and sends the corresponding animation (healer might be null)
		/// </summary>
		/// <param name="value">The amount to be healed</param>
		/// <param name="healer">The object that heals this Unit (or null)</param>
		/// <param name="effect">The effect of the spell that triggered the healing (or null)</param>
		public void Heal(int value, Unit healer = null, SpellEffect effect = null)
		{
			var crit = false;
			int overheal = 0;

			if (effect != null)
			{
				var oldVal = value;

				if (healer != null)
				{
					if (effect.IsPeriodic)
					{
						// add periodic boni
						value = healer.Auras.GetModifiedInt(SpellModifierType.PeriodicEffectValue, effect.Spell, value);
					}
					else
					{
						// add healing mods (spell power for healing)
						value = healer.AddHealingModsToAction(value, effect, effect.Spell.Schools[0]);
					}
				}

				if (this is Character)
				{
					value += (int)((oldVal * ((Character)this).HealingTakenModPct) / 100);
				}

				var critChance = GetCritChance(effect.Spell.Schools[0]) * 100;

				// do a critcheck
				if (!effect.Spell.AttributesExB.HasFlag(SpellAttributesExB.CannotCrit) && critChance != 0)
				{
					var roll = Utility.Random(1f, 10001);

					if (roll <= critChance)
					{
						value = (int)(value * (SpellHandler.SpellCritBaseFactor + GetIntMod(StatModifierInt.CriticalHealValuePct)));
						crit = true;
					}
				}
			}

			if (value > 0)
			{
				value = (int)(value * Utility.Random(0.95f, 1.05f));
				if (Health + value > MaxHealth)
				{
					overheal = (Health + value) - MaxHealth;
					value = (MaxHealth - Health);
				}
				Health += value;
				value += overheal;
				CombatLogHandler.SendHealLog(healer ?? this, this, effect != null ? effect.Spell.Id : 0, value, crit, overheal);
			}

			if (healer != null)
			{
				var action = new HealAction
				{
					Attacker = healer,
					Victim = this,
					Spell = effect != null ? effect.Spell : null,
					IsCritical = crit,
					IsHot = effect.IsPeriodic,
					Value = value,
				};

				OnHeal(action);
			}
		}

		#region OnHeal
		/// <summary>
		/// This method is called whenever a heal is placed on a Unit by another Unit
		/// </summary>
		/// <param name="healer">The healer</param>
		/// <param name="value">The amount of points healed</param>
		protected virtual void OnHeal(HealAction action)
		{
			if (action.Value > 0)
			{
				TriggerProcOnHeal(action);
			}

			// TODO: Remove method and instead trigger map-wide event (a lot more efficient than this)
			this.IterateEnvironment(40.0f, obj =>
			{
				if (obj is Unit && ((Unit)obj).m_brain != null)
				{
					((Unit)obj).m_brain.OnHeal(action.Attacker, this, action.Value);
				}
				return true;
			});
		}

		private void TriggerProcOnHeal(HealAction action)
		{
			if (action.IsHot)
			{
				ProcHitFlags hitFlags = action.IsCritical ? ProcHitFlags.CriticalHit : ProcHitFlags.NormalHit;
				action.Attacker.Proc(ProcTriggerFlags.DonePeriodicDamageOrHeal, this, action, true, hitFlags);
				Proc(ProcTriggerFlags.ReceivedPeriodicDamageOrHeal, action.Attacker, action, true, hitFlags);
			}
		}
		#endregion

		/// <summary>
		/// Leeches the given amount of health from this Unit and adds it to the receiver (if receiver != null and is Unit).
		/// </summary>
		/// <param name="factor">The factor applied to the amount that was leeched before adding it to the receiver</param>
		public void LeechHealth(Unit receiver, int amount, float factor, SpellEffect effect)
		{
			var initialHealth = Health;

			DealSpellDamage(receiver != null ? receiver.Master : this, effect, amount);

			// only apply as much as was leeched
			amount = initialHealth - Health;

			if (factor > 0)
			{
				amount = (int)(amount * factor);
			}

			if (receiver != null)
			{
				receiver.Heal(amount, this, effect);
			}
		}

		/// <summary>
		/// Restores Power and sends the corresponding Packet
		/// </summary>
		public void EnergizePercent(int value, Unit energizer = null, SpellEffect effect = null)
		{
			Energize((value * MaxPower + 50) / 100, energizer, effect);
		}

		/// <summary>
		/// Restores Power and sends the corresponding Packet
		/// </summary>
		public void Energize(int value, Unit energizer = null, SpellEffect effect = null)
		{
			if (value != 0)
			{
				var power = Power;

				value = MathUtil.ClampMinMax(value, -power, MaxPower - value);

				CombatLogHandler.SendEnergizeLog(energizer, this, effect != null ? effect.Spell.Id : 0, PowerType, value);
				Power = power + value;
			}
		}

		/// <summary>
		/// Leeches the given amount of power from this Unit and adds it to the receiver (if receiver != null and is Unit).
		/// </summary>
		public void LeechPower(int amount, float factor = 1, Unit receiver = null, SpellEffect effect = null)
		{
			var currentPower = Power;

			// Resilience reduces mana drain by 2.2%, amount is rounded.
			amount -= MathUtil.RoundInt(amount * GetResiliencePct() * 2.2f);
			if (amount > currentPower)
			{
				amount = currentPower;
			}
			Power = currentPower - amount;
			if (receiver != null)
			{
				receiver.Energize(amount, this, effect);
			}
		}

		/// <summary>
		/// Drains the given amount of power and applies damage for it
		/// </summary>
		/// <param name="dmgTyp">The type of the damage applied</param>
		/// <param name="dmgFactor">The factor to be applied to amount for the damage to be received by this unit</param>
		public void BurnPower(int amount, float dmgFactor = 1, Unit attacker = null, SpellEffect effect = null)
		{
			int currentPower = Power;

			// Resilience reduces mana drain by 2.2%, amount is rounded.
			amount -= MathUtil.RoundInt(amount * GetResiliencePct() * 2.2f);
			if (amount > currentPower)
			{
				amount = currentPower;
			}
			Power = currentPower - amount;

			DealSpellDamage(attacker, effect, (int)(amount * dmgFactor));
		}
		#endregion

		#region Movement Handling
		internal protected override void OnEnterMap()
		{
			m_lastMoveTime = Environment.TickCount;

			if (Flying > 0)
			{
				// resend flying info, if still flying
				MovementHandler.SendFlyModeStart(this);
			}
		}

		/// <summary>
		/// Is called whenever a Unit moves
		/// </summary>
		public virtual void OnMove()
		{
			var cast = m_spellCast;
			if (cast != null)
			{
				var spell = cast.Spell;
				if (spell != null)
				{
					//if ((spell.InterruptFlags & InterruptFlag.OnMovement) != 0) {
					if (!cast.GodMode && (spell.CastDelay > 0 || spell.IsChanneled))
					{
						cast.Cancel();
					}
				}
			}

			m_auras.RemoveByFlag(AuraInterruptFlags.OnMovement);

			m_lastMoveTime = Environment.TickCount;
		}

		/// <summary>
		/// whether this Unit is currently moving
		/// </summary>
		public virtual bool IsMoving
		{
			get
			{
				return (Environment.TickCount - m_lastMoveTime) < MinStandStillDelay;
			}
		}

		/// <summary>
		/// Makes this Unit move their face towards the given object
		/// </summary>
		public void Face(WorldObject obj)
		{
			if (obj != m_target || IsPlayerControlled)
			{
				Face(m_orientation);
			}
			else
			{
				// client displays NPCs facing Target anyway
				m_orientation = GetAngleTowards(obj);
			}
		}

		/// <summary>
		/// Makes this Unit look at the given location
		/// </summary>
		public void Face(Vector3 pos)
		{
			Face(GetAngleTowards(pos));
		}

		/// <summary>
		/// Makes this Unit move their face towards the given orientation
		/// </summary>
		public void Face(float orientation)
		{
			// orientation = (orientation + Utility.PI) % (2 * Utility.PI);

			m_orientation = orientation;
			MovementHandler.SendFacingPacket(this, orientation, (uint)(314 / TurnSpeed));
		}

		#endregion

		#region Visibility
		/// <summary>
		/// Checks whether this Unit can currently see the given obj
		/// 
		/// TODO: Higher staff ranks can always see lower staff ranks (too bad there are no ranks)
		/// TODO: Line of Sight
		/// </summary>
		public override bool CanSee(WorldObject obj)
		{
			if (!base.CanSee(obj) || !obj.IsInWorld)
			{
				return false;
			}

			if (this == obj)			// one can always see oneself
			{
				return true;
			}

			// check for visibility overrides
			var visibility = obj.DetermineVisibilityFor(this);
			if (visibility == VisibilityStatus.Invisible)
			{
				return false;
			}
			if (visibility == VisibilityStatus.Visible)
			{
				return true;
			}

			if (this is Character && ((Character)this).Role.IsStaff && (!(obj is Character) || ((Character)obj).Role < ((Character)this).Role))
			{
				// GMs see everything (just don't display the Spirit Healer to the living - because it is too confusing)
				return !(obj is Unit) || !((Unit)obj).IsSpiritHealer || !IsAlive;
			}

			if (!(obj is Unit))
			{
				// Object
				return true;
			}

			// Unit
			var unit = (Unit)obj;
			if (IsGhost)
			{
				// dead can only see the dead and those near their corpse!
				if (this is Character)
				{
					var corpse = ((Character)this).Corpse;
					if (corpse != null)
					{
						return unit.IsInRadiusSq(corpse, Corpse.GhostVisibilityRadiusSq);
					}
				}
				return false;
			}

			if (obj is Character)
			{
				var chr = (Character)obj;
				// GMs cannot be seen, except by their superiors
				if (chr.Role.IsStaff && chr.Stealthed > 0 &&
					(!(this is Character) || ((Character)this).Role < chr.Role))
				{
					return false;
				}

				// same Group?
				if ((this is Character) && chr.GroupMember != null && ((Character)this).Group == chr.Group)
				{
					// Group members can always see each other (living or dead)
					return true;
				}
			}

			if (unit.IsSpiritHealer || unit.IsGhost)
			{
				return IsGhost;
			}

			return HandleStealthDetection(unit);

		}
		public bool HandleStealthDetection(Unit unit)
		{
			var val = unit.Stealthed;
			if (val > 0)
			{
				// stealthed Unit
				// TODO: Calc detection, based on Stealth value, bonuses, detection, detection bonuses, distance and viewing angle

				//if we are stunned we don't see anything
				if ((this.UnitFlags & UnitFlags.Stunned) != 0)
					return false;

				//he's so close we can allready touch him
				if (GetDistance(unit.Position) <= 0.24f)
					return true;

				//he's behind us, we can't see him
				if (!unit.IsInFrontOf(this))
					return false;

				bool HasStealthDetection = false;

				//check if we have a stealth detection aura
				if (Auras.GetTotalAuraModifier(AuraType.Aura_228) > 0)
					HasStealthDetection = true;

				float visibleDistance;
				//we have no stealth detection so check how stealthy he is
				if (!HasStealthDetection)
				{
					visibleDistance = 10.5f - (((float)unit.Stealthed) / 100.0f);
					// -1.0f per level diff
					visibleDistance += Level - unit.Level;

					//ModStealthLevel -> auras that increase his stealth
					var stealthMod = Auras.GetTotalAuraModifier(AuraType.ModStealthLevel);
					if (stealthMod < 0)
						stealthMod = 0;
					//ModDetect -> auras that we have that decrease his stealth
					//every 5 mod he can get closer by 1.0f 
					var DetectMod = Auras.GetTotalAuraModifier(AuraType.ModDetect, 0);
					visibleDistance -= ((DetectMod - stealthMod) / 5.0f);
					visibleDistance = visibleDistance > 45.0f ? 45.0f : visibleDistance;

					//we see you!
					if (GetDistance(unit.Position) <= visibleDistance)
						return true;
				}
				else
					return true;

				return false;
			}
			return true;
		}
		#endregion

		#region Misc
		/// <summary>
		/// The spoken language of this Unit.
		/// If Character's have a SpokenLanguage, they cannot use any other.
		/// Default: <c>ChatLanguage.Universal</c>
		/// </summary>
		public ChatLanguage SpokenLanguage
		{
			get;
			set;
		}

		/// <summary>
		/// Cancels whatever this Unit is currently doing.
		/// </summary>
		public virtual void CancelAllActions()
		{
			if (m_spellCast != null)
			{
				m_spellCast.Cancel();
			}
			Target = null;
		}

		public virtual void CancelSpellCast()
		{
			if (m_spellCast != null)
			{
				m_spellCast.Cancel();
			}
		}

		public virtual void CancelEmote()
		{
			EmoteState = EmoteType.None;
		}

		/// <summary>
		/// Makes this Unit show an animation
		/// </summary>
		public void Emote(EmoteType emote)
		{
			EmoteHandler.SendEmote(this, emote);
		}

		/// <summary>
		/// Makes this Unit do a text emote
		/// </summary>
		/// <param name="emote">Anything that has a name (to do something with) or null</param>
		public void TextEmote(TextEmote emote, INamed target)
		{
			if (this is Character)
			{
				((Character)this).Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DoEmote, (uint)emote);
			}
			EmoteHandler.SendTextEmote(this, emote, target);
		}


		/// <summary>
		/// When pinned down, a Character cannot be 
		/// logged out, moved or harmed.
		/// </summary>
		public bool IsPinnedDown
		{
			get { return m_IsPinnedDown; }
			set
			{
				if (!IsInWorld)
				{
					LogUtil.ErrorException(new InvalidOperationException("Character was already disposed when pinning down: " + this), true);
					return;
				}
				m_Map.EnsureContext();

				if (m_IsPinnedDown != value)
				{
					m_IsPinnedDown = value;
					if (m_IsPinnedDown)
					{
						// invul and stun
						IsEvading = true;
						Stunned++;
					}
					else
					{
						if (this is Character && ((Character)this).Client.IsOffline)
						{
							// Client already gone
							((Character)this).Logout(true, 0);
						}
						else
						{
							IsEvading = false;
							Stunned--;
						}
					}
				}
			}
		}

		public bool IsStunned
		{
			get { return UnitFlags.HasFlag(UnitFlags.Stunned); }
		}
		#endregion

		#region Taxi
		internal void OnTaxiStart()
		{
			UnitFlags |= UnitFlags.Influenced;
			IsOnTaxi = true;

			//taxi interpolation timer
			taxiTime = 0;
			m_TaxiMovementTimer = new TimerEntry(0, TaxiMgr.InterpolationDelayMillis, TaxiTimerCallback);
			m_TaxiMovementTimer.Start();
			IsEvading = true;
		}

		internal void OnTaxiStop()
		{
			TaxiPaths.Clear();
			LatestTaxiPathNode = null;
			//Dismount();
			DoDismount();
			IsOnTaxi = false;
			UnitFlags &= ~UnitFlags.Influenced;
			m_TaxiMovementTimer.Stop();
			IsEvading = false;
		}

		/// <summary>
		/// Time spent on the current taxi-ride in millis.
		/// </summary>
		public int TaxiTime
		{
			get { return taxiTime; }
		}

		protected virtual void TaxiTimerCallback(int elapsedTime)
		{
			// if (!IsOnTaxi) return;
			// if (TaxiPaths.Count < 1) return;
			TaxiMgr.InterpolatePosition(this, elapsedTime);
		}

		/// <summary>
		/// A list of the TaxiPaths the unit is currently travelling to. The TaxiPath currently being travelled is first.
		/// </summary>
		protected Queue<TaxiPath> m_TaxiPaths = new Queue<TaxiPath>(5);

		private LinkedListNode<PathVertex> m_LatestTaxiPathNode;

		/// <summary>
		/// Returns the players currently planned taxi paths.
		/// </summary>
		public Queue<TaxiPath> TaxiPaths
		{
			get { return m_TaxiPaths; }
		}

		/// <summary>
		/// The point on the currently travelled TaxiPath that the Unit past most recently, or null if not on a taxi.
		/// </summary>
		public LinkedListNode<PathVertex> LatestTaxiPathNode
		{
			get { return m_LatestTaxiPathNode; }
			internal set { m_LatestTaxiPathNode = value; }
		}

		/// <summary>
		/// Whether or not this unit is currently flying on a taxi.
		/// </summary>
		public bool IsOnTaxi
		{
			get { return UnitFlags.HasFlag(UnitFlags.TaxiFlight); }
			set
			{
				if (value != IsOnTaxi)
				{
					if (value)
					{
						UnitFlags |= UnitFlags.TaxiFlight;
					}
					else
					{
						UnitFlags &= ~UnitFlags.TaxiFlight;
					}
				}
			}
		}

		/// <summary>
		/// Whether or not this Unit is currently under the influence of an effect that won't allow it to be controled by itself or its master
		/// </summary>
		public bool IsInfluenced
		{
			get { return UnitFlags.HasFlag(UnitFlags.Influenced); }
			set
			{
				if (value)
					UnitFlags |= UnitFlags.Influenced;
				else
					UnitFlags &= ~UnitFlags.Influenced;
			}
		}

		public void CancelTaxiFlight()
		{
			if (IsOnTaxi)
			{
				MovementHandler.SendStopMovementPacket(this);
				OnTaxiStop();
			}
		}

		/// <summary>
		/// Cancel any enforced movement
		/// </summary>
		public void CancelMovement()
		{
			CancelTaxiFlight();
			if (m_Movement != null)
			{
				m_Movement.Stop();
			}
		}
		#endregion

		#region Spells
		public bool HasSpells
		{
			get { return m_spells != null; }
		}

		/// <summary>
		/// All spells known to this unit.
		/// Could be null for NPCs that are not spell-casters (check with <see cref="HasSpells"/>).
		/// Use <see cref="NPC.NPCSpells"/> to enforce a SpellCollection.
		/// </summary>
		public virtual SpellCollection Spells
		{
			get { return m_spells; }
		}

		public bool HasEnoughPowerToCast(Spell spell, WorldObject selected)
		{
			if (!spell.CostsPower)
			{
				return true;
			}
			if (selected is Unit)
			{
				return Power >= spell.CalcPowerCost(this, ((Unit)selected).GetLeastResistantSchool(spell));
			}
			return Power >= spell.CalcPowerCost(this, spell.Schools[0]);
		}

		public DamageSchool GetLeastResistantSchool(Spell spell)
		{
			if (spell.Schools.Length == 1)
			{
				return spell.Schools[0];
			}

			var least = int.MaxValue;
			var leastSchool = DamageSchool.Physical;
			foreach (var school in spell.Schools)
			{
				var res = GetResistance(school);
				if (res < least)
				{
					least = res;
					leastSchool = school;
				}
			}
			return leastSchool;
		}
		#endregion

		#region Minions
		public virtual bool MaySpawnPet(NPCEntry entry)
		{
			return true;
		}

		/// <summary>
		/// Tries to spawn the given pet for this Unit.
		/// </summary>
		/// <returns>null, if the Character already has that kind of Pet.</returns>
		public NPC SpawnMinion(NPCId id)
		{
			return SpawnMinion(id, 0);
		}

		/// <summary>
		/// Tries to spawn the given pet for this Unit.
		/// </summary>
		/// <returns>null, if the Character already has that kind of Pet.</returns>
		public NPC SpawnMinion(NPCId id, int durationMillis)
		{
			var entry = NPCMgr.GetEntry(id);
			if (entry != null)
			{
				return SpawnMinion(entry, ref m_position, durationMillis);
			}
			return null;
		}

		/// <summary>
		/// Creates and makes visible the Unit's controlled Minion
		/// </summary>
		/// <param name="entry">The template for the Minion</param>
		/// <param name="position">The place to spawn the minion.</param>
		/// <param name="duration">Time till the minion goes away.</param>
		/// <returns>A reference to the minion.</returns>
		public NPC CreateMinion(NPCEntry entry, int durationMillis)
		{
			var minion = entry.Create();
			minion.Phase = Phase;
			minion.Zone = Zone;
			minion.RemainingDecayDelayMillis = durationMillis;
			minion.Brain.IsRunning = true;

			if (Health > 0)
			{
				Enslave(minion, durationMillis);
			}
			return minion;
		}

		/// <summary>
		/// Creates and makes visible the Unit's controlled Minion
		/// </summary>
		/// <param name="entry">The template for the Minion</param>
		/// <param name="position">The place to spawn the minion.</param>
		/// <param name="duration">Time till the minion goes away.</param>
		/// <returns>A reference to the minion.</returns>
		public virtual NPC SpawnMinion(NPCEntry entry, ref Vector3 position, int durationMillis)
		{
			//return SpawnMinion(entry, summonSpell, ref position, durationMillis != 0 ? DateTime.Now.AddMilliseconds(durationMillis) : (DateTime?)null);
			var minion = CreateMinion(entry, durationMillis);
			minion.Position = position;
			m_Map.AddObjectLater(minion);
			return minion;
		}

		public void Enslave(NPC minion)
		{
			Enslave(minion, 0);
		}

		public void Enslave(NPC minion, int durationMillis)
		{
			//Enslave(minion, durationMillis != 0 ? DateTime.Now.AddMilliseconds(durationMillis) : (DateTime?)null);

			minion.Phase = Phase;
			minion.Master = this;

			var type = minion.Entry.Type;
			if (type != CreatureType.None && type != CreatureType.NotSpecified)
			{
				if (type == CreatureType.NonCombatPet)
				{
					minion.Brain.DefaultState = BrainState.Follow;
				}
				else if (type == CreatureType.Totem)
				{
					// can't move
					minion.Brain.DefaultState = BrainState.Roam;
				}
				else
				{
					minion.Brain.DefaultState = BrainState.Guard;
				}

				minion.Brain.EnterDefaultState();
			}

			if (durationMillis != 0)
			{
				// ReSharper disable PossibleLossOfFraction
				minion.RemainingDecayDelayMillis = durationMillis;
				// ReSharper restore PossibleLossOfFraction
			}
		}

		internal protected virtual void OnMinionDied(NPC minion)
		{
		}

		internal protected virtual void OnMinionEnteredMap(NPC minion)
		{
		}

		internal protected virtual void OnMinionLeftMap(NPC minion)
		{
		}
		#endregion

		#region Procs
		internal protected List<IProcHandler> m_procHandlers;

		/// <summary>
		/// Can be null if no handlers have been added.
		/// </summary>
		public List<IProcHandler> ProcHandlers
		{
			get { return m_procHandlers; }
		}

		public IProcHandler GetProcHandler(Func<IProcHandler, bool> predicate)
		{
			if (m_procHandlers != null)
			{
				foreach (var handler in m_procHandlers)
				{
					if (predicate(handler))
					{
						return handler;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns the first proc handler of the given type
		/// </summary>
		public T GetProcHandler<T>() where T : IProcHandler
		{
			if (m_procHandlers != null)
			{
				foreach (var handler in m_procHandlers)
				{
					if (handler is T)
					{
						return (T)handler;
					}
				}
			}
			return default(T);
		}

		public void AddProcHandler(ProcHandlerTemplate templ)
		{
			AddProcHandler(new ProcHandler(this, this, templ));
		}

		public void AddProcHandler(IProcHandler handler)
		{
			if (m_procHandlers == null)
			{
				m_procHandlers = new List<IProcHandler>(5);
			}
			m_procHandlers.Add(handler);
		}

		public void RemoveProcHandler(IProcHandler handler)
		{
			if (m_procHandlers != null)
			{
				m_procHandlers.Remove(handler);
			}
		}

		/// <summary>
		/// Remnoves the first proc that triggers the given spell
		/// </summary>
		public void RemoveProcHandler(SpellId procId)
		{
			if (m_procHandlers != null)
			{
				foreach (var handler in m_procHandlers)
				{
					if (handler.ProcSpell != null && handler.ProcSpell.SpellId == procId)
					{
						m_procHandlers.Remove(handler);
						break;
					}
				}
			}
		}

		public void RemoveProcHandler(Func<IProcHandler, bool> predicate)
		{
			var handler = GetProcHandler(predicate);
			if (handler != null)
			{
				m_procHandlers.Remove(handler);
			}
		}

		public void RemoveProcHandler<T>() where T : IProcHandler
		{
			var handler = GetProcHandler<T>();
			if (handler != null)
			{
				m_procHandlers.Remove(handler);
			}
		}

		/// <summary>
		/// Removes the first custom ProcHandler that uses the given template.
		/// </summary>
		public void RemoveProcHandler(ProcHandlerTemplate template)
		{
			if (m_procHandlers != null)
			{
				foreach (var handler in m_procHandlers)
				{
					if (handler is ProcHandler && ((ProcHandler)handler).Template == template)
					{
						m_procHandlers.Remove(handler);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Trigger all procs that can be triggered by the given action
		/// </summary>
		/// <param name="active">Whether the triggerer is the attacker/caster (true), or the victim (false)</param>
		public void Proc(ProcTriggerFlags flags, Unit triggerer, IUnitAction action, bool active, ProcHitFlags hitFlags = ProcHitFlags.None)
		{
			if (m_brain != null && m_brain.CurrentAction != null && m_brain.CurrentAction.InterruptFlags.HasAnyFlag(flags))
			{
				// check if the current action has been interrupted
				m_brain.StopCurrentAction();
			}

			if (m_procHandlers == null)
			{
				return;
			}

			if (flags == ProcTriggerFlags.None)
			{
				return;
			}

			if (triggerer == null)
			{
				log.Error("triggerer was null when triggering Proc by action: {0} (Flags: {1})", action, flags);
				return;
			}

			var now = DateTime.Now;
			for (var i = m_procHandlers.Count - 1; i >= 0; i--)	// need to reverse iteration because procs can be removed in the process
			{
				if (i >= m_procHandlers.Count)
				{
					// In case that the list was changed during iteration
					continue;
				}

				var proc = m_procHandlers[i];
				bool procFlagsMatch = proc.ProcTriggerFlags.HasAnyFlag(flags);
				bool procHitFlagsMatch = flags.RequireHitFlags() ? proc.ProcHitFlags.HasAnyFlag(hitFlags) : true;

				if (proc.NextProcTime <= now &&
					procFlagsMatch &&
					procHitFlagsMatch &&
					proc.CanBeTriggeredBy(triggerer, action, active))
				{
					var chance = (int)proc.ProcChance;
					if (chance > 0 && action.Spell != null)
					{
						chance = Auras.GetModifiedInt(SpellModifierType.ProcChance, action.Spell, chance);
					}

					if (proc.ProcChance <= 0 || Utility.Random(0, 101) <= chance)
					{
						var charges = proc.StackCount;
						proc.TriggerProc(triggerer, action);
						if (proc.MinProcDelay > 0)
						{
							proc.NextProcTime = now.AddMilliseconds(proc.MinProcDelay);
						}

						if (charges > 0 && proc.StackCount == 0)
						{
							proc.Dispose();
							//proc.NextProcTime = DateTime.MaxValue;	// won't be proc'ed again
						}
					}
				}
			}
		}
		#endregion

		#region Gossip

		protected GossipMenu m_gossipMenu;

		/// <summary>
		/// The GossipMenu, associated with this WorldObject.
		/// </summary>
		public GossipMenu GossipMenu
		{
			get
			{
				return m_gossipMenu;
			}
			set
			{
				m_gossipMenu = value;
				if (value != null)
				{
					NPCFlags |= NPCFlags.Gossip;
				}
				else
				{
					NPCFlags &= ~NPCFlags.Gossip;
				}
			}
		}
		#endregion

		#region Affinity
		/// <summary>
		/// Is called when Master / Faction has changed and this Unit now has a different circle of friends
		/// </summary>
		protected virtual void OnAffinityChanged()
		{
			if (IsPlayerOwned)
			{
				// whenever we change affinity: Remove Auras of everyone but ourselves
				m_auras.RemoveOthersAuras();
			}
		}
		#endregion

		#region Dispose
		public override void Dispose(bool disposing)
		{
			if (m_auras == null)
			{
				// already disposed
				return;
			}

			if (m_Movement != null)
			{
				m_Movement.m_owner = null;
				m_Movement = null;
			}

			base.Dispose(disposing);

			m_attackTimer = null;
			m_target = null;

			if (m_brain != null)
			{
				m_brain.Dispose();
				m_brain = null;
			}

			m_spells.Recycle();
			m_spells = null;

			m_auras.Owner = null;
			m_auras = null;

			m_charm = null;
			m_channeled = null;
		}

		#endregion

		protected internal override void DeleteNow()
		{
			IsFighting = false;
			if (m_brain != null)
			{
				m_brain.IsRunning = false;
			}
			Target = null;
			base.DeleteNow();
		}

		protected virtual HighId HighId
		{
			get { return HighId.Unit; }
		}

		protected void GenerateId(uint entryId)
		{
			EntityId = new EntityId(NPCMgr.GenerateUniqueLowId(), entryId, HighId);
		}

	} // end class

	public delegate void AttackHandler(IDamageAction action);
}