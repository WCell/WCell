using System.Collections.Generic;
using System.Linq;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Spawns;
using WCell.Util;

namespace WCell.RealmServer.NPCs.Spawns
{
	public class NPCSpawnPool : SpawnPool<NPCSpawnPool, NPCSpawnPoolTemplate, NPCSpawnPoint, NPCSpawnEntry, NPC>
	{
		protected internal AIGroup m_spawnlings;

		public NPCSpawnPool(Map map, NPCSpawnPoolTemplate templ)
		{
			m_spawnlings = new AIGroup();
			Map = map;
			Template = templ;

			// add SpawnPoints
			foreach (var entry in templ.Entries)
			{
				AddSpawnPoint(entry);
			}
		}

		/// <summary>
		/// Whether all SpawnPoints of this pool are spawned
		/// </summary>
		public bool IsFullySpawned
		{
			get
			{
				var count = m_spawnPoints.Count(spawn => !spawn.IsActive);
				return count >= Template.RealMaxSpawnAmount;
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

						if (m_spawnlings.Count == 0)
						{
							// first time start
							SpawnFull();
							//Reset(m_spawnEntry.RandomNormalDelay());
						}
						else if (m_spawnlings.Count < Template.RealMaxSpawnAmount)
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

		public NPCSpawnPoint GetSpawnPoint(NPCSpawnEntry entry)
		{
			return GetSpawnPoint(entry.SpawnId);
		}

		public NPCSpawnPoint GetSpawnPoint(uint spawnId)
		{
			Map.EnsureContext();

			return SpawnPoints.FirstOrDefault(point => point.SpawnEntry.SpawnId == spawnId);
		}

		/// <summary>
		/// Returns a spawn point that is currently inactive
		/// </summary>
		public NPCSpawnPoint GetRandomInactiveSpawnPoint()
		{
			Map.EnsureContext();

			var totalProb = 0f;
			var totalCount = 0;
			var zeroCount = 0;

			// count and calculate total probability
			foreach (var spawn in m_spawnPoints)
			{
				if (!spawn.IsActive)
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
				if (!spawn.IsActive)
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
			NPCSpawnPoint point;
			for (var i = m_spawnlings.Count; i < Template.RealMaxSpawnAmount && (point = GetRandomInactiveSpawnPoint()) != null; i++)
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

		internal NPCSpawnPoint AddSpawnPoint(NPCSpawnEntry entry)
		{
			var point = new NPCSpawnPoint(this, entry);
			m_spawnPoints.Add(point);
			return point;
		}

		/// <summary>
		/// Removes all spawned NPCs from this pool and makes it
		/// collectable by the GC (if not in Map anymore).
		/// </summary>
		/// <remarks>Requires map context</remarks>
		public void Clear()
		{
			Map.EnsureContext();

			var arr = m_spawnlings.ToArray();
			for (var i = 0; i < arr.Length; i++)
			{
				var spawn = arr[i];
				spawn.Delete();
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
				IsActive = false;					// disable all timers
				Clear();						// remove all NPCs
				Map.RemoveNPCSpawnPool(this);	// remove Pool
				Map = null;						// cleanup
			}
		}
	}
}
