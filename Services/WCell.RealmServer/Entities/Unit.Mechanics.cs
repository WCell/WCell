/*************************************************************************
 *
 *   file		: Unit.Movement.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-04 01:38:40 +0800 (Mon, 04 Feb 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 97 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Linq;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Util.Variables;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Unit
	{
		public static float DefaultSpeedFactor = 1f;

		public static float DefaultWalkSpeed = 2.5f;
		public static float DefaultWalkBackSpeed = 2.5f;
		public static float DefaultRunSpeed = 7f;
		public static float DefaultSwimSpeed = 4.7222f;
		public static float DefaultSwimBackSpeed = 4.5f;
		public static float DefaultFlightSpeed = 7f;
		public static float DefaultFlightBackSpeed = 4.5f;

		[NotVariable]
		public static readonly float DefaultTurnSpeed = MathUtil.PI;
		[NotVariable]
		public static readonly float DefaulPitchSpeed = MathUtil.PI;

		[Initialization(InitializationPass.Tenth)]
		public static void InitSpeeds()
		{

		}

		public static readonly int MechanicCount = (int)Convert.ChangeType(Utility.GetMaxEnum<SpellMechanic>(), typeof(int)) + 1;
		// public static readonly int DamageTypeCount = (int)Convert.ChangeType(Utility.GetMaxEnum<DamageType>(), typeof(int)) + 1;
		public static readonly int DamageSchoolCount = (int)DamageSchool.Count;
		public static readonly int DispelTypeCount = (int)Convert.ChangeType(Utility.GetMaxEnum<DispelType>(), typeof(int)) + 1;

		/// <summary>
		/// All CombatRatings
		/// </summary>
		public static readonly CombatRating[] CombatRatings = (CombatRating[])Enum.GetValues(typeof(CombatRating));

		/// <summary>
		/// Creates an array for a set of SpellMechanics
		/// </summary>
		protected static int[] CreateMechanicsArr()
		{
			return new int[MechanicCount];
		}

		protected static int[] CreateDamageSchoolArr()
		{
			return new int[DamageSchoolCount];
		}

		protected static int[] CreateDispelTypeArr()
		{
			return new int[DispelTypeCount];
		}

		#region Fields
		// mod counters
		protected uint m_flying, m_waterWalk, m_hovering, m_featherFalling;
		protected int m_stealthed;
		private int m_Pacified;

		protected int[] m_mechanics;
		protected int[] m_mechanicImmunities;
		protected int[] m_mechanicResistances;
		protected int[] m_mechanicDurationMods;
		protected int[] m_debuffResistances;
		protected int[] m_dispelImmunities;
		protected int[] m_TargetResMods;
		protected int[] m_spellInterruptProt;
		protected int[] m_threatMods;
		protected int[] m_attackerSpellHitChance;
		protected int[] m_SpellHitChance;
		protected int[] m_CritMods;
		protected int[] m_damageTakenMods;
		protected int[] m_damageTakenPctMods;

		/// <summary>
		/// Immunities against damage-schools
		/// </summary>
		protected int[] m_dmgImmunities;

		/// <summary>
		/// Whether the physical state of this Unit allows it to move
		/// </summary>
		protected bool m_canMove;

		protected bool m_canInteract, m_canHarm, m_canCastSpells, m_evades, m_canDoPhysicalActivity;

		protected float m_speedFactor, m_swimFactor, m_flightFactor, m_mountMod;

		protected float m_walkSpeed;
		protected float m_walkBackSpeed;
		protected float m_runSpeed;
		protected float m_swimSpeed;
		protected float m_swimBackSpeed;
		protected float m_flightSpeed;
		protected float m_flightBackSpeed;
		protected float m_turnSpeed;
		protected float m_pitchSpeed;

		MovementFlags m_movementFlags;
		MovementFlags2 m_movementFlags2;

		protected Movement m_Movement;
		#endregion

		#region SpellMechanic Types
		/// <summary>
		/// Whether the Unit is allowed to cast spells that are not physical abilities
		/// </summary>
		public bool CanCastSpells
		{
			get { return m_canCastSpells; }
		}

		/// <summary>
		/// Whether the Unit is allowed to attack and use physical abilities
		/// </summary>
		public bool CanDoPhysicalActivity
		{
			get { return m_canDoPhysicalActivity; }
			private set
			{
				if (m_canDoPhysicalActivity != value) return;

				m_canDoPhysicalActivity = value;
				if (value)
				{
					// disable physical abilities
					IsFighting = false;
					if (m_spellCast != null && m_spellCast.IsCasting && m_spellCast.Spell.IsPhysicalAbility)
					{
						m_spellCast.Cancel(SpellFailedReason.Pacified);
					}
				}
				else
				{
					// enable physical abilities
				}
			}
		}

		/// <summary>
		/// Whether the physical state and permissions of this Unit allows it to move.
		/// To control whether a unit is physically capable of moving, use IncMechanicCount/DecMechanicCount to change <see cref="SpellMechanic.Rooted">Rooted</cref> or any other movement-effecting Mechanic.
		/// However to control it's actual desire/permission to move, use <see cref="HasPermissionToMove"/>.
		/// </summary>
		public bool CanMove
		{
			get { return m_canMove && HasPermissionToMove; }
		}

		/// <summary>
		/// Whether the owner or controlling AI allows this unit to move.
		/// Always returns true for uncontrolled players.
		/// </summary>
		public bool HasPermissionToMove
		{
			get { return m_Movement == null || m_Movement.MayMove; }
			set
			{
				if (m_Movement != null)
				{
					m_Movement.MayMove = value;
				}
			}
		}

		/// <summary>
		/// Whether the Unit is currently evading (cannot be hit, generate threat etc)
		/// </summary>
		public bool IsEvading
		{
			get { return m_evades; }
			set
			{
				m_evades = value;
				if (value)
				{
					m_auras.RemoveOthersAuras();
					IncMechanicCount(SpellMechanic.Invulnerable);
				}
				else
				{
					DecMechanicCount(SpellMechanic.Invulnerable);
				}
			}
		}

		/// <summary>
		/// whether the Unit can be interacted with
		/// </summary>
		public bool CanInteract
		{
			get { return m_canInteract; }
		}

		/// <summary>
		/// 
		/// </summary>
		public override bool CanDoHarm
		{
			get { return m_canHarm && base.CanDoHarm; }
		}

		/// <summary>
		/// Whether this Unit is currently stunned (!= rooted)
		/// </summary>
		public int Stunned
		{
			get
			{
				if (m_mechanics == null)
				{
					return 0;
				}
				return m_mechanics[(int)SpellMechanic.Stunned];
			}
			set
			{
				if (m_mechanics == null)
				{
					m_mechanics = CreateMechanicsArr();
				}
				if (value <= 0)
				{
					m_mechanics[(int)SpellMechanic.Stunned] = 1;
					DecMechanicCount(SpellMechanic.Stunned);
				}
				else if (Stunned == 0)
				{
					IncMechanicCount(SpellMechanic.Stunned);
					m_mechanics[(int)SpellMechanic.Stunned] = value;
				}
			}
		}

		public int Invulnerable
		{
			get
			{
				if (m_mechanics == null)
				{
					return 0;
				}
				return m_mechanics[(int)SpellMechanic.Invulnerable];
			}
			set
			{
				if (m_mechanics == null)
				{
					m_mechanics = CreateMechanicsArr();
				}
				if (value <= 0)
				{
					m_mechanics[(int)SpellMechanic.Invulnerable] = 1;
					DecMechanicCount(SpellMechanic.Invulnerable);
				}
				else if (Invulnerable == 0)
				{
					IncMechanicCount(SpellMechanic.Invulnerable);
					m_mechanics[(int)SpellMechanic.Invulnerable] = value;
				}
			}
		}

		/// <summary>
		/// Pacified units cannot attack or use physical abilities
		/// </summary>
		public int Pacified
		{
			get { return m_Pacified; }
			set
			{
				if (m_Pacified == value) return;
				if (value <= 0 && m_Pacified > 0)
				{
					// disable pacify
					UnitFlags &= ~UnitFlags.Pacified;
				}
				else if (value > 0)
				{
					// enable pacify
					UnitFlags |= UnitFlags.Pacified;
				}
				m_Pacified = value;
				SetCanHarmState();
			}
		}

		/// <summary>
		/// Return whether the given Mechanic applies to the Unit
		/// </summary>
		public bool IsUnderInfluenceOf(SpellMechanic mechanic)
		{
			if (m_mechanics == null)
			{
				return false;
			}
			return m_mechanics[(int)mechanic] > 0;
		}

		/// <summary>
        /// Increase the mechanic modifier count for the given SpellMechanic
		/// </summary>
		public void IncMechanicCount(SpellMechanic mechanic, bool isCustom = false)
		{
			if (m_mechanics == null)
			{
				m_mechanics = CreateMechanicsArr();
			}

			var val = m_mechanics[(int)mechanic];
			if (val == 0)
			{
				if (isCustom == false)
				{
					// movement
					if (m_canMove && SpellConstants.MoveMechanics[(int)mechanic])
					{
						m_canMove = false;
						if (!IsPlayer)
						{
							// NPCs need to unset their target, or else the client will keep display them rotating
							Target = null;
						}
						UnitFlags |= UnitFlags.Stunned;

						// can't fly while stunned
						CancelTaxiFlight();

						if (this is Character)
						{
							MovementHandler.SendRooted((Character)this, 1);
						}
						//UnitFlags |= UnitFlags.Influenced;
						if (IsUsingSpell && SpellCast.Spell.SpellInterrupts.InterruptFlags.HasFlag(InterruptFlags.OnStunned))
						{
							SpellCast.Cancel();
						}
                        StopMoving();
					}

					// interaction
					if (m_canInteract && SpellConstants.InteractMechanics[(int)mechanic])
					{
						m_canInteract = false;
						//UnitFlags |= UnitFlags.UnInteractable;
					}

					// harmfulness
					if (m_canHarm && SpellConstants.HarmPreventionMechanics[(int)mechanic])
					{
						SetCanHarmState();
					}

					// check if we can still cast spells
					if (m_canCastSpells && SpellConstants.SpellCastPreventionMechanics[(int)mechanic])
					{
						// check if we can still cast spells
						m_canCastSpells = false;
						if (IsUsingSpell && SpellCast.Spell.SpellInterrupts.InterruptFlags.HasFlag(InterruptFlags.OnSilence))
						{
							SpellCast.Cancel();
						}
						if (!m_canDoPhysicalActivity && m_canHarm)
						{
							// no spells and no physical activities -> No harm
							SetCanHarmState();
						}
					}
				}
				switch (mechanic)
				{
					case SpellMechanic.Custom_Immolate:
						AuraState |= AuraStateMask.Immolate;
						break;
					case SpellMechanic.Frozen:
						AuraState |= AuraStateMask.Frozen;
						break;
					case SpellMechanic.Bleeding:
						AuraState |= AuraStateMask.Bleeding;
						break;
					case SpellMechanic.Mounted:
						UnitFlags |= UnitFlags.Mounted;
						SpeedFactor += MountSpeedMod;
						m_auras.RemoveByFlag(AuraInterruptFlags.OnMount);
						break;
					case SpellMechanic.Silenced:
						UnitFlags |= UnitFlags.Silenced;
						break;
					case SpellMechanic.Fleeing:
						UnitFlags |= UnitFlags.Feared;
						break;
					case SpellMechanic.Disoriented:
						UnitFlags |= UnitFlags.Confused;
						break;
					case SpellMechanic.Invulnerable:
						UnitFlags |= UnitFlags.SelectableNotAttackable;
						break;
					case SpellMechanic.Enraged:
						AuraState |= AuraStateMask.Enraged;
						break;
				}
			}

			// change the value
			m_mechanics[(int)mechanic]++;
		}

		/// <summary>
        /// Decrease the mechanic modifier count for the given SpellMechanic
		/// </summary>
		public void DecMechanicCount(SpellMechanic mechanic, bool isCustom = false)
		{
			if (m_mechanics == null)
			{
				return;
			}
			int val = m_mechanics[(int)mechanic];
			if (val > 0)
			{
				// change the value
				m_mechanics[(int)mechanic] = val - 1;

				if (val == 1)
				{
						// All of this Mechanic's influences have been removed
					if (isCustom == false)
					{

						if (!m_canMove && SpellConstants.MoveMechanics[(int)mechanic] && !IsAnySetNoCheck(SpellConstants.MoveMechanics))
						{
							m_canMove = true;
							UnitFlags &= ~UnitFlags.Stunned;
							m_lastMoveTime = Environment.TickCount;

							//UnitFlags &= ~UnitFlags.Influenced;
							if (this is Character)
								MovementHandler.SendUnrooted((Character)this);
						}

						if (!m_canInteract && SpellConstants.InteractMechanics[(int)mechanic] && !IsAnySetNoCheck(SpellConstants.InteractMechanics))
						{
							m_canInteract = true;
							//UnitFlags &= ~UnitFlags.UnInteractable;
						}

						if (!m_canHarm && SpellConstants.HarmPreventionMechanics[(int)mechanic])
						{
							SetCanHarmState();
						}

						if (!m_canCastSpells && SpellConstants.SpellCastPreventionMechanics[(int)mechanic] &&
							!IsAnySetNoCheck(SpellConstants.SpellCastPreventionMechanics))
						{
							m_canCastSpells = true;
							if (!m_canDoPhysicalActivity && !m_canHarm)
							{
								// can do spells and no physical activities -> Can harm
								SetCanHarmState();
							}
						}
					}
					switch (mechanic)
					{
						case SpellMechanic.Custom_Immolate:
							AuraState ^= AuraStateMask.Immolate;
							break;
						case SpellMechanic.Frozen:
							AuraState ^= AuraStateMask.Frozen;
							break;
						case SpellMechanic.Bleeding:
							AuraState ^= AuraStateMask.Bleeding;
							break;
						case SpellMechanic.Mounted:
							UnitFlags &= ~UnitFlags.Mounted;
							SpeedFactor -= MountSpeedMod;
							break;
						case SpellMechanic.Silenced:
							UnitFlags &= ~UnitFlags.Silenced;
							break;
						case SpellMechanic.Fleeing:
							UnitFlags &= ~UnitFlags.Feared;
							break;
						case SpellMechanic.Disoriented:
							UnitFlags &= ~UnitFlags.Confused;
							break;
						case SpellMechanic.Invulnerable:
							UnitFlags &= ~UnitFlags.SelectableNotAttackable;
							break;
						case SpellMechanic.Enraged:
							AuraState &= ~AuraStateMask.Enraged;
							break;
					}
				}
			}
		}

		/// <summary>
		/// Checks whether any of the mechanics of the given set are influencing the owner
		/// </summary>
		private bool IsAnySetNoCheck(bool[] set)
		{
			if (m_mechanics == null)
			{
				return false;
			}
			for (var i = 0; i < set.Length; i++)
			{
				if (set[i] && m_mechanics[i] > 0)
				{
					return true;
				}
			}
			return false;
		}

		private void SetCanHarmState()
		{
			if (!IsAnySetNoCheck(SpellConstants.HarmPreventionMechanics))
			{
				CanDoPhysicalActivity = m_Pacified <= 0;
				m_canHarm = m_canDoPhysicalActivity || !IsUnderInfluenceOf(SpellMechanic.Silenced);
			}
			else
			{
				CanDoPhysicalActivity = false;
				m_canHarm = false;
			}
		}

		#endregion

		#region Immunities
		/// <summary>
		/// Whether the owner is completely invulnerable
		/// </summary>
		public bool IsInvulnerable
		{
			get
			{
				return m_mechanics != null && 
					(m_mechanics[(int)SpellMechanic.Invulnerable] > 0 || 
					m_mechanics[(int)SpellMechanic.Invulnerable2] > 0);
			}
			set
			{
				if (m_mechanics == null)
				{
					m_mechanics = CreateMechanicsArr();
				}
				if (value)
				{
					m_mechanics[(int)SpellMechanic.Invulnerable]++;
				}
				else
				{
					m_mechanics[(int)SpellMechanic.Invulnerable] = 0;
				}
			}
		}

		/// <summary>
		/// Indicates whether the owner is immune against the given SpellMechanic
		/// </summary>
		public bool IsImmune(SpellMechanic mechanic)
		{
			if (mechanic == SpellMechanic.None)
			{
				return false;
			}

			return m_mechanicImmunities != null && m_mechanicImmunities[(int)mechanic] > 0;
		}

		/// <summary>
		/// Indicates whether the owner is immune against the given DamageSchool
		/// </summary>
		public bool IsImmune(DamageSchool school)
		{
			return m_dmgImmunities != null && m_dmgImmunities[(int)school] > 0;
		}



		/// <summary>
		/// Adds immunity against given damage-school
		/// </summary>
		public void IncDmgImmunityCount(DamageSchool school)
		{
			if (m_dmgImmunities == null)
			{
				m_dmgImmunities = CreateDamageSchoolArr();
			}
			var val = m_dmgImmunities[(int)school];

			if (val == 0)
			{
				// new immunity: Gets rid of all Auras that use this school
				Auras.RemoveWhere(aura => aura.Spell.SchoolMask.HasAnyFlag((DamageSchoolMask)(1 << (int)school)));
			}

			m_dmgImmunities[(int)school]++;
		}

		/// <summary>
		/// Adds immunity against given damage-schools
		/// </summary>
		public void IncDmgImmunityCount(uint[] schools)
		{
			foreach (var school in schools)
			{
				IncDmgImmunityCount((DamageSchool)school);
			}
		}

		/// <summary>
		/// Adds immunity against given damage-schools
		/// </summary>
		public void IncDmgImmunityCount(SpellEffect effect)
		{
			if (m_dmgImmunities == null)
			{
				m_dmgImmunities = CreateDamageSchoolArr();
			}

			foreach (var school in effect.MiscBitSet)
			{
				m_dmgImmunities[(int)school]++;
			}

			Auras.RemoveWhere(aura =>
				aura.Spell.AuraUID != effect.Spell.AuraUID &&
				aura.Spell.SchoolMask.HasAnyFlag(effect.Spell.SchoolMask) &&
				!aura.Spell.Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability));
		}

		/// <summary>
		/// Decreases immunity-count against given damage-school
		/// </summary>
		public void DecDmgImmunityCount(DamageSchool school)
		{
			if (m_dmgImmunities == null)
			{
				return;
			}
			int val = m_dmgImmunities[(int)school];
			if (val > 0)
			{
				m_dmgImmunities[(int)school]--;
			}
		}

		/// <summary>
		/// Decreases immunity-count against given damage-schools
		/// </summary>
		public void DecDmgImmunityCount(uint[] damageSchools)
		{
			foreach (var school in damageSchools)
			{
				DecDmgImmunityCount((DamageSchool)school);
			}
		}

		/// <summary>
		/// Adds immunity against given SpellMechanic-school
		/// </summary>
		public void IncMechImmunityCount(SpellMechanic mechanic, Spell exclude)
		{
			if (m_mechanicImmunities == null)
			{
				m_mechanicImmunities = CreateMechanicsArr();
			}

			var val = m_mechanicImmunities[(int)mechanic];
			if (val == 0)
			{
				// new immunity: Gets rid of all Auras that use this Mechanic
				Auras.RemoveWhere(aura => aura.Spell.SpellCategories.Mechanic == mechanic &&
					aura.Spell != exclude &&
					(aura.Spell.TargetTriggerSpells == null || !aura.Spell.TargetTriggerSpells.Contains(exclude)) &&
					(aura.Spell.CasterTriggerSpells == null || !aura.Spell.CasterTriggerSpells.Contains(exclude)) &&
					((mechanic != SpellMechanic.Invulnerable && mechanic != SpellMechanic.Invulnerable2) || !aura.Spell.Attributes.HasFlag(SpellAttributes.UnaffectedByInvulnerability)));
			}

			m_mechanicImmunities[(int)mechanic]++;
		}

		/// <summary>
		/// Decreases immunity-count against given SpellMechanic-school
		/// </summary>
		public void DecMechImmunityCount(SpellMechanic mechanic)
		{
			if (m_mechanicImmunities == null)
			{
				return;
			}
			int val = m_mechanicImmunities[(int)mechanic];
			if (val > 0)
			{
				m_mechanicImmunities[(int)mechanic]--;
			}
		}


		// TODO: ModDamageTaken and ModDamageTakenPercent
		#endregion

		#region Mechanic Resistance
		/// <summary>
		/// Returns the resistance chance for the given SpellMechanic
		/// </summary>
		public int GetMechanicResistance(SpellMechanic mechanic)
		{
			if (m_mechanicResistances == null)
			{
				return 0;
			}
			return m_mechanicResistances[(int)mechanic];
		}

		/// <summary>
		/// Changes the amount of resistance against certain SpellMechanics
		/// </summary>
		public void ModMechanicResistance(SpellMechanic mechanic, int delta)
		{
			if (m_mechanicResistances == null)
				m_mechanicResistances = CreateMechanicsArr();
			int val = m_mechanicResistances[(int)mechanic] + delta;
			if (val < 0)
			{
				val = 0;
			}
			m_mechanicResistances[(int)mechanic] = val;
		}
		#endregion

		#region Mechanic Durations
		/// <summary>
		/// Returns the duration modifier for a certain SpellMechanic
		/// </summary>
		public int GetMechanicDurationMod(SpellMechanic mechanic)
		{
			if (m_mechanicDurationMods == null || mechanic == SpellMechanic.None)
			{
				return 0;
			}
			return m_mechanicDurationMods[(int)mechanic];
		}

		/// <summary>
		/// Changes the duration-modifier for a certain SpellMechanic in %
		/// </summary>
		public void ModMechanicDurationMod(SpellMechanic mechanic, int delta)
		{
			if (m_mechanicDurationMods == null)
				m_mechanicDurationMods = CreateMechanicsArr();
			int val = m_mechanicDurationMods[(int)mechanic] + delta;
			if (val < 0)
			{
				val = 0;
			}
			m_mechanicDurationMods[(int)mechanic] = val;
		}
		#endregion

		#region Debuff Resistance
		public int GetDebuffResistance(DamageSchool school)
		{
			if (m_debuffResistances == null)
			{
				return 0;
			}
			return m_debuffResistances[(int)school];
		}

		public void SetDebuffResistance(DamageSchool school, int value)
		{
			if (m_debuffResistances == null)
			{
				m_debuffResistances = CreateDamageSchoolArr();
			}
			m_debuffResistances[(uint)school] = value;
		}

		public void ModDebuffResistance(DamageSchool school, int delta)
		{
			if (m_debuffResistances == null)
			{
				m_debuffResistances = CreateDamageSchoolArr();
			}
			m_debuffResistances[(int)school] += delta;
		}

		//public void ModDebuffResistance(DamageTypeMask schools, float delta)
		//{
		//    foreach (var school in UnitMechanics.DamageTypeMasks) {
		//        if ((schools & school) != 0) {
		//            ModDebuffResistance(UnitMechanics.DamageTypeLookup[school], delta);
		//        }
		//    }
		//}
		#endregion

		#region Dispel Immunities
		public bool IsImmune(DispelType school)
		{
			if (m_dispelImmunities == null)
			{
				return false;
			}
			return m_dispelImmunities[(int)school] > 0;
		}

		public void IncDispelImmunity(DispelType school)
		{
			if (m_dispelImmunities == null)
			{
				m_dispelImmunities = CreateDispelTypeArr();
			}
			int value = m_dispelImmunities[(uint)school];
			if (value == 0)
			{
				// new immunity: Gets rid of all Auras that use this DispelType
				Auras.RemoveWhere(aura => aura.Spell.SpellCategories.DispelType == school);
			}
			m_dispelImmunities[(uint)school] = value + 1;
		}

		public void DecDispelImmunity(DispelType school)
		{
			if (m_dispelImmunities == null)
			{
				return;
			}
			int value = m_dispelImmunities[(uint)school];
			if (value > 0)
			{
				m_dispelImmunities[(int)school] = value - 1;
			}
		}

		//public void ModDebuffResistance(DamageTypeMask schools, float delta)
		//{
		//    foreach (var school in UnitMechanics.DamageTypeMasks) {
		//        if ((schools & school) != 0) {
		//            ModDebuffResistance(UnitMechanics.DamageTypeLookup[school], delta);
		//        }
		//    }
		//}
		#endregion

		#region Target Resistances
		public int GetTargetResistanceMod(DamageSchool school)
		{
			if (m_TargetResMods == null)
			{
				return 0;
			}
			return m_TargetResMods[(int)school];
		}

		void SetTargetResistanceMod(DamageSchool school, int value)
		{
			if (m_TargetResMods == null)
			{
				m_TargetResMods = CreateDamageSchoolArr();
			}
			m_TargetResMods[(uint)school] = value;

			if (school == DamageSchool.Physical && this is Character)
			{
				SetInt32(PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE, value);
			}
		}

		internal void ModTargetResistanceMod(DamageSchool school, int delta)
		{
			if (m_TargetResMods == null)
			{
				m_TargetResMods = CreateDamageSchoolArr();
			}

			var val = m_TargetResMods[(int)school] + delta;
			m_TargetResMods[(int)school] = val;

			if (school == DamageSchool.Physical && this is Character)
			{
				SetInt32(PlayerFields.MOD_TARGET_PHYSICAL_RESISTANCE, val);
			}
		}

		/// <summary>
		/// If modifying a single value, we have a simple bonus that will not be displayed
		/// to the user (except for physical), if there are more than one, we assume it's a change
		/// in all-over spell penetration
		/// </summary>
		/// <param name="dmgTypes"></param>
		/// <param name="delta"></param>
		public void ModTargetResistanceMod(int delta, params uint[] dmgTypes)
		{
			for (var i = 0; i < dmgTypes.Length; i++)
			{
				var school = dmgTypes[i];
				ModTargetResistanceMod((DamageSchool)school, delta);
			}

			if (dmgTypes.Length > 0 && this is Character)
			{
				// overall Spell penetration
				var penetration = GetInt32(PlayerFields.MOD_TARGET_RESISTANCE);
				SetInt32(PlayerFields.MOD_TARGET_RESISTANCE, penetration + delta);
			}
		}
		#endregion

		#region SpellInterruptionProt
		public void ModSpellInterruptProt(DamageSchool school, int delta)
		{
			if (m_spellInterruptProt == null)
			{
				m_spellInterruptProt = CreateDamageSchoolArr();
			}
			var val = m_spellInterruptProt[(int)school] + delta;
			m_spellInterruptProt[(int)school] = val;
		}

		public void ModSpellInterruptProt(uint[] dmgTypes, int delta)
		{
			foreach (var school in dmgTypes)
			{
				ModSpellInterruptProt((DamageSchool)school, delta);
			}
		}

		public int GetSpellInterruptProt(Spell spell)
		{
			if (m_spellInterruptProt == null)
			{
				return 0;
			}
			return m_spellInterruptProt[(int)spell.Schools[0]];
		}
		#endregion

		#region Spell Hit Chance
		public int GetSpellHitChanceMod(DamageSchool school)
		{
			return m_SpellHitChance != null ? m_SpellHitChance[(int)school] : 0;
		}

		public int GetHighestSpellHitChanceMod(DamageSchool[] schools)
		{
			if (m_SpellHitChance == null)
			{
				return 0;
			}

			var spellHitChanceMods = from school in schools
									 select m_SpellHitChance[(int)school];

			return spellHitChanceMods.Max();
		}

		/// <summary>
		/// Spell avoidance
		/// </summary>
		public virtual void ModSpellHitChance(DamageSchool school, int delta)
		{
			if (m_SpellHitChance == null)
			{
				m_SpellHitChance = CreateDamageSchoolArr();
			}
			var val = m_SpellHitChance[(int)school] + delta;
			m_SpellHitChance[(int)school] = val;
		}

		/// <summary>
		/// Spell avoidance
		/// </summary>
		public void ModSpellHitChance(uint[] schools, int delta)
		{
			foreach (var school in schools)
			{
				ModSpellHitChance((DamageSchool)school, delta);
			}
		}
		#endregion

		#region Spell Crit Chance
		/// <summary>
		/// Returns the SpellCritChance for the given DamageType
		/// </summary>
		public virtual float GetCritChance(DamageSchool school)
		{
			return GetCritMod(school);
		}

		public int GetCritMod(DamageSchool school)
		{
			if (m_CritMods == null)
			{
				return 0;
			}
			return m_CritMods[(int)school];
		}

		public void SetCritMod(DamageSchool school, int value)
		{
			if (m_CritMods == null)
			{
				m_CritMods = CreateDamageSchoolArr();
			}
			m_CritMods[(uint)school] = value;
			if (this is Character)
			{
				((Character)this).UpdateSpellCritChance();
			}
		}

		public void ModCritMod(DamageSchool school, int delta)
		{
			if (m_CritMods == null)
			{
				m_CritMods = CreateDamageSchoolArr();
			}
			m_CritMods[(int)school] += delta;

			if (this is Character)
			{
				UnitUpdates.UpdateSpellCritChance((Character)this);
			}
		}

		public void ModCritMod(DamageSchool[] schools, int delta)
		{
			if (m_CritMods == null)
			{
				m_CritMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_CritMods[(int)school] += delta;
			}
			if (this is Character)
			{
				((Character)this).UpdateSpellCritChance();
			}
		}

		public void ModCritMod(uint[] schools, int delta)
		{
			if (m_CritMods == null)
			{
				m_CritMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_CritMods[school] += delta;
			}
			if (this is Character)
			{
				((Character)this).UpdateSpellCritChance();
			}
		}
		#endregion

		#region Damage Taken Mods
		/// <summary>
		/// Returns the damage taken modifier for the given DamageSchool
		/// </summary>
		public int GetDamageTakenMod(DamageSchool school)
		{
			if (m_damageTakenMods == null)
			{
				return 0;
			}
			return m_damageTakenMods[(int)school];
		}

		public void SetDamageTakenMod(DamageSchool school, int value)
		{
			if (m_damageTakenMods == null)
			{
				m_damageTakenMods = CreateDamageSchoolArr();
			}
			m_damageTakenMods[(uint)school] = value;
		}

		public void ModDamageTakenMod(DamageSchool school, int delta)
		{
			if (m_damageTakenMods == null)
			{
				m_damageTakenMods = CreateDamageSchoolArr();
			}
			m_damageTakenMods[(int)school] += delta;
		}

		public void ModDamageTakenMod(DamageSchool[] schools, int delta)
		{
			if (m_damageTakenMods == null)
			{
				m_damageTakenMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_damageTakenMods[(int)school] += delta;
			}
		}

		public void ModDamageTakenMod(uint[] schools, int delta)
		{
			if (m_damageTakenMods == null)
			{
				m_damageTakenMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_damageTakenMods[school] += delta;
			}
		}
		#endregion

		#region Damage Taken % Mods
		/// <summary>
		/// Returns the damage taken modifier for the given DamageSchool
		/// </summary>
		public int GetDamageTakenPctMod(DamageSchool school)
		{
			if (m_damageTakenPctMods == null)
			{
				return 0;
			}
			return m_damageTakenPctMods[(int)school];
		}

		public void SetDamageTakenPctMod(DamageSchool school, int value)
		{
			if (m_damageTakenPctMods == null)
			{
				m_damageTakenPctMods = CreateDamageSchoolArr();
			}
			m_damageTakenPctMods[(uint)school] = value;
		}

		public void ModDamageTakenPctMod(DamageSchool school, int delta)
		{
			if (m_damageTakenPctMods == null)
			{
				m_damageTakenPctMods = CreateDamageSchoolArr();
			}
			m_damageTakenPctMods[(int)school] += delta;
		}

		public void ModDamageTakenPctMod(DamageSchool[] schools, int delta)
		{
			if (m_damageTakenPctMods == null)
			{
				m_damageTakenPctMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_damageTakenPctMods[(int)school] += delta;
			}
		}

		public void ModDamageTakenPctMod(uint[] schools, int delta)
		{
			if (m_damageTakenPctMods == null)
			{
				m_damageTakenPctMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_damageTakenPctMods[school] += delta;
			}
		}
		#endregion

		#region Threat Mods
		/// <summary>
		/// Threat mod in percent
		/// </summary>
		public void ModThreat(DamageSchool school, int delta)
		{
			if (m_threatMods == null)
			{
				m_threatMods = CreateDamageSchoolArr();
			}
			var val = m_threatMods[(int)school] + delta;
			m_threatMods[(int)school] = val;
		}

		/// <summary>
		/// Threat mod in percent
		/// </summary>
		public void ModThreat(uint[] dmgTypes, int delta)
		{
			foreach (var school in dmgTypes)
			{
				ModThreat((DamageSchool)school, delta);
			}
		}

		public int GetGeneratedThreat(IDamageAction action)
		{
			return GetGeneratedThreat(action.ActualDamage, action.UsedSchool, action.SpellEffect);
		}

		/// <summary>
		/// Threat mod in percent
		/// </summary>
		public virtual int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
		{
			if (m_threatMods == null)
			{
				return dmg;
			}
			return Math.Max(0, dmg + ((dmg * m_threatMods[(int)school]) / 100));
		}
		#endregion

		#region Spell Avoidance
		public int GetAttackerSpellHitChanceMod(DamageSchool school)
		{
			return m_attackerSpellHitChance != null ? m_attackerSpellHitChance[(int)school] : 0;
		}

		/// <summary>
		/// Spell avoidance
		/// </summary>
		public void ModAttackerSpellHitChance(DamageSchool school, int delta)
		{
			if (m_attackerSpellHitChance == null)
			{
				m_attackerSpellHitChance = CreateDamageSchoolArr();
			}
			var val = m_attackerSpellHitChance[(int)school] + delta;
			m_attackerSpellHitChance[(int)school] = val;
		}

		/// <summary>
		/// Spell avoidance
		/// </summary>
		public void ModAttackerSpellHitChance(uint[] schools, int delta)
		{
			foreach (var school in schools)
			{
				ModAttackerSpellHitChance((DamageSchool)school, delta);
			}
		}
		#endregion

		#region Teleport
		/// <summary>
		/// Whether this Character is currently allowed to teleport
		/// </summary>
		public virtual bool MayTeleport
		{
			get { return true; }
		}

		/// <summary>
		/// Teleports the owner to the given position in the current map.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(Vector3 pos)
		{
			TeleportTo(m_Map, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the current map.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(ref Vector3 pos)
		{
			TeleportTo(m_Map, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the current map.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(ref Vector3 pos, float? orientation)
		{
			TeleportTo(m_Map, ref pos, orientation);
		}

		/// <summary>
		/// Teleports the owner to the given position in the current map.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(Vector3 pos, float? orientation)
		{
			TeleportTo(m_Map, ref pos, orientation);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given map.
		/// </summary>
		/// <param name="map">the target <see cref="Map" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(MapId map, ref Vector3 pos)
		{
			TeleportTo(World.GetNonInstancedMap(map), ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given map.
		/// </summary>
		/// <param name="map">the target <see cref="Map" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(MapId map, Vector3 pos)
		{
			TeleportTo(World.GetNonInstancedMap(map), ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given map.
		/// </summary>
		/// <param name="map">the target <see cref="Map" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(Map map, ref Vector3 pos)
		{
			TeleportTo(map, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given map.
		/// </summary>
		/// <param name="map">the target <see cref="Map" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(Map map, Vector3 pos)
		{
			TeleportTo(map, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given WorldObject.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		public bool TeleportTo(IWorldLocation location)
		{
			var pos = location.Position;
			var map = location.Map;
			if (map == null)
			{
				if (Map.Id != location.MapId)
				{
					return false;
				}
				map = Map;
			}

			TeleportTo(map, ref pos, m_orientation);
			Phase = location.Phase;

			if (location is WorldObject)
			{
				Zone = ((WorldObject)location).Zone;
			}
			return true;
		}

		/// <summary>
		/// Teleports the owner to the given position in the given map.
		/// </summary>
		/// <param name="map">the target <see cref="Map" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		/// <param name="orientation">the target orientation</param>
		public void TeleportTo(Map map, Vector3 pos, float? orientation)
		{
			TeleportTo(map, ref pos, orientation);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given map.
		/// </summary>
		/// <param name="map">the target <see cref="Map" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		/// <param name="orientation">the target orientation</param>
		public void TeleportTo(Map map, ref Vector3 pos, float? orientation)
		{
			var ownerMap = m_Map;
			if (map.IsDisposed)
				return;

			// must not be moving or logging out when being teleported
			CancelMovement();
			CancelAllActions();

			if (this is Character)
			{
			    //MovementHandler.SendStopMovementPacket(this);
                ((Character)this).CancelLogout();
			}

			if (ownerMap == map)
			{
				if (Map.MoveObject(this, ref pos))
				{
					if (orientation.HasValue)
						Orientation = orientation.Value;

					if (this is Character)
					{
						var chr = ((Character)this);
						chr.LastPosition = pos;

                        // reset movement flags otherwise
                        // the unit will continue move with these flags after teleport
                        MovementFlags = MovementFlags.None;
                        MovementFlags2 = MovementFlags2.None;

						MovementHandler.SendMoved(chr);
					}
				}
			}
			else
			{
				if (ownerMap != null && !ownerMap.IsInContext)
				{
					var position = pos;
					ownerMap.AddMessage(new Message(() => TeleportTo(map, ref position, orientation)));
				}
				else if (map.TransferObjectLater(this, pos))
				{
					if (orientation.HasValue)
					{
						Orientation = orientation.Value;
					}

					if (this is Character)
					{
						var chr = ((Character)this);
						chr.LastPosition = pos;

                        // reset movement flags otherwise
                        // the unit will continue move with these flags after teleport
                        MovementFlags = MovementFlags.None;
                        MovementFlags2 = MovementFlags2.None;
						MovementHandler.SendNewWorld(chr.Client, map.Id, ref pos, Orientation);
					}
				}
				else
				{
					// apparently, the target map has a colliding entity ID. this should NEVER 
					// happen for any kind of Unit

					log.Error("ERROR: Tried to teleport object, but failed to add player to the new map - " + this);
				}
			}
		}

		#endregion

		#region Stealth
		/// <summary>
		/// Count of stealth-modifiers
		/// </summary>
		public int Stealthed
		{
			get { return m_stealthed; }
			set
			{
				if (m_stealthed != value)
				{
					if (m_stealthed > 0 && value <= 0)
					{
						// deactivated stealth
						StateFlags &= ~StateFlag.Sneaking;
					}
					else if (m_stealthed <= 0 && value > 0)
					{
						// activated stealth
						StateFlags |= StateFlag.Sneaking;

						// some auras don't live through Stealth
						Auras.RemoveByFlag(AuraInterruptFlags.OnStealth);
					}
					m_stealthed = value;
				}
			}
		}
		#endregion

		#region Movement

		public MovementFlags MovementFlags
		{
			get { return m_movementFlags; }
			set { m_movementFlags = value; }
		}

		public MovementFlags2 MovementFlags2
		{
			get { return m_movementFlags2; }
			set { m_movementFlags2 = value; }
		}

		public bool IsMovementControlled
		{
			get { return m_Movement != null; }
		}

		/// <summary>
		/// Stops this Unit's movement (if it's movement is controlled)
		/// </summary>
		public void StopMoving()
		{
			if (m_Movement != null)
			{
				m_Movement.Stop();
			}
		}

		/// <summary>
		/// An object to control this Unit's movement. 
		/// Only used for NPCs and posessed Characters.
		/// </summary>
		public Movement Movement
		{
			get
			{
				if (m_Movement == null)
				{
					m_Movement = new Movement(this);
				}
				return m_Movement;
			}
		}

		/// <summary>
		/// Whether the Unit is currently flying
		/// </summary>
		public bool IsFlying
		{
			get
			{
				return m_flying > 0;
			}
		}

		/// <summary>
		/// Whether the character may walk over water
		/// </summary>
		public uint WaterWalk
		{
			get
			{
				return m_waterWalk;
			}
			set
			{
				if ((m_waterWalk == 0) != (value == 0))
				{
					if (this is Character)
					{
						if (value == 0)
						{
							MovementHandler.SendWalk((Character)this);
						}
						else
						{
							MovementHandler.SendWaterWalk((Character)this);
						}
					}
				}
				m_waterWalk = value;
			}
		}

		/// <summary>
		/// Whether a character can fly or not
		/// </summary>
		public uint Flying
		{
			get { return m_flying; }
			set
			{
				if ((m_flying == 0) != (value == 0))
				{
					if (value > 0)
					{
						MovementFlags |= MovementFlags.Flying;
					}
					else
					{
						MovementFlags &= ~MovementFlags.Flying;
					}

					if (this is Character)
					{
						if (value == 0)
						{
							MovementHandler.SendFlyModeStop(this);
						}
						else
						{
							MovementHandler.SendFlyModeStart(this);
						}
					}
				}
				m_flying = value;
			}
		}


		/// <summary>
		/// Whether a character can hover
		/// </summary>
		public uint Hovering
		{
			get
			{
				return m_hovering;
			}
			set
			{
				if ((m_hovering == 0) != (value == 0))
				{
					if (this is Character)
					{
						if (value == 0)
						{
							MovementHandler.SendHoverModeStop(this);
						}
						else
						{
							MovementHandler.SendHoverModeStart(this);
						}
					}
				}
				m_hovering = value;
			}
		}


		/// <summary>
		/// Whether a character would take falling damage or not
		/// </summary>
		public uint FeatherFalling
		{
			get
			{
				return m_featherFalling;
			}
			set
			{
				if ((m_featherFalling == 0) != (value == 0))
				{
					if (this is Character)
					{
						if (value == 0)
						{
							MovementHandler.SendFeatherModeStop(this);
						}
						else
						{
							MovementHandler.SendFeatherModeStart(this);
						}
					}
				}
				m_featherFalling = value;
			}
		}
		#endregion

		#region Speeds
		/// <summary>
		/// The overall-factor for all speeds. Set by the owner's ModifierCollection
		/// </summary>
		public float SpeedFactor
		{
			get { return m_speedFactor; }
			set
			{
				if (value != m_speedFactor)
				{
					m_speedFactor = value;
					WalkSpeed = DefaultWalkSpeed * m_speedFactor;
					RunBackSpeed = DefaultWalkBackSpeed * m_speedFactor;
					RunSpeed = DefaultRunSpeed * m_speedFactor;
					SwimSpeed = DefaultSwimSpeed * (m_speedFactor + m_swimFactor);
					SwimBackSpeed = DefaultSwimBackSpeed * (m_speedFactor + m_swimFactor);
					FlightSpeed = DefaultFlightSpeed * (m_speedFactor + m_flightFactor);
					FlightBackSpeed = DefaultFlightBackSpeed * (m_speedFactor + m_flightFactor);
				}
			}
		}

		/// <summary>
		/// The factor for all flying-related speeds. Set by the owner's ModifierCollection
		/// </summary>
		public float FlightSpeedFactor
		{
			get
			{
				return m_flightFactor;
			}
			internal set
			{
				if (value != m_flightFactor)
				{
					m_flightFactor = value;
					FlightSpeed = DefaultFlightSpeed * (m_speedFactor + m_flightFactor);
					FlightBackSpeed = DefaultFlightBackSpeed * (m_speedFactor + m_flightFactor);
				}
			}
		}

		/// <summary>
		/// The factor for mounted speed
		/// </summary>
		public float MountSpeedMod
		{
			get
			{
				return m_mountMod;
			}
			internal set
			{
				if (value != m_mountMod)
				{
					if (IsMounted)
					{
						SpeedFactor += value - m_mountMod;
					}
					m_mountMod = value;
				}
			}
		}

		/// <summary>
		/// The factor for all swimming-related speeds
		/// </summary>
		public float SwimSpeedFactor
		{
			get
			{
				return m_swimFactor;
			}
			internal set
			{
				if (value != m_swimFactor)
				{
					m_swimFactor = value;
					SwimSpeed = DefaultSwimSpeed * (m_speedFactor + m_swimFactor);
					SwimBackSpeed = DefaultSwimBackSpeed * (m_speedFactor + m_swimFactor);
				}
			}
		}


		/// <summary>
		/// Forward walking speed.
		/// </summary>
		public float WalkSpeed
		{
			get { return m_walkSpeed; }
			set
			{
				if (m_walkSpeed != value)
				{
					m_walkSpeed = value;
					MovementHandler.SendSetWalkSpeed(this);
				}
			}
		}

		/// <summary>
		/// Backwards walking speed.
		/// </summary>
		public float RunBackSpeed
		{
			get { return m_walkBackSpeed; }
			set
			{
				if (m_walkBackSpeed != value)
				{
					m_walkBackSpeed = value;
					MovementHandler.SendSetRunBackSpeed(this);
				}
			}
		}

		/// <summary>
		/// Forward running speed.
		/// </summary>
		public float RunSpeed
		{
			get { return m_runSpeed; }
			set
			{
				if (m_runSpeed != value)
				{
					m_runSpeed = value;
					MovementHandler.SendSetRunSpeed(this);
				}
			}
		}

		/// <summary>
		/// Forward swimming speed.
		/// </summary>
		public float SwimSpeed
		{
			get { return m_swimSpeed; }
			set
			{
				if (m_swimSpeed != value)
				{
					m_swimSpeed = value;
					MovementHandler.SendSetSwimSpeed(this);
				}
			}
		}

		/// <summary>
		/// Backwards swimming speed.
		/// </summary>
		public float SwimBackSpeed
		{
			get { return m_swimBackSpeed; }
			set
			{
				if (m_swimBackSpeed != value)
				{
					m_swimBackSpeed = value;
					MovementHandler.SendSetSwimBackSpeed(this);
				}
			}
		}

		/// <summary>
		/// Forward flying speed.
		/// </summary>
		public float FlightSpeed
		{
			get { return m_flightSpeed; }
			set
			{
				if (m_flightSpeed != value)
				{
					m_flightSpeed = value;
					MovementHandler.SendSetFlightSpeed(this);
				}
			}
		}

		/// <summary>
		/// Backwards flying speed.
		/// </summary>
		public float FlightBackSpeed
		{
			get { return m_flightBackSpeed; }
			set
			{
				if (m_flightBackSpeed != value)
				{
					m_flightBackSpeed = value;
					MovementHandler.SendSetFlightBackSpeed(this);
				}
			}
		}

		/// <summary>
		/// Turning speed.
		/// </summary>
		public float TurnSpeed
		{
			get { return m_turnSpeed; }
			set
			{
				if (m_turnSpeed != value)
				{
					m_turnSpeed = value;
					MovementHandler.SendSetTurnRate(this);
				}
			}
		}

		public float PitchRate
		{
			get { return m_pitchSpeed; }
			set
			{
				if (m_pitchSpeed != value)
				{
					m_pitchSpeed = value;
					MovementHandler.SendSetPitchRate(this);
				}
			}
		}
		#endregion

		#region Misc
		public void ResetMechanicDefaults()
		{
			SpeedFactor = 1f;
			m_mountMod = 0;

			m_flying = m_waterWalk = m_hovering = m_featherFalling = 0;
			m_canMove = m_canHarm = m_canInteract = m_canCastSpells = m_canDoPhysicalActivity = true;

			m_walkSpeed = DefaultWalkSpeed;
			m_walkBackSpeed = DefaultWalkBackSpeed;
			m_runSpeed = DefaultRunSpeed;
			m_swimSpeed = DefaultSwimSpeed;
			m_swimBackSpeed = DefaultSwimBackSpeed;
			m_flightSpeed = DefaultFlightSpeed;
			m_flightBackSpeed = DefaultFlightBackSpeed;
			m_turnSpeed = DefaultTurnSpeed;
			m_pitchSpeed = DefaulPitchSpeed;
		}
		#endregion

		#region Resilience
		public virtual float GetResiliencePct()
		{
			return 0;
		}
		#endregion

		public int AttackerSpellCritChancePercentMod
		{
			get;
			set;
		}

		public int AttackerPhysicalCritChancePercentMod
		{
			get;
			set;
		}
	}
}