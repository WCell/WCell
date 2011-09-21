using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
	public class MapDifficultyConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var entry = new MapDifficultyEntry();

			entry.Id = (uint)GetInt32(rawData, 0);
			entry.MapId = (MapId)GetUInt32(rawData, 1);
			entry.Index = GetUInt32(rawData, 2);
			entry.RequirementString = GetString(rawData, 3);
			//info.TextFlags = GetUInt32(rawData, 19);
			entry.ResetTime = GetInt32(rawData, 20);
			entry.MaxPlayerCount = GetInt32(rawData, 21);

			var map = World.GetMapTemplate(entry.MapId);
			if (map != null)
			{
				if (entry.Index >= (double) RaidDifficulty.End)
				{
					LogManager.GetCurrentClassLogger().Warn("Invalid MapDifficulty for {0} with Index {1}.", entry.MapId, entry.Index);
					return;
				}

				if (entry.MaxPlayerCount == 0)
				{
					entry.MaxPlayerCount = map.MaxPlayerCount;
				}

				if (map.Difficulties == null)
				{
					map.Difficulties = new MapDifficultyEntry[(int) RaidDifficulty.End];
				}

				entry.Finalize(map);
				map.Difficulties[entry.Index] = entry;
			}
		}
	}

	public class MapConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var map = new MapTemplate();

			map.Id = (MapId)GetUInt32(rawData, 0);

			int i = 2;
			//rgn.InternalName = GetString(rawData, 1);
			map.Type = (MapType)GetUInt32(rawData, i++);
			i++;

			map.HasTwoSides = GetUInt32(rawData, i++) != 0;


			map.Name = GetString(rawData, i++);
			i += 16;

			i++;
			//rgn.MinLevel = GetInt32(rawData, 21);
			//rgn.MaxLevel = GetInt32(rawData, 22);
			//rgn.MaxPlayerCount = GetInt32(rawData, 23);

			map.AreaTableId = GetUInt32(rawData, i++);		// 22
			//rgn.LoadScreen = GetUInt32(rawData, 56);

			//rgn.HordeText = GetString(rawData, 22);
			//rgn.AllianceText = GetString(rawData, 39);

			map.ParentMapId = (MapId)GetUInt32(rawData, 59);

			map.RepopMapId = map.ParentMapId;
			map.RepopPosition = new Vector3(GetFloat(rawData, 60), GetFloat(rawData, 61), 500);

			map.RequiredClientId = (ClientId)GetUInt32(rawData, 63);
			map.DefaultResetTime = GetInt32(rawData, 64);
			map.MaxPlayerCount = GetInt32(rawData, 65);

			//rgn.HeroicResetTime = GetUInt32(rawData, 113);
			//rgn.RaidResetTime = GetUInt32(rawData, 112);

			ArrayUtil.Set(ref World.s_MapTemplates, (uint)map.Id, map);
		}
	}

    public class WorldMapOverlayConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var worldMapOverlayEntry = new WorldMapOverlayEntry();
            worldMapOverlayEntry.WorldMapOverlayId = (WorldMapOverlayId) GetUInt32(rawData, 0);

            for (var i = 0; i < worldMapOverlayEntry.ZoneTemplateId.Length; i++)
            {
                var zoneId = (ZoneId) GetUInt32(rawData, 2 + i);
                if(zoneId == 0)
                    // We reached to the last ZoneId reference.
                    break;

                worldMapOverlayEntry.ZoneTemplateId[i] = zoneId;
                var zoneTemplate = World.s_ZoneTemplates[(int) zoneId];
                if(zoneTemplate == null)
                {
                    LogManager.GetCurrentClassLogger().Warn(string.Format(
                        "Invalid ZoneId #{0} found at WorldMapOverlay #{1} during the DBC loading.", zoneId, worldMapOverlayEntry.WorldMapOverlayId));
                    continue;
                }
                zoneTemplate.WorldMapOverlays.Add(worldMapOverlayEntry.WorldMapOverlayId);
            }

            ArrayUtil.Set(ref World.s_WorldMapOverlayEntries, (uint) worldMapOverlayEntry.WorldMapOverlayId,
                          worldMapOverlayEntry);

        }
    }
}