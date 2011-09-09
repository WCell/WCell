using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Global
{
	public partial class MapTemplate
	{
		/// <summary>
		/// Called when a new Map of this MapInfo has been created.
		/// </summary>
		public event Action<Map> Created;

		/// <summary>
		/// Called after some Map has started
		/// </summary>
		public event Action<Map> Started;

		/// <summary>
		/// Called when this Map is about to add default spawn points.
		/// The returned value determines whether spawning should commence or be skipped.
		/// </summary>
		public event Func<Map, bool> Spawning;

		/// <summary>
		/// Called when this Map added all default spawns and objects.
		/// </summary>
		public event Action<Map> Spawned;

		/// <summary>
		/// Called when this Map is about to stop.
		/// The returned value determines whether the Map should stop.
		/// </summary>
		public event Func<Map, bool> Stopping;

		///// <summary>
		///// Called when this Map has stopped
		///// </summary>
		//public event Action<Map> Stopped;

		#region Characters
		/// <summary>
		/// Called when the given Character enters this Map.
		/// </summary>
		public event Action<Map, Character> PlayerEntered;

		/// <summary>
		/// Called when the given Character left this Map (Character might already be logging off).
		/// </summary>
		public event Action<Map, Character> PlayerLeft;
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