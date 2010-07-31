
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

		public static bool HasAnyFlag(this ProcTriggerFlags flags, ProcTriggerFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this AuraStateMask flags, AuraState state)
		{
			return (flags & (AuraStateMask)(1 << ((int)state - 1))) != 0;
		}

		public static bool HasAnyFlag(this DamageSchoolMask flags, DamageSchoolMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this DamageSchoolMask flags, DamageSchool school)
		{
			return (flags & (DamageSchoolMask)(1 << (int)school)) != 0;
		}
		#endregion
	}
}