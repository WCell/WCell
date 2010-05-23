

namespace WCell.Constants.Spells
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this SpellTargetFlags flags, SpellTargetFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this SpellAttributes flags, SpellAttributes otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this SpellAttributesEx flags, SpellAttributesEx otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}
