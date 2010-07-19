using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	public enum BattlegroundId : uint
	{
		None = 0,
		AlteracValley = 1,
		WarsongGulch = 2,
		ArathiBasin = 3,
		NagrandArena = 4,
		BladesEdgeArena = 5,
		AllArenas = 6,
		EyeOfTheStorm = 7,
		RuinsOfLordaeron = 8,
		StrandOfTheAncients = 9,
		DalaranSewers = 10,
		TheRingOfValor = 11,
		IsleOfConquest = 30,
		ABGUnknown = 31,
		End
	}

	public enum BattlegroundStatus
	{
		None,

		/// <summary>
		/// Didn't enter yet
		/// </summary>
		Enqueued = 1,

		/// <summary>
		/// Entered but didn't start yet
		/// </summary>
		Preparing = 2,

		/// <summary>
		/// BG is active
		/// </summary>
		Active = 3,

		/// <summary>
		/// A team has won or timeout has been reached
		/// </summary>
		Finished = 4,
	}

	public enum BattlegroundJoinError
	{
		None = 0,
		Nothing = -1,

		/// <summary>
		/// You or one of your party members is a deserter
		/// </summary>
		Deserter = -2,

		/// <summary>
		/// Your group is not in the same team
		/// </summary>
		NotSameTeam = -3,

		/// <summary>
		/// Can only be enqueued for 3 battles at once
		/// </summary>
		Max3Battles = -4,

		/// <summary>
		/// You cannot queue for a rated match while enqueued for other battles
		/// </summary>
		StillEnqueued = -5,

		/// <summary>
		/// You cannot queue for another battle while queued for a rated arena match
		/// </summary>
		InRatedMatch = -6,

		/// <summary>
		/// Your team has left the arena queue
		/// </summary>
		TeamLeftQueue = -7,

		/// <summary>
		/// Your group has joined a battleground queue but you are not eglible.
		/// (This is the same error message for all other numbers but valid BattlegroundIds)
		/// </summary>
		GroupJoinedNotEligible = -8,

        JoinXpGain = -9,

        JoinRangeIndex = -10,

        JoinTimedOut = -11,

        JoinFailed = -12,

        LfgCantUseBg = -13,

        InRandomBg = -14,

        InNonRandomBg = -15
	}

	public enum BattlegroundSide
	{
		Alliance = 0,
		Horde = 1,
		End
	}

	public enum ArenaType
	{
		None = 0,
		TwoVsTwo = 2,
		ThreeVsThree = 3,
		FiveVsFive = 5
	}
}