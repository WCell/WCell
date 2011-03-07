using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
	public abstract class SpawnPoolTemplate<T, E, O, POINT, POOL>
		where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
		where E : SpawnEntry<T, E, O, POINT, POOL>
		where O : WorldObject
		where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
		where POOL : SpawnPool<T, E, O, POINT, POOL>
	{
		private static int highestId = 1000000;

		public uint PoolId;

		public int MaxSpawnAmount;

		public int RealMaxSpawnAmount
		{
			get { return Math.Min(MaxSpawnAmount, Entries.Count(entry => entry.AutoSpawns)); }
		}

		/// <summary>
		/// Whether any SpawnEntry has AutoSpawns set to true
		/// </summary>
		[NotPersistent]
		public bool AutoSpawns { get; internal set; }

		[NotPersistent]
		public MapId MapId = MapId.End;

		[NotPersistent]
		public List<E> Entries = new List<E>(5);

		/// <summary>
		/// It would not make sense to create a pool that contains entries of different events
		/// </summary>
		public uint EventId
		{
			get
			{
				// can assume that at least one Entry is present, else the pool would not be created
				return (uint)Entries[0].EventId;
			}
		}

		protected SpawnPoolTemplate(uint id, int maxSpawnAmount)
		{
			AutoSpawns = false;
			PoolId = id != 0 ? id : (uint)Interlocked.Increment(ref highestId);
			MaxSpawnAmount = maxSpawnAmount != 0 ? maxSpawnAmount : int.MaxValue;
		}

		protected SpawnPoolTemplate(SpawnPoolTemplateEntry entry)
			: this(entry.PoolId, entry.MaxSpawnAmount)
		{
		}

		public abstract List<T> PoolTemplatesOnSameMap { get; }

		public void AddEntry(E entry)
		{
			if (MapId == MapId.End)
			{
				// set map and add to map's set of SpawnPools
				MapId = entry.MapId;
				if (entry.MapId != MapId.End)
				{
					PoolTemplatesOnSameMap.Add((T)this);
				}
			}
			else if (entry.MapId != MapId)
			{
				// make sure, Map is the same
				LogManager.GetCurrentClassLogger().Warn("Tried to add \"{0}\" with map = \"{1}\" to a pool that contains Entries of Map \"{2}\"",
					entry, entry.MapId, MapId);
				return;
			}

			// add entry
			Entries.Add(entry);
			AutoSpawns = AutoSpawns || entry.AutoSpawns;
		}
	}
}
