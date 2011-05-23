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
				spell.GetEffect(AuraType.PeriodicTriggerSpell).ImplicitTargetA = ImplicitSpellTargetType.SingleEnemy;
			}, SpellId.ClassSkillTameBeast);

			// Only one Aspect can be active at a time
			AuraHandler.AddAuraGroup(SpellLineId.HunterAspectOfTheFox,
                                     SpellLineId.HunterAspectOfTheCheetah,
									 SpellLineId.HunterAspectOfTheHawk,
									 SpellLineId.HunterAspectOfThePack,
									 SpellLineId.HunterAspectOfTheWild);

			// Only one Sting per Hunter can be active on any one target
			AuraHandler.AddAuraGroup(SpellLineId.HunterSurvivalWyvernSting, SpellLineId.HunterSerpentSting,
									 SpellLineId.HunterSurvivalNoxiousStings, SpellLineId.HunterSerpentSting);

		}
	}
}