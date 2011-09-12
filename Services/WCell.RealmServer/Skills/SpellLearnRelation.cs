using WCell.Constants.Spells;
using WCell.RealmServer.Content;
using WCell.RealmServer.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.Skills
{
	[DataHolder]
	public class SpellLearnRelation : IDataHolder
	{
		public SpellId SpellId;

		public SpellId AddSpellId;

		public void FinalizeDataHolder()
		{
			var spell = SpellHandler.Get(SpellId);
			var addSpell = SpellHandler.Get(AddSpellId);
			if (spell == null || addSpell == null)
			{
				ContentMgr.OnInvalidDBData("Invalid SpellLearnRelation: Spell {0} (#{1}) and AddSpell {2} (#{3})",
					SpellId, (uint)SpellId, AddSpellId, (uint)AddSpellId);
			}
			else
			{
				spell.AdditionallyTaughtSpells.Add(addSpell);
			}
		}
	}
}