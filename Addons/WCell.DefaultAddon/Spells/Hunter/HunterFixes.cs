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
			SpellHandler.Apply(spell =>
			{
				spell.GetAuraEffect(AuraType.PeriodicTriggerSpell).ImplicitTargetA = ImplicitTargetType.SingleEnemy;
			},
			SpellId.ClassSkillTameBeast);

			// Only one Aspect can be active at a time
			AuraHandler.AddAuraGroup(SpellLineId.HunterAspectOfTheBeast, SpellLineId.HunterAspectOfTheCheetah,
				SpellLineId.HunterAspectOfTheDragonhawk, SpellLineId.HunterAspectOfTheHawk, SpellLineId.HunterAspectOfTheMonkey,
				SpellLineId.HunterAspectOfThePack, SpellLineId.HunterAspectOfTheViper, SpellLineId.HunterAspectOfTheWild);

			// Only one Sting per Hunter can be active on any one target
			AuraHandler.AddAuraGroup(SpellLineId.HunterSurvivalWyvernSting, SpellLineId.HunterSerpentSting,
				SpellLineId.HunterScorpidSting, SpellLineId.HunterViperSting, SpellLineId.HunterSerpentSting);

			// Expose Weakness aura applied on the target  - Seems the spell has changed
			//SpellHandler.Apply(spell => spell.Effects[0].ImplicitTargetA = ImplicitTargetType.SingleEnemy,
			//                   SpellId.ExposeWeakness_2);
		}
	}
}
