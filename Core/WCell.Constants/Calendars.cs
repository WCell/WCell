using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{

	public enum CalendarEventFlags
	{
		Player = 0x001,
		System = 0x004,
		Holiday = 0x008,
		Locked = 0x10,
		AutoApprove = 0x20,
		GuildAnnouncement = 0x040,
		RaidLockout = 0x080,
		RaidReset = 0x200,
		GuildEvent = 0x400
	}

	public enum CalendarInviteType : byte
	{
		Normal = 0,
		Signup = 1
	}

	public enum CalendarModType : byte
	{
		Participant = 0,
		Moderator = 1,
		Creator = 2,
	}
	public enum CalendarEventModFlags
	{
		Moderator = 0x2,
		Creator = 0x4,
	}

	public enum CalendarEventType : byte
	{
		Raid = 0,
		Dungeon = 1,
		PvP = 2,
		Meeting = 3,
		Other = 4,
	}

	public enum CalendarInviteStatus : byte
	{
		Invited = 0,
		Accepted = 1,
		Declined = 2,
		Confirmed = 3,
		Out = 4,
		StandBy = 5,
		SignedUp = 6,
		NotSignedUp = 7,
	}

	public enum CalendarRepeatOption : byte
	{
		Never = 0,
		Weekly = 1,
		BiWeekly = 2,
		Monthly = 3,
	}

	public enum CalendarErrors : uint
	{
		GuildEventsExceeded = 1,
		EventsExceeded = 2,
		SelfInvitesExceeded = 3,
		OtherInvitesExceeded = 4,
		NoPermissions = 5,
		EventInvalid = 6,
		NotInvited = 7,
		InternalError = 8,
		PlayerNotInGuild = 9,
		AlreadyInvitedToEvent = 10,
		PlayerNotFound = 11,
		NotAllied = 12,
		PlayerIsIgnoringYou = 13,
		InvitesExceeded = 14,
		InvalidDate = 16,
		InvalidTime = 17,
		NeedsTitle = 19,
		EventPassed = 20,
		EventLocked = 21,
		DeleteCreatorFailed = 22,
		SystemDisabled = 24,
		RestrictedAccount = 25,
		ArenaEventsExceeded = 26,
		RestrictedLevel = 27,
		UserSquelched = 28,
		NoInvite = 29,
		WrongServer = 36,
		InviteWrongServer = 37,
		NoGuildInvites = 38,
		InvalidSignup = 39,
		NoModerator = 40
	}
}
