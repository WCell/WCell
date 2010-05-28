
namespace WCell.Constants.Guilds
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this GuildPrivileges flags, GuildPrivileges otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		public static bool HasAnyFlag(this GuildBankTabPrivileges flags, GuildBankTabPrivileges otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}
