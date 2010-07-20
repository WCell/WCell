using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.Hunter
{
	public static class HunterFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixHunter()
		{
			// taming has an invalid target
			SpellHandler.Apply(
				spell => { spell.GetAuraEffect(AuraType.PeriodicTriggerSpell).ImplicitTargetA = ImplicitTargetType.SingleEnemy; },
				SpellId.ClassSkillTameBeast);

			// Only one Aspect can be active at a time
			AuraHandler.AddAuraGroup(SpellLineId.HunterAspectOfTheBeast, SpellLineId.HunterAspectOfTheCheetah,
									 SpellLineId.HunterAspectOfTheDragonhawk, SpellLineId.HunterAspectOfTheHawk,
									 SpellLineId.HunterAspectOfTheMonkey,
									 SpellLineId.HunterAspectOfThePack, SpellLineId.HunterAspectOfTheViper,
									 SpellLineId.HunterAspectOfTheWild);

			// Only one Sting per Hunter can be active on any one target
			AuraHandler.AddAuraGroup(SpellLineId.HunterSurvivalWyvernSting, SpellLineId.HunterSerpentSting,
									 SpellLineId.HunterScorpidSting, SpellLineId.HunterViperSting, SpellLineId.HunterSerpentSting);

			// Expose Weakness aura applied on the target  - Seems the spell has changed
			//SpellHandler.Apply(spell => spell.Effects[0].ImplicitTargetA = ImplicitTargetType.SingleEnemy,
			//                   SpellId.ExposeWeakness_2);
            
            
            FixMarkmanshipSpells();
            FixSurvivalSpells();


		}
        #region Markmanship
        private static void FixMarkmanshipSpells()
		{
            // Volley does incorrect damage ($RAP*0.083700 + $m1)
            SpellHandler.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.08370f;

                }, SpellId.EffectClassSkillVolleyRank1, SpellId.EffectClassSkillVolleyRank2,
                          SpellId.EffectClassSkillVolleyRank3, SpellId.EffectClassSkillVolleyRank4,
                          SpellId.EffectClassSkillVolleyRank5, SpellId.EffectClassSkillVolleyRank6
                          );

            // Arcane Shot does incorrect damage ($RAP*0.15 + $m1)
            SpellLineId.HunterArcaneShot.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.15f;
                });

            // Multi-Shot affects three targets
            SpellLineId.HunterMultiShot.Apply(spell => 
                {
                    spell.Effects[0].ImplicitTargetA = ImplicitTargetType.Chain;
                });

            SpellLineId.HunterMarksmanshipReadiness.Apply(spell =>
                {
                    spell.Effects[0].SpellEffectHandlerCreator =
                        (cast, effect) => new ReadinessHandler(cast, effect);
                });
        }
        #endregion

        #region Survival
        private static void FixSurvivalSpells()
        {
            // Moongonse Bite does incorrect damage ($AP*0.2 + $m1)
            SpellLineId.HunterMongooseBite.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.2f;
                });

            // Black Arrow does incorrect damage ($RAP*0.1 + $m1)
            SpellLineId.HunterSurvivalBlackArrow.Apply(spell =>
                {
                    spell.Effects[0].APValueFactor = 0.1f;
                });
        }
        #endregion
    }
}