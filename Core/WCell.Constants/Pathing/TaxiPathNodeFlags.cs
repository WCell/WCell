using System;

namespace WCell.Constants.Pathing
{
	[Flags]
	public enum TaxiPathNodeFlags : byte
	{
		/// <summary>
		/// Show the teleport screen even if MapId doesnt change
		/// </summary>
		IsTeleport = 1,
		ArrivalOrDeparture = 2,
	}
}