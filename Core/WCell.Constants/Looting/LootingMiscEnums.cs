namespace WCell.Constants.Looting
{	/// <summary>
	/// Loot method
	/// </summary>
	public enum LootMethod : uint
	{
		FreeForAll = 0,
		RoundRobin = 1,
		MasterLoot = 2,
		GroupLoot = 3,
		NeedBeforeGreed = 4,
		End
	}

	public enum LootResponseType : byte
	{
		Fail = 0,
		Default = 1,
		Profession
	}

	public enum LootRollType : byte
	{
		Pass = 0,
		Need = 1,
		Greed = 2,
		Disenchant = 3,
		NotEmited,
		Invalid
	}

	public enum LootDecision : byte
	{
		/// <summary>
		/// Free to be taken
		/// </summary>
		Free = 0,
		/// <summary>
		/// Must be rolled for
		/// </summary>
		Rolling = 1,
		/// <summary>
		/// MasterLooter decides
		/// </summary>
		Master = 2
	}
}