using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Achievements
{
	[Flags]
	public enum AchievementFlags : uint
	{
		AchievementFlagCounter = 0x00000001,				 // Just count statistic (never stop and complete)
		AchievementFlagUnk2 = 0x00000002,					 // not used
		AchievementFlagStoreMaxValue = 0x00000004,			 // Store only max value? used only in "Reach level xx"
		AchievementFlagSumm = 0x00000008,					 // Use summ criteria value from all reqirements (and calculate max value)
		AchievementFlagMaxUsed = 0x00000010,				 // Show max criteria (and calculate max value ??)
		AchievementFlagReqCount = 0x00000020,			     // Use not zero req count (and calculate max value)
		AchievementFlagAverage = 0x00000040,			     //מ Show as average value (value / time_in_days) depend from other flag (by def use last criteria value)
		AchievementFlagBar = 0x00000080,					 // Show as progress bar (value / max vale) depend from other flag (by def use last criteria value)
		AchievementFlagRealmFirstReach = 0x00000100,         //
		AchievementFlagRealmFirstKill = 0x00000200,			 //
	}
}
