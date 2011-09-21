using WCell.Constants.Talents;

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