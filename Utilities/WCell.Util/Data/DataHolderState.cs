using System;

namespace WCell.Util.Data
{
	[Flags]
	public enum DataHolderState : uint
	{
		/// <summary>
		/// No changes
		/// </summary>
		Steady = 0,

		/// <summary>
		/// New DataHolder
		/// </summary>
		JustCreated = 1,

		/// <summary>
		/// Old DataHolder with new data
		/// </summary>
		Dirty = 2
	}
}