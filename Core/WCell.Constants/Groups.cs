using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants
{
	/// <summary>
	/// Group Type
	/// </summary>
	[Flags]
	public enum GroupFlags : byte
	{
		Party = 0,
		Raid = 1,
		Battleground = 2,
		BattlegroundOrRaid = Raid | Battleground,
		LFD = 0x8
	}

	/// <summary>
	/// Group Update flags used when sending group member stats
	/// </summary>
	[Flags]
	public enum GroupUpdateFlags : uint
	{
		None = 0x00000000,
		Status = 0x00000001,
		Health = 0x00000002,
		MaxHealth = 0x00000004,
		PowerType = 0x00000008,
		Power = 0x00000010,
		MaxPower = 0x00000020,
		Level = 0x00000040,
		ZoneId = 0x00000080,
		Position = 0x00000100,
		Auras = 0x00000200,

		PetGuid = 0x00000400,
		PetName = 0x00000800,
		PetDisplayId = 0x00001000,
		PetHealth = 0x00002000,
		PetMaxHealth = 0x00004000,
		PetPowerType = 0x00008000,
		PetPower = 0x00010000,
		PetMaxPower = 0x00020000,
		PetAuras = 0x00040000,

		Vehicle = 0x00080000,

		Unused2 = 0x00100000,
		Unused3 = 0x00200000,
		Unused4 = 0x00400000,
		Unused5 = 0x00800000,
		Unused6 = 0x01000000,
		Unused7 = 0x02000000,
		Unused8 = 0x04000000,
		Unused9 = 0x08000000,
		Unused10 = 0x10000000,
		Unused11 = 0x20000000,
		Unused12 = 0x40000000,
		Unused13 = 0x80000000,

		UpdatePlayer = 0x000003FF,		// all players flags, without pet flags
		UpdatePet = 0x0007FC00,			// all pet flags
		UpdateFull = 0x0007FFFF			// all known flags
	}

	[Flags]
	public enum GroupMemberFlags : byte
	{
		Normal = 0x0,
		Assistant = 0x1,
		MainTank = 0x2,
		MainAssistant = 0x4
	}

	/// <summary>
	/// Priviledge levels
	/// </summary>
	public enum GroupPrivs
	{
		Normal = 0,
		Assistant,
		MainAsisstant,
		Leader
	}

	/// <summary>
	/// Values sent in the SMSG_PARTY_COMMAND_RESULT packet
	/// </summary>
	public enum GroupResult
	{
		/// <summary>
		/// Either means everything is OK or the member left?
		/// </summary>
		NoError = 0,
		OfflineOrDoesntExist = 1,
		NotInYourParty = 2,
		NotInYourInstance = 3,
		GroupIsFull = 4,
		AlreadyInGroup = 5,
		PlayerNotInParty = 6,
		DontHavePermission = 7,
		TargetIsUnfriendly = 8,
		TargetIsIgnoringYou = 9,
		PendingMatch = 12,
		TrialCantInviteHighLevel = 13
	}

}