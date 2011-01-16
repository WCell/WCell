using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.World;
using WCell.RealmServer.Lang;

namespace WCell.RealmServer.Achievements
{
    public class AchievementEntry
    {
		public uint ID;                                           // 0
		public int FactionFlag;                                  // 1 -1=all, 0=horde, 1=alliance
        public MapId MapID;                                        // 2 -1=none
        //public uint ParentAchievement;                             // 3 its Achievement parent (can`t start while parent uncomplete, use its Criteria if don`t have own, use its progress on begin)
        public string[] Name;                                         // 4-19
        //public uint NameFlags;                                    // 20
        //public string Description[16];                                // 21-36
        //public uint DescFlags;                                    // 37
        public AchievementCategoryEntry Category;                                   // 38
        public uint Points;                                       // 39 reward points
        //public uint OrderInCategory;                               // 40
        public AchievementFlags Flags;                                        // 41
        //public uint Icon;                                       // 42 icon (from SpellIcon.dbc)
        //public string TitleReward[16];                                // 43-58
        //public string TitleRewardFlags;                             // 59
        public uint Count;                                           // 60 - need this count of completed criterias (own or referenced achievement criterias)
		public uint RefAchievement;                                  // 61 - referenced achievement (counting of all completed criterias)

		/// <summary>
		/// List of criteria that needs to be satisfied to achieve this achievement
		/// </summary>
    	public readonly List<AchievementCriteriaEntry> Criteria = new List<AchievementCriteriaEntry>();

        /// <summary>
        /// List of all rewards that will be given once achievement is completed.
        /// </summary>
        public readonly List<AchievementReward> Rewards = new List<AchievementReward>();

        public bool IsRealmFirstType()
        {
            return Flags.HasAnyFlag(AchievementFlags.RealmFirstKill | AchievementFlags.RealmFirstReach);
            //return (Flags.HasFlag(AchievementFlags.RealmFirstReach) || Flags.HasFlag(AchievementFlags.RealmFirstKill);

        }

		public override string ToString()
		{
			return Name.LocalizeWithDefaultLocale();
		}
    }
}