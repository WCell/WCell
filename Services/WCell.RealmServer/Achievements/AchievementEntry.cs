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
        public string Name;                                         // 4
        //public string Description;                                // 5
        public AchievementCategoryEntry Category;                                   // 6
        public uint Points;                                       // 7 reward points
        //public uint OrderInCategory;                               // 8
        public AchievementFlags Flags;                                        // 9
        //public uint Icon;                                       // 10 icon (from SpellIcon.dbc)
        //public string TitleReward;                                // 11
        public uint Count;                                           // 12 - need this count of completed criterias (own or referenced achievement criterias)
		public uint RefAchievement;                                  // 13 - referenced achievement (counting of all completed criterias)

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
			return Name;
		}
    }
}