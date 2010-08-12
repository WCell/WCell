using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Achievements
{
	[Flags]
	public enum AchievementCriteriaCompletionFlags : uint
	{
		AchievementCriteriaFlagShowProgressBar = 0x00000001,        // Show progress as bar
		AchievementCriteriaFlagHideCriteria = 0x00000002,			// Not show criteria in client
		AchievementCriteriaFlagUnk3 = 0x00000004,					// BG related??
		AchievementCriteriaFlagUnk4 = 0x00000008,					//
		AchievementCriteriaFlagUnk5 = 0x00000010,					// not used
		AchievementCriteriaFlagMoneyCounter = 0x00000020,			// Displays counter as money
	}
}
