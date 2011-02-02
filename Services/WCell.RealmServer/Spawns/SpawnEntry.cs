using System;
using System.Linq;
using System.Threading;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
	public interface ISpawnEntry
	{
		MapId MapId
		{
			get;
			set;
		}

		Map Map
		{
			get;
		}

		/// <summary>
		/// The position of this SpawnEntry
		/// </summary>
		Vector3 Position
		{
			get;
			set;
		}

		float Orientation
		{
			get;
			set;
		}

		uint Phase
		{
			get;
			set;
		}

		int RespawnSeconds
		{
			get;
			set;
		}

		int DespawnSeconds
		{
			get;
			set;
		}

		/// <summary>
		/// Min Delay in milliseconds until the unit should be respawned
		/// </summary>
		int RespawnSecondsMin
		{
			get;
			set;
		}

		/// <summary>
		/// Max Delay in milliseconds until the unit should be respawned
		/// </summary>
		int RespawnSecondsMax
		{
			get;
			set;
		}

		uint EventId
		{
			get;
			set;
		}

		/// <summary>
		/// Whether this Entry spawns automatically (or is spawned by certain events)
		/// </summary>
		bool AutoSpawns
		{
			get;
			set;
		}

		int GetRandomRespawnMillis();
	}

	public abstract class SpawnEntry<T, E, O, POINT, POOL> : ISpawnEntry, IWorldLocation
		where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
		where E : SpawnEntry<T, E, O, POINT, POOL>
		where O : WorldObject
		where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
		where POOL : SpawnPool<T, E, O, POINT, POOL>
	{
		private static uint highestSpawnId;
		public static uint GenerateSpawnId()
		{
			return ++highestSpawnId;
		}

		public uint SpawnId;
		public uint PoolId;
		public float PoolRespawnProbability;
		public readonly SynchronizedList<POINT> SpawnPoints = new SynchronizedList<POINT>(2);

		protected T m_PoolTemplate;

		protected MapId m_MapId = MapId.End;

		private bool m_AutoSpawns = true;

		protected SpawnEntry()
		{
			Phase = 1;
		}

		public MapId MapId
		{
			get { return m_MapId; }
			set { m_MapId = value; }
		}

		public Map Map
		{
			get { return World.GetNonInstancedMap(MapId); }
		}

		/// <summary>
		/// The position of this SpawnEntry
		/// </summary>
		public Vector3 Position
		{
			get;
			set;
		}

		public float Orientation
		{
			get;
			set;
		}

		public uint Phase
		{
			get;
			set;
		}

		public int RespawnSeconds
		{
			get;
			set;
		}

		public int DespawnSeconds
		{
			get;
			set;
		}

		/// <summary>
		/// Min Delay in milliseconds until the unit should be respawned
		/// </summary>
		public int RespawnSecondsMin
		{
			get;
			set;
		}

		/// <summary>
		/// Max Delay in milliseconds until the unit should be respawned
		/// </summary>
		public int RespawnSecondsMax
		{
			get;
			set;
		}

		public uint EventId
		{
			get;
			set;
		}

		/// <summary>
		/// Whether this Entry spawns automatically (or is spawned by certain events)
		/// </summary>
		public bool AutoSpawns
		{
			get { return m_AutoSpawns; }
			set
			{
				if (m_AutoSpawns != value)
				{
					m_AutoSpawns = value;
					if (m_PoolTemplate != null)
					{
						// update pool template
						m_PoolTemplate.AutoSpawns = m_PoolTemplate.Entries.Any(entry => entry.AutoSpawns);
					}
				}
			}
		}

		[NotPersistent]
		public T PoolTemplate
		{
			get { return m_PoolTemplate; }
		}

		public int GetRandomRespawnMillis()
		{
			return 1000 * Utility.Random(RespawnSecondsMin, RespawnSecondsMax);
		}

		public abstract O SpawnObject(POINT point);

		public virtual void FinalizeDataHolder(bool addToPool)
		{
			// overly complicated and annoying correction of respawn time
			// considering two cases: Code-generated SpawnEntries and those read from DB
			// If read from UDB, RespawnSeconds < 0 means that its not auto-spawning
			// and despawning after -RespawnSeconds
			if (RespawnSecondsMin == 0)
			{
				RespawnSecondsMin = RespawnSeconds;
			}
			if (RespawnSecondsMax == 0)
			{
				RespawnSecondsMax = Math.Max(RespawnSeconds, RespawnSecondsMin);
			}
			AutoSpawns = RespawnSecondsMax > 0;
			if (!AutoSpawns)
			{
				DespawnSeconds = -RespawnSeconds;
				RespawnSecondsMin = RespawnSecondsMax = 0;
			}

			if (Phase == 0)
			{
				Phase = 1;
			}

			if (SpawnId > highestSpawnId)
			{
				highestSpawnId = SpawnId;
			}
			else if (SpawnId == 0)
			{
				SpawnId = GenerateSpawnId();
			}
		}
	}
}
