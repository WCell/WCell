
namespace WCell.Constants.Updates
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this ObjectTypes flags, ObjectTypes otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this ObjectTypeCustom flags, ObjectTypeCustom otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this UpdateFieldFlags flags, UpdateFieldFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this UpdateFlags flags, UpdateFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}