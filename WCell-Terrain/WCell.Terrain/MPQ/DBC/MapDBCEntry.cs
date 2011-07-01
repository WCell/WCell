using System.Collections.Generic;
using System.IO;
using System.Linq;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.Terrain.MPQ.DBC
{
	/// <summary>
	/// Represents an entry in Map.dbc
	/// </summary>
	public class MapInfo
	{
		public MapId Id; // 0
		public string InternalName; // 1
		public MapType MapType; // 2
		public uint Unknown1; // 3
		public bool HasTwoSides; // 4
		public string Name; // 5-20
		public uint UnknownFlags1; // 21
		public int AreaTable; // 22
		public string AllianceText; // 23-38
		public uint UnknownFlags2; // 39
		public string HordeText; // 40-55
		public uint UnknownFlags3; // 56
		public int LoadingScreen; // 57
		public float BattlefieldMapIconScale; // 58
		/// <summary>
		/// Points to InternalName column, -1 if no parent map
		/// </summary>
		public int ParentMap; // 59
		public float EntranceX; // 60
		public float EntranceY; // 61
		public int TimeOfDayOverride; // 62
		/// <summary>
		/// 0 - Vanilla, 1 - TBC, 2 - WotLK
		/// </summary>
		public int ExpansionId; // 63
		/// <summary>
		/// In seconds
		/// </summary>
		public uint RaidResetTimer; // 64
		/// <summary>
		/// Values: 0, 5, 10, 20, 25, 40
		/// </summary>
		public uint MaxNumberOfPlayers; // 65

		private static MappedDBCReader<MapInfo, DBCMapEntryConverter> dbcMapReader;
		public static Dictionary<int, MapInfo> GetMapInfoMap()
		{
			if (dbcMapReader == null)
			{
				var dbcPath = Path.Combine(WCellTerrainSettings.DBCDir, WCellTerrainSettings.MapDBCName);
				dbcMapReader = new MappedDBCReader<MapInfo, DBCMapEntryConverter>(dbcPath);
			}
			return dbcMapReader.Entries;
		}

		public static List<MapInfo> GetMapEntries()
		{
			return GetMapInfoMap().Values.ToList();
		}

		public static MapInfo GetMapInfo(MapId map)
		{
			MapInfo info;
			GetMapInfoMap().TryGetValue((int) map, out info);
			return info;
		}
	}

	public class DBCMapEntryConverter : AdvancedDBCRecordConverter<MapInfo>
	{
		public override MapInfo ConvertTo(byte[] rawData, ref int id)
		{
			var i = 0;
			var info = new MapInfo
			{
				Id = (MapId)(id = GetInt32(rawData, i++)),
				InternalName = GetString(rawData, WCellTerrainSettings.DefaultLocale, i++),
				MapType = (MapType)GetInt32(rawData, i++),
				Unknown1 = GetUInt32(rawData, i++),
				HasTwoSides = (GetUInt32(rawData, i++) == 1),
				Name = GetString(rawData, WCellTerrainSettings.DefaultLocale, i++)
			};

			i = 21;
			info.UnknownFlags1 = GetUInt32(rawData, i++);
			info.AreaTable = GetInt32(rawData, i++);
			info.AllianceText = GetString(rawData, WCellTerrainSettings.DefaultLocale, i++);

			i = 39;
			info.UnknownFlags2 = GetUInt32(rawData, i++);
			info.HordeText = GetString(rawData, WCellTerrainSettings.DefaultLocale, i++);

			i = 56;
			info.UnknownFlags3 = GetUInt32(rawData, i++);
			info.LoadingScreen = GetInt32(rawData, i++);
			info.BattlefieldMapIconScale = GetFloat(rawData, i++);
			info.ParentMap = GetInt32(rawData, i++);
			info.EntranceX = GetFloat(rawData, i++);
			info.EntranceY = GetFloat(rawData, i++);
			info.TimeOfDayOverride = GetInt32(rawData, i++);
			info.ExpansionId = GetInt32(rawData, i++);
			info.RaidResetTimer = GetUInt32(rawData, i++);
			info.MaxNumberOfPlayers = GetUInt32(rawData, i++);

			return info;
		}
	}
}
