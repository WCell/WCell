using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;

namespace WCell.RealmServer.Achievement
{
	[StructLayout(LayoutKind.Sequential)]
	public abstract class AchievementCriteriaEntry
	{
		public AchievementCriteria Criteria;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class KillCreatureAchievementCriteriaEntry : AchievementCriteriaEntry
	{
		public NPCId CreatureId;
		public int CreatureCount;
	}
}
