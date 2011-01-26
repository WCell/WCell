using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;

namespace WCell.RealmServer.Spawns
{
	public abstract class SpawnPool<T, E, O, POINT, POOL>
		where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
		where E : SpawnEntry<T, E, O, POINT, POOL>
		where O : WorldObject
		where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
		where POOL : SpawnPool<T, E, O, POINT, POOL>
	{
		protected internal List<POINT> m_spawnPoints = new List<POINT>(5);
		protected bool m_active;

		protected SpawnPool(Map map, T templ)
		{
			Map = map;
			Template = templ;

			// add SpawnPoints
			foreach (var entry in templ.Entries)
			{
				AddSpawnPoint(entry);
			}
		}

		public Map Map
		{
			get;
			protected set;
		}

		public T Template
		{
			get;
			protected set;
		}

		public abstract IList<O> SpawnedObjects { get; }

		public List<POINT> SpawnPoints
		{
			get { return m_spawnPoints; }
		}

		/// <summary>
		/// Whether all SpawnPoints of this pool are spawned
		/// </summary>
		public bool IsFullySpawned
		{
			get
			{
				var count = m_spawnPoints.Count(spawn => spawn.IsReadyToSpawn);
				return count == 0;
			}
		}

		/// <summary>
		/// Whether this Spawn point is actively spawning.
		/// Requires map context.
		/// </summary>
		public bool IsActive
		{
			get
			{
				return m_active;
			}
			set
			{
				if (m_active != value)
				{
					m_active = value;
					if (m_active)
					{
						// activate

						if (SpawnedObjects.Count == 0)
						{
							// first time start
							SpawnFull();
							//Reset(m_spawnEntry.RandomNormalDelay());
						}
						else if (!IsFullySpawned)
						{
							// select spawn point and start respawn timer
							SpawnOneLater();
						}
					}
					else
					{
						// stop all spawning but don't remove spwaned NPCs from pool
						foreach (var spawn in m_spawnPoints)
						{
							spawn.StopTimer();
						}
					}
				}
			}
		}

		internal POINT AddSpawnPoint(E entry)
		{
			var point = new POINT();
			m_spawnPoints.Add(point);
			point.InitPoint((POOL)this, entry);
			return point;
		}

		public POINT GetSpawnPoint(E entry)
		{
			return GetSpawnPoint(entry.SpawnId);
		}

		public POINT GetSpawnPoint(uint spawnId)
		{
			Map.EnsureContext();

			return SpawnPoints.FirstOrDefault(point => point.SpawnEntry.SpawnId == spawnId);
		}

		/// <summary>
		/// Returns a spawn point that is currently inactive
		/// </summary>
		public POINT GetRandomInactiveSpawnPoint()
		{
			Map.EnsureContext();

			var totalProb = 0f;
			var totalCount = 0;
			var zeroCount = 0;

			// count and calculate total probability
			foreach (var spawn in m_spawnPoints)
			{
				if (spawn.IsReadyToSpawn)
				{
					var prob = spawn.SpawnEntry.PoolRespawnProbability;
					totalProb += prob;
					++totalCount;
					if (prob == 0)
					{
						++zeroCount;
					}
				}
			}

			if (totalCount == 0)
			{
				// no inactive spawns found
				return null;
			}

			var avgProb = 100f / totalCount;
			totalProb += zeroCount * avgProb;					// count avgProb for every entry that has no explicit probability
			var rand = Utility.RandomFloat() * totalProb;		// rand is in [0, totalProb)
			var accumulatedProb = 0f;

			// make individual tests
			foreach (var spawn in m_spawnPoints)
			{
				if (spawn.IsReadyToSpawn)
				{
					var prob = spawn.SpawnEntry.PoolRespawnProbability;
					if (prob == 0)
					{
						prob = avgProb;
					}
					accumulatedProb += prob;

					if (rand <= accumulatedProb)
					{
						return spawn;
					}
				}
			}

			// Shouldn't happen
			return null;
		}

		#region Spawning
		/// <summary>
		/// Spawns NPCs until MaxAmount of NPCs are spawned.
		/// </summary>
		public void SpawnFull()
		{
			Map.EnsureContext();
			POINT point;
			for (var i = SpawnedObjects.Count; i < Template.RealMaxSpawnAmount && (point = GetRandomInactiveSpawnPoint()) != null; i++)
			{
				point.SpawnNow();
			}
		}

		public void RespawnFull()
		{
			Clear();
			SpawnFull();
		}

		/// <summary>
		/// Spawns an NPC from a random inactive spawn point or returns false, if all SpawnPoints are active
		/// </summary>
		public bool SpawnOneNow()
		{
			var point = GetRandomInactiveSpawnPoint();
			if (point != null)
			{
				point.SpawnNow();
				return true;
			}
			return false;
		}

		public bool SpawnOneLater()
		{
			var point = GetRandomInactiveSpawnPoint();
			if (point != null)
			{
				point.SpawnLater();
				return true;
			}
			return false;
		}
		#endregion

		/// <summary>
		/// Removes all spawned NPCs from this pool and makes it
		/// collectable by the GC (if not in Map anymore).
		/// </summary>
		/// <remarks>Requires map context</remarks>
		public void Clear()
		{
			Map.EnsureContext();

			var arr = SpawnedObjects.ToArray();
			for (var i = 0; i < arr.Length; i++)
			{
				var spawn = arr[i];
				spawn.DeleteNow();
			}
		}

		public void RemovePoolLater()
		{
			Map.AddMessage(() => RemovePoolNow());
		}

		public void RemovePoolNow()
		{
			if (Map != null)
			{
				IsActive = false;											// disable all timers
				Clear();													// remove all NPCs
				Map.RemoveSpawnPool<T, E, O, POINT, POOL>((POOL)this);		// remove Pool
				Map = null;													// cleanup
			}
		}
	}
}
