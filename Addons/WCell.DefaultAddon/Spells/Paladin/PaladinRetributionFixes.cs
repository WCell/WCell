using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinRetributionFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Eye for an Eye should reflect damage
			SpellLineId.PaladinRetributionEyeForAnEye.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.None;
				spell.GetEffect(AuraType.Dummy).AuraEffectHandlerCreator = () => new ReflectDamagePctHandler();
			});
		}
	}

	/// <summary>
	/// Reflects damage, but caps at 50% of wearer's max health
	/// </summary>
	public class ReflectDamagePctHandler : AttackEventEffectHandler
	{
		public ReflectDamagePctHandler()
		{
		}

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
			if (!action.IsCritical)
			{
				return;
			}

			var max = action.Victim.MaxHealth / 2;

			// reflect % damage
			var effect = m_spellEffect;
			action.Victim.AddMessage(() =>
			{
				if (action.Victim.MayAttack(action.Attacker))
				{
					var dmg = (EffectValue*action.Damage + 50) / 100;
					if (dmg > max)
					{
						dmg = max;
					}
					action.Attacker.DoSpellDamage(action.Victim, effect, dmg);
				}
			});
		}
	}
}
