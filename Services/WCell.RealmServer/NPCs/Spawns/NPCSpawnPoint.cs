using System;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs.Spawns
{
	/// <summary>
	/// Represents a SpawnPoint that continuesly spawns NPCs.
	/// </summary>
	public class NPCSpawnPoint : IWorldLocation
	{
		protected NPCSpawnEntry m_spawnEntry;
		protected internal TimerEntry m_timer;
		protected int m_nextRespawn;
		protected NPC m_spawnling;

		internal NPCSpawnPoint(NPCSpawnPool pool, NPCSpawnEntry entry)
		{
			m_timer = new TimerEntry(SpawnNow);
			Pool = pool;
			m_spawnEntry = entry;
		}

		#region Properties
		public NPCSpawnPool Pool
		{
			get;
			private set;
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

		public NPCSpawnEntry SpawnEntry
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
		/// Whether NPC is alread spawned or timer is running
		/// </summary>
		public bool IsActive
		{
			get { return HasSpawned || IsSpawning; }
		}

		/// <summary>
		/// The currently active NPC of this SpawnPoint (or null)
		/// </summary>
		public NPC ActiveSpawnling
		{
			get { return m_spawnling; }
		}
		#endregion

		public void SpawnNow(int dtMillis = 0)
		{
			m_spawnling = SpawnEntry.Entry.Create(this);

			Pool.m_spawnlings.Add(m_spawnling);
			m_spawnling.Position = m_spawnEntry.Position;
			Map.AddObjectNow(m_spawnling);
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
				m_timer.Start(delay, 0);
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
		protected internal void SignalSpawnlingDied(NPC npc)
		{
			m_spawnling = null;
			Pool.m_spawnlings.Remove(npc);

			SpawnLater();
		}
	}
}