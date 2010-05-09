using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Chat
{
	public enum ChannelNotification : byte
	{
		PlayerJoined = 0x00,
		PlayerLeft = 0x01,
		YouJoined = 0x02,
		YouLeft = 0x03,
		WrongPassword = 0x04,
		NotOnChannel = 0x05,
		NotModerator = 0x06,
		PasswordChanged = 0x07,
		OwnerChanged = 0x08,
		PlayerNotOnChannel = 0x09,
		NotOwner = 0x0A,
		CurrentOwner = 0x0B,
		Mute = 0x0C,
		Moderator = 0x0C,
		Announcing = 0x0D,
		NotAnnouncing = 0x0E,
		Moderated = 0x0F,
		NotModerated = 0x10,
		YouAreMuted = 0x11,
		Kicked = 0x12,
		YouAreBanned = 0x13,
		Banned = 0x14,
		Unbanned = 0x15,
		AlreadyOnChannel = 0x17,
		BeenInvitedToChannel = 0x18,
        InviteWrongFaction = 0x19,
        WrongAlliance = 0x1A,
        InvalidChannelName = 0x1B,
        ChannelIsNotModerated = 0x1C,
		HaveInvitedToChannel = 0x1D,
        CannotInviteBannedPlayer = 0x1E,
        ChatThrottledNotice = 0x1F,
        NotInCorrectAreaForChannel = 0x20,
        NotInLFGQueue = 0x21,
        VoiceEnabled = 0x22,
        VoiceDisabled = 0x23,
	}

	/// <summary>
	/// Same as <see cref="ChatChannelFlagsClient"/> but according to how its read  from DBC files.
	/// The client actually expects the format defined in <see cref="ChatChannelFlagsClient"/>.
	/// </summary>
	[Flags]
	public enum ChatChannelFlags : uint
	{
		None = 0,
		AutoJoin = 0x1, // General, Trade, LocalDefense, LookingForGroup.
		ZoneSpecific = 0x2,
		Global = 0x4,
		Trade = 0x8,
		//CityOnly = 0x30,
		CityOnly = 0x20,
		Defense = 0x10000,
		RequiresUnguilded = 0x20000,
		LookingForGroup = 0x40000
	}

    /// <summary>
    /// Per player
    /// </summary>
    [Flags]
    public enum ChannelMemberFlags
    {
        None = 0,
        Owner = 0x1,
        Moderator = 0x2,
        Voiced = 0x4,
        Muted = 0x8,
        Custom = 0x10, // ?
        VoiceMuted = 0x20,
    }

	[Flags]
	public enum ChatChannelFlagsClient : uint
	{
		None = 0,
		Custom = 0x1, 
		Trade = 0x4,
		FFA = 0x8,						// basically anything but LFG (since you need to access it through the LFG-menu)
		Predefined = 0x10,				// any non custom channel
		CityOnly = 0x20,
		LFG = 0x40,
		Voice = 0x80
	}
}