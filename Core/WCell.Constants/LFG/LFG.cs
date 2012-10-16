using System;

namespace WCell.Constants.LFG
{
	public static class LFGConstants
	{
		public static TimeSpan RoleCheckTime = TimeSpan.FromMinutes(2);
		public static TimeSpan BootTime = TimeSpan.FromMinutes(2);
		public static TimeSpan ProposalTime = TimeSpan.FromMinutes(2);
		public static int TanksNeeded = 1;
		public static int HealersNeeded = 1;
		public static int DPSNeeded = 3;
		public static int MaxKicks = 3;
		public static int VotesNeededToKick = 3;
		public static TimeSpan QueueUpdateInterval = TimeSpan.FromMilliseconds(15);
	}

	/// <summary>
	/// Determines the type of instance
	/// </summary>
	public enum LFGType
	{
		None = 0,
		Dungeon = 1,
		Raid = 2,
		Quest = 3,
		Zone = 4,
		Heroic = 5,
		Random = 6
	}

	/// <summary>
	/// Proposal states
	/// </summary>
	public enum LFGProposalState
	{
		Initiating = 0,
		Failed = 1,
		Success = 2
	}

	/// <summary>
	/// Teleport errors
	/// </summary>
	public enum LFGTeleportError
	{
		// 3, 7, 8 = "You can't do that right now" | 5 = No client reaction
		/// <summary>
		///  Internal use
		/// </summary>
		Ok = 0,
		PlayerDead = 1,
		Falling = 2,
		Fatigue = 4,
		InvalidLocation = 6
	}

	/// <summary>
	/// Queue join results
	/// </summary>
	public enum LFGJoinResult
	{
		/// <summary>
		/// Joined (no client msg)
		/// </summary>
		Ok = 0,
		/// <summary>
		/// RoleCheck Failed
		/// </summary>
		Failed = 1,
		/// <summary>
		/// Your group is full
		/// </summary>
		GroupFull = 2,
		/// <summary>
		/// Unknown, No client reaction
		/// </summary>
		Unknown = 3,
		/// <summary>
		/// Internal LFG Error
		/// </summary>
		InternalError = 4,
		/// <summary>
		/// You do not meet the requirements for the chosen dungeons
		/// </summary>
		DontMeetRequierments = 5,
		/// <summary>
		/// One or more party members do not meet the requirements for the chosen dungeons
		/// </summary>
		PartyMemeberDoesntMeetRequirements = 6,
		/// <summary>
		/// You cannot mix dungeons, raids, and random when picking dungeons
		/// </summary>
		CannotMixRaidsAndDungeons = 7,
		/// <summary>
		/// The dungeon you chose does not support players from multiple realms
		/// </summary>
		MultiRealmUnsupported = 8,
		/// <summary>
		/// One or more party members are pending invites or disconnected
		/// </summary>
		Disconnected = 9,
		/// <summary>
		/// Could not retrieve information about some party members
		/// </summary>
		PartyInfoFailed = 10,
		/// <summary>
		/// One or more dungeons was not valid
		/// </summary>
		InvalidDungeon = 11,
		/// <summary>
		/// You can not queue for dungeons until your deserter debuff wears off
		/// </summary>
		Deserter = 12,
		/// <summary>
		/// One or more party members has a deserter debuff
		/// </summary>
		PartyMemberIsDeserter = 13,
		/// <summary>
		/// You can not queue for random dungeons while on random dungeon cooldown
		/// </summary>
		RandomCooldown = 14,
		/// <summary>
		/// One or more party members are on random dungeon cooldown
		/// </summary>
		PartyMemberIsRandomCooldown = 15,
		/// <summary>
		/// You can not enter dungeons with more that 5 party members
		/// </summary>
		TooManyMembers = 16,
		/// <summary>
		/// You can not use the dungeon system while in BG or arenas
		/// </summary>
		CannotJoinWhileInBgOrArena = 17,
		/// <summary>
		/// Rolecheck failed
		/// </summary>
		Failed2 = 18
	}

	/// <summary>
	/// Roles
	/// </summary>
	public enum LFGRoles
	{
		None = 0,
		Leader = 1,
		Tank = 2,
		Healer = 4,
		Damage = 8
	};

	/// <summary>
	/// Role check states
	/// </summary>
	public enum LFGRoleCheckState
	{
		/// <summary>
		/// Internal use = Not initialized.
		/// </summary>
		None = 0,
		/// <summary>
		/// Role check finished
		/// </summary>
		Finished = 1,
		/// <summary>
		/// Role check begins
		/// </summary>
		Initializing = 2,
		/// <summary>
		/// Someone didn't select a role after 2 mins
		/// </summary>
		RoleNotSelected = 3,
		/// <summary>
		/// Can't form a group with that role selection
		/// </summary>
		InvalidRoleSelections = 4,
		/// <summary>
		/// Someone left the group
		/// </summary>
		Aborted = 5,
		/// <summary>
		/// Someone didnt select a role
		/// </summary>
		NoRoleSelected = 6
	}


	/// <summary>
	/// Answer state (Also used to check compatibilites)
	/// </summary>
	public enum LFGAnswer
	{
		Pending = -1,
		Deny = 0,
		Agree = 1
	}

	public enum LFGUpdateType
	{
		/// <summary>
		/// Internal Use
		/// </summary>
		Default = 0,
		Leader = 1,
		RoleCheckAborted = 4,
		JoinProposal = 5,
		RoleCheckFailed = 6,
		RemovedFromQueue = 7,
		ProposalFailed = 8,
		ProposalDeclined = 9,
		GroupFound = 10,
		AddedToQueue = 12,
		ProposalBegin = 13,
		ClearLockList = 14,
		GroupMemberOffline = 15,
		GroupDisband = 16
	};

	public enum LFGState
	{
		/// <summary>
		/// Not using LFG / LFR
		/// </summary>
		None,
		/// <summary>
		/// Rolecheck active
		/// </summary>
		RoleCheck,
		/// <summary>
		/// Queued
		/// </summary>
		Queued,
		/// <summary>
		/// Proposal active
		/// </summary>
		Proposal,
		/// <summary>
		/// Vote to kick active
		/// </summary>
		Boot,
		/// <summary>
		/// In LFG Group, in a Dungeon
		/// </summary>
		InDungeon,
		/// <summary>
		/// In LFG Group, in a finished Dungeon
		/// </summary>
		FinishedDungeon,
		/// <summary>
		/// Using Raid finder
		/// </summary>
		RaidBrowser
	};

	/// <summary>
	/// Reason players cant join
	/// </summary>
	public enum LFGLockStatusType
	{
		/// <summary>
		/// Internal use only
		/// </summary>
		Ok = 0,
		/// <summary>
		/// 
		/// </summary>
		InsufficientExpansion = 1,
		/// <summary>
		/// 
		/// </summary>
		TooLowLevel = 2,
		/// <summary>
		/// 
		/// </summary>
		TooHighLevel = 3,
		/// <summary>
		/// 
		/// </summary>
		GearScoreTooLow = 4,
		/// <summary>
		/// 
		/// </summary>
		GearScoreTooHigh = 5,
		/// <summary>
		/// 
		/// </summary>
		RaidLocked = 6,
		/// <summary>
		/// 
		/// </summary>
		AttonementTooLowLevel = 1001,
		/// <summary>
		/// 
		/// </summary>
		AttonementTooHighLevel = 1002,
		/// <summary>
		/// 
		/// </summary>
		QuestNotCompleted = 1022,
		/// <summary>
		/// 
		/// </summary>
		MissingItem = 1025,
		/// <summary>
		/// 
		/// </summary>
		NotInSeason = 1031
	};
}