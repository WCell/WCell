using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Global
{
	public partial class Region
	{
		/// <summary>
		/// Called after a new Region has been created.
		/// </summary>
		public static event Action<Region> Created;
	}

	public partial class RegionInfo
	{
		/// <summary>
		/// Called when a new Region of this RegionInfo has been created.
		/// </summary>
		public event Action<Region> Created;

		/// <summary>
		/// Called after some Region has started
		/// </summary>
		public event Action<Region> Started;

		/// <summary>
		/// Called when this Region is about to add default spawn points.
		/// The returned value determines whether spawning should commence or be skipped.
		/// </summary>
		public event Func<Region, bool> Spawning;

		/// <summary>
		/// Called when this Region added all default spawns and objects.
		/// </summary>
		public event Action<Region> Spawned;

		/// <summary>
		/// Called when this Region is about to stop.
		/// The returned value determines whether the Region should stop.
		/// </summary>
		public event Func<Region, bool> Stopping;

		///// <summary>
		///// Called when this Region has stopped
		///// </summary>
		//public event Action<Region> Stopped;

		#region Characters
		/// <summary>
		/// Called when the given Character enters this Region.
		/// </summary>
		public event Action<Region, Character> PlayerEntered;

		/// <summary>
		/// Called when the given Character left this Region (Character might already be logging off).
		/// </summary>
		public event Action<Region, Character> PlayerLeft;
		/// <summary>
		/// Is called before a Character dies.
		/// If false is returned, the Character won't die.
		/// </summary>
		public static event Func<Character, bool> PlayerBeforeDeath;

		/// <summary>
		/// Is called when the Target of the given action died
		/// </summary>
		public static event Action<IDamageAction> PlayerDied;

		// TODO: Add resurrecter as argument
		/// <summary>
		/// Is called when the given Character has been resurrected
		/// </summary>
		public static event Action<Character> PlayerResurrected;
		#endregion

		#region Other Objects
		#endregion
	}
}