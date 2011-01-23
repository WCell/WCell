using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using WCell.Constants.World;
using WCell.Util.Data;

namespace WCell.RealmServer.NPCs.Spawns
{
	[DataHolder]
	public class NPCSpawnPoolTemplate : IDataHolder
	{
		private static int highestId;

		public uint PoolId;

		public int MaxSpawnAmount;

		public int RealMaxSpawnAmount
		{
			get { return Math.Min(MaxSpawnAmount, Entries.Count); }
		}

		[NotPersistent]
		public bool AutoSpawns = false;

		[NotPersistent]
		public MapId MapId = MapId.End;

		[NotPersistent]
		public List<NPCSpawnEntry> Entries = new List<NPCSpawnEntry>(5);

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

		internal void AddEntry(NPCSpawnEntry entry)
		{
			if (MapId == MapId.End)
			{
				// set map and add to map's set of SpawnPools
				MapId = entry.MapId;
				NPCMgr.GetOrCreateSpawnPoolTemplatesByMap(MapId).Add(this);
			}
			else if (entry.MapId != MapId)
			{
				// make sure, Map is the same
				LogManager.GetCurrentClassLogger().Warn("Tried to add NPCSpawnEntry \"{0}\" with map = \"{1}\" to a pool that contains Entries of Map \"{2}\"",
					entry, entry.MapId, MapId);
				return;
			}

			// add entry
			Entries.Add(entry);
			AutoSpawns = AutoSpawns || entry.AutoSpawns;
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
