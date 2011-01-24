using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Spawns
{
	public abstract class SpawnPoolTemplate<T, E, O, POINT, POOL>
		where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
		where E : SpawnEntry<T, E, O, POINT, POOL>
		where O : WorldObject
		where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
		where POOL : SpawnPool<T, E, O, POINT, POOL>
	{
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
		public List<E> Entries = new List<E>(5);

		public abstract List<T> PoolTemplatesOnSameMap { get; }

		internal void AddEntry(E entry)
		{
			if (MapId == MapId.End)
			{
				// set map and add to map's set of SpawnPools
				MapId = entry.MapId;
				PoolTemplatesOnSameMap.Add((T)this);
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
	}
}
