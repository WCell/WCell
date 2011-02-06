using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs.Pets;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// TODO: Move everything Unit-related from UnitUpdates in here
	/// </summary>
	public partial class Unit
	{
		/// <summary>
		/// Amount of mana to be added per point of Intelligence
		/// </summary>
		public static int ManaPerIntelligence = 15;

		/// <summary>
		/// Amount of heatlh to be added per point of Stamina
		/// </summary>
		public static int HealthPerStamina = 10;

		/// <summary>
		/// Amount of armor to be added per point of Agility
		/// </summary>
		public static int ArmorPerAgility = 2;

		#region Str, Sta, Agi, Int, Spi
		protected internal virtual void UpdateStrength()
		{
			var str = GetBaseStatValue(StatType.Strength) + StrengthBuffPositive + StrengthBuffNegative;
			//str = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Strength], str);
			SetInt32(UnitFields.STAT0, str);

			this.UpdateBlockChance();
			this.UpdateAllAttackPower();
		}

		protected internal virtual void UpdateStamina()
		{
			var stam = GetBaseStatValue(StatType.Stamina) + StaminaBuffPositive + StaminaBuffNegative;

			SetInt32(UnitFields.STAT2, stam);

			UpdateMaxHealth();
		}

		internal void UpdateAgility()
		{
			var oldAgil = Agility;
			var agil = GetBaseStatValue(StatType.Agility) + AgilityBuffPositive + AgilityBuffNegative;
			//agil = GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Agility], agil);
			SetInt32(UnitFields.STAT1, agil);

			ModBaseResistance(DamageSchool.Physical, (agil - oldAgil) * ArmorPerAgility);	// armor

			this.UpdateDodgeChance();
			this.UpdateCritChance();
			this.UpdateAllAttackPower();
		}

		protected internal virtual void UpdateIntellect()
		{
			var intel = GetBaseStatValue(StatType.Intellect) + IntellectBuffPositive + IntellectBuffNegative;
			//intel = intel < 0 ? 0 : GetMultiMod(unit.MultiplierMods[(int)StatModifierFloat.Intellect], intel);
			SetInt32(UnitFields.STAT3, intel);

			UpdateMaxPower();
		}

		protected internal virtual void UpdateSpirit()
		{
			var spirit = GetBaseStatValue(StatType.Spirit) + SpiritBuffPositive + SpiritBuffNegative;

			SetInt32(UnitFields.STAT4, spirit);

			this.UpdateNormalHealthRegen();

			// We don't need to call when we are still in the process of loading
			if (Intellect != 0)
			{
				this.UpdatePowerRegen();
			}
		}

		protected internal virtual void UpdateStat(StatType stat)
		{
			switch (stat)
			{
				case StatType.Strength:
					UpdateStrength();
					break;
				case StatType.Agility:
					UpdateAgility();
					break;
				case StatType.Stamina:
					UpdateStamina();
					break;
				case StatType.Intellect:
					UpdateIntellect();
					break;
				case StatType.Spirit:
					UpdateSpirit();
					break;
			}
		}
		#endregion

		#region Health & Power
		protected internal virtual void UpdateMaxHealth()
		{
			var stamina = Stamina;
			var uncontributed = StaminaWithoutHealthContribution;
			var stamBonus = Math.Max(stamina, uncontributed) + (Math.Max(0, stamina - uncontributed) * HealthPerStamina);

			var value = BaseHealth + stamBonus + MaxHealthModFlat;
			value += (int)(value * MaxHealthModScalar + 0.5f);

			SetInt32(UnitFields.MAXHEALTH, value);

			this.UpdateHealthRegen();
		}

		/// <summary>
		/// Amount of mana, contributed by intellect
		/// </summary>
		protected internal virtual int IntellectManaBonus
		{
			get { return Intellect; }
		}

		protected internal void UpdateMaxPower()
		{
			var value = BasePower + IntMods[(int)StatModifierInt.Power];
			if (PowerType == PowerType.Mana)
			{
				value += IntellectManaBonus;
			}
			value += (value * IntMods[(int)StatModifierInt.PowerPct] + 50) / 100;
			if (value < 0)
			{
				value = 0;
			}

			MaxPower = value;

			this.UpdatePowerRegen();
		}
		#endregion
	}
}
