using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Data;
using WCell.Constants.Skills;
using WCell.RealmServer.Content;
using WCell.RealmServer.Spells;
using WCell.Constants.Spells;

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
				ContentHandler.OnInvalidDBData("Invalid SpellLearnRelation: Spell {0} (#{1}) and AddSpell {2} (#{3})",
					SpellId, (uint)SpellId, AddSpellId, (uint)AddSpellId);
			}
			else
			{
				spell.AdditionallyTaughtSpells.Add(addSpell);
			}
		}
	}
}
