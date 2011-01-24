using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spawns;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Spawns
{
	[DataHolder]
	public class NPCSpawnPoolTemplate : SpawnPoolTemplate<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>, IDataHolder
	{
		private static int highestId;

		public NPCSpawnPoolTemplate()
		{
		}

		/// <summary>
		/// Constructor for custom pools
		/// </summary>
		public NPCSpawnPoolTemplate(int maxSpawnAmount)
		{
			if (maxSpawnAmount == 0)
			{
				maxSpawnAmount = int.MaxValue;
			}
			PoolId = (uint)Interlocked.Increment(ref highestId);
			MaxSpawnAmount = maxSpawnAmount;
			FinalizeDataHolder();
		}

		/// <summary>
		/// Constructor for custom pools
		/// </summary>
		public NPCSpawnPoolTemplate(NPCSpawnEntry entry, int maxSpawnAmount = 0) :
			this(maxSpawnAmount)
		{
			AddEntry(entry);
		}

		public override List<NPCSpawnPoolTemplate> PoolTemplatesOnSameMap
		{
			get { return NPCMgr.GetOrCreateSpawnPoolTemplatesByMap(MapId); }
		}

		public void FinalizeDataHolder()
		{
			highestId = (int)Math.Max(highestId, PoolId);
			NPCMgr.SpawnPoolTemplates.Add(PoolId, this);
			if (MaxSpawnAmount == 0)
			{
				MaxSpawnAmount = int.MaxValue;
			}
		}
	}
}
