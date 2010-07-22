using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Do flat damage to any attacker
	/// </summary>
	public class DamageShieldEffectHandler : AttackEventEffectHandler
	{
		public override void OnBeforeAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnAttack(DamageAction action)
		{
			// do nothing
		}

		public override void OnDefend(DamageAction action)
		{
			action.Victim.AddMessage(() =>
			{
				if (action.Victim.MayAttack(action.Attacker))
				{
					// TODO: Add mods to damage?
					action.Attacker.DoSpellDamage(action.Victim, SpellEffect, EffectValue);
				}
			});
		}
	}
}