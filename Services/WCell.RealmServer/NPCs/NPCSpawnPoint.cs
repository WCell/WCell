using System;
using System.Collections.Generic;
using System.Linq;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;
using WCell.Util.Data;
using WCell.Constants;
using WCell.RealmServer.Handlers;
using WCell.Constants.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs
{
	/// <summary>
	/// Represents a SpawnPoint that continuesly spawns NPCs.
	/// </summary>
	public class NPCSpawnPoint : IWorldLocation
	{
		public static SpellId ConnectingSpell = SpellId.ClassSkillDrainLifeRank1;
			//SpellId.ClassSkillDrainMana;

		protected NPCSpawnEntry m_spawnEntry;
		protected bool m_active;
		protected TimerEntry m_timer;
		protected int m_nextRespawn;
		protected ICollection<Unit> m_spawns;

		public Map Map
		{
			get;
			private set;
		}

		public MapId MapId
		{
			get { return Map.Id; }
		}

		public Vector3 Position
		{
			get { return m_spawnEntry.Position; }
		}

		public NPCSpawnPoint(NPCSpawnEntry entry, Map map)
		{
			m_spawns = new List<Unit>();
			m_timer = new TimerEntry { Action = Spawn };
			m_timer.Stop();
			Map = map;
			SpawnEntry = entry;
		}

		#region Properties
		public uint Id
		{
			get
			{
				return m_spawnEntry.SpawnId;
			}
			set
			{
				if (Id != m_spawnEntry.SpawnId)
				{
					SpawnEntry = NPCMgr.SpawnEntries[value];
				}
			}
		}

		public NPCSpawnEntry SpawnEntry
		{
			get
			{
				return m_spawnEntry;
			}
			set
			{
				if (m_spawnEntry != value)
				{
					m_spawnEntry = value;
					//Figurine = new SpawnFigurine(this);
				}
			}
		}

		/// <summary>
		/// Whether this Spawn point is actively spawning.
		/// Requires map context.
		/// </summary>
		public bool Active
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

						if (m_spawns.Count == 0)
						{
							// first time start
							SpawnFull();
							//Reset(m_spawnEntry.RandomNormalDelay());
						}
						else if (m_spawns.Count < m_spawnEntry.MaxAmount)
						{
							var now = Environment.TickCount;
							if (now >= m_nextRespawn)
							{
								Spawn(0);
							}
							else
							{
								Reset(m_nextRespawn - now);
							}
						}
					}
					else
					{
						// deactivate
						m_timer.Stop();
					}
				}
			}
		}
		#endregion

		#region Spawning
		/// <summary>
		/// Is called when the given spawn died or was removed from Map.
		/// </summary>
		protected internal void SignalSpawnlingDied(NPC npc)
		{
			m_spawns.Remove(npc);

			Reset(m_spawnEntry.GetRandomRespawnMillis());
		}


		/// <summary>
		/// Spawns NPCs until MaxAmount NPCs are spawned.
		/// Requires map context.
		/// </summary>
		public void SpawnFull()
		{
			for (int i = m_spawns.Count; i < m_spawnEntry.MaxAmount; i++)
			{
				SpawnOne();
			}
		}

		public void RespawnFull()
		{
			Clear();
			SpawnFull();
		}

		/// <summary>
		/// Restarts the spawn timer with the given delay
		/// </summary>
		public void Reset(int delay)
		{
			if (Active && !m_timer.IsRunning)
			{
				m_nextRespawn = Environment.TickCount + delay;
				m_timer.Start(delay, 0);
				Map.RegisterUpdatableLater(m_timer);
			}
		}

		/// <summary>
		/// Spawns a single NPC.
		/// Requires map context.
		/// </summary>
		public void SpawnOne()
		{
			var spawn = SpawnEntry.Entry.Create(this);
			spawn.Position = m_spawnEntry.Position;

			m_spawns.Add(spawn);
			Map.AddObjectLater(spawn);
		}

		protected void Spawn(int dtMillis)
		{
			SpawnOne();
			if (m_spawns.Count < m_spawnEntry.MaxAmount)
			{
				// spawn more later
				var delay = m_spawnEntry.GetRandomRespawnMillis();
				Reset(delay);
			}
			else
			{
				// stop spawning
				m_timer.Stop();
				Map.UnregisterUpdatableLater(m_timer);
			}
		}
		#endregion

		/// <summary>
		/// Removes all active Spawns from this SpawnPoint and makes it
		/// collectable by the GC (if not in Map anymore).
		/// </summary>
		/// <remarks>Requires map context</remarks>
		public void Clear()
		{
			var arr = m_spawns.ToArray();
			for (var i = 0; i < arr.Length; i++)
			{
				var spawn = arr[i];
				spawn.Delete();
			}
        }

        public void RemoveSpawnLater()
        {
            Map.AddMessage(() => RemoveSpawnNow());
        }

		public void RemoveSpawnNow()
		{
			if (Map != null)
			{
				Map.UnregisterUpdatable(m_timer);
				Active = false;
				Clear();
				Map.RemoveSpawn(this);
				Map = null;
			}
		}
	}
}