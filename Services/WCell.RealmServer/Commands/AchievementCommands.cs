using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Achievements;
using WCell.RealmServer.Achievement;
using WCell.RealmServer.Entities;
using WCell.Util.Commands;

namespace WCell.RealmServer.Commands
{
	/// <summary>
	/// TODO: Localize
	/// </summary>
	public class AchievementCommands : RealmServerCommand
	{
		protected override void Initialize()
		{
			Init("Achievement");
			EnglishDescription = "Provides commands for managing achivements";
		}

		public class AddAchievementCommand : SubCommand
		{
			protected override void Initialize()
			{
				Init("Add", "Create");
				EnglishParamInfo = "<achievement>";
				EnglishDescription = "Adds the given achievement entry id to the player completed achievement list.";
			}

			public override void Process(CmdTrigger<RealmServerCmdArgs> trigger)
			{
				var achievementId = trigger.Text.NextEnum(AchievementEntryId.None);
				var achivementEntry = AchievementMgr.GetAchievementEntry(achievementId);
				if (achivementEntry != null)
				{
					AddAchievement((Character) trigger.Args.Target, achievementId);
					trigger.Reply("Achievement \"{0}\" added sucessfully.", achivementEntry.Name);
				}
				else
				{
					trigger.Reply("Invalid AchievementId");
					return;
				}
			}

			public static bool AddAchievement(Character character, AchievementEntryId achievementEntryId)
			{
				character.Achievements.EarnAchievement(achievementEntryId);
				return true;
			}
		}
	}
}
