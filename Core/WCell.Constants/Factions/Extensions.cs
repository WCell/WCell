
namespace WCell.Constants.Factions
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this FactionGroupMask flags, FactionGroupMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}
