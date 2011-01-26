using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
	public abstract class SpawnPoint<T, E, O, POINT, POOL>
		where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
		where E : SpawnEntry<T, E, O, POINT, POOL>
		where O : WorldObject
		where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
		where POOL : SpawnPool<T, E, O, POINT, POOL>
	{
		protected E m_spawnEntry;
		protected internal TimerEntry m_timer;
		protected int m_nextRespawn;
		protected O m_spawnling;

		internal void InitPoint(POOL pool, E entry)
		{
			m_timer = new TimerEntry(SpawnNow);
			Pool = pool;
			m_spawnEntry = entry;
		}

		#region Properties
		public POOL Pool
		{
			get;
			protected set;
		}

		public Map Map
		{
			get { return Pool.Map; }
		}

		public MapId MapId
		{
			get { return Map.Id; }
		}

		public Vector3 Position
		{
			get { return m_spawnEntry.Position; }
		}

		public E SpawnEntry
		{
			get { return m_spawnEntry; }
		}

		public bool HasSpawned
		{
			get { return m_spawnling != null; }
		}

		/// <summary>
		/// Whether timer is running and will spawn a new NPC when the timer elapses
		/// </summary>
		public bool IsSpawning
		{
			get { return m_timer.IsRunning; }
		}

		/// <summary>
		/// Inactive and autospawns
		/// </summary>
		public bool IsReadyToSpawn
		{
			get { return !IsActive && m_spawnEntry.AutoSpawns; }
		}

		/// <summary>
		/// Whether NPC is alread spawned or timer is running
		/// </summary>
		public bool IsActive
		{
			get { return HasSpawned || IsSpawning; }
		}

		/// <summary>
		/// The currently active NPC of this SpawnPoint (or null)
		/// </summary>
		public O ActiveSpawnling
		{
			get { return m_spawnling; }
		}
		#endregion

		public void SpawnNow(int dtMillis = 0)
		{
			Map.AddMessage(() =>
			{
				m_spawnling = SpawnEntry.SpawnObject((POINT)this);

				Pool.SpawnedObjects.Add(m_spawnling);

				Map.UnregisterUpdatable(m_timer);
			});
		}

		public void SpawnLater()
		{
			SpawnAfter(m_spawnEntry.GetRandomRespawnMillis());
		}

		/// <summary>
		/// Restarts the spawn timer with the given delay
		/// </summary>
		public void SpawnAfter(int delay)
		{
			if (Pool.IsActive && !m_timer.IsRunning)
			{
				m_nextRespawn = Environment.TickCount + delay;
				m_timer.Start(delay);
				Map.RegisterUpdatableLater(m_timer);
			}
		}

		/// <summary>
		/// Stops the Respawn timer, if it was running
		/// </summary>
		public void StopTimer()
		{
			m_timer.Stop();
			Map.UnregisterUpdatable(m_timer);
		}

		/// <summary>
		/// Stops timer and deletes spawnling
		/// </summary>
		public void Disable()
		{
			StopTimer();
			if (HasSpawned)
			{
				m_spawnling.Delete();
			}
		}

		/// <summary>
		/// Is called when the given spawn died or was removed from Map.
		/// </summary>
		protected internal void SignalSpawnlingDied(O obj)
		{
			// remove object from set of spawned objects
			m_spawnling = null;
			Pool.SpawnedObjects.Remove(obj);

			if (m_spawnEntry.AutoSpawns)
			{
				// select a spawn to respawn
				Pool.SpawnOneLater();
			}
		}
	}
}
