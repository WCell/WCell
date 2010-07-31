using System;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Util;
using WCell.Util.Graphics;
using NLog;

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

			var map = World.GetRegionTemplate(entry.MapId);
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
				map.Difficulties[entry.Index] = entry;

				entry.Finalize(map);
			}
		}
	}

	public class MapConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var rgn = new RegionTemplate();

			rgn.Id = (MapId)GetUInt32(rawData, 0);

			int i = 2;
			//rgn.InternalName = GetString(rawData, 1);
			rgn.Type = (MapType)GetUInt32(rawData, i++);
			i++;

			rgn.HasTwoSides = GetUInt32(rawData, i++) != 0;


			rgn.Name = GetString(rawData, i++);
			i += 16;

			i++;
			//rgn.MinLevel = GetInt32(rawData, 21);
			//rgn.MaxLevel = GetInt32(rawData, 22);
			//rgn.MaxPlayerCount = GetInt32(rawData, 23);

			rgn.AreaTableId = GetUInt32(rawData, i++);		// 22
			//rgn.LoadScreen = GetUInt32(rawData, 56);

			//rgn.HordeText = GetString(rawData, 22);
			//rgn.AllianceText = GetString(rawData, 39);

			rgn.ParentMapId = (MapId)GetUInt32(rawData, 59);

			rgn.RepopRegionId = rgn.ParentMapId;
			rgn.RepopPosition = new Vector3(GetFloat(rawData, 60), GetFloat(rawData, 61), 500);

			rgn.RequiredClientId = (ClientId)GetUInt32(rawData, 63);
			rgn.DefaultResetTime = GetInt32(rawData, 64);
			rgn.MaxPlayerCount = GetInt32(rawData, 65);

			//rgn.HeroicResetTime = GetUInt32(rawData, 113);
			//rgn.RaidResetTime = GetUInt32(rawData, 112);

			ArrayUtil.Set(ref World.s_RegionTemplates, (uint)rgn.Id, rgn);
		}
	}
}