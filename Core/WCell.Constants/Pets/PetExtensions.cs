using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Login;
using WCell.Constants.NPCs;
using WCell.Constants.Items;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.Pets;
using WCell.Constants.Talents;
using WCell.Constants.Updates;
using WCell.Util;

namespace WCell.Constants.Pets
{
	public static class PetExtensions
	{
		private static readonly TalentTreeId[] TreesByPetTalentType = new TalentTreeId[(int)PetTalentType.End + 1];

		static PetExtensions()
		{
			TreesByPetTalentType[(uint)PetTalentType.Cunning] = TalentTreeId.PetTalentsCunning;
			TreesByPetTalentType[(uint)PetTalentType.Ferocity] = TalentTreeId.PetTalentsFerocity;
			TreesByPetTalentType[(uint)PetTalentType.Tenacity] = TalentTreeId.PetTalentsTenacity;
			TreesByPetTalentType[(uint)PetTalentType.End] = TalentTreeId.None;
		}

		#region HasAnyFlag
		public static bool HasAnyFlag(this PetFoodMask flags, PetFoodMask otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this PetFoodMask flags, PetFoodType foodType)
		{
			return (flags & (PetFoodMask)(1 << ((int)foodType-1))) != 0;
		}
		#endregion

		public static TalentTreeId GetTalentTreeId(this PetTalentType type)
		{
			return TreesByPetTalentType[(int) type];
		}
	}
}