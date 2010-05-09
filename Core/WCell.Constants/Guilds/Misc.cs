using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Guilds
{

	/// <summary>
	/// Common guild events
	/// </summary>
	public enum GuildEvents : byte
	{
		PROMOTION = 0,
		DEMOTION = 1,
		MOTD = 2,
		JOINED = 3,
		LEFT = 4,
		REMOVED = 5,
		LEADER_IS = 6,
		LEADER_CHANGED = 7,
		DISBANDED = 8,
		TABARDCHANGE = 9,
		/// <summary>
		/// 3 Strings, 0 Additional
		/// 1: int
		/// 2: string
		/// 3: int
		/// </summary>
		Event_10 = 10,
		/// <summary>
		/// 1 String Arg, 0 Additional
		/// 1: int
		/// </summary>
		Event_11 = 11,
		/// <summary>
		/// 1 String arg, 1 Additional
		/// ulong guid
		/// </summary>
		ONLINE = 12,
		OFFLINE = 13,
		/// <summary>
		/// 0 Strings, 0 Additional
		/// </summary>
		Event_14 = 14,
		/// <summary>
		/// 0 Strings, 0 Additional
		/// </summary>
		BANKTABBOUGHT = 15,
		/// <summary>
		/// 3 Strings
		/// Strings: int, string, string
		/// </summary>
		Event_16 = 16,
		SETNEWBALANCE = 17,
		/// <summary>
		/// 0 Strings, 0 Additional
		/// Makes client send packet MSG_GUILD_BANK_MONEY_WITHDRAWN
		/// 
		/// </summary>
		Event_18 = 18,
		/// <summary>
		/// 1 String
		/// Strings: int
		/// </summary>
		Event_19 = 19,


	}

	/// <summary>
	/// Results of a guild command request
	/// </summary>
	public enum GuildResult
	{
		SUCCESS = 0,
		INTERNAL = 1,
		ALREADY_IN_GUILD = 2,
		ALREADY_IN_GUILD_S = 3,
		INVITED_TO_GUILD = 4,
		ALREADY_INVITED_TO_GUILD = 5,
		NAME_INVALID = 6,
		NAME_EXISTS = 7,
		PERMISSIONS = 8,
		LEADER_LEAVE = 8,
		PLAYER_NOT_IN_GUILD = 9,
		PLAYER_NOT_IN_GUILD_S = 10,
		PLAYER_NOT_FOUND = 11,
		NOT_ALLIED = 12,
		PlayerRankTooHigh = 13,
		PLAYER_ALREADY_NOOB = 14,
		TEMPORARY_ERROR = 17,
		GUILD_RANK_IN_USE = 18,
		PLAYER_IGNORING_YOU = 19,
	}

	/// <summary>
	/// Results of a Set Tabard request
	/// </summary>
	public enum GuildTabardResult
	{
		Success = 0,
		InvalidTabardColors = 1,
		NoGuild = 2,
		NotGuildMaster = 3,
		NotEnoughMoney = 4,
		InvalidVendor = 5
	}

	/// <summary>
	/// Common guild commands
	/// </summary>
	public enum GuildCommandId
	{
		CREATE = 0,
		INVITE = 1,
		QUIT = 2, // not found in 3.0.9
		PROMOTE = 3,
		Unknown1 = 10,
		FOUNDER = 12,// not found in 3.0.9
		MEMBER = 13, // not found in 3.0.9
		BANK = 0x15,
		PUBLIC_NOTE_CHANGED = 19,
		OFFICER_NOTE_CHANGED = 20
	}

	/// <summary>
	/// Rights of a Guild Member
	/// </summary>
	[Flags]
	public enum GuildPrivileges : uint
	{
		EMPTY = 0x000040,
		GCHATLISTEN = 0x000041,
		GCHATSPEAK = 0x000042,
		OFFCHATLISTEN = 0x000044,
		OFFCHATSPEAK = 0x000048,
		INVITE = 0x000050,
		REMOVE = 0x000060,
		PROMOTE = 0x0000C0,
		DEMOTE = 0x000140,
		SETMOTD = 0x001040,
		EPNOTE = 0x002040,
		VIEWOFFNOTE = 0x004040,
		EOFFNOTE = 0x008040,
		EGUILDINFO = 0x010040,
		WITHDRAW_GOLD_LOCK = 0x020000,
		WITHDRAW_REPAIR = 0x040000,
		WITHDRAW_GOLD = 0x080000,
		CREATE_EVENT = 0x100000,
		ALL = 0x1DF1FF,
		DEFAULT = EMPTY | GCHATLISTEN | GCHATSPEAK
	}

	[Flags]
	public enum GuildBankTabPrivileges : uint
	{
		None = 0x00,
		ViewTab = 0x01,
		PutItem = 0x02,
		UpdateText = 0x04,
		DepositItem = (ViewTab | PutItem),
		Full = 0xFF
	}

	public enum GuildEventLogEntryType : byte
	{
		INVITE_PLAYER = 1,
		JOIN_GUILD = 2,
		PROMOTE_PLAYER = 3,
		DEMOTE_PLAYER = 4,
		UNINVITE_PLAYER = 5,
		LEAVE_GUILD = 6,
	}

	public enum GuildBankLogEntryType : uint
	{
		None = 0,
		DepositItem = 1,
		WithdrawItem = 2,
		MoveItem = 3,
		DepositMoney = 4,
		WithdrawMoney = 5,
		MoneyUsedForRepairs = 6,
		MoveItem_2 = 7,
		Unknown1 = 8,
		Unknown2 = 9
	}

	public static class _GuildEnumExtensions
	{
		public static bool HasFlag(this GuildPrivileges flags, GuildPrivileges toCheck)
		{
			return (flags & toCheck) != GuildPrivileges.EMPTY;
		}

		public static bool HasFlag(this GuildBankTabPrivileges flags, GuildBankTabPrivileges toCheck)
		{
			return (flags & toCheck) != GuildBankTabPrivileges.None;
		}
	}
}
