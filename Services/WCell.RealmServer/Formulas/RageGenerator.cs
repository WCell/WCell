using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Formulas
{
	/// <summary>
	/// RageGenerator
	/// <see href="http://www.wowwiki.com/Formulas:Rage_generation"></see>
	/// </summary>
	public static class RageGenerator
	{
		public delegate void RageCalculator(DamageAction action);

		public static RageCalculator GenerateAttackerRage = GenerateDefaultAttackerRage;

		public static RageCalculator GenerateTargetRage = GenerateDefaultVictimRage;

		/// <summary>
		/// Rage for the attacker of an AttackAction
		/// </summary>
		public static void GenerateDefaultAttackerRage(DamageAction action)
		{
			var attacker = action.Attacker;

			// only generate Rage for white damage
			if (action.IsWeaponAttack)
			{
				double hitFactor;
				if (action.Weapon == attacker.OffHandWeapon)
				{
					hitFactor = 1.75;
				}
				else
				{
					hitFactor = 3.5;
				}
				if (action.IsCritical)
				{
					hitFactor *= 2;
				}

				hitFactor *= action.Weapon.AttackTime;

				var lvl = attacker.Level;
				var c = 0.0092f * lvl * lvl + 3.23f * lvl + 4.27f;
				var rageRight = ((15 * action.ActualDamage / (4f * c)) + (hitFactor / 2000));
				var rageLeft = 15 * action.ActualDamage / c;

				var rage = rageRight;
				if(rageRight <= rageLeft)
					rage = rageLeft;
				// Multiplied by 2 to match an approximate value, check the formula instead.
				attacker.Power += (int)(rage)*10;
			}
		}

		/// <summary>
		/// Rage for the victim of an AttackAction
		/// </summary>
		public static void GenerateDefaultVictimRage(DamageAction action)
		{
			var victim = action.Victim;

			var lvl = victim.Level;
			var c = (int)(0.0092 * lvl * lvl + 3.23f * lvl + 4.27f);			// polynomial rage co-efficient
			victim.Power += (5 / 2 * action.ActualDamage / c)*10;
		}
	}
}
