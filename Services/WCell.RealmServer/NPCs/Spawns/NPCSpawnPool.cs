using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spawns;
using WCell.Util;

namespace WCell.RealmServer.NPCs.Spawns
{
	public class NPCSpawnPool : SpawnPool<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>
	{
		public NPCSpawnPool(Map map, NPCSpawnPoolTemplate templ)
		{
			AIGroup = new AIGroup();
			Map = map;
			Template = templ;

			// add SpawnPoints
			foreach (var entry in templ.Entries)
			{
				AddSpawnPoint(entry);
			}
		}

		public AIGroup AIGroup
		{
			get;
			private set;
		}

		public override IList<NPC> SpawnedObjects
		{
			get { return AIGroup; }
		}
	}
}
