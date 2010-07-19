using System;
using WCell.RealmServer.Chat;

namespace WCell.RealmServer.Global
{
	public partial class World
	{
		/// <summary>
		/// Is called after the World paused or unpaused completely and before the PauseLock is released.
		/// </summary>
		public static event Action<bool> WorldPaused;

		/// <summary>
		/// Is called after the World has been saved and before the PauseLock is released (eg. during shutdown)
		/// </summary>
		public static event Action Saved;

		/// <summary>
		/// Is called when the given Chatter (can be null) broadcasts something
		/// </summary>
		public static event Action<IChatter, string> Broadcasted;
	}
}