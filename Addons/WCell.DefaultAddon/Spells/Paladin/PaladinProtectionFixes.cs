using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.Addons.Default.Spells.Paladin
{
	public static class PaladinProtectionFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{
			// Devotion Aura: Improved devotion aura needs all auras to have a 2nd effect for healing
			SpellHandler.Apply(spell =>
			{
				var firstEffect = spell.Effects[0];
				for (var i = 2; i < 3; i++)
				{
					// add up to two filler effects, so the heal effect is at the 4th place
					if (spell.Effects.Length < i)
					{
						spell.AddEffect(SpellEffectType.Dummy);
					}
				}

				if (spell.Effects.Length != 3)
				{
					throw new Exception("Spell has invalid amount of effects: " + spell);
				}

				// add heal effect as the 4th effect
				var healEffect = spell.AddEffect(SpellEffectType.ApplyGroupAura);

				healEffect.ImplicitTargetA = firstEffect.ImplicitTargetA;
				healEffect.Radius = firstEffect.Radius;
				healEffect.AuraType = AuraType.ModHealingPercent;
			}, PaladinFixes.PaladinAuras);


			// Improved devotion needs to apply the healing bonus to the 4th effect
			SpellLineId.PaladinProtectionImprovedDevotionAura.Apply(spell =>
			{
				var effect = spell.Effects[1];
				effect.MiscValue = (int)SpellModifierType.EffectValue4AndBeyond;
				effect.AddToEffectMask(PaladinFixes.PaladinAuras);	// applies to all auras
			});


			// Spiritual Attunement gives mana "equal to $s1% of the amount healed"
			SpellLineId.PaladinProtectionSpiritualAttunement.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraEffectHandlerCreator = () => new SpiritualAttunementHandler();
			});
		}
	}

	public class SpiritualAttunementHandler : AuraEffectHandler
	{
		public override void OnProc(Unit target, IUnitAction action)
		{
			if (action is HealAction)
			{
				var haction = (HealAction)action;
				var value = (haction.Value * EffectValue + 50) / 100;
				Owner.Energize(action.Attacker, value, SpellEffect);
			}
		}
	}
}
