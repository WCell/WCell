using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Constants.World;

namespace WCell.Addons.Default
{
	/// <summary>
	/// Provides some hardcoded functions that are just a little nice-to-have.
	/// </summary>
	public static class UnitExtensions
	{
		//public static Vector3 StormwindLocation = new Vector3(-9005f, 869f, 29.621f);

		// Deprecated, use Unit.TeleportTo(ZoneId) instead
		//public static void TeleportToStormwind(this Unit unit)
		//{
		//    unit.TeleportTo(MapId.EasternKingdoms, ref StormwindLocation);
		//}
	}
}
