using WCell.Constants.Achievements;

namespace WCell.RealmServer.Achievements
{
    public class AchievementCategoryEntry
    {
		public AchievementCategoryEntryId ID;                                           // 0
		public AchievementCategoryEntryId ParentCategory;                               // 1 -1 for main category
        //public string Name[16];                                       // 2-17
        //public uint NameFlags;                                    // 18
        //public uint SortOrder;                                  // 19
    }
}
