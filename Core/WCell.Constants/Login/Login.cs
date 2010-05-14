using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Login
{
	public enum VersionComparison
	{
		LessThan = -1,
		LessThanOrExact = 0,
		Exact = 1
	}

	[Flags]
	public enum CharEnumFlags
	{
		None = 0,
		Alive = 0x1,
		//Flag_0x2 = 0x2,
		LockedForTransfer = 0x4,
		//Flag_0x8 = 0x8,
		//Flag_0x10 = 0x10, // No Visible Change
		HideHelm = 0x400,
		HideCloak = 0x800,
		Ghost = 0x2000, // Correct
		NeedsRename = 0x4000, // Correct
		Unknown = 0xA00000,
		LockedForBilling = 0x1000000 // Correct
	}

	public enum RealmStatus : byte
	{
		Open = 0x00,
		Locked = 0x01
	}

	[Flags]
	public enum RealmFlags : byte
	{
        None = 0x0,
        RedName = 0x1,
        Offline = 0x2,
        SpecifyBuild = 0x4,
        Unk1 = 0x8,
        Unk2 = 0x10,
		NewPlayers = 0x20,
		Recommended = 0x40,
		Full = 0x80,
	}

	[Flags]
	public enum RealmServerType : byte
	{
		Normal = 0x00,
		PVP = 0x01,
		ServerType3 = 3,
		ServerType4 = 4,
		ServerType5 = 5,
		RP = 0x06,
		RPPVP = 0x07
	}

	public enum RealmPopulation
	{
		Low = 0,
		Medium = 1,
		High = 2,
		New = 200,
		Full = 400,
		Recommended = 600
	}

	/// <summary>
	/// Logout Codes used in Logout Sequence on the Game
	/// </summary>
	public enum LogoutResponseCodes // byte???
	{
		LOGOUT_RESPONSE_ACCEPTED = 00, //$00
		LOGOUT_RESPONSE_DENIED = 0xC //$0C
	}
}