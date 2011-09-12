using System.Collections.Generic;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer;
using WCell.RealmServer.Spells;

namespace WCell.Tools.Spells
{
	public static class SpellFocusObjectReader
	{
		public static Dictionary<uint, string> Read()
		{
			var reader = new MappedDBCReader<Spell.SpellFocusEntry, Spell.DBCSpellFocusConverter>(
                RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_SPELLFOCUSOBJECT));
			var dict = new Dictionary<uint, string>(300);

			foreach (var entry in reader.Entries.Values)
			{
				dict.Add(entry.Id, entry.Name);
			}
			return dict;
		}
	}
}