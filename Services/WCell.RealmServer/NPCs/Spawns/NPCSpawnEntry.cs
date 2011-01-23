using System;
using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Waypoints;
using WCell.Util;
using WCell.Util.Data;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.NPCs.Spawns
{
	/// <summary>
	/// Spawn-information for NPCs
	/// </summary>
	[DataHolder]
	public partial class NPCSpawnEntry : INPCDataHolder, IWorldLocation
	{
		private static uint highestSpawnId;
		public static uint GenerateSpawnId()
		{
			return ++highestSpawnId;
		}

		public NPCSpawnEntry()
		{

		}

		public uint SpawnId;
		public NPCId EntryId;

		[NotPersistent]
		public NPCEntry Entry
		{
			get;
			set;
		}

		private MapId m_MapId = MapId.End;

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

		public uint PhaseMask = 1;

		public float Orientation;
		public AIMovementType MoveType;

		public int RespawnSeconds;

		/// <summary>
		/// Min Delay in milliseconds until the unit should be respawned
		/// </summary>
		public int RespawnSecondsMin;

		/// <summary>
		/// Max Delay in milliseconds until the unit should be respawned
		/// </summary>
		public int RespawnSecondsMax;

		public bool IsDead;

		public uint DisplayIdOverride;

		public uint EquipmentId;

		public uint EventId;

		/// <summary>
		/// Whether this Entry spawns automatically (or is spawned by certain events)
		/// </summary>
		public bool AutoSpawns = true;

		public NPCAddonData AddonData
		{
			get;
			set;
		}


		public uint PoolId;
		public float PoolRespawnProbability;

		private NPCSpawnPoolTemplate m_PoolTemplate;

		[NotPersistent]
		public NPCSpawnPoolTemplate PoolTemplate
		{
			get { return m_PoolTemplate; }
			private set { m_PoolTemplate = value; }
		}


		[NotPersistent]
		public NPCEquipmentEntry Equipment;

		/// <summary>
		/// If the number of spawned creatures drops below this limit, the critical respawn-delay is used
		/// </summary>
		//public uint CriticalAmount;

		/// <summary>
		/// Min Delay when amount of spawned NPCs drops below CriticalAmount
		/// </summary>
		//public uint CriticalDelayMin;

		/// <summary>
		/// Max Delay when amount of spawned NPCs drops below CriticalAmount
		/// </summary>
		//public uint CriticalDelayMax;;

		[NotPersistent]
		public readonly LinkedList<WaypointEntry> Waypoints = new LinkedList<WaypointEntry>();

		private bool m_HasDefaultWaypoints;

		/// <summary>
		/// Whether this SpawnEntry has fixed Waypoints from DB
		/// </summary>
		[NotPersistent]
		public bool HasDefaultWaypoints
		{
			get { return m_HasDefaultWaypoints; }
			set
			{
				m_HasDefaultWaypoints = value;
				Entry.MovesRandomly = false;
			}
		}

		public int GetRandomRespawnMillis()
		{
			return 1000 * Utility.Random(RespawnSecondsMin, RespawnSecondsMax);
		}

		#region WPs
		public int WPCount
		{
			get { return Waypoints.Count; }
		}

		public WaypointEntry CreateWP(Vector3 pos, float orientation)
		{
			var last = Waypoints.Last;
			WaypointEntry entry;
			if (last != null)
			{
				entry = new WaypointEntry
				{
					Id = last.Value.Id + 1,
					SpawnEntry = this,
				};
			}
			else
			{
				entry = new WaypointEntry
				{
					Id = 1,
					SpawnEntry = this,
				};
			}

			entry.Position = pos;
			entry.Orientation = orientation;
			return entry;
		}

		/// <summary>
		/// Adds the given positions as WPs
		/// </summary>
		/// <param name="wps"></param>
		public void AddWPs(Vector3[] wps)
		{
			if (wps.Length < 1)
			{
				throw new ArgumentException("wps are empty.");
			}
			var len = wps.Length;
			Vector3 first;
			if (Waypoints.Count > 0)
			{
				// adding to already existing WPs: Make sure that the orientation is correct
				first = Waypoints.First.Value.Position;
				var last = Waypoints.Last.Value;
				last.Orientation = last.Position.GetAngleTowards(wps[0]);
			}
			else
			{
				first = wps[0];
			}

			for (var i = 0; i < len; i++)
			{
				var pos = wps[i];
				WaypointEntry wp;
				if (i < len - 1)
				{
					wp = CreateWP(pos, pos.GetAngleTowards(wps[i + 1]));
				}
				else if (i > 0)
				{
					wp = CreateWP(pos, pos.GetAngleTowards(first));
				}
				else
				{
					wp = CreateWP(pos, Utility.Random(0f, 2 * MathUtil.PI));
				}
				Waypoints.AddLast(wp);
			}
		}

		public LinkedListNode<WaypointEntry> AddWP(Vector3 pos, float orientation)
		{
			var newWp = CreateWP(pos, orientation);
			return Waypoints.AddLast(newWp);
		}

		public LinkedListNode<WaypointEntry> InsertWPAfter(WaypointEntry entry, Vector3 pos, float orientation)
		{
			var newWp = CreateWP(pos, orientation);
			return Waypoints.AddAfter(entry.Node, newWp);
		}

		public LinkedListNode<WaypointEntry> InsertWPBefore(WaypointEntry entry, Vector3 pos, float orientation)
		{
			var newWp = CreateWP(pos, orientation);
			return Waypoints.AddBefore(entry.Node, newWp);
		}
		#endregion

		/// <summary>
		/// Increases the Id of all Waypoints, starting from the given node
		/// </summary>
		/// <param name="node">May be null</param>
		public void IncreaseWPIds(LinkedListNode<WaypointEntry> node)
		{
			while (node != null)
			{
				node.Value.Id++;
				node = node.Next;
			}
		}

		//public int GetRandomCriticalDelay()
		//{
		//    return (int)Utility.Random(CriticalDelayMin, CriticalDelayMax);
		//}

		/// <summary>
		/// Finalize this NPCSpawnEntry
		/// </summary>
		/// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom NPCSpawnEntry that share a pool)</param>
		public void FinalizeDataHolder()
		{
			FinalizeDataHolder(true);
		}

		/// <summary>
		/// Finalize this NPCSpawnEntry
		/// </summary>
		/// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom NPCSpawnEntry that share a pool)</param>
		public void FinalizeDataHolder(bool addToPool)
		{
			if (Entry == null)
			{
				Entry = NPCMgr.GetEntry(EntryId);
				if (Entry == null)
				{
					ContentMgr.OnInvalidDBData("{0} had an invalid EntryId.", this);
					return;
				}
			}

			var respawn = RespawnSeconds != 0 ? RespawnSeconds : NPCMgr.DefaultMinRespawnDelay;
			AutoSpawns = RespawnSeconds > 0;
			if (RespawnSecondsMin == 0)
			{
				RespawnSecondsMin = respawn;
			}
			if (RespawnSecondsMax == 0)
			{
				RespawnSecondsMax = Math.Max(respawn, RespawnSecondsMin);
			}

			if (PhaseMask == 0)
			{
				PhaseMask = 1;
			}

			if (EquipmentId != 0)
			{
				Equipment = NPCMgr.GetEquipment(EquipmentId);
			}

			// seems to be a DB issue where the id was inserted as a float
			if (DisplayIdOverride == 16777215)
			{
				DisplayIdOverride = 0;
			}

			if (SpawnId > highestSpawnId)
			{
				highestSpawnId = SpawnId;
			}
			else if (SpawnId == 0)
			{
				SpawnId = GenerateSpawnId();
			}

			if (AddonData != null)
			{
				AddonData.InitAddonData(this);
			}


			if (MapId != MapId.End)
			{
				// valid map
				// add to NPCEntry
				Entry.SpawnEntries.Add(this);

				// add to list of NPCSpawnEntries
				ArrayUtil.Set(ref NPCMgr.SpawnEntries, SpawnId, this);

				if (addToPool)
				{
					// add to pool
					AddToPoolTemplate();
				}
			}

			// finished initializing, now call the hooks
			foreach (var handler in Entry.SpawnTypeHandlers)
			{
				if (handler != null)
				{
					handler(this);
				}
			}
		}

		private void AddToPoolTemplate()
		{
			if (PoolId != 0 && NPCMgr.SpawnPoolTemplates.TryGetValue(PoolId, out m_PoolTemplate))
			{
				// pool already exists
				m_PoolTemplate.AddEntry(this);
			}
			else
			{
				// create and register new pool
				m_PoolTemplate = new NPCSpawnPoolTemplate(this);
			}
		}

		#region Events
		internal void NotifySpawned(NPC npc)
		{
			var evt = Spawned;
			if (evt != null)
			{
				evt(npc);
			}
		}
		#endregion

		public void GenerateRandomWPs()
		{
			Waypoints.Clear();
			AddRandomWPs();
		}

		public void AddRandomWPs()
		{
			var terrain = TerrainMgr.GetTerrain(MapId);
			if (terrain != null)
			{
				var gen = new RandomWaypointGenerator();
				var wps = gen.GenerateWaypoints(terrain, Position);
				AddWPs(wps);
			}
		}

		public override string ToString()
		{
			return string.Format(GetType().Name + " #{0} ({1} #{2})", SpawnId, EntryId, (uint)EntryId);
		}

		public static IEnumerable<NPCSpawnEntry> GetAllDataHolders()
		{
			return NPCMgr.SpawnEntries;
		}
	}
}