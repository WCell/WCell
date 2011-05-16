using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Core.ClientDB;
using NLog;

namespace WCell.RealmServer.Talents
{
	public class GlyphSlotEntry
	{
		public uint Id;
		public uint TypeFlags;
		public uint Order;
	}

	public class GlyphPropertiesEntry
	{
		public uint Id;
		public uint SpellId;
		public uint TypeFlags;
		public uint Unk1;                                           // GlyphIconId (SpellIcon.dbc)
	}

	public class GlyphSlotConverter : ClientDBRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var entry = new GlyphSlotEntry();
			entry.Id = GetUInt32(rawData, 0);
			entry.TypeFlags = GetUInt32(rawData, 1);
			entry.Order = GetUInt32(rawData, 2);
			GlyphInfoHolder.GlyphSlots.Add(entry.Id, entry);
		}
	}

	public class GlyphPropertiesConverter : ClientDBRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var entry = new GlyphPropertiesEntry();
			entry.Id = GetUInt32(rawData, 0);
			entry.SpellId = GetUInt32(rawData, 1);
			entry.TypeFlags = GetUInt32(rawData, 2);
			entry.Unk1 = GetUInt32(rawData, 3);
			GlyphInfoHolder.GlyphProperties.Add(entry.Id, entry);
		}
	}
	public static class GlyphInfoHolder
	{
		public static Dictionary<uint, GlyphSlotEntry> GlyphSlots = new Dictionary<uint, GlyphSlotEntry>();
		public static Dictionary<uint, GlyphPropertiesEntry> GlyphProperties = new Dictionary<uint, GlyphPropertiesEntry>();
		public static void Init()
		{
			new DBCReader<GlyphSlotConverter>(RealmServerConfiguration.GetDBCFile(ClientDBConstants.DBC_GLYPHSLOT));
			new DBCReader<GlyphPropertiesConverter>(RealmServerConfiguration.GetDBCFile(ClientDBConstants.DBC_GLYPHPROPERTIES));
		}

		public static GlyphPropertiesEntry GetPropertiesEntryForGlyph(uint glyphid)
		{
			return GlyphProperties[glyphid];
		}
		public static GlyphSlotEntry GetGlyphSlotEntryForGlyphSlotId(uint id)
		{
			return GlyphSlots[id];
		}
	}

}
