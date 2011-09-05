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
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Warrior
{
	public static class WarriorProtectionFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixIt()
		{

			// Shield Spec is proc'ed when owner dodges, blocks or parries
			SpellLineId.WarriorProtectionShieldSpecialization.Apply(spell =>
				{
					spell.ProcHitFlags = ProcHitFlags.Dodge | ProcHitFlags.Block | ProcHitFlags.Parry;
				});

			// Gag Order has a ProcTriggerSpell effect and is only trigged by bash and throw
			SpellLineId.WarriorProtectionGagOrder.Apply(spell =>
			{
				spell.ProcTriggerFlags = ProcTriggerFlags.DoneMeleeSpell | ProcTriggerFlags.DoneRangedSpell;

				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraType = AuraType.ProcTriggerSpell;
				effect.SetAffectMask(SpellLineId.WarriorShieldBash, SpellLineId.WarriorHeroicThrow);
			});

			// Concussion Blow deals AP based school damage
			SpellLineId.WarriorProtectionConcussionBlow.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				effect.SpellEffectHandlerCreator = (cast, eff) => new SchoolDamageByAPPctEffectHandler(cast, eff);
				effect = spell.GetEffect(SpellEffectType.SchoolDamage);
			});

			// Last Stand has a Dummy and does not apply an Aura (through triggering the Aura spell)
			SpellEffect lastStandEffect = null;
			SpellLineId.WarriorProtectionLastStand.Apply(spell =>
			{
				lastStandEffect = spell.GetEffect(SpellEffectType.Dummy);
				lastStandEffect.EffectType = SpellEffectType.TriggerSpell;
				lastStandEffect.TriggerSpellId = SpellId.ClassSkillLastStand;
			});
			SpellHandler.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ModIncreaseHealth);
				effect.AuraType = AuraType.ModIncreaseHealthPercent;	// increase health in %
				effect.BasePoints = lastStandEffect.BasePoints;			// set correct value
				effect.DiceSides = lastStandEffect.DiceSides;
			}, SpellId.ClassSkillLastStand);

			// Safe Guard should only affect Intervene
			SpellLineId.WarriorProtectionSafeguard.Apply(spell =>
			{
				spell.Effects.First().AffectMask = SpellHandler.Get(SpellId.ClassSkillIntervene).SpellClassMask;
			});

			// S&B should only affect Devastate and Revenge
			SpellLineId.WarriorProtectionSwordAndBoard.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.ProcTriggerSpell);
				effect.AddToAffectMask(SpellLineId.WarriorRevenge, SpellLineId.WarriorProtectionDevastate);
			});

			// Shockwave needs to deal damage in % of AP
			SpellLineId.WarriorProtectionShockwave.Apply(spell =>
			{
				var stunEffect = spell.GetEffect(AuraType.ModStun);
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				effect.ImplicitTargetA = stunEffect.ImplicitTargetA;
				effect.Radius = stunEffect.Radius;

				effect.SpellEffectHandlerCreator = (cast, effct) => new SchoolDamageByAPPctEffectHandler(cast, effct);
			});

			// Damage Shield should reflect damage on block
			SpellLineId.WarriorProtectionDamageShield.Apply(spell =>
			{
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.AuraEffectHandlerCreator = () => new WarriorDamageShieldHandler();
			});
		}

		class WarriorDamageShieldHandler : AttackEventEffectHandler
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
				// inflict damage upon block
				if (action.Blocked > 0)
				{
					var dmg = (action.Blocked * EffectValue + 50) / 100;
					var victim = action.Victim;
					var attacker = action.Attacker;

					victim.AddMessage(() =>
					{
						if (victim.MayAttack(attacker))
						{
							attacker.DealSpellDamage(victim, SpellEffect, dmg);
						}
					});
				}
			}
		}
	}
}
