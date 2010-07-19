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
		public static readonly bool[] HarmMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] MoveMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] InteractMechanics = new bool[(int)SpellMechanic.End];
		public static readonly bool[] SpellMechanics = new bool[(int)SpellMechanic.End];
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
			HarmMechanics[(int)SpellMechanic.Fleeing] = true;
			HarmMechanics[(int)SpellMechanic.Frozen] = true;
			HarmMechanics[(int)SpellMechanic.Healing] = true;
			HarmMechanics[(int)SpellMechanic.Banished] = true;
			HarmMechanics[(int)SpellMechanic.Incapacitated] = true;
			HarmMechanics[(int)SpellMechanic.Stunned] = true;

			SpellMechanics[(int)SpellMechanic.Silenced] = true;
			SpellMechanics[(int)SpellMechanic.Disoriented] = true;
			SpellMechanics[(int)SpellMechanic.Fleeing] = true;
			SpellMechanics[(int)SpellMechanic.Incapacitated] = true;
			SpellMechanics[(int)SpellMechanic.Asleep] = true;
			SpellMechanics[(int)SpellMechanic.Charmed] = true;
			SpellMechanics[(int)SpellMechanic.Banished] = true;
			SpellMechanics[(int)SpellMechanic.Horrified] = true;
			SpellMechanics[(int)SpellMechanic.Turned] = true;
			SpellMechanics[(int)SpellMechanic.Stunned] = true;
			SpellMechanics[(int)SpellMechanic.Frozen] = true;

			foreach (SpellMechanic mech in Enum.GetValues(typeof(SpellMechanic)))
			{
				if (mech >= SpellMechanic.End)
				{
					break;
				}

				if (MoveMechanics[(int)mech] || InteractMechanics[(int)mech] ||
					HarmMechanics[(int)mech] || SpellMechanics[(int)mech])
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