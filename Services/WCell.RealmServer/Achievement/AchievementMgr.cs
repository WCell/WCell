using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Core.DBC;
using WCell.Core.Initialization;

namespace WCell.RealmServer.Achievement
{
	internal delegate AchievementCriteriaEntry AchievementCriteriaEntryCreator();

	/// <summary>
	/// Global container for Achievement-related data
	/// </summary>
	public static class AchievementMgr
	{
		private static readonly AchievementCriteriaEntryCreator[] AchievementEntryCreators =
			new AchievementCriteriaEntryCreator[(int)AchievementCriteria.End];

		public static readonly Dictionary<uint, AchievementEntry> AchievementEntries = new Dictionary<uint, AchievementEntry>();

		public static readonly Dictionary<uint, AchievementCategoryEntry> AchievementCategoryEntries = new Dictionary<uint, AchievementCategoryEntry>();

		[Initialization(InitializationPass.Fifth)]
		public static void InitAchievements()
		{
			LoadCriteria();
		}

		public static void LoadCriteria()
		{
			SetEntryCreator(AchievementCriteria.KillCreature, () => new KillCreatureAchievementCriteriaEntry());
			// SetEntryCreator(AchievementType..., () => new ...());
			// ...

			// DBCReader<>
		}

		internal static AchievementCriteriaEntryCreator GetCriteriaEntryCreator(AchievementCriteria criteria)
		{
			return AchievementEntryCreators[(int)criteria];
		}

		internal static void SetEntryCreator(AchievementCriteria criteria, AchievementCriteriaEntryCreator creator)
		{
			AchievementEntryCreators[(int)criteria] = creator;
		}
	}
}