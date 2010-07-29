using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Core.DBC;

namespace WCell.RealmServer.Achievement
{

    public class AchievementEntryConverter : DBCRecordConverter
    {
        public override void Convert(byte[] rawData)
        {
            var achievementEntry = new AchievementEntry();
            achievementEntry.ID = (AchievementEntryId)GetUInt32(rawData, 0);
            achievementEntry.FactionFlag = GetUInt32(rawData, 1);
            achievementEntry.MapID = GetUInt32(rawData, 2);
            achievementEntry.Name = new string[16];
            for (int i = 0; i < 16; i++)
                achievementEntry.Name[i] = GetString(rawData, i + 4);
            achievementEntry.CategoryId = GetUInt32(rawData, 38);
            achievementEntry.Points = GetUInt32(rawData, 39);
            achievementEntry.Flags = GetUInt32(rawData, 41);
            achievementEntry.Count = GetUInt32(rawData, 60);
            achievementEntry.RefAchievement = GetUInt32(rawData, 61);

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
			var criteria = (AchievementCriteriaType)GetUInt32(rawData, 0);
			var entry = AchievementMgr.GetCriteriaEntryCreator(criteria)();

			// TODO: Finish
			Copy(rawData, 1, 2, entry);
			//AchievementMgr.AddCriteriaEntry(entry);
        }
    }
}