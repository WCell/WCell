namespace WCell.Constants.Factions
{
	/// <summary>
	/// The actual reputation value of each Standing
	/// </summary>
	public enum Standing : int
	{
		Exalted = 42000,
		Revered = 21000,
		Honored = 9000,
		Friendly = 3000,
		Neutral = 0,
		Unfriendly = -3000,
		Hostile = -6000,
		Hated = -42000
	}

	/// <summary>
	/// The level of each standing (0 - hated to 7 - exhalted)
	/// </summary>
	public enum StandingLevel : uint
	{
		Exalted = 7,
		Revered = 6,
		Honored = 5,
		Friendly = 4,
		Neutral = 3,
		Unfriendly = 2,
		Hostile = 1,
		Hated = 0,
		Unknown = Hated
	}
}