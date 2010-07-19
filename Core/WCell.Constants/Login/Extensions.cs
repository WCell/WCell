namespace WCell.Constants.Login
{
	public static class Extensions
	{

		#region HasAnyFlag (thanks Microsoft, for giving us HasFlag, but not HasAnyFlag)
		public static bool HasAnyFlag(this RealmServerType flags, RealmServerType otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this CharEnumFlags flags, CharEnumFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}