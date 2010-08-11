using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;
using WCell.RealmServer.Spells.Effects;

namespace WCell.Addons.Default.Spells.Warrior
{
	public static class WarriorFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixWarrior()
		{
			// Charge doesn't generate rage
			SpellLineId.WarriorCharge.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				if (effect != null)
				{
					effect.EffectType = SpellEffectType.Energize;
					//effect.SpellEffectHandlerCreator =
					//(cast, efct) => new EnergizeEffectHandler(cast, efct);
					effect.MiscValue = (int)PowerType.Rage;
				}
			});

			// Slam has the wrong effect type
			SpellLineId.WarriorSlam.Apply(spell =>
			{
				var effect = spell.GetEffect(SpellEffectType.Dummy);
				if (effect != null)
				{
					effect.EffectType = SpellEffectType.NormalizedWeaponDamagePlus;
				}
			});

			// Enrage and Wrecking Crew proc effects don't stack
			AuraHandler.AddAuraGroup(
				SpellId.EffectEnrageRank1_3, SpellId.EffectEnrageRank2_3, SpellId.EffectEnrageRank3_2,
				SpellId.EffectEnrageRank4_2, SpellId.EffectEnrageRank5_2,
				SpellId.EffectEnrageRank1, SpellId.EffectEnrageRank2, SpellId.EffectEnrageRank3,
				SpellId.EffectEnrageRank4, SpellId.EffectEnrageRank5);

			// Intimidating Shout should not have a single enemy target
			SpellLineId.WarriorChallengingShout.Apply(spell => spell.ForeachEffect(
				effect => { effect.ImplicitTargetA = ImplicitTargetType.AllEnemiesAroundCaster; }));

			// thunder clap should add about 15% AP to damage
			SpellLineId.WarriorThunderClap.Apply(spell => {
				var effect = spell.AddEffect(SpellEffectType.Dummy, ImplicitTargetType.AllEnemiesAroundCaster);
				effect.Radius = 8;
				effect.BasePoints = 15;
				effect.SpellEffectHandlerCreator = (cast, effct) => new SchoolDamageByAPPctEffectHandler(cast, effct);
			});

			// Retaliation needs to retaliate
			SpellLineId.WarriorRetaliation.Apply(spell => {
				var effect = spell.GetEffect(AuraType.Dummy);
				effect.IsProc = true;
				effect.AuraEffectHandlerCreator = () => new RetaliationEffectHandler();
			});

			// cleave should attack two enemies
			SpellLineId.WarriorCleave.Apply(spell =>
			{
				spell.Effects[0].ChainTargets = 2;
			});
		}
	}

	class RetaliationEffectHandler : AttackEventEffectHandler
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
			// strike back
			var victim = action.Victim;
			var atk = action.Attacker;
			if (!atk.IsBehind(victim))
			{
				victim.AddMessage(() =>
				{
					if (atk.IsInWorld && victim.MayAttack(atk))
					{
						victim.Strike(atk);
					}
				});
			}
		}
	}
}