using System;

namespace WCell.Constants.Achievements
{
    [Flags]
    public enum AchievementFlags : uint
    {
        Counter = 0x00000001,				 // Just count statistic (never stop and complete)
        Unk2 = 0x00000002,					 // not used
        StoreMaxValue = 0x00000004,			 // Store only max value? used only in "Reach level xx"
        Summ = 0x00000008,					 // Use summ criteria value from all reqirements (and calculate max value)
        MaxUsed = 0x00000010,				 // Show max criteria (and calculate max value ??)
        ReqCount = 0x00000020,			     // Use not zero req count (and calculate max value)
        Average = 0x00000040,			     //מ Show as average value (value / time_in_days) depend from other flag (by def use last criteria value)
        Bar = 0x00000080,					 // Show as progress bar (value / max vale) depend from other flag (by def use last criteria value)
        RealmFirstReach = 0x00000100,         //
        RealmFirstKill = 0x00000200,			 //
    }
}
