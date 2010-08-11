
namespace WCell.Constants.Spells
{
	public static class SpellConstantsExtensions
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

		public static bool HasAnyFlag(this AuraStateMask mask, AuraState state)
		{
			return (mask & (AuraStateMask)(1 << ((int)state - 1))) != 0;
		}

		public static bool HasAnyFlag(this AuraStateMask mask, AuraStateMask mask2)
		{
			return (mask & mask2) != 0;
		}

		public static bool HasAnyFlag(this DamageSchoolMask flags, DamageSchoolMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this DamageSchoolMask flags, DamageSchool school)
		{
			return (flags & (DamageSchoolMask)(1 << (int)school)) != 0;
		}

		public static RuneMask ToMask(this RuneType type)
		{
			return (RuneMask)(1 << (int)type);
		}

		public static bool HasAnyFlag(this RuneMask mask, RuneMask mask2)
		{
			return (mask & mask2) != 0;
		}

		public static bool HasAnyFlag(this RuneMask mask, RuneType type)
		{
			return (mask & type.ToMask()) != 0;
		}
		#endregion
	}
}