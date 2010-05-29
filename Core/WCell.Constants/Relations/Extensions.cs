namespace WCell.Constants.Relations
{
	public static class Extensions
	{

		#region HasAnyFlag (thanks Microsoft, for giving us HasFlag, but not HasAnyFlag)
		public static bool HasAnyFlag(this RelationTypeFlag flags, RelationTypeFlag otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}
