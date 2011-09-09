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
					action.Attacker.DealSpellDamage(action.Victim, SpellEffect, EffectValue);
				}
			});
		}
	}
}