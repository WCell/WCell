using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.Spells
{
	public static class SpellConstants
	{

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
		}

		public static bool IsNegative(this SpellMechanic mech)
		{
			return NegativeMechanics[(int)mech];
		}
		#endregion
	}
}