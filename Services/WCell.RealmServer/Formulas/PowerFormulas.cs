using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Formulas
{
	public delegate int PowerCalculator(Unit unit);

	/// <summary>
	/// Determines the amount of base power per level and regenaration speed of all powers
	/// </summary>
	public static class PowerFormulas
	{
		public static readonly PowerCalculator[] PowerRegenCalculators = new PowerCalculator[(int)PowerType.End];
		public static readonly PowerCalculator[] BasePowerForLevelCalculators = new PowerCalculator[(int)PowerType.End];

		public static void SetPowerRegenCalculator(PowerType type, PowerCalculator calc)
		{
			PowerRegenCalculators[(int)type] = calc;
		}

		public static void SetBasePowerCalculator(PowerType type, PowerCalculator calc)
		{
			BasePowerForLevelCalculators[(int)type] = calc;
		}

		static PowerFormulas()
		{
			// TODO: Focus, RunicPower, Runes
			SetPowerRegenCalculator(PowerType.Mana, CalculateManaRegen);
			SetPowerRegenCalculator(PowerType.Rage, CalculateRageRegen);
			SetPowerRegenCalculator(PowerType.Energy, CalculateEnergyRegen);
			SetPowerRegenCalculator(PowerType.Focus, CalculateFocusRegen);
			SetPowerRegenCalculator(PowerType.RunicPower, CalculateRunicPowerRegen);
			SetPowerRegenCalculator(PowerType.Runes, CalculateRuneRegen);

			SetBasePowerCalculator(PowerType.Mana, GetPowerForLevelDefault);
			SetBasePowerCalculator(PowerType.Rage, GetRageForLevel);
			SetBasePowerCalculator(PowerType.Energy, GetEnergyForLevel);
			SetBasePowerCalculator(PowerType.Focus, GetFocusForLevel);
			SetBasePowerCalculator(PowerType.RunicPower, GetRunicPowerForLevel);
			SetBasePowerCalculator(PowerType.Runes, GetRunesForLevel);
		}

		#region Standard Regen Formulas
		public static int GetPowerRegen(Unit unit)
		{
			var calc = PowerRegenCalculators[(int)unit.PowerType];
			if (calc == null)
			{
				return 0;
			}
			return calc(unit);
		}

		/// <summary>
		/// Calculates the amount of power regeneration for the class at a specific level, Intellect and Spirit.
		/// Changed in 3.1, overrides for casters are redundant.
		/// </summary>
		public static int CalculateManaRegen(Unit unit)
		{
			// default mana generation
			//return (10f + (spirit / 7f));
			var regen = (float)(0.001f + (float)Math.Sqrt(unit.Intellect) * unit.Spirit * GameTables.BaseRegen[unit.Level]) * Unit.RegenTickMultiplier;
			if (unit.IsInCombat)
			{
				regen = (regen * unit.ManaRegenPerTickInterruptedPct) / 100;
			}
			return (int)regen;
		}

		/// <summary>
		/// 1 Rage to the client is a value of 10
		/// </summary>
		public static int CalculateRageRegen(Unit unit)
		{
			if (unit.IsInCombat)
			{
				return 0;
			}
			return -50;
		}


		public static int CalculateEnergyRegen(Unit unit)
		{
			return 20;
		}

		public static int CalculateRunicPowerRegen(Unit unit)
		{
			return 4 + (unit.Spirit / 5);
		}

		private static int CalculateRuneRegen(Unit unit)
		{
			return 0;
		}

		public static int CalculateFocusRegen(Unit unit)
		{
			return 5;
		}
		#endregion


		#region BasePower per Level Values
		public static int GetPowerForLevel(Unit unit)
		{
			var calc = BasePowerForLevelCalculators[(int) unit.PowerType];
			if (calc == null)
			{
				calc = GetPowerForLevelDefault;
			}
			return calc(unit);
		}

		public static int GetPowerForLevelDefault(Unit unit)
		{
			return unit.GetBaseClass().GetLevelSetting(unit.Level).Mana;
		}

		public static int GetRageForLevel(Unit unit)
		{
			return 1000;
		}

		public static int GetRunicPowerForLevel(Unit unit)
		{
			return 1000;
		}

		public static int GetRunesForLevel(Unit unit)
		{
			return 6;
		}

		public static int GetFocusForLevel(Unit unit)
		{
			return 5 * unit.Level;
		}

		public static int GetEnergyForLevel(Unit unit)
		{
			return 100;
		}

		public static int GetZero(Unit unit)
		{
			return 0;
		}
		#endregion
	}
}
