using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Achievement
{
	/// <summary>
	/// Global container for Achievement-related data
	/// </summary>
	public static class AchievementMgr
	{
        public static Dictionary<uint, AchievementEntry> AchievementEntries = new Dictionary<uint, AchievementEntry>();

        public static Dictionary<uint, AchievementCategoryEntry> AchievementCategoryEntries = new Dictionary<uint, AchievementCategoryEntry>();

	}
}