using System;
using System.Threading;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Spawns
{
	public abstract class SpawnEntry<T, E, O, POINT, POOL>
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

		protected T m_PoolTemplate;

		protected MapId m_MapId = MapId.End;

		protected SpawnEntry()
		{
			PhaseMask = 1;
		}

		public MapId MapId
		{
			get { return m_MapId; }
			set { m_MapId = value; }
		}

		public Map Map
		{
			get { return World.GetMap(MapId); }
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

		public uint PhaseMask
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
		public bool AutoSpawns = true;

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

			if (PhaseMask == 0)
			{
				PhaseMask = 1;
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
