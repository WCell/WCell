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
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Modifiers;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Constants.World;
using WCell.RealmServer.Spells.Auras.Handlers;
using System.Collections.Generic;
using WCell.RealmServer.Items;
using WCell.Util.Variables;
using WCell.Core.Initialization;

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
		/// A dictionary that maps each specific DamageTypeMask-flag to its corresponding DamageType counterpart
		/// </summary>
		// public static readonly Dictionary<DamageTypeMask, DamageType> DamageTypeLookup = new Dictionary<DamageTypeMask, DamageType>();
		/// <summary>
		/// All DamageTypeMasks
		/// </summary>
		// public static readonly DamageTypeMask[] DamageTypeMasks = Utility.GetEnumValues<DamageTypeMask>();
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

		protected int[] m_mechanics;
		protected int[] m_mechanicImmunities;
		protected int[] m_mechanicResistances;
		protected int[] m_mechanicDurationMods;
		protected int[] m_debuffResistances;
		protected int[] m_dispelImmunities;
		protected int[] m_TargetResMods;
		protected int[] m_spellInterruptProt;
		protected int[] m_threatMods;

		protected int m_ManaShieldAmount;
		protected float m_ManaShieldFactor;

		/// <summary>
		/// Immunities against damage-schools
		/// </summary>
		protected int[] m_dmgImmunities;

		/// <summary>
		/// List of <see cref="IDamageAbsorber"/> which absorb any damage taken
		/// </summary>
		protected List<IDamageAbsorber> m_absorbers;

		/// <summary>
		/// Whether the physical state of this Unit allows it to move
		/// </summary>
		protected bool m_canMove;

		protected bool m_canInteract, m_canHarm, m_canCastSpells, m_evades;

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
		/// Whether the Unit is allowed to cast spells
		/// </summary>
		public bool CanCastSpells
		{
			get
			{
				return m_canCastSpells;
			}
		}

		/// <summary>
		/// Whether the physical state of this Unit allows it to move.
		/// To stop a char from moving, use IncMechanicCount to increase Rooted or any other movement-effecting Mechanic-school.
		/// Use MayMove to also take Movement-controlling (eg by AI etc) into consideration.
		/// </summary>
		public bool CanMove
		{
			get
			{
				return m_canMove;
			}
		}

		/// <summary>
		/// Whether the owner (if any) allows this unit to move
		/// </summary>
		public bool MayMove
		{
			get { return CanMove && (m_Movement == null || m_Movement.MayMove); }
			set
			{
				if (m_Movement != null)
				{
					m_Movement.MayMove = value;
				}
			}
		}

		/// <summary>
		/// Whether the Unit is currently evading (cannot be hit etc)
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
			get
			{
				return m_canHarm && base.CanDoHarm;
			}
		}

		/// <summary>
		/// Whether the owner is disarmed
		/// </summary>
		public bool IsDisarmed
		{
			get
			{
				return IsUnderInfluenceOf(SpellMechanic.Disarmed);
			}
		}

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
		/// Checks whether any of the mechanics of the given set are influencing the owner
		/// </summary>
		bool IsAnySetNoCheck(bool[] set)
		{
			for (int i = 1; i < set.Length; i++)
			{
				if (set[i] && m_mechanics[i] > 0)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Increase the mechnanic modifier count for the given SpellMechanic
		/// </summary>
		public void IncMechanicCount(SpellMechanic mechanic)
		{
			if (m_mechanics == null)
			{
				m_mechanics = CreateMechanicsArr();
			}

			var val = m_mechanics[(int)mechanic];
			if (val == 0)
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
					if (IsUsingSpell && SpellCast.Spell.InterruptFlags.Has(InterruptFlags.OnStunned))
					{
						SpellCast.Cancel();
					}
				}

				// interaction
				if (m_canInteract && SpellConstants.InteractMechanics[(int)mechanic])
				{
					m_canInteract = false;
					//UnitFlags |= UnitFlags.UnInteractable;
				}

				// harmfulnes
				if (m_canHarm && SpellConstants.HarmMechanics[(int)mechanic])
				{
					m_canHarm = false;
					if (IsUsingSpell && SpellCast.Spell.HasHarmfulEffects)
					{
						SpellCast.Cancel();
					}
				}

				// check if we can still cast spells
				if (m_canCastSpells && SpellConstants.SpellMechanics[(int)mechanic])
				{
					// check if we can still cast spells
					m_canCastSpells = false;
					UnitFlags |= UnitFlags.Silenced;
					if (IsUsingSpell && SpellCast.Spell.InterruptFlags.Has(InterruptFlags.OnSilence))
					{
						SpellCast.Cancel();
					}
				}

				if (mechanic == SpellMechanic.Frozen)
				{
					// Apply Frozen AuraState
					AuraState |= AuraStateMask.Frozen;
				}


				if (mechanic == SpellMechanic.Mounted)
				{
					// Now mounted
					UnitFlags |= UnitFlags.Mounted;
					SpeedFactor += MountSpeedMod;
					m_auras.RemoveByFlag(AuraInterruptFlags.OnMount);
				}
				else if (mechanic == SpellMechanic.Slowed)
				{
					UnitFlags |= UnitFlags.Pacified;
				}
				else if (mechanic == SpellMechanic.Disarmed)
				{
					UnitFlags |= UnitFlags.Disarmed;
					MainWeapon = GenericWeapon.Fists;
					OffHandWeapon = null;
					RangedWeapon = null;
				}
				else if (mechanic == SpellMechanic.Fleeing)
				{
					UnitFlags |= UnitFlags.Feared;
				}
				else if (mechanic == SpellMechanic.Disoriented)
				{
					UnitFlags |= UnitFlags.Confused;
				}
				else if (mechanic == SpellMechanic.Invulnerable)
				{
					UnitFlags |= UnitFlags.SelectableNotAttackable;
				}
			}

			// change the value
			m_mechanics[(int)mechanic]++;
		}

		/// <summary>
		/// Decrease the mechnanic modifier count for the given SpellMechanic
		/// </summary>
		public void DecMechanicCount(SpellMechanic mechanic)
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

					if (!m_canHarm && SpellConstants.HarmMechanics[(int)mechanic] && !IsAnySetNoCheck(SpellConstants.HarmMechanics))
					{
						m_canHarm = true;
					}

					if (!m_canCastSpells && SpellConstants.SpellMechanics[(int)mechanic] && !IsAnySetNoCheck(SpellConstants.SpellMechanics))
					{
						m_canCastSpells = true;
						UnitFlags &= ~UnitFlags.Silenced;
					}

					if (mechanic == SpellMechanic.Frozen)
					{
						// Remove Frozen AuraState
						AuraState ^= AuraStateMask.Frozen;
					}

					if (mechanic == SpellMechanic.Mounted)
					{
						UnitFlags &= ~UnitFlags.Mounted;
						SpeedFactor -= MountSpeedMod;
					}
					else if (mechanic == SpellMechanic.Disarmed && m_mechanics[(int)SpellMechanic.Disarmed] == 0)
					{
						UnitFlags &= ~UnitFlags.Disarmed;
						// TODO: Put weapons back in place
					}
					else if (mechanic == SpellMechanic.Slowed && m_mechanics[(int)SpellMechanic.Slowed] == 0)
					{
						UnitFlags &= ~UnitFlags.Pacified;
					}
					else if (mechanic == SpellMechanic.Fleeing && m_mechanics[(int)SpellMechanic.Horrified] == 0)
					{
						UnitFlags &= ~UnitFlags.Feared;
					}
					else if (mechanic == SpellMechanic.Disoriented && m_mechanics[(int)SpellMechanic.Disoriented] == 0)
					{
						UnitFlags &= ~UnitFlags.Confused;
					}
					else if (mechanic == SpellMechanic.Invulnerable && m_mechanics[(int)SpellMechanic.Invulnerable] == 0)
					{
						UnitFlags &= ~UnitFlags.SelectableNotAttackable;
					}
				}
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
				return m_mechanics != null && m_mechanics[(int)SpellMechanic.Invulnerable] > 0;
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
				Auras.RemoveWhere(aura => (aura.Spell.SchoolMask & (DamageSchoolMask)(1 << (int)school)) != 0);
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
				(aura.Spell.SchoolMask & effect.Spell.SchoolMask) != 0 &&
				!aura.Spell.Attributes.Has(SpellAttributes.UnaffectedByInvulnerability));
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
		public void IncMechImmunityCount(SpellMechanic mechanic)
		{
			if (m_mechanicImmunities == null)
			{
				m_mechanicImmunities = CreateMechanicsArr();
			}

			var val = m_mechanicImmunities[(int)mechanic];
			if (val == 0)
			{
				// new immunity: Gets rid of all Auras that use this Mechanic
				Auras.RemoveWhere(aura => aura.Spell.Mechanic == mechanic &&
					((mechanic != SpellMechanic.Invulnerable && mechanic != SpellMechanic.Invulnerable_2) || !aura.Spell.Attributes.Has(SpellAttributes.UnaffectedByInvulnerability)));
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
		//public int GetDmgResistance(DamageType school)
		//{
		//    if (m_dmgResist == null) {
		//        return 0;
		//    }

		//    m_dmgResist[(int)school] += value;
		//}

		///// <summary>
		///// Adds resistance against certain damage
		///// </summary>
		//public void SetDmgnNullifier(DamageType school, int value)
		//{
		//    if (m_dmgResist == null) {
		//        m_dmgResist = CreateDamageSchoolArr();
		//    }

		//    m_dmgResist[(int)school] += value;
		//}
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

		#region Absorb
		/// <summary>
		/// Absorbs the given school and amount of damage
		/// </summary>
		/// <returns>The amount of damage absorbed</returns>
		public int Absorb(DamageSchool school, int amount)
		{
			if (m_absorbers == null || m_absorbers.Count == 0)
			{
				return 0;
			}
			if (m_AttackAction != null)
			{
				if (m_AttackAction.Spell.AttributesExD.Has(SpellAttributesExD.CannotBeAbsorbed))
					return 0;
			}
			var absorb = 0;

			// count backwards so removal of elements won't disturb anything
			for (var i = m_absorbers.Count - 1; i >= 0; i--)
			{
				var absorber = m_absorbers[i];
				if (absorber.AbsorbSchool.Has(school))
				{
					var abs = Math.Min(amount, absorber.AbsorbValue);
					absorb += abs;
					absorber.AbsorbValue -= abs;
				}
			}
			return absorb;
		}

		/// <summary>
		/// Adds a new <see cref="IDamageAbsorber"/>
		/// </summary>
		public void AddDmgAbsorption(IDamageAbsorber absorber)
		{
			if (m_absorbers == null)
			{
				m_absorbers = new List<IDamageAbsorber>(1);
			}

			m_absorbers.Add(absorber);
		}

		/// <summary>
		/// Removes an existing <see cref="IDamageAbsorber"/>
		/// </summary>
		public void RemoveDmgAbsorption(IDamageAbsorber absorber)
		{
			if (m_absorbers != null)
			{
				m_absorbers.Remove(absorber);
			}
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
				Auras.RemoveWhere(aura => aura.Spell.DispelType == school);
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

		#region Spell Crit Chance
		/// <summary>
		/// Returns the SpellCritChance for the given DamageType
		/// </summary>
		public virtual float GetSpellCritChance(DamageSchool school)
		{
			return 0;
		}

		protected int[] m_spellCritMods;

		public int GetSpellCritMod(DamageSchool school)
		{
			if (m_spellCritMods == null)
			{
				return 0;
			}
			return m_spellCritMods[(int)school];
		}

		public void SetSpellCritMod(DamageSchool school, int value)
		{
			if (m_spellCritMods == null)
			{
				m_spellCritMods = CreateDamageSchoolArr();
			}
			m_spellCritMods[(uint)school] = value;
			if (this is Character)
			{
				UnitUpdates.UpdateSpellCritChance((Character)this);
			}
		}

		public void ModSpellCritMod(DamageSchool school, int delta)
		{
			if (m_spellCritMods == null)
			{
				m_spellCritMods = CreateDamageSchoolArr();
			}
			m_spellCritMods[(int)school] += delta;

			if (this is Character)
			{
				UnitUpdates.UpdateSpellCritChance((Character)this);
			}
		}

		public void ModSpellCritMod(DamageSchool[] schools, int delta)
		{
			if (m_spellCritMods == null)
			{
				m_spellCritMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_spellCritMods[(int)school] += delta;
			}
			if (this is Character)
			{
				UnitUpdates.UpdateSpellCritChance((Character)this);
			}
		}

		public void ModSpellCritMod(uint[] schools, int delta)
		{
			if (m_spellCritMods == null)
			{
				m_spellCritMods = CreateDamageSchoolArr();
			}

			foreach (var school in schools)
			{
				m_spellCritMods[school] += delta;
			}
			if (this is Character)
			{
				UnitUpdates.UpdateSpellCritChance((Character)this);
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
				ModSpellInterruptProt((DamageSchool)school, delta);
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
			return dmg + ((dmg * m_threatMods[(int)school]) / 100);
		}
		#endregion

		#region Teleport
		/// <summary>
		/// Whether this Character is currently allowed to teleport
		/// </summary>
		public virtual bool MayTeleport
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Teleports the owner to the given position in the current region.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(Vector3 pos)
		{
			TeleportTo(m_region, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the current region.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(ref Vector3 pos)
		{
			TeleportTo(m_region, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the current region.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(ref Vector3 pos, float? orientation)
		{
			TeleportTo(m_region, ref pos, orientation);
		}

		/// <summary>
		/// Teleports the owner to the given position in the current region.
		/// </summary>
		/// <returns>Whether the Zone had a globally unique Site.</returns>
		public void TeleportTo(Vector3 pos, float? orientation)
		{
			TeleportTo(m_region, ref pos, orientation);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given region.
		/// </summary>
		/// <param name="region">the target <see cref="Region" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(MapId region, ref Vector3 pos)
		{
			TeleportTo(World.GetRegion(region), ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given region.
		/// </summary>
		/// <param name="region">the target <see cref="Region" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(MapId region, Vector3 pos)
		{
			TeleportTo(World.GetRegion(region), ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given region.
		/// </summary>
		/// <param name="region">the target <see cref="Region" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(Region region, ref Vector3 pos)
		{
			TeleportTo(region, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given region.
		/// </summary>
		/// <param name="region">the target <see cref="Region" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		public void TeleportTo(Region region, Vector3 pos)
		{
			TeleportTo(region, ref pos, null);
		}

		/// <summary>
		/// Teleports the owner to the given WorldObject.
		/// </summary>
		/// <param name="location"></param>
		/// <returns></returns>
		public bool TeleportTo(IWorldLocation location)
		{
			var pos = location.Position;
			var rgn = location.Region;
			if (rgn == null)
			{
				if (Region.Id != location.RegionId)
				{
					return false;
				}
				rgn = Region;
			}

			TeleportTo(rgn, ref pos, m_orientation);
			if (location is WorldObject)
			{
				Zone = ((WorldObject)location).Zone;
			}
			return true;
		}

		/// <summary>
		/// Teleports the owner to the given position in the given region.
		/// </summary>
		/// <param name="region">the target <see cref="Region" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		/// <param name="orientation">the target orientation</param>
		public void TeleportTo(Region region, Vector3 pos, float? orientation)
		{
			TeleportTo(region, ref pos, orientation);
		}

		/// <summary>
		/// Teleports the owner to the given position in the given region.
		/// </summary>
		/// <param name="region">the target <see cref="Region" /></param>
		/// <param name="pos">the target <see cref="Vector3">position</see></param>
		/// <param name="orientation">the target orientation</param>
		public void TeleportTo(Region region, ref Vector3 pos, float? orientation)
		{
			var ownerRegion = m_region;
			if (region.IsDisposed)
				return;

			// must not be moving or logging out when being teleported
			CancelMovement();
			if (this is Character)
			{
				((Character)this).CancelLogout();
			}

			if (ownerRegion == region)
			{
				if (Region.MoveObject(this, ref pos))
				{
					if (orientation.HasValue)
						Orientation = orientation.Value;

					if (this is Character)
					{
						var chr = ((Character)this);
						chr.LastPosition = pos;

						MovementHandler.SendMoved(chr);
					}
				}
			}
			else
			{
				if (ownerRegion != null && !ownerRegion.IsInContext)
				{
					var position = pos;
					ownerRegion.AddMessage(new Message(() => TeleportTo(region, ref position, orientation)));
				}
				else if (region.TransferObjectLater(this, pos))
				{
					if (orientation.HasValue)
					{
						Orientation = orientation.Value;
					}

					if (this is Character)
					{
						var chr = ((Character)this);
						chr.LastPosition = pos;

						MovementHandler.SendNewWorld(chr.Client, region.Id, ref pos, Orientation);
					}
				}
				else
				{
					// apparently, the target region has a colliding entity ID. this should NEVER 
					// happen for any kind of Unit

					log.Error("ERROR: Tried to teleport object, but failed to add player to the new region - " + this);
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
			get
			{
				return m_stealthed;
			}
			set
			{
				if (m_stealthed != value)
				{
					if (m_stealthed > 0 && value <= 0)
					{
						// deactivated stealth
						ShapeShiftForm = ShapeShiftForm.Normal;
						StateFlags &= ~StateFlag.Sneaking;
					}
					else if (m_stealthed <= 0 && value > 0)
					{
						// activated stealth
						ShapeShiftForm = ShapeShiftForm.Stealth;
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

		public bool IsMovementConrolled
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

		#region Mana Shield
		/// <summary>
		/// Amount of remaining mana shield
		/// </summary>
		public int ManaShieldAmount
		{
			get { return m_ManaShieldAmount; }
			set { m_ManaShieldAmount = value; }
		}

		/// <summary>
		/// Amount of mana to be subtracted for each hit taken
		/// </summary>
		public float ManaShieldFactor
		{
			get { return m_ManaShieldFactor; }
			set { m_ManaShieldFactor = value; }
		}

		/// <summary>
		/// Drains as much damage from a currently active mana shield as possible.
		/// Deactivates the mana shield once its used up.
		/// </summary>
		/// <param name="damage"></param>
		public void DrainManaShield(ref int damage)
		{
			var power = Power;
			var amount = damage;
			if (amount >= m_ManaShieldAmount)
			{
				amount = m_ManaShieldAmount;
				m_auras.RemoveWhere(aura => aura.Spell.HasManaShield);
			}

			var powerPoints = (int)(power / m_ManaShieldFactor);
			amount = Math.Min(amount, powerPoints);

			m_ManaShieldAmount -= powerPoints;
			Power = power - (int)(amount * m_ManaShieldFactor);
			damage -= amount;
		}

		public void SetManaShield(float factor, int amount)
		{
			m_ManaShieldFactor = factor;
			m_ManaShieldAmount = amount;
		}
		#endregion

		#region Misc
		public void ResetMechanicDefaults()
		{
			SpeedFactor = 1f;
			m_mountMod = 0;

			m_flying = m_waterWalk = m_hovering = m_featherFalling = 0;
			m_canMove = m_canHarm = m_canInteract = m_canCastSpells = true;

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
	}
}
