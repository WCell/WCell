using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;

namespace WCell.Addons.Default.Spells
{
	/// <summary>
	/// All non-Instance or battleground bound NPC spells are to be fixed here
	/// </summary>
	public static class NPCSpells
	{

		/// <summary>
		/// Most NPC spells miss basic spell information
		/// </summary>
		[Initialization(InitializationPass.Second)]
		public static void AdjustNPCSpells()
		{
			FixTotems();
			FixOthers();
		}

		private static void FixTotems()
		{
			
		}

		private static void FixOthers()
		{
            SpellHandler.Apply(spell =>
                                   {
                                       spell.SpellPower = new SpellPower {PowerCost = 100};
                                       spell.SpellCooldowns = new SpellCooldowns {CooldownTime = 2000};
                                       spell.Range = new SimpleRange(0, 10);
                                   }, SpellId.ConeOfFire);

			SpellHandler.Apply(spell =>
			{
                spell.SpellPower = new SpellPower { PowerCost = 150 };
				spell.CastDelay = 3000;
                spell.SpellCooldowns = new SpellCooldowns { CooldownTime = 6000 };
			}, SpellId.Chilled);
		}
	}
}