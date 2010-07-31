using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.Util;

namespace WCell.RealmServer.Achievement
{

    public class AchievementEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var achievementEntry = new AchievementEntry();
            achievementEntry.ID = (AchievementEntryId)GetUInt32(rawData, 0);
            achievementEntry.FactionFlag = GetUInt32(rawData, 1);
            achievementEntry.MapID = (MapId)GetUInt32(rawData, 2);
            achievementEntry.Name = new string[16];
            for (int i = 0; i < 16; i++)
                achievementEntry.Name[i] = GetString(rawData, i + 4);
            achievementEntry.CategoryId = (AchievementCategoryEntryId)GetUInt32(rawData, 38);
            achievementEntry.Points = GetUInt32(rawData, 39);
            achievementEntry.Flags = GetUInt32(rawData, 41);
            achievementEntry.Count = GetUInt32(rawData, 60);
			achievementEntry.RefAchievement = (AchievementEntryId)GetUInt32(rawData, 61);

            AchievementMgr.AchievementEntries[achievementEntry.ID] = achievementEntry;
        }
    }

    public class AchievementCategoryEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var achievementCategoryEntry = new AchievementCategoryEntry();
            achievementCategoryEntry.ID = (AchievementCategoryEntryId)GetUInt32(rawData, 0);
            achievementCategoryEntry.ParentCategory = (AchievementCategoryEntryId)GetUInt32(rawData, 1);

            AchievementMgr.AchievementCategoryEntries[achievementCategoryEntry.ID] = achievementCategoryEntry;
        }
    }

	public class AchievementCriteriaEntryMapper : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var criteriaType = (AchievementCriteriaType)GetUInt32(rawData, 2);
			var entry = AchievementMgr.GetCriteriaEntryCreator(criteriaType)();
			entry.AchievementCriteriaId = (AchievementCriteriaId)GetUInt32(rawData, 0);
			entry.AchievementEntryId = (AchievementEntryId)GetUInt32(rawData, 1);

			Copy(rawData, 3, entry);
			ArrayUtil.Set(ref AchievementMgr.CriteriaEntries, (uint)entry.AchievementCriteriaId,entry);
		}
	}
}