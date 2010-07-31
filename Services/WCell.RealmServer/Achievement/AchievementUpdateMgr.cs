using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievement
{
	/// <summary>
	/// Callback to be called to check the requirements for the given possible achievement
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	/// <param name="involved">The object that is involved in this achievement (e.g. a slain creature or acquired Item etc)</param>
	public delegate void AchievementUpdater(AchievementCriteriaType type, Character chr, uint value1, uint value2, ObjectBase involved);

	/// <summary>
	/// Hook to InitializationPass.Seven to customize the Updater delegates.
	/// </summary>
	public static class AchievementUpdateMgr
	{
		public static readonly AchievementUpdater[] Updaters = new AchievementUpdater[(int)AchievementCriteriaType.End];

		public static AchievementUpdater GetUpdater(AchievementCriteriaType type)
		{
			return Updaters[(int)type];
		}

		public static void SetUpdater(AchievementCriteriaType type, AchievementUpdater updater)
		{
			Updaters[(int)type] = updater;
		}

		[Initialization(InitializationPass.Sixth)]
		public static void InitializeUpdater()
		{
			SetUpdater(AchievementCriteriaType.KillCreature, OnKillCreature);
			SetUpdater(AchievementCriteriaType.ReachLevel, OnReachLevel);
			// ...
		}

		private static void OnKillCreature(AchievementCriteriaType type, Character chr, uint value1, uint value2, ObjectBase involved)
		{
			// TODO
		}

		private static void OnReachLevel(AchievementCriteriaType type, Character chr, uint value1, uint value2, ObjectBase involved)
		{
			// TODO
		}
	}
}
