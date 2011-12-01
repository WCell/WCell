using System;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.GameObjects
{
	public partial class GOEntry
	{
		public delegate bool GOUseHandler(GameObject go, Character user);

		/// <summary>
		/// Is called whenever an instance of this entry is used by the given user
		/// </summary>
		public event GOUseHandler Used;

		/// <summary>
		/// Is called whenever a new GameObject of this entry is added to the world 
		/// (usually only happens once, when the GO is created)
		/// </summary>
		public event Action<GameObject> Activated;
	}
}