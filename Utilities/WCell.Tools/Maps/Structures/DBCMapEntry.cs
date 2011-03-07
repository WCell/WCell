using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Core;
using WCell.Core.DBC;
using WCell.RealmServer;

namespace WCell.Tools.Maps.Structures
{
	public static class DBCMapReader
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		private static ListDBCReader<DBCMapEntry, DBCMapConverter> reader;

		public static List<DBCMapEntry> GetMapEntries()
		{
			if (reader == null)
			{
				reader = new ListDBCReader<DBCMapEntry, DBCMapConverter>(
					RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_MAPS));
			}

			return reader.EntryList;
		}
	}

	public class DBCMapEntry
	{
		public uint Id;
		public string MapDirName;
	}

	public class DBCMapConverter : AdvancedDBCRecordConverter<DBCMapEntry>
	{
		public override DBCMapEntry ConvertTo(byte[] rawData, ref int id)
		{
			int i = 0;
			var mapEntry = new DBCMapEntry();
			id = (int)(mapEntry.Id = GetUInt32(rawData, i++)); // 0
			mapEntry.MapDirName = GetString(rawData, i++); // 1

			return mapEntry;
		}
	}
}