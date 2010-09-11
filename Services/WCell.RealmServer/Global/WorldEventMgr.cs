using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util;

namespace WCell.RealmServer.Global
{	
	/// <summary>
	/// Manages world events
	/// TODO: Load events
	/// </summary>
	public static class WorldEventMgr
	{
		internal static WorldEvent[] AllEvents = new WorldEvent[100];
		internal static WorldEvent[] ActiveEvents = new WorldEvent[100];
		
		#region Manage Events
		public static WorldEvent GetEvent(uint id)
		{
			return AllEvents.Get(id);
		}
		#endregion

		#region Active Events
		public static bool IsEventActive(uint id)
		{
			return id == 0 || ActiveEvents.Get(id) != null;
		}

		public static IEnumerable<WorldEvent> GetActiveEvents()
		{
			return ActiveEvents.Where(evt => evt != null);
		}
		#endregion
	}
}
