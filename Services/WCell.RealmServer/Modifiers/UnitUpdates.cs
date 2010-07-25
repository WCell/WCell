/*************************************************************************
 *
 *   file		: Spirit.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-02-05 04:47:44 +0800 (Tue, 05 Feb 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 106 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Skills;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.RacesClasses;
using WCell.Util;
using System.Collections;

namespace WCell.RealmServer.Modifiers
{
	/// <summary>
	/// Container for methods to update Unit-Fields
	/// </summary>
	public static class UnitUpdates
	{
		#region Init Update Handlers
		delegate void UpdateHandler(Unit unit);
		static readonly UpdateHandler NothingHandler = (Unit unit) => { };

		/// <summary>
		/// Amount of mana to be added per point of Intelligence
		/// </summary>
		public static int ManaPerInt = 15;

		/// <summary>
		/// Amount of armor to be added per point of Agility
		/// </summary>
		public static int ArmorPerAgil = 2;

		public static readonly int FlatIntModCount = (int)Utility.GetMaxEnum<StatModifierInt>();
		public static readonly int MultiplierModCount = (int)Utility.GetMaxEnum<StatModifierFloat>();

		static readonly UpdateHandler[] FlatIntModHandlers = new UpdateHandler[FlatIntModCount + 1];
		static readonly UpdateHandler[] MultiModHandlers = new UpdateHandler[MultiplierModCount + 1];
		//static UpdateHandler[] BaseModHandlers = new UpdateHandler[BaseModCount];
		//static UpdateHandler[] FlatFloatHandlers = new UpdateHandler[FlatFloatModCount];
		//public static readonly int BaseModCount = (int)Utility.GetMaxEnum<ModifierBase>();
		//public static readonly int FlatFloatModCount = (int)Utility.GetMaxEnum<ModifierFlatFloat>();

		static UnitUpdates()
		{
			FlatIntModHandlers[(int)StatModifierInt.Power] = UpdateMaxPower;
			FlatIntModHandlers[(int)StatModifierInt.PowerPct] = UpdateMaxPower;
			FlatIntModHandlers[(int)StatModifierInt.HealthRegen] = UpdateHealthRegen;
			FlatIntModHandlers[(int)StatModifierInt.HealthRegenInCombat] = UpdateCombatHealthRegen;
			FlatIntModHandlers[(int)StatModifierInt.HealthRegenNoCombat] = UpdateNormalHealthRegen;
			FlatIntModHandlers[(int)StatModifierInt.Power] = UpdateMaxPower;
			FlatIntModHandlers[(int)StatModifierInt.PowerRegen] = UpdatePowerRegen;
			FlatIntModHandlers[(int)StatModifierInt.RangedAttackPowerByPercentOfIntellect] = UpdateRangedAttackPower;
			FlatIntModHandlers[(int)StatModifierInt.DodgeChance] = UpdateDodgeChance;
			FlatIntModHandlers[(int)StatModifierInt.BlockValue] = UpdateBlockChance;
			FlatIntModHandlers[(int)StatModifierInt.BlockChance] = UpdateBlockChance;
			FlatIntModHandlers[(int)StatModifierInt.CritChance] = UpdateCritChance;
			FlatIntModHandlers[(int)StatModifierInt.ParryChance] = UpdateParryChance;
			FlatIntModHandlers[(int)StatModifierInt.AttackerMeleeHitChance] = UpdateMeleeHitChance;
			FlatIntModHandlers[(int)StatModifierInt.AttackerRangedHitChance] = UpdateRangedHitChance;

			MultiModHandlers[(int)StatModifierFloat.AttackerCritChance] = NothingHandler;
			//MultiModHandlers[(int)ModifierMulti.BlockChance] = UpdateBlockChance;
			MultiModHandlers[(int)StatModifierFloat.BlockValue] = UpdateBlockChance;
			MultiModHandlers[(int)StatModifierFloat.CritChance] = UpdateCritChance;
			MultiModHandlers[(int)StatModifierFloat.AttackTime] = UpdateAllAttackTimes;
			MultiModHandlers[(int)StatModifierFloat.HealthRegen] = UpdateHealthRegen;
			MultiModHandlers[(int)StatModifierFloat.PowerRegen] = UpdatePowerRegen;
		}
		#endregion

		internal static void UpdateAll(this Unit unit)
		{
			UpdateStrength(unit);
			UpdateAgility(unit);
			UpdateIntellect(unit);
			UpdateSpirit(unit);
			UpdateStamina(unit);
		}

		#region Stats

		internal static void UpdateLevel(this Character chr)
		{
			var clss = chr.Archetype.Class;
			var lvl = chr.Level;
			var agil = chr.Agility;

			UpdateDodgeChance(chr);

			UpdateBlockChance(chr);
			UpdateCritChance(chr);
			UpdateAllAttackPower(chr);
		}

		internal static void UpdateStrength(this Unit unit)
		{
			var str = unit.GetBaseStatValue(StatType.Strength) + unit.StrengthBuffPositive - unit.StrengthBuffNegative;
			//str = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Strength], str);
			unit.SetInt32(UnitFields.STAT0, str);

			UpdateBlockChance(unit);
			UpdateAllAttackPower(unit);
		}

		internal static void UpdateAgility(this Unit unit)
		{
			var oldAgil = unit.Agility;
			var agil = unit.GetBaseStatValue(StatType.Agility) + unit.AgilityBuffPositive - unit.AgilityBuffNegative;
			//agil = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Agility], agil);
			unit.SetInt32(UnitFields.STAT1, agil);
			unit.ModBaseResistance(DamageSchool.Physical, (agil - oldAgil) * ArmorPerAgil);	// armor

			UpdateDodgeChance(unit);
			UpdateCritChance(unit);
			UpdateAllAttackPower(unit);
		}

		internal static void UpdateStamina(this Unit unit)
		{
			//var oldStam = unit.Stamina;
			var stam = unit.GetBaseStatValue(StatType.Stamina) + unit.StaminaBuffPositive - unit.StaminaBuffNegative;
			//stam = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Stamina], stam);

			//var delta = stam - oldStam;
			unit.SetInt32(UnitFields.STAT2, stam);

			//var healthBonus = 10 * delta;
			//if (unit.Race == RaceId.Tauren)
			//{
			//    healthBonus += (delta + 1) / 2;
			//}

			//unit.BaseHealth += healthBonus;
			UpdateHealth(unit);
		}

		internal static void UpdateIntellect(this Unit unit)
		{
			var intel = unit.GetBaseStatValue(StatType.Intellect) + unit.IntellectBuffPositive - unit.IntellectBuffNegative;
			//intel = intel < 0 ? 0 : GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Intellect], intel);
			unit.SetInt32(UnitFields.STAT3, intel);

			if (unit is Character)
			{
				var chr = (Character)unit;
				if (unit.PowerType == PowerType.Mana)
				{
					UpdateSpellCritChance(chr);
				}

				// TODO Update spell power: AddDamageMod & HealingDoneMod

				UpdatePowerRegen(unit);
			}

			UpdateMaxPower(unit);
			if (unit.IntMods[(int)StatModifierInt.RangedAttackPowerByPercentOfIntellect] > 0)
			{
				unit.UpdateRangedAttackPower();
			}
		}

		internal static void UpdateSpirit(this Unit unit)
		{
			var spirit = unit.GetBaseStatValue(StatType.Spirit) + unit.SpiritBuffPositive - unit.SpiritBuffNegative;
			//spirit = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Spirit], spirit);
			unit.SetInt32(UnitFields.STAT4, spirit);

			UpdateNormalHealthRegen(unit);

			// We don't need to call when we are still in the process of loaing
			if (unit.Intellect != 0)
			{
				UpdatePowerRegen(unit);
			}
		}

		internal static void UpdateStat(this Unit unit, StatType stat)
		{
			switch (stat)
			{
				case StatType.Agility:
					unit.UpdateAgility();
					break;
				case StatType.Intellect:
					unit.UpdateIntellect();
					break;
				case StatType.Spirit:
					unit.UpdateSpirit();
					break;
				case StatType.Stamina:
					unit.UpdateStamina();
					break;
				case StatType.Strength:
					unit.UpdateStrength();
					break;
			}
		}
		#endregion

		internal static void UpdateHealth(this Unit unit)
		{
			var stamina = unit.Stamina;
			var stamBonus = Math.Max(stamina, 20) + (Math.Max(0, stamina - 20) * 10);
			var value = unit.BaseHealth + stamBonus + unit.MaxHealthMod;

			unit.SetInt32(UnitFields.MAXHEALTH, value);
		}

		internal static void UpdateMaxPower(this Unit unit)
		{
			var value = unit.BasePower + unit.IntMods[(int)StatModifierInt.Power];
			if (unit is Character && unit.PowerType == PowerType.Mana)
			{
				var intelBase = ((Character)unit).Archetype.FirstLevelStats.Intellect;
				value += intelBase + (unit.Intellect - intelBase) * ManaPerInt;
			}
			value += (value*unit.IntMods[(int) StatModifierInt.PowerPct] + 50) / 100;
			if (value < 0)
			{
				value = 0;
			}

			unit.MaxPower = value;
		}

		#region Regen

		/// <summary>
		/// Updates the amount of Health regenerated per regen-tick, independent on the combat state
		/// </summary>
		internal static void UpdateHealthRegen(this Unit unit)
		{
			UpdateNormalHealthRegen(unit);
			UpdateCombatHealthRegen(unit);
		}

		/// <summary>
		/// Updates the amount of Health regenerated per regen-tick
		/// </summary>
		internal static void UpdateNormalHealthRegen(this Unit unit)
		{
			int regen;
			if (unit is Character)
			{
				regen = ((Character)unit).Archetype.Class.CalculateHealthRegen(unit.Level, unit.Spirit);
			}
			else
			{
				regen = 0;
			}
			regen += unit.IntMods[(int)StatModifierInt.HealthRegen] +
				unit.IntMods[(int)StatModifierInt.HealthRegenNoCombat];

			regen = GetMultiMod((int)unit.FloatMods[(int)StatModifierFloat.HealthRegen], regen);

			unit.HealthRegenPerTickNoCombat = regen;
		}

		/// <summary>
		/// Updates the amount of Health regenerated per regen-tick
		/// </summary>
		internal static void UpdateCombatHealthRegen(this Unit unit)
		{
			unit.HealthRegenPerTickCombat = unit.IntMods[(int)StatModifierInt.HealthRegen] +
				unit.IntMods[(int)StatModifierInt.HealthRegenInCombat];
		}


		/// <summary>
		/// Updates the amount of Power regenerated per regen-tick (while not being "interrupted")
		/// </summary>
		internal static void UpdatePowerRegen(this Unit unit)
		{
			var regen = 0;

			if (unit.IsAlive)
			{
				if (unit is Character)
				{
					regen = ((Character)unit).Archetype.Class.CalculatePowerRegen((Character)unit);
				}
				else if (unit.IsMinion && unit.PowerType == PowerType.Focus)
				{
					regen = 5;
				}
				else
				{
					// TODO: NPC regen
					regen = unit.BasePower / 20;
				}

				regen += unit.IntMods[(int)StatModifierInt.PowerRegen];
				regen = GetMultiMod((int)unit.FloatMods[(int)StatModifierFloat.PowerRegen], regen);
			}

			unit.PowerRegenPerTick = regen;
		}
		#endregion

		internal static void UpdateAllAttackPower(this Unit unit)
		{
			UpdateMeleeAttackPower(unit);
			UpdateRangedAttackPower(unit);
		}

		internal static void UpdateMeleeAttackPower(this Unit unit)
		{
			if (unit is Character)
			{
				var chr = (Character)unit;
				var clss = chr.Archetype.Class;
				var lvl = chr.Level;
				var agil = chr.Agility;
				var str = unit.Strength;

				chr.MeleeAttackPower = clss.CalculateMeleeAP(lvl, str, agil);
			}

			unit.UpdateMainDamage();
			unit.UpdateOffHandDamage();
		}

		internal static void UpdateRangedAttackPower(this Unit unit)
		{
			if (unit is Character)
			{
				var chr = (Character)unit;
				var clss = chr.Archetype.Class;
				var lvl = chr.Level;
				var agil = chr.Agility;
				var str = unit.Strength;

				var val = clss.CalculateRangedAP(lvl, str, agil);

				var apBonus = unit.IntMods[(int)StatModifierInt.RangedAttackPowerByPercentOfIntellect];
				if (apBonus > 0)
				{
					val += (apBonus * unit.Intellect + 50) / 100;
				}
				chr.RangedAttackPower = val;
			}
			unit.UpdateRangedDamage();
		}

		internal static void UpdateBlockChance(this Unit unit)
		{
			var chr = unit as Character;
			if (chr == null)
			{
				return;
			}

			var inv = chr.Inventory;
			if (inv == null)
			{
				// called too early
				return;
			}

			var shield = inv[InventorySlot.OffHand];
			var blockValue = 0;
			var blockChance = 0f;

			if (shield != null && shield.Template.InventorySlotType == InventorySlotType.Shield)
			{
				blockChance = chr.IntMods[(int)StatModifierInt.BlockChance];

				blockValue = 5 + (int)shield.Template.BlockValue + (int)blockChance;

				// + block from block rating
				blockChance += chr.GetCombatRatingMod(CombatRating.Block) / GameTables.GetCRTable(CombatRating.Block)[chr.Level - 1];
			}

			blockValue += chr.Strength / 2 - 10;
			blockValue = GetMultiMod(chr.FloatMods[(int)StatModifierFloat.BlockValue], blockValue);
			chr.BlockValue = (uint)blockValue;
			chr.BlockChance = blockChance;
		}

		internal static void UpdateSpellCritChance(this Character chr)
		{
			for (var school = DamageSchool.Physical; school < DamageSchool.Count; school++)
			{
				var chance = chr.GetCombatRatingMod(CombatRating.SpellCritChance) /
						  GameTables.GetCRTable(CombatRating.SpellCritChance)[chr.Level - 1];
				chance += chr.Archetype.Class.CalculateMagicCritChance(chr.Level, chr.Intellect);
				chance += chr.GetSpellCritMod(school);
				chr.SetSpellCritChance(school, chance);
			}
		}

		internal static void UpdateParryChance(this Unit unit)
		{
			var chr = unit as Character;
			if (chr != null)
			{
				float parryChance = 0;
				parryChance += 5f + chr.Archetype.Class.CalculateParry(chr.Level, (chr.GetCombatRatingMod(CombatRating.Parry)), chr.Strength);
				parryChance += unit.IntMods[(int)StatModifierInt.ParryChance];
				chr.ParryChance = parryChance;
			}
		}

		#region Damages and Speeds
		/// <summary>
		/// Weapon changed
		/// </summary>
		internal static void UpdateDamage(this Character unit, InventorySlot slot)
		{
			if (slot == InventorySlot.MainHand)
			{
				unit.UpdateMainDamage();
				unit.UpdateMainAttackTime();
			}
			else if (slot == InventorySlot.OffHand)
			{
				unit.UpdateOffHandDamage();
				unit.UpdateOffHandAttackTime();
			}
			else if (slot == InventorySlot.ExtraWeapon || slot == InventorySlot.Invalid)	// ranged & ammo
			{
				unit.UpdateRangedDamage();
				unit.UpdateRangedAttackTime();
			}
		}

		internal static void UpdateAllDamages(this Unit unit)
		{
			unit.UpdateMainDamage();
			unit.UpdateOffHandDamage();
			unit.UpdateRangedDamage();
		}

		/// <summary>
		/// Re-calculates the mainhand melee damage of this Unit
		/// References:
		/// http://www.wowwiki.com/Attack_power
		/// </summary>
		internal static void UpdateMainDamage(this Unit unit)
		{
			var apBonus = (unit.TotalMeleeAP * unit.MainHandAttackTime) / 14000;
			//if (unit is Character)
			//{
			//    var skillId = weapon.Skill;
			//    if (skillId != SkillId.None)
			//    {
			//        var skill = ((Character)unit).Skills.GetValue(skillId);

			//    }
			//}

			float minDam = 0;
			float maxDam = 0;
			var weapon = unit.MainWeapon;
			foreach (var dmg in weapon.Damages)
			{
				minDam += dmg.Minimum;
				maxDam += dmg.Maximum;
			}

			unit.MinDamage = minDam + apBonus;
			unit.MaxDamage = maxDam + apBonus;
		}

		/// <summary>
		/// Re-calculates the offhand damage of this Unit
		/// See http://www.wowwiki.com/Attack_power
		/// </summary>
		internal static void UpdateOffHandDamage(this Unit unit)
		{
			if (unit.OffHandWeapon != null)
			{
				var apBonus = (unit.TotalMeleeAP * unit.OffHandAttackTime) / 14000;
				var min = (unit.OffHandWeapon.Damages.TotalMin() + apBonus) / 2;
				var max = (unit.OffHandWeapon.Damages.TotalMax() + apBonus) / 2;
				if (unit is Character)
				{
					var bonus = ((Character)unit).OffhandDmgPctMod;
					min = ((min * bonus) + 50) / 100; // rounded
					max = ((max * bonus) + 50) / 100; // rounded
				}
				unit.MinOffHandDamage = min;
				unit.MaxOffHandDamage = max;
			}
			else
			{
				unit.MinOffHandDamage = 0f;
				unit.MaxOffHandDamage = 0f;
			}
		}

		internal static void UpdateRangedDamage(this Unit unit)
		{
			var apBonus = unit.TotalRangedAP / 14 * unit.RangedAttackTime / 1000;

			var weapon = unit.RangedWeapon;
			if (weapon != null && weapon.IsRanged)
			{
				Item ammo;
				if (unit is Character)
				{
					ammo = ((Character)unit).Inventory.Ammo;
				}
				else
				{
					ammo = null;
				}
				unit.MinRangedDamage = weapon.Damages.TotalMin() + apBonus + (ammo != null ? ammo.Damages.TotalMin() : 0);
				unit.MaxRangedDamage = weapon.Damages.TotalMax() + apBonus + (ammo != null ? ammo.Damages.TotalMax() : 0);
			}
			else
			{
				unit.MinRangedDamage = 0f;
				unit.MaxRangedDamage = 0f;
			}
		}

		internal static void UpdateAllAttackTimes(this Unit unit)
		{
			unit.UpdateMainAttackTime();
			unit.UpdateOffHandAttackTime();
			unit.UpdateRangedAttackTime();
		}

		/// <summary>
		/// Re-calculates the mainhand melee damage of this Unit
		/// </summary>
		internal static void UpdateMainAttackTime(this Unit unit)
		{
			var baseTime = unit.MainWeapon.AttackTime;
			baseTime = GetMultiMod(unit.FloatMods[(int)StatModifierFloat.AttackTime], baseTime);
			if (baseTime < 30)
			{
				baseTime = 30;
			}
			unit.MainHandAttackTime = baseTime;
		}

		/// <summary>
		/// Re-calculates the offhand damage of this Unit
		/// </summary>
		internal static void UpdateOffHandAttackTime(this Unit unit)
		{
			var weapon = unit.OffHandWeapon;
			if (weapon != null)
			{
				var baseTime = weapon.AttackTime;
				baseTime = GetMultiMod(unit.FloatMods[(int)StatModifierFloat.AttackTime], baseTime);
				unit.OffHandAttackTime = baseTime;
			}
			else
			{
				unit.OffHandAttackTime = 0;
			}
		}

		internal static void UpdateRangedAttackTime(this Unit unit)
		{
			var weapon = unit.RangedWeapon;
			if (weapon != null && weapon.IsRanged)
			{
				var baseTime = weapon.AttackTime;
				baseTime = GetMultiMod(unit.FloatMods[(int)StatModifierFloat.AttackTime], baseTime);
				unit.RangedAttackTime = baseTime;
			}
			else
			{
				unit.RangedAttackTime = 0;
			}
		}
		#endregion

		// TODO: Check if we get the correct CR table
		internal static void UpdateCritChance(this Unit unit)
		{
			var chr = unit as Character;
			if (chr != null)
			{
				float critChance = 0;
				// Crit chance from crit rating
				critChance += chr.GetCombatRatingMod(CombatRating.MeleeCritChance) /
							  GameTables.GetCRTable(CombatRating.MeleeCritChance)[chr.Level - 1];

				var rangedCritChance = chr.GetCombatRatingMod(CombatRating.RangedCritChance) /
							  GameTables.GetCRTable(CombatRating.RangedCritChance)[chr.Level - 1];

				// Crit chance from agility
				rangedCritChance += ((Character)unit).Archetype.Class.CalculateRangedCritChance(unit.Level, unit.Agility);
				rangedCritChance += unit.IntMods[(int)StatModifierInt.RangedCritChance];
				rangedCritChance = GetMultiMod(unit.FloatMods[(int)StatModifierFloat.CritChance], rangedCritChance);

				critChance += ((Character)unit).Archetype.Class.CalculateMeleeCritChance(unit.Level, unit.Agility);
				critChance += unit.IntMods[(int)StatModifierInt.CritChance];
				critChance = GetMultiMod(unit.FloatMods[(int)StatModifierFloat.CritChance], critChance);

				chr.CritChanceMeleePct = critChance;
				chr.CritChanceRangedPct = rangedCritChance;
				chr.CritChanceOffHandPct = critChance;
			}
		}

		internal static void UpdateDodgeChance(this Unit unit)
		{
			var chr = unit as Character;
			if (chr != null)
			{
				float dodgeChance = 0;
				if (chr.Agility == 0)
				{
					return; // too early
				}
				dodgeChance += (chr.GetCombatRatingMod(CombatRating.Dodge) / GameTables.GetCRTable(CombatRating.Dodge)[chr.Level - 1]);
				dodgeChance += ((Character)unit).Archetype.Class.CalculateDodge(unit.Level, unit.Agility,
																				 unit.BaseStats[(int)StatType.Agility],
																				 (int)chr.Skills.GetValue(SkillId.Defense),
																				 chr.GetCombatRatingMod(CombatRating.Dodge),
																				 chr.GetCombatRatingMod(CombatRating.DefenseSkill)
																				 );
				dodgeChance += unit.IntMods[(int)StatModifierInt.DodgeChance];
				chr.DodgeChance = dodgeChance;
			}
		}

		/// <summary>
		/// Increases the defense skill according to your defense rating
		/// Updates Dodge and Parry chances
		/// </summary>
		/// <param name="unit"></param>
		internal static void UpdateDefense(this Unit unit)
		{
			var chr = unit as Character;
			if (chr != null)
			{
				var defense = chr.GetCombatRatingMod(CombatRating.DefenseSkill) /
							  GameTables.GetCRTable(CombatRating.DefenseSkill)[chr.Level - 1];

				//chr.Defense = chr.Skills[SkillId.Defense].ActualValue + defense;
				chr.Defense = (uint)defense;
				UpdateDodgeChance(unit);
				UpdateParryChance(unit);
			}
		}

		internal static void UpdateModel(this Unit unit)
		{
			unit.BoundingRadius = unit.Model.BoundingRadius * unit.ScaleX;
		}

		internal static void UpdateMeleeHitChance(this Unit unit)
		{
			float hitChance;
			hitChance = unit.IntMods[(int) StatModifierInt.HitChance];
			if(unit is Character)
			{
				var chr = unit as Character;

				hitChance += chr.GetCombatRatingMod(CombatRating.MeleeHitChance)/
				             GameTables.GetCRTable(CombatRating.MeleeHitChance)[chr.Level - 1];
				chr.HitChance = hitChance;
			}
		}

		internal static void UpdateRangedHitChance(this Unit unit)
		{
			float hitChance;
			hitChance = unit.IntMods[(int)StatModifierInt.HitChance];
			if (unit is Character)
			{
				var chr = unit as Character;

				hitChance += chr.GetCombatRatingMod(CombatRating.RangedHitChance) /
							 GameTables.GetCRTable(CombatRating.RangedHitChance)[chr.Level - 1];
				chr.HitChance = hitChance;
			}
		}

		internal static void UpdateExpertise(this Unit unit)
		{
			if(unit is Character)
			{
				var chr = unit as Character;
				var expertise = (uint)chr.IntMods[(int) StatModifierInt.Expertise];
				expertise += (uint)(chr.GetCombatRatingMod(CombatRating.Expertise)/GameTables.GetCRTable(CombatRating.Expertise)[chr.Level - 1]);
				chr.Expertise = expertise;
			}
		}
		//static int ApplyMultiMod(ModifierMulti mod, int value)
		//{
		//    var modValue = unit.MultiplierMods[(int)mod];
		//    if (modValue > 0) {
		//        // increase
		//        value += (int)(value * modValue);
		//    }
		//    else {
		//        // decrease
		//        value = (int)(value / (1f - modValue));	// (-mod) is a positive number
		//    }
		//    return value;
		//}

		/// <summary>
		/// Applies a percent modifier to the given value:
		/// 0 means unchanged; +x means multiply with x; -x means divide by (1 - x)
		/// </summary>
		public static int GetMultiMod(float modValue, int value)
		{
			return (int)(value * (1 + modValue) + 0.5f);
		}

		public static float GetMultiMod(float modValue, float value)
		{
			return value * (1 + modValue);
		}

		#region Generic Change methods

		/// <summary>
		/// Changes a flat modifier
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="mod"></param>
		/// <param name="delta"></param>
		public static void ChangeModifier(this Unit unit, StatModifierInt mod, int delta)
		{
			unit.IntMods[(int)mod] += delta;
			if (FlatIntModHandlers[(int)mod] != null)
			{
				FlatIntModHandlers[(int)mod](unit);
			}
		}

		/// <summary>
		/// Changes a multiplier modifier
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="mod"></param>
		/// <param name="delta"></param>
		public static void ChangeModifier(this Unit unit, StatModifierFloat mod, float delta)
		{
			unit.FloatMods[(int)mod] += delta;
			if (MultiModHandlers[(int)mod] != null)
			{
				MultiModHandlers[(int)mod](unit);
			}
		}

		//public static void SetModifier(this Unit unit, ModifierBase mod, int value)
		//{
		//    unit.BaseMods[(int)mod] = value;
		//    BaseModHandlers[(int)mod](unit);
		//}

		//public static void ChangeModifier(this Unit unit, ModifierBase mod, int delta)
		//{
		//    unit.BaseMods[(int)mod] += delta;
		//    BaseModHandlers[(int)mod](unit);
		//}

		//public static void ChangeModifier(this Unit unit, ModifierFlatFloat mod, float delta)
		//{
		//    unit.FlatModsFloat[(int)mod] += delta;
		//    FlatFloatHandlers[(int)mod](unit);
		//}
		#endregion
	}
}