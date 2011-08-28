namespace WCell.Constants.Updates
{
	/// <summary>
	/// UpdatePriority determines required frequency of Updates for an Action
	/// </summary>
	public enum UpdatePriority
	{
		/// <summary>
		/// There are no nearby Players.
		/// </summary>
		Inactive,

		/// <summary>
		/// Very slow updates
		/// </summary>
		Background,

		/// <summary>
		/// Nothing special, barely needs Updating (misc Actions, like Roaming, Idling etc)
		/// </summary>
		VeryLowPriority,

		/// <summary>
		/// Could use Updates every once in a while (moving, following)
		/// </summary>
		LowPriority,

		/// <summary>
		/// Needs quite frequent Updates (Combat etc)
		/// </summary>
		Active,

		/// <summary>
		/// Does something that requires the highest possible Update-frequency
		/// </summary>
		HighPriority,
		End
	}
}