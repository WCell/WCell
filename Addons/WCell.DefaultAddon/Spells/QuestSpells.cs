using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells
{
	/// <summary>
	/// Some hardcoded fixes for quest-related Spells
	/// </summary>
	public static class QuestSpells
	{
		[Initialization(InitializationPass.Second)]
		public static void FixQuestSpells()
		{
			FixTameQuests();
		}

		static void FixTameQuests()
		{
			FixTameSpell(SpellId.TameAdultPlainstrider_2, SpellId.TameAdultPlainstrider);
			FixTameSpell(SpellId.TameLargeCragBoar, SpellId.TameLargeCragBoar_2);
			FixTameSpell(SpellId.TameIceClawBear, SpellId.TameIceClawBear_2);
			FixTameSpell(SpellId.TameSnowLeopard_2, SpellId.TameSnowLeopard);
			FixTameSpell(SpellId.TamePrairieStalker_2, SpellId.TamePrairieStalker);
			FixTameSpell(SpellId.TameSwoop_2, SpellId.TameSwoop);
			FixTameSpell(SpellId.TameDireMottledBoar_2, SpellId.TameDireMottledBoar);
			FixTameSpell(SpellId.TameSurfCrawler_2, SpellId.TameSurfCrawler);
			FixTameSpell(SpellId.TameArmoredScorpid_2, SpellId.TameArmoredScorpid);
			FixTameSpell(SpellId.TameWebwoodLurker_2, SpellId.TameWebwoodLurker);
			FixTameSpell(SpellId.TameNightsaberStalker_2, SpellId.TameNightsaberStalker);
			FixTameSpell(SpellId.TameStrigidScreecher_2, SpellId.TameStrigidScreecher);
		}

		static void FixTameSpell(SpellId id, SpellId triggerId)
		{
			// add a spell-trigger
			var spell = SpellHandler.Get(id);
			var effect = spell.AddTriggerSpellEffect(triggerId, ImplicitSpellTargetType.SingleEnemy);
			effect.Amplitude = spell.Durations.Min;
		}
	}
}