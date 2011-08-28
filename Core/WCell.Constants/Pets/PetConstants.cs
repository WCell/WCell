namespace WCell.Constants.Pets
{
	public static class PetConstants
	{
		public const int PetActionCount = 10;
		public const int PetSpellCount = 4;		// amount of pet spells in the pet spell bar
	}

    public enum PetLoyaltyLevel : uint
    {
        Rebelious = 1,
        Unruly = 2,
        Submissive = 3,
        Dependable = 4,
        Faithful = 5,
        BestFriend = 6
    }

    public enum StableResult : byte
    {
        NotEnoughMoney = 0x01,
        Fail = 0x06,
        StableSuccess = 0x08,
        DeStableSuccess = 0x09,
        BuySlotSuccess = 0x0A
    }
}