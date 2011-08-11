using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.Core.DBC;

namespace WCell.RealmServer.Achievements
{
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

	public class AchievementEntryConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var achievementEntry = new AchievementEntry();
			achievementEntry.ID = GetUInt32(rawData, 0);
			achievementEntry.FactionFlag = GetInt32(rawData, 1);
			achievementEntry.MapID = (MapId)GetUInt32(rawData, 2);
			achievementEntry.Names = GetStrings(rawData, 4);
			
			var category = (AchievementCategoryEntryId)GetUInt32(rawData, 38);		// set category
			achievementEntry.Category = AchievementMgr.GetCategoryEntry(category);

			achievementEntry.Points = GetUInt32(rawData, 39);
			achievementEntry.Flags = (AchievementFlags)GetUInt32(rawData, 41);
			achievementEntry.Count = GetUInt32(rawData, 60);
			achievementEntry.RefAchievement = (uint)GetUInt32(rawData, 61);

			AchievementMgr.AchievementEntries[achievementEntry.ID] = achievementEntry;
		}
	}

	public class AchievementCriteriaConverter : DBCRecordConverter
	{
		public override void Convert(byte[] rawData)
		{
			var criteriaType = (AchievementCriteriaType)GetUInt32(rawData, 2);
			var creator = AchievementMgr.GetCriteriaEntryCreator(criteriaType);
			if (creator == null)
			{
				// unknown type
				return;
			}

			var entry = creator();

			entry.AchievementCriteriaId = GetUInt32(rawData, 0);
			entry.AchievementEntryId = GetUInt32(rawData, 1);


			var achievement = entry.AchievementEntry;
			if (achievement == null)
			{
				// invalid criteria does not belong to any entry
				return;
			}

			// add criterion to achievement
			achievement.Criteria.Add(entry);

			CopyTo(rawData, entry, 3);

			entry.CompletionFlag = GetUInt32(rawData, 26);
			entry.GroupFlag = (AchievementCriteriaGroupFlags) GetUInt32(rawData, 27);
			entry.TimeLimit = GetUInt32(rawData, 29);

			// add to critera map
			var list = AchievementMgr.GetCriteriaEntriesByType(criteriaType);
			if (list != null)
			{
				list.Add(entry);
			}

			// create requirement set
			entry.RequirementSet = new AchievementCriteriaRequirementSet(entry.AchievementCriteriaId);

            AchievementMgr.CriteriaEntriesById[entry.AchievementCriteriaId] = entry;
		}
	}
}
