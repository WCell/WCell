

namespace WCell.Util.Graphics
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this IntersectionType flags, IntersectionType otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}