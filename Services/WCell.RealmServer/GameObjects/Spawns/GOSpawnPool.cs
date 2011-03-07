using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spawns;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.Spawns
{
	public class GOSpawnPool : SpawnPool<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>
	{
		public GOSpawnPool(Map map, GOSpawnPoolTemplate templ) : base(map, templ)
		{
			GameObjects = new List<GameObject>();
		}

		public List<GameObject> GameObjects
		{
			get;
			private set;
		}

		public override IList<GameObject> SpawnedObjects
		{
			get { return GameObjects; }
		}
	}
}
