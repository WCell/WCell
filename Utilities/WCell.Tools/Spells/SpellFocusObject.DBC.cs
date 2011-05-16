using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core;
using WCell.RealmServer;
using WCell.RealmServer.Spells;
using WCell.Core.ClientDB;

namespace WCell.Tools.Spells
{
	public static class SpellFocusObjectReader
	{
		public static Dictionary<uint, string> Read()
		{
			var reader = new MappedDBCReader<Spell.SpellFocusEntry, Spell.DBCSpellFocusConverter>(
                RealmServerConfiguration.GetDBCFile(ClientDBConstants.DBC_SPELLFOCUSOBJECT));
			var dict = new Dictionary<uint, string>(300);

			foreach (var entry in reader.Entries.Values)
			{
				dict.Add(entry.Id, entry.Name);
			}
			return dict;
		}
	}
}