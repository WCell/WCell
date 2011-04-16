using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Achievements
{
	[Flags]
	public enum AchievementCriteriaCompletionFlags : uint
	{
		ShowProgressBar = 0x00000001,           // Show progress as bar
		FlagHideCriteria = 0x00000002,		    // Not show criteria in client
		FailAchievement = 0x00000004,			// 
		ResetOnStart = 0x00000008,				//
		IsDate = 0x00000010,					// 
		IsMoney = 0x00000020,			        // Displays counter as money
	}
}