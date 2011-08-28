using System;
using System.Linq;
using WCell.Util;

namespace WCell.Constants.Spells
{
	public static class SpellConstants
	{
		public const int SpellClassMaskSize = 3;

		#region Mechanic Influences

		/// <summary>
		/// Several Hashsets containing all SpellMechanics that can toggle 
		/// CanHarm, CanMove and CanCastSpells respectively
		/// </summary>
		public static readonly bool[] HarmPreventionMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] MoveMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] InteractMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] SpellCastPreventionMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] NegativeMechanics = new bool[(int)SpellMechanic.End];

		static SpellConstants()
		{
			MoveMechanics[(int)SpellMechanic.Disoriented] = true;
			MoveMechanics[(int)SpellMechanic.Asleep] = true;
			MoveMechanics[(int)SpellMechanic.Incapacitated] = true;
			MoveMechanics[(int)SpellMechanic.Frozen] = true;
			MoveMechanics[(int)SpellMechanic.Rooted] = true;
			MoveMechanics[(int)SpellMechanic.Stunned] = true;
			MoveMechanics[(int)SpellMechanic.Horrified] = true;
			MoveMechanics[(int)SpellMechanic.Shackled] = true;
			MoveMechanics[(int)SpellMechanic.Turned] = true;
			MoveMechanics[(int)SpellMechanic.Sapped] = true;
			MoveMechanics[(int)SpellMechanic.Snared] = true;

			InteractMechanics[(int)SpellMechanic.Disoriented] = true;
			InteractMechanics[(int)SpellMechanic.Asleep] = true;
			InteractMechanics[(int)SpellMechanic.Charmed] = true;
			InteractMechanics[(int)SpellMechanic.Frozen] = true;
			InteractMechanics[(int)SpellMechanic.Incapacitated] = true;
			InteractMechanics[(int)SpellMechanic.Stunned] = true;
			InteractMechanics[(int)SpellMechanic.Fleeing] = true;
			InteractMechanics[(int)SpellMechanic.Horrified] = true;
			InteractMechanics[(int)SpellMechanic.Shackled] = true;
			InteractMechanics[(int)SpellMechanic.Turned] = true;
			InteractMechanics[(int)SpellMechanic.Sapped] = true;

			//HarmMechanics[(int)SpellMechanic.Dazed] = true;
			HarmPreventionMechanics[(int)SpellMechanic.Fleeing] = true;
			HarmPreventionMechanics[(int)SpellMechanic.Frozen] = true;
			HarmPreventionMechanics[(int)SpellMechanic.Healing] = true;
			HarmPreventionMechanics[(int)SpellMechanic.Banished] = true;
			HarmPreventionMechanics[(int)SpellMechanic.Incapacitated] = true;
			HarmPreventionMechanics[(int)SpellMechanic.Stunned] = true;

			SpellCastPreventionMechanics[(int)SpellMechanic.Silenced] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Disoriented] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Fleeing] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Incapacitated] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Asleep] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Charmed] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Banished] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Horrified] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Turned] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Stunned] = true;
			SpellCastPreventionMechanics[(int)SpellMechanic.Frozen] = true;

			foreach (SpellMechanic mech in Enum.GetValues(typeof(SpellMechanic)))
			{
				if (mech >= SpellMechanic.End)
				{
					break;
				}

				if (MoveMechanics[(int)mech] || InteractMechanics[(int)mech] ||
					HarmPreventionMechanics[(int)mech] || SpellCastPreventionMechanics[(int)mech])
				{
					NegativeMechanics[(int)mech] = true;
				}
			}

			//InitRunes();
		}

		public static bool IsNegative(this SpellMechanic mech)
		{
			return NegativeMechanics[(int)mech];
		}
		#endregion

		#region Runes

		/// <summary>
		/// Amount of different types of runes (3)
		/// </summary>
		public const int StandardRuneTypeCount = 3;
		public const int MaxRuneCount = 6;

		/// <summary>
		/// Amount of runes per type (usually 2)
		/// </summary>
		public const int MaxRuneCountPerType = MaxRuneCount / StandardRuneTypeCount;

		/// <summary>
		/// Amount of bits that are necessary to store a single rune's type.
		/// Start counting from 1 to End, instead of 0 to End - 1
		/// </summary>
		public static readonly int BitsPerRune = (int)(Math.Log((int)RuneType.End + 1, 2) + 0.9999999);	// always round up

		/// <summary>
		/// BitsPerRune 1 bits to mask away anything but a single rune's bit set
		/// </summary>
		public static readonly int SingleRuneFullBitMask = (1 << BitsPerRune) - 1;

		public static readonly uint[,] IndicesPerType = new[,]
		{
			{0u, 1u},
			{2u, 3u},
			{4u, 5u}
		};

		/// <summary>
		/// Default rune layout, 2 of every kind, in this order
		/// </summary>
		public static readonly RuneType[] DefaultRuneSet = new[]
		{
			RuneType.Blood, RuneType.Blood,
			RuneType.Unholy, RuneType.Unholy,
			RuneType.Frost, RuneType.Frost
		};
		#endregion

		public static readonly DamageSchool[] AllDamageSchools =
			((DamageSchool[])Enum.GetValues(typeof(DamageSchool))).Except(new[] { DamageSchool.Count }).ToArray();

		public static readonly uint[] AllDamageSchoolSet = Utility.GetSetIndices((uint)DamageSchoolMask.AllSchools);

		public static readonly uint[] MagicDamageSchoolSet = Utility.GetSetIndices((uint)DamageSchoolMask.MagicSchools);
	}
}