using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Constants.NPCs
{
	public static class Extensions
	{
		#region HasAnyFlag
		public static bool HasAnyFlag(this VehicleFlags flags, VehicleFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this VehicleSeatFlags flags, VehicleSeatFlags otherFlags)
		{
			return (flags & otherFlags) != 0;
		}

		public static bool HasAnyFlag(this VehicleSeatFlagsB flags, VehicleSeatFlagsB otherFlags)
		{
			return (flags & otherFlags) != 0;
		}
		#endregion
	}
}
