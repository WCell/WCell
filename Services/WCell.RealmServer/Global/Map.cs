/*************************************************************************
 *
 *   file		: Map.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-30 01:30:45 +0800 (Mon, 30 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 542 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Res;
using WCell.RealmServer.Spawns;
using WCell.Util.Collections;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Battlegrounds.Arenas;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Util.Threading.TaskParallel;
using WCell.Core.Timers;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.NPCs;
using WCell.Core.Paths;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;
using WCell.Intercommunication.DataTypes;
using WCell.Constants.GameObjects;
using WCell.Constants.NPCs;
using WCell.RealmServer.AI;
using WCell.Core.TerrainAnalysis;
using WCell.Constants.Achievements;


namespace WCell.RealmServer.Global
{
	/// <summary>
	/// Represents a continent or Instance (including Battleground instances), that may or may not contain Zones.
	/// X-Axis: South -> North
	/// Y-Axis: East -> West
	/// Z-Axis: Down -> Up
	/// Orientation: North = 0, 2*Pi, counter-clockwise
	/// </summary>
	public partial class Map : IGenericChatTarget, IMapId, IContextHandler, IWorldSpace
	{
		#region Global Variables

		/// <summary>
		/// Whether to spawn NPCs and GOs immediately, whenever a new Map gets activated the first time.
		/// For developing you might want to toggle this off and use the "Map Spawn" command ingame to spawn the map, if necessary.
		/// </summary>
		[Variable("AutoSpawnMaps")]
		public static bool AutoSpawnMaps = true;

		/// <summary>
		/// Default update delay in milliseconds
		/// </summary>
		public static int DefaultUpdateDelay = 120;

		/// <summary>
		/// Every how many ticks to send UpdateField-changes to Characters
		/// </summary>
		public static int CharacterUpdateEnvironmentTicks = 1000 / DefaultUpdateDelay;

		[Variable("UpdateInactiveAreas")]
		public static bool UpdateInactiveAreasDefault = false;

		/// <summary>
		/// Whether to have NPCs in inactive areas scan for enemies
		/// </summary>
		[Variable("ScanInactiveAreas")]
		public static bool ScanInactiveAreasDefault = false;

		/// <summary>
		/// Whether NPCs can evade and run back to their spawn point when pulled too far away
		/// </summary>
		[Variable("NPCsCanEvade")]
		public static bool CanNPCsEvadeDefault = true;

		// 
		private static int[] updatePriorityTicks = new int[(int)UpdatePriority.End];

		[NotVariable]
		public static int[] UpdatePriorityTicks
		{
			get { return updatePriorityTicks; }
			set
			{
				updatePriorityTicks = value;
				SetUpdatePriorityTicks();
			}
		}

		static void SetDefaultUpdatePriorityTick(UpdatePriority priority, int ticks)
		{
			if (UpdatePriorityTicks[(int)priority] == 0)
			{
				UpdatePriorityTicks[(int)priority] = ticks;
			}
		}

		//[Initialization(InitializationPass.Tenth)]
		static void SetUpdatePriorityTicks()
		{
			if (UpdatePriorityTicks == null)
			{
				UpdatePriorityTicks = new int[(int)UpdatePriority.End];
			}
			else if (UpdatePriorityTicks.Length != (int)UpdatePriority.End)
			{
				Array.Resize(ref updatePriorityTicks, (int)UpdatePriority.End);
			}

			SetDefaultUpdatePriorityTick(UpdatePriority.Inactive, 20);
			SetDefaultUpdatePriorityTick(UpdatePriority.Background, 10);
			SetDefaultUpdatePriorityTick(UpdatePriority.VeryLowPriority, 5);
			SetDefaultUpdatePriorityTick(UpdatePriority.LowPriority, 3);
			SetDefaultUpdatePriorityTick(UpdatePriority.Active, 2);
			SetDefaultUpdatePriorityTick(UpdatePriority.HighPriority, 1);
		}

		public static int GetTickCount(UpdatePriority priority)
		{
			return UpdatePriorityTicks[(int)priority];
		}

		public static void SetTickCount(UpdatePriority priority, int count)
		{
			UpdatePriorityTicks[(int)priority] = count;
		}
		#endregion

		static Map()
		{
			SetUpdatePriorityTicks();
		}

		protected static Logger s_log = LogManager.GetCurrentClassLogger();

		private static float _avgUpdateTime;

		public static string LoadAvgStr
		{
			get
			{
				return string.Format("{0:0.00}/{1} ({2} %)", _avgUpdateTime, DefaultUpdateDelay, (_avgUpdateTime / DefaultUpdateDelay) * 100);
			}
		}

		#region Fields
		internal protected MapTemplate m_MapTemplate;
		protected Dictionary<EntityId, WorldObject> m_objects;
		protected ZoneSpacePartitionNode m_root;
		protected ZoneTileSet m_zoneTileSet;
		protected ITerrain m_Terrain;

		protected volatile bool m_running;
		protected DateTime m_lastUpdateTime;
		protected int m_tickCount;

		protected int m_updateDelay;
		protected bool m_updateInactiveAreas, m_ScanInactiveAreas;
		protected bool m_canNPCsEvade;

		protected List<Character> m_characters = new List<Character>(50);
		int m_allyCount, m_hordeCount;
		protected List<NPC> m_spiritHealers = new List<NPC>();

		protected LockfreeQueue<IMessage> m_messageQueue = new LockfreeQueue<IMessage>();
		protected List<IUpdatable> m_updatables = new List<IUpdatable>();
		protected int m_currentThreadId;

		#region Spawns
		bool m_npcsSpawned, m_gosSpawned, m_isUpdating;
		int m_lastNPCPoolId;
		internal protected Dictionary<uint, GOSpawnPool> m_goSpawnPools = new Dictionary<uint, GOSpawnPool>();
		internal protected Dictionary<uint, NPCSpawnPool> m_npcSpawnPools = new Dictionary<uint, NPCSpawnPool>();

		internal uint GenerateNewNPCPoolId()
		{
			return (uint)Interlocked.Increment(ref m_lastNPCPoolId);
		}
		#endregion


		protected bool m_CanFly;

		Zone m_defaultZone;
		/// <summary>
		/// List of all Main zones (that have no parent-zone but only children)
		/// </summary>
		public readonly List<Zone> MainZones = new List<Zone>(5);

		/// <summary>
		/// All the Zone-instances within this map.
		/// </summary>
		public readonly IDictionary<ZoneId, Zone> Zones = new Dictionary<ZoneId, Zone>();

		/// <summary>
		/// The first TaxiPath-node of this Map (or null).
		/// This is used to send individual Taxi Maps to Players.
		/// </summary>
		public PathNode FirstTaxiNode;

		public WorldStateCollection WorldStates
		{
			get;
			private set;
		}
		#endregion

		#region Creation
		protected Map()
		{
			m_objects = new Dictionary<EntityId, WorldObject>();

			m_updateDelay = DefaultUpdateDelay;

			m_updateInactiveAreas = UpdateInactiveAreasDefault;
			m_ScanInactiveAreas = ScanInactiveAreasDefault;
			m_canNPCsEvade = CanNPCsEvadeDefault;
		}

		/// <summary>
		/// Creates a map from the given map info.
		/// </summary>
		/// <param name="rgnTemplate">the info for this map to use</param>
		public Map(MapTemplate rgnTemplate) :
			this()
		{
			m_MapTemplate = rgnTemplate;
			m_CanFly = rgnTemplate.Id == MapId.Outland || rgnTemplate.Id == MapId.Northrend;
		}

		protected internal void InitMap(MapTemplate template)
		{
			m_MapTemplate = template;
			InitMap();
		}

		/// <summary>
		/// Method is called after Creation of Map
		/// </summary>
		protected internal virtual void InitMap()
		{
			// Set our map's bounds and Terrain
			m_Terrain = TerrainMgr.GetTerrain(m_MapTemplate.Id);
			m_zoneTileSet = m_MapTemplate.ZoneTileSet;
			m_root = new ZoneSpacePartitionNode(m_MapTemplate.Bounds);
			//m_root = new ZoneSpacePartitionNode(new BoundingBox(
			//                                        new Vector3(-(TerrainConstants.MapLength / 2), -(TerrainConstants.MapLength / 2), -MAP_HEIGHT),
			//                                        new Vector3((TerrainConstants.MapLength / 2), (TerrainConstants.MapLength / 2), MAP_HEIGHT)
			//                                    ));
			PartitionSpace();

			var states = Constants.World.WorldStates.GetStates(m_MapTemplate.Id) ?? WorldState.EmptyArray;
			WorldStates = new WorldStateCollection(this, states);

			CreateZones();

			m_MapTemplate.NotifyCreated(this);
		}

		private void CreateZones()
		{
			for (var i = 0; i < m_MapTemplate.ZoneInfos.Count; i++)
			{
				var templ = m_MapTemplate.ZoneInfos[i];
				var zone = templ.Creator(this, templ);
				if (zone.ParentZone == null)
				{
					MainZones.Add(zone);
				}
				Zones.Add(templ.Id, zone);
			}

			m_defaultZone = MainZones.FirstOrDefault();
		}

		#endregion

		#region Properties
		public MapId MapId
		{
			get { return Id; }
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual uint InstanceId
		{
			get { return 0; }
		}

		public MapTemplate MapTemplate
		{
			get { return m_MapTemplate; }
		}

		public ITerrain Terrain
		{
			get { return m_Terrain; }
		}

		public IWorldSpace ParentSpace
		{
			get { return World.Instance; }
		}

		public int MainZoneCount
		{
			get { return MainZones.Count; }
		}

		/// <summary>
		/// The first MainZone (for reference and Maps that only contain one Zone)
		/// </summary>
		public Zone DefaultZone
		{
			get { return m_defaultZone; }
		}

		/// <summary>
		/// The bounds of this map.
		/// </summary>
		public BoundingBox Bounds
		{
			get { return m_root.Bounds; }
		}

		public ICollection<NPC> SpiritHealers
		{
			get { return m_spiritHealers; }
		}

		/// <summary>
		/// The display name of this map.
		/// </summary>
		public string Name
		{
			get { return m_MapTemplate.Name; }
		}

		/// <summary>
		/// The map ID of this map.
		/// </summary>
		public MapId Id
		{
			get { return m_MapTemplate.Id; }
		}

		/// <summary>
		/// The minimum required ClientId
		/// </summary>
		public ClientId RequiredClient
		{
			get { return m_MapTemplate.RequiredClientId; }
		}

		/// <summary>
		/// Whether or not the map is instanced
		/// </summary>
		public bool IsInstance
		{
			get { return this is InstancedMap; }
		}

		public bool IsArena
		{
			get { return this is Arena; }
		}

		public bool IsBattleground
		{
			get { return this is Battleground; }
		}

		/// <summary>
		/// The type of the map (normal, battlegrounds, instance, etc)
		/// </summary>
		public MapType Type
		{
			get { return m_MapTemplate.Type; }
		}

		public bool IsHeroic
		{
			get
			{
				var diff = Difficulty;
				return diff != null && diff.IsHeroic;
			}
		}

		public uint DifficultyIndex
		{
			get
			{
				var diff = Difficulty;
				return diff != null ? diff.Index : 0;
			}
		}

		/// <summary>
		/// Difficulty of the instance
		/// </summary>
		public virtual MapDifficultyEntry Difficulty
		{
			get { return null; }
		}

		/// <summary>
		/// The minimum level a player has to be to enter the map (instance)
		/// </summary>
		public virtual int MinLevel
		{
			get { return m_MapTemplate.MinLevel; }
		}

		/// <summary>
		/// The maximum level a player can be to enter the map (instance)
		/// </summary>
		public virtual int MaxLevel
		{
			get { return m_MapTemplate.MaxLevel; }
		}

		/// <summary>
		/// Maximum number of players allowed in the map (instance)
		/// </summary>
		public int MaxPlayerCount
		{
			get { return m_MapTemplate.MaxPlayerCount; }
		}

		/// <summary>
		/// Whether or not the map is currently processing object updates
		/// </summary>
		public bool IsRunning
		{
			get { return m_running; }
			set
			{
				if (m_running != value)
				{
					if (value)
					{
						Start();
					}
					else
					{
						Stop();
					}
				}
			}
		}

		/// <summary>
		/// Indicates whether the current Thread is the Map's update-thread.
		/// </summary>
		public bool IsInContext
		{
			get { return Thread.CurrentThread.ManagedThreadId == m_currentThreadId; }
		}

		public bool IsUpdating
		{
			get { return m_isUpdating; }
		}

		public virtual DateTime CreationTime
		{
			get { return RealmServer.StartTime; }
		}

		/// <summary>
		/// The amount of all Players in this Map (excludes Staff members)
		/// </summary>
		public int PlayerCount
		{
			get { return m_allyCount + m_hordeCount; }
		}

		/// <summary>
		/// The amount of all Characters in this Map (includes Staff members)
		/// </summary>
		public int CharacterCount
		{
			get { return m_characters.Count; }
		}

		/// <summary>
		/// The number of Alliance Players currently in the map (not counting Staff)
		/// </summary>
		public int AllianceCount
		{
			get { return m_allyCount; }
		}

		/// <summary>
		/// The number of Alliance Players currently in the map (not counting Staff)
		/// </summary>
		public int HordeCount
		{
			get { return m_hordeCount; }
		}

		public int NPCSpawnPoolCount
		{
			get { return m_npcSpawnPools.Count; }
		}

		/// <summary>
		/// Amount of passed ticks in this Map
		/// </summary>
		public int TickCount
		{
			get { return m_tickCount; }
		}

		/// <summary>
		/// Don't modify the List.
		/// </summary>
		public List<Character> Characters
		{
			get
			{
				EnsureContext();
				return m_characters;
			}
		}

		/// <summary>
		/// The calculator used to compute Xp given for killing NPCs
		/// </summary>
		public ExperienceCalculator XpCalculator
		{
			get;
			set;
		}

		/// <summary>
		/// Called whenever an object is removed from the map to determine whether it may stop now.
		/// </summary>
		public virtual bool ShouldStop
		{
			get { return m_objects.Count == 0; }
		}

		/// <summary>
		/// Whether to also update Nodes in areas without Players.
		/// Default: <see cref="UpdateInactiveAreasDefault"/>
		/// </summary>
		public bool UpdateInactiveAreas
		{
			get { return m_updateInactiveAreas; }
			set { m_updateInactiveAreas = value; }
		}

		/// <summary>
		/// Whether to let NPCs scan inactive Nodes for hostility.
		/// Default: <see cref="ScanInactiveAreasDefault"/>
		/// </summary>
		public bool ScanInactiveAreas
		{
			get { return m_ScanInactiveAreas; }
			set { m_ScanInactiveAreas = value; }
		}

		/// <summary>
		/// Time in milliseconds between the beginning of 
		/// one Map-Update and the next.
		/// </summary>
		public int UpdateDelay
		{
			get { return m_updateDelay; }
			set { Interlocked.Exchange(ref m_updateDelay, value); }
		}

		public int CharacterUpdateEnvironmentDelay
		{
			get { return CharacterUpdateEnvironmentTicks * (int)m_updateDelay; }
		}

		/// <summary>
		/// Total amount of objects within this Map
		/// </summary>
		public int ObjectCount
		{
			get { return m_objects.Count; }
		}

		/// <summary>
		/// Whether NPCs in this Map will try to evade after Combat
		/// </summary>
		public bool CanNPCsEvade
		{
			get { return m_canNPCsEvade; }
			set { m_canNPCsEvade = value; }
		}

		public bool CanFly
		{
			get { return m_CanFly; }
			set { m_CanFly = value; }
		}

		private bool m_IsAIFrozen = false;

		/// <summary>
		/// Toggles all NPCs to be invul and idle
		/// </summary>
		public bool IsAIFrozen
		{
			get { return m_IsAIFrozen; }
			set
			{
				EnsureContext();

				if (m_IsAIFrozen != value)
				{
					m_IsAIFrozen = value;
					foreach (var obj in m_objects.Values)
					{
						if (obj is NPC)
						{
							var npc = ((NPC)obj);
							if (value)
							{
								npc.Brain.State = BrainState.Idle;
								npc.Invulnerable++;
							}
							else
							{
								npc.Brain.EnterDefaultState();
								npc.Invulnerable--;
							}
						}
					}
				}
			}
		}

		#endregion

		#region Start / Stop
		/// <summary>
		/// Starts the Map's update-, message- and timer- loop
		/// </summary>
		public void Start()
		{
			if (!m_running)
			{
				// lock and check again, so Map won't be started twice or start while stoping
				lock (m_objects)
				{
					if (m_running)
						return;

					// global sync
					lock (World.PauseLock)
					{
						if (World.Paused)
						{
							// must ensure that IsRunning does not change during World Pause
							// in order to prevent dead-locks, we cannot allow to wait for the World to unpause
							// at this point
							throw new InvalidOperationException("Tried to start Map while World is paused.");
						}

						m_running = true;
					}

					s_log.Debug(Resources.MapStarted, m_MapTemplate.Id);

					// start updating
					Task.Factory.StartNewDelayed(m_updateDelay, MapUpdateCallback, this);

					if (AutoSpawnMaps)
					{
						SpawnMapLater();
					}

					m_lastUpdateTime = DateTime.Now;

					m_MapTemplate.NotifyStarted(this);
				}
			}
		}

		/// <summary>
		/// Stops map updating and stops the update delta measuring
		/// </summary>
		public void Stop()
		{
			if (m_running)
			{
				if (!m_MapTemplate.NotifyStopping(this))
				{
					return;
				}

				// lock and check again, so map won't be stopped twice or stopped while starting
				lock (m_objects)
				{
					if (!m_running)
					{
						return;
					}
					m_running = false;

					s_log.Debug(Resources.MapStopped, m_MapTemplate.Id);

					m_MapTemplate.NotifyStopped(this);
				}
			}
		}

		public bool ExecuteInContext(Action action)
		{
			if (!IsInContext)
			{
				AddMessage(new Message(action));
				return false;
			}

			action();
			return true;
		}

		/// <summary>
		/// Ensures execution within the map.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the calling thread isn't the map thread</exception>
		public void EnsureContext()
		{
			if (Thread.CurrentThread.ManagedThreadId != m_currentThreadId && IsRunning)
			{
				Stop();
				throw new InvalidOperationException(string.Format(Resources.MapContextNeeded, this));
			}
		}

		/// <summary>
		/// Ensures execution outside the Map-context.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the calling thread is the map thread</exception>
		public void EnsureNoContext()
		{
			if (Thread.CurrentThread.ManagedThreadId == m_currentThreadId)
			{
				Stop();
				throw new InvalidOperationException(string.Format(Resources.MapContextProhibited, this));
			}
		}

		/// <summary>
		/// Ensures that Map is not updating.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the Map is currently updating</exception>
		public void EnsureNotUpdating()
		{
			if (m_isUpdating)
			{
				Stop();
				throw new InvalidOperationException(string.Format(Resources.MapUpdating, this));
			}
		}
		#endregion

		#region Spawning

		/// <summary>
		/// Whether this Map's NPCs and GOs have been fully spawned
		/// </summary>
		public bool IsSpawned
		{
			get
			{
				return m_npcsSpawned && m_gosSpawned;
			}
		}

		public bool NPCsSpawned
		{
			get { return m_npcsSpawned; }
		}

		public bool GOsSpawned
		{
			get { return m_gosSpawned; }
		}

		public void AddNPCSpawnPoolLater(NPCSpawnPoolTemplate templ)
		{
			AddMessage(() => AddNPCSpawnPoolNow(templ));
		}

		public NPCSpawnPool AddNPCSpawnPoolNow(NPCSpawnPoolTemplate templ)
		{
			var pool = new NPCSpawnPool(this, templ);
			AddNPCSpawnPoolNow(pool);
			return pool;
		}

		public void AddNPCSpawnPoolNow(NPCSpawnPool pool)
		{
			if (!m_npcSpawnPools.ContainsKey(pool.Template.PoolId))
			{
				m_npcSpawnPools.Add(pool.Template.PoolId, pool);
				OnPoolAdded<NPCSpawnPoolTemplate, NPCSpawnEntry, NPC, NPCSpawnPoint, NPCSpawnPool>(pool);
			}
			pool.IsActive = true;
		}

		public GOSpawnPool AddGOSpawnPool(GOSpawnPoolTemplate templ)
		{
			var pool = new GOSpawnPool(this, templ);
			AddGOSpawnPool(pool);
			return pool;
		}

		public void AddGOSpawnPool(GOSpawnPool pool)
		{
			if (!m_goSpawnPools.ContainsKey(pool.Template.PoolId))
			{
				m_goSpawnPools.Add(pool.Template.PoolId, pool);
				OnPoolAdded<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>(pool);
			}
			pool.IsActive = true;
		}

		static void OnPoolAdded<T, E, O, POINT, POOL>(POOL pool) 
			where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
			where E : SpawnEntry<T, E, O, POINT, POOL>
			where O : WorldObject
			where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
			where POOL : SpawnPool<T, E, O, POINT, POOL>
		{
			foreach (var point in pool.SpawnPoints)
			{
				point.SpawnEntry.SpawnPoints.Add(point);
			}
		}

		/// <summary>
		/// Called by SpawnPool.
		/// Use SpawnPool.Remove* methods to remove pools.
		/// </summary>
		internal void RemoveSpawnPool<T, E, O, POINT, POOL>(POOL pool)
			where T : SpawnPoolTemplate<T, E, O, POINT, POOL>
			where E : SpawnEntry<T, E, O, POINT, POOL>
			where O : WorldObject
			where POINT : SpawnPoint<T, E, O, POINT, POOL>, new()
			where POOL : SpawnPool<T, E, O, POINT, POOL>
		{
			if (typeof(O) == typeof(NPC))
			{
				if (m_npcSpawnPools.Remove(pool.Template.PoolId))
				{
					pool.IsActive = false;
				}

				// remove from list of SpawnEntry.SpawnPoints
				foreach (var point in pool.SpawnPoints)
				{
					point.SpawnEntry.SpawnPoints.Remove(point);
				}
			}
			else if (typeof(O) == typeof(GameObject))
			{
				if (m_goSpawnPools.Remove(pool.Template.PoolId))
				{
					pool.IsActive = false;
				}
			}
			else
			{
				throw new ArgumentException("Invalid Pool type: " + pool);
			}
		}

		/// <summary>
		/// Adds a message to the Map to clear it
		/// </summary>
		public void ClearLater()
		{
			AddMessage(RemoveAll);
		}

		/// <summary>
		/// Removes all Objects, NPCs and Spawns from this Map.
		/// </summary>
		/// <remarks>Requires map context</remarks>
		public virtual void RemoveAll()
		{
			RemoveObjects();
		}

		/// <summary>
		/// Removes all Objects and NPCs from the Map
		/// </summary>
		public void RemoveObjects()
		{
			if (IsInContext && !IsUpdating)
			{
				RemoveObjectsNow();
			}
			else
			{
				AddMessage(() => RemoveObjectsNow());
			}
		}

		private void RemoveObjectsNow()
		{
			foreach (var pool in m_npcSpawnPools.Values.ToArray())
			{
				pool.RemovePoolNow();
			}
			foreach (var pool in m_goSpawnPools.Values.ToArray())
			{
				pool.RemovePoolNow();
			}

			var objs = CopyObjects();
			for (var i = 0; i < objs.Length; i++)
			{
				var obj = objs[i];
				if (!(obj is Character) && !obj.IsOwnedByPlayer)
				{
					// only delete things that are not Characters or belong to Characters
					obj.DeleteNow();
				}
			}

			m_gosSpawned = false;
			m_npcsSpawned = false;
		}

		/// <summary>
		/// Clears all Objects and NPCs and spawns the default ones again
		/// </summary>
		public virtual void Reset()
		{
			RemoveAll();
			SpawnMap();

			foreach (var pool in m_npcSpawnPools.Values)
			{
				pool.RespawnFull();
			}
		}

		/// <summary>
		/// If not added already, this method adds all default GameObjects and NPC spawnpoints to this map.
		/// </summary>
		public void SpawnMapLater()
		{
			AddMessage(SpawnMap);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="SpawnMapLater"/>
		/// <remarks>Requires map context</remarks>
		public virtual void SpawnMap()
		{
			EnsureContext();

			if (!IsSpawned)
			{
				if (!m_MapTemplate.NotifySpawning(this))
				{
					return;
				}

				if (!m_gosSpawned)
				{
					if (GOMgr.Loaded)
					{
						var count = ObjectCount;
						SpawnGOs();
						if (count > 0)
						{
							s_log.Debug("Added {0} Objects to Map: {1}", ObjectCount - count, this);
						}
						m_gosSpawned = true;
					}
				}

				if (!m_npcsSpawned)
				{
					if (NPCMgr.Loaded)
					{
						var count = ObjectCount;
						SpawnNPCs();
						AddMessage(() =>
						{
							if (count > 0)
							{
								s_log.Debug("Added {0} NPC Spawnpoints to Map: {1}", ObjectCount - count, this);
							}
						});
						m_npcsSpawned = true;
					}
				}

				if (IsSpawned)
				{
					m_MapTemplate.NotifySpawned(this);
				}
			}
		}

		protected virtual void SpawnGOs()
		{
			var templates = GOMgr.GetSpawnPoolTemplatesByMap(MapId);
			if (templates != null)
			{
				foreach (var templ in templates)
				{
					if (templ.AutoSpawns && IsEventActive(templ.EventId))
					{
						AddGOSpawnPool(templ);
					}
				}
			}
		}

		protected virtual void SpawnNPCs()
		{
			var poolTemplates = NPCMgr.GetSpawnPoolTemplatesByMap(Id);
			if (poolTemplates != null)
			{
				foreach (var templ in poolTemplates)
				{
					if (templ.AutoSpawns && IsEventActive(templ.EventId))
					{
						AddNPCSpawnPoolNow(templ);
					}
				}
			}
		}

		public void ForeachSpawnPool(Action<NPCSpawnPool> func)
		{
			ForeachSpawnPool(Vector3.Zero, 0, func);
		}

		public void ForeachSpawnPool(Vector3 pos, float radius, Action<NPCSpawnPool> func)
		{
			var radiusSq = radius * radius;
			foreach (var pool in m_npcSpawnPools.Values)
			{
				if (pool.Template.Entries.Any(spawn => radius <= 0 || spawn.GetDistSq(pos) < radiusSq))
				{
					func(pool);
				}
			}
		}

		public void RespawnInRadius(Vector3 pos, float radius)
		{
			ForeachSpawnPool(pos, radius, pool => pool.RespawnFull());
		}

		public void SpawnZone(ZoneId id)
		{
			// TODO: Add Zone spawning
		}
		#endregion

		#region Messages & Updates
		/// <summary>
		/// The time in seconds when the last 
		/// Update started.
		/// </summary>
		public DateTime LastUpdateTime
		{
			get { return m_lastUpdateTime; }
		}

		/// <summary>
		/// Adds a new Updatable right away.
		/// Requires Map context.
		/// <see cref="RegisterUpdatableLater"/>
		/// </summary>
		/// <param name="updatable"></param>
		public void RegisterUpdatable(IUpdatable updatable)
		{
			EnsureContext();
			m_updatables.Add(updatable);
		}

		/// <summary>
		/// Unregisters an Updatable right away.
		/// In map context.
		/// <see cref="UnregisterUpdatableLater"/>
		/// </summary>
		public void UnregisterUpdatable(IUpdatable updatable)
		{
			EnsureContext();
			m_updatables.Remove(updatable);
		}

		/// <summary>
		/// Registers the given Updatable during the next Map Tick
		/// </summary>
		public void RegisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => RegisterUpdatable(updatable)));
		}

		/// <summary>
		/// Unregisters the given Updatable during the next Map Update
		/// </summary>
		public void UnregisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => UnregisterUpdatable(updatable)));
		}

		/// <summary>
		/// Executes the given action after the given delay within this Map's context.
		/// </summary>
		/// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Map's context.</remarks>
		public TimerEntry CallDelayed(int millis, Action action)
		{
			var timer = new TimerEntry();
			timer.Action = delay =>
			{
				action();
				UnregisterUpdatableLater(timer);
			};
			timer.Start(millis, 0);
			RegisterUpdatableLater(timer);
			return timer;
		}

		/// <summary>
		/// Executes the given action after the given delay within this Map's context.
		/// </summary>
		/// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Map's context.</remarks>
		public TimerEntry CallPeriodically(int seconds, Action action)
		{
			var timer = new TimerEntry
			{
				Action = (delay => { action(); })
			};
			timer.Start(seconds, seconds);
			RegisterUpdatableLater(timer);
			return timer;
		}

		/// <summary>
		/// Adds a message to the message queue for this map.
		/// TODO: Consider extra-message for Character that checks whether Char is still in Map?
		/// </summary>
		/// <param name="action">the action to be enqueued</param>
		public void AddMessage(Action action)
		{
			AddMessage((Message)action);
		}

		/// <summary>
		/// Adds a message to the message queue for this map.
		/// </summary>
		/// <param name="msg">the message</param>
		public void AddMessage(IMessage msg)
		{
			// make sure, Map is running
			Start();
			m_messageQueue.Enqueue(msg);
		}

		/// <summary>
		/// Callback for executing updates of the map, which includes updates for all inhabiting objects.
		/// </summary>
		/// <param name="state">the <see cref="Map" /> to update</param>
		private void MapUpdateCallback(object state)
		{
			// we need some locking here because else MapUpdateCallback could be called twice
			if (Interlocked.CompareExchange(ref m_currentThreadId, Thread.CurrentThread.ManagedThreadId, 0) == 0)
			//lock (m_mapTime)
			{
				// get the time at the start of our callback
				var updateStart = DateTime.Now;
				var updateDelta = (updateStart - m_lastUpdateTime).ToMilliSecondsInt();

				// see if we have any messages to execute
				if (m_messageQueue.Count > 0)
				{
					// process all the messages
					IMessage msg;

					while (m_messageQueue.TryDequeue(out msg))
					{
						try
						{
							msg.Execute();
						}
						catch (Exception e)
						{
							LogUtil.ErrorException(e, "Exception raised when processing Message.");
						}
					}
				}

				for (var i = m_characters.Count - 1; i >= 0; i--)
				{
					var chr = m_characters[i];

					// process all the Character messages
					IMessage msg;

					while (chr.MessageQueue.TryDequeue(out msg))
					{
						try
						{
							msg.Execute();
						}
						catch (Exception e)
						{
							LogUtil.ErrorException(e, "Exception raised when processing Message for: {0}", chr);
							chr.Client.Disconnect();
						}
					}
				}

				// check to see if it's time to run a map update yet again
				//if (m_lastMapUpdate + DefaultUpdateDelay <= now)

				m_isUpdating = true;

				// Updatables
				// progress pending updates
				foreach (var updatable in m_updatables)
				{
					try
					{
						updatable.Update(updateDelta);
					}
					catch (Exception e)
					{
						LogUtil.ErrorException(e, "Exception raised when updating Updatable: " + updatable);
						UnregisterUpdatableLater(updatable);
					}
				}

				// update all Objects in the Map
				// copy objects, so object-internal message processing won't interfere with updating
				foreach (var obj in m_objects.Values)
				{
					if (obj.IsTeleporting)
					{
						// dont update while teleporting
						continue;
					}

					UpdatePriority priority;
					if (!obj.IsAreaActive)
					{
						if (!m_updateInactiveAreas)
						{
							continue;
						}
						priority = UpdatePriority.Inactive;
					}
					else
					{
						//priority = obj.UpdatePriority;
						priority = UpdatePriority.HighPriority;
					}

					var tickMatch = m_tickCount + obj.GetUInt32(ObjectFields.GUID_2);

					try
					{
						// Update Object
						var ticks = UpdatePriorityTicks[(int)priority];
						if (tickMatch % ticks == 0)
						{
							if (ticks > 1)
							{
								// TODO: Fix exact amount of passed time
								obj.Update(updateDelta + (((ticks - 1) * m_updateDelay)));
							}
							else
							{
								obj.Update(updateDelta);
							}
						}
					}
					catch (Exception e)
					{
						LogUtil.ErrorException(e, "Exception raised when updating Object: " + obj);

						// Fail-safe:
						if (obj is Unit)
						{
							var unit = (Unit)obj;
							if (unit.Brain != null)
							{
								unit.Brain.IsRunning = false;
							}
						}
						if (obj is Character)
						{
							((Character)obj).Client.Disconnect();
						}
						else
						{
							obj.Delete();
						}
					}
				}

				if (m_tickCount % CharacterUpdateEnvironmentTicks == 0)
				{
					UpdateCharacters();
				}

				UpdateMap();	// map specific stuff

				// we updated the map, so set our last update time to now
				m_lastUpdateTime = updateStart;
				m_tickCount++;
				m_isUpdating = false;

				// get the time, now that we've finished our update callback
				var updateEnd = DateTime.Now;
				var newUpdateDelta = updateEnd - updateStart;

				// weigh old update-time 9 times and new update-time once
				_avgUpdateTime = ((_avgUpdateTime * 9) + (float)(newUpdateDelta).TotalMilliseconds) / 10;

				// make sure to unset the ID *before* enqueuing the task in the ThreadPool again
				Interlocked.Exchange(ref m_currentThreadId, 0);
				if (m_running)
				{
					var callbackTimeout = m_updateDelay - newUpdateDelta.ToMilliSecondsInt();
					if (callbackTimeout < 0)
					{
						// even if we are in a hurry: For the sake of load-balance we have to give control back to the ThreadPool
						callbackTimeout = 0;
					}

					Task.Factory.StartNewDelayed(callbackTimeout, MapUpdateCallback, this);
				}
				else
				{
					if (IsDisposed)
					{
						// the Map was marked as disposed and can now be trashed
						Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Can be overridden to add any kind of to be executed every Map-tick
		/// </summary>
		protected virtual void UpdateMap()
		{
		}

		internal void UpdateCharacters()
		{
			var updatedObjs = WorldObject.WorldObjectSetPool.Obtain();
			var count = m_characters.Count;
			for (var i = 0; i < count; i++)
			{
				var chr = m_characters[i];
				try
				{
					chr.UpdateEnvironment(updatedObjs);
				}
				catch (Exception e)
				{
					LogUtil.ErrorException(e, "Exception raised when updating Character {0}", chr);
				}
			}

			foreach (var updated in updatedObjs)
			{
				updated.ResetUpdateInfo();
			}
			updatedObjs.Clear();
			WorldObject.WorldObjectSetPool.Recycle(updatedObjs);
		}

		/// <summary>
		/// Instantly updates all active Characters' environment: Collect environment info and send update deltas
		/// </summary>
		public void ForceUpdateCharacters()
		{
			if (IsInContext && !IsUpdating)
			{
				UpdateCharacters();
			}
			else
			{
				AddMessageAndWait(UpdateCharacters);
			}
		}

		public void CallOnAllCharacters(Action<Character> action)
		{
			ExecuteInContext(() =>
			{
				foreach (var chr in m_characters)
				{
					action(chr);
				}
			});
		}

		public void CallOnAllNPCs(Action<NPC> action)
		{
			ExecuteInContext(() =>
			{
				foreach (var obj in m_objects.Values)
				{
					if (obj is NPC)
					{
						action(((NPC)obj));
					}
				}
			});
		}

		internal void UpdateWorldStates(uint index, int value)
		{
			foreach (var zone in MainZones)
			{
				zone.WorldStates.UpdateWorldState(index, value);
			}
		}

		#endregion

		#region Space Management

		/// <summary>
		/// Partitions the space of the zone.
		/// </summary>
		void PartitionSpace()
		{
			// Start partitioning the map space
			m_root.PartitionSpace(null, ZoneSpacePartitionNode.DefaultPartitionThreshold, 0);
		}

		/// <summary>
		/// Gets the partitioned node in which the point lies.
		/// </summary>
		/// <param name="pt">the point to search for</param>
		/// <returns>a <see cref="ZoneSpacePartitionNode" /> if found; null otherwise</returns>
		internal ZoneSpacePartitionNode GetNodeFromPoint(ref Vector3 pt)
		{
			if (m_root == null)
				return null;

			// Return the node at the given point, if it exists
			return m_root.GetLeafFromPoint(ref pt);
		}

		/// <summary>
		/// Checks to see if the supplied location is within this zone's bounds.
		/// </summary>
		/// <param name="point">the point to check for containment</param>
		/// <returns>true if the location is within the bounds, false otherwise</returns>
		public bool IsPointInMap(ref Vector3 point)
		{
			if (m_root != null)
			{
				return m_root.Bounds.Contains(ref point);
			}

			return false;
		}

		/// <summary>
		/// Gets a zone by its area ID.
		/// </summary>
		/// <param name="id">the ID of the zone to search for</param>
		/// <returns>the Zone object representing the specified ID; null if the zone was not found</returns>
		public Zone GetZone(ZoneId id)
		{
			Zone zone;

			// Try to pull out the zone from the zone collection
			Zones.TryGetValue(id, out zone);

			return zone;
		}

		public Zone GetZone(float x, float y)
		{
			if (m_MapTemplate.ZoneTileSet != null)
			{
				var zoneId = m_MapTemplate.ZoneTileSet.GetZoneId(x, y);
				return GetZone(zoneId);
			}
			//return DefaultZone;
			return null;
		}
		#endregion

		#region GetObjectsInRadius
		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="filter">the entities to return</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public IList<WorldObject> GetObjectsInRadius(Vector3 origin, float radius, ObjectTypes filter, uint phase, int limit)
		{
			EnsureContext();

			var entities = new List<WorldObject>();

			if (!(m_root == null || radius < 1))
			{
				// define our search radius
				// we use a sphere because we're searching in 3D
				var sphere = new BoundingSphere(origin, radius);

				m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
			}

			return entities;
		}

		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<T> GetObjectsInRadius<T>(Vector3 origin, float radius, uint phase, int limit = int.MaxValue) where T : WorldObject
		{
			EnsureContext();

			var entities = new List<T>();

			if (!(m_root == null || radius < 1f))
			{
				var sphere = new BoundingSphere(origin, radius);

				m_root.GetEntitiesInArea(ref sphere, entities, phase, ref limit);
			}

			return entities;
		}

		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="filter">the entities to return</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<T> GetObjectsInRadius<T>(Vector3 origin, float radius, Func<T, bool> filter, uint phase, int limit = int.MaxValue) where T : WorldObject
		{
			EnsureContext();

			var entities = new List<T>();

			if (!(m_root == null || radius < 1f))
			{
				var sphere = new BoundingSphere(origin, radius);
				m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
			}

			return entities;
		}

		/// <summary>
		/// Gets all objects in a radius around the origin, regarding the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="filter">a delegate to filter search results</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public IList<WorldObject> GetObjectsInRadius(Vector3 origin, float radius, Func<WorldObject, bool> filter, uint phase, int limit)
		{
			EnsureContext();

			var entities = new List<WorldObject>();

			if (!(m_root == null || radius < 1))
			{
				// Define our search radius
				var sphere = new BoundingSphere(origin, radius);
				m_root.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limit);
			}

			return entities;
		}
		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="box">the BoundingBox object that matches the area we want to search in</param>
		/// <param name="filter">the entities to return</param>
		/// <param name="limit">max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<WorldObject> GetObjectsInBox(ref Vector3 origin, BoundingBox box, ObjectTypes filter, int limit, uint phase)
		{
			EnsureContext();

			var entities = new List<WorldObject>();

			if (!(m_root == null || box.Min == box.Max))
			{
				m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
			}

			return entities;
		}

		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="box">the BoundingBox object that matches the area we want to search in</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<T> GetObjectsInBox<T>(ref Vector3 origin, BoundingBox box, uint phase, int limit) where T : WorldObject
		{
			EnsureContext();

			var entities = new List<T>();

			if (!(m_root == null || box.Min == box.Max))
			{
				m_root.GetEntitiesInArea(ref box, entities, phase, ref limit);
			}

			return entities;
		}

		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="box">the BoundingBox object that matches the area we want to search in</param>
		/// <param name="filter">the entities to return</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<T> GetObjectsInBox<T>(ref Vector3 origin, BoundingBox box, Func<T, bool> filter, uint phase, int limit) where T : WorldObject
		{
			EnsureContext();

			var entities = new List<T>();

			if (!(m_root == null || box.Min == box.Max))
			{
				m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
			}

			return entities;
		}

		/// <summary>
		/// Gets all objects in a radius around the origin, regarding the search filter.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="box">the BoundingBox object that matches the area we want to search in</param>
		/// <param name="filter">a delegate to filter search results</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<WorldObject> GetObjectsInBox(ref Vector3 origin, BoundingBox box, Func<WorldObject, bool> filter, uint phase, int limit)
		{
			EnsureContext();

			var entities = new List<WorldObject>();

			if (!(m_root == null || box.Min == box.Max))
			{
				m_root.GetEntitiesInArea(ref box, entities, filter, phase, ref limit);
			}

			return entities;
		}
		#endregion

		#region IterateObjects
		/// <summary>
		/// Iterates over all objects in the given radius around the given origin.
		/// If the predicate returns false, iteration is cancelled and false is returned.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		/// <param name="predicate">Returns whether to continue iteration.</param>
		/// <returns>Whether Iteration was not cancelled (usually indicating that we did not find what we were looking for).</returns>
		public bool IterateObjects(Vector3 origin, float radius, uint phase, Func<WorldObject, bool> predicate)
		{
			EnsureContext();

			var sphere = new BoundingSphere(origin, radius);
			return m_root.Iterate(ref sphere, predicate, phase);
		}

		/// <summary>
		/// Sends a packet to all nearby characters.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		/// <param name="includeSelf">whether or not to send the packet to ourselves (if we're a character)</param>
		public void SendPacketToArea(RealmPacketOut packet, ref Vector3 center, uint phase)
		{
			IterateObjects(center, WorldObject.BroadcastRange, phase, obj =>
			{
				if (obj is Character)
				{
					((Character)obj).Send(packet.GetFinalizedPacket());
				}
				return true;
			});
		}

		/// <summary>
		/// Sends a packet to all characters in the map
		/// </summary>
		/// <param name="packet">the packet to send</param>
		public void SendPacketToMap(RealmPacketOut packet)
		{
			CallOnAllCharacters(chr => chr.Send(packet.GetFinalizedPacket()));
		}
		#endregion

		#region Terrain Management
		public void QueryDirectPath(PathQuery query)
		{
			m_Terrain.QueryDirectPath(query);
		}
		#endregion

		#region Object Management
		/// <summary>
		/// Removes an entity from the zone.
		/// </summary>
		/// <param name="obj">the entity to remove</param>
		/// <returns>true if the entity was removed; false otherwise</returns>
		public bool RemoveObjectLater(WorldObject obj)
		{
			// We can't remove an object if we don't know of it
			if (!m_objects.ContainsKey(obj.EntityId))
			{
				return false;
			}

			m_messageQueue.Enqueue(GetRemoveObjectTask(obj, this));

			return true;
		}

		public void AddObject(WorldObject obj)
		{
			if (m_isUpdating || !IsInContext)
			{
				// Cannot add during Map Update
				AddObjectLater(obj);
			}
			else
			{
				AddObjectNow(obj);
			}
		}

		public void AddObject(WorldObject obj, Vector3 pos)
		{
			if (m_isUpdating || !IsInContext)
			{
				// Cannot add during Map Update
				TransferObjectLater(obj, pos);
			}
			else
			{
				AddObjectNow(obj, ref pos);
			}
		}

		public void AddObject(WorldObject obj, ref Vector3 pos)
		{
			if (m_isUpdating || !IsInContext)
			{
				// Cannot add during Map Update
				TransferObjectLater(obj, pos);
			}
			else
			{
				AddObjectNow(obj, ref pos);
			}
		}

		/// <summary>
		/// Adds the given Object to this Map.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		public void AddObjectNow(WorldObject obj, Vector3 pos)
		{
			obj.Position = pos;
			AddObjectNow(obj);
		}

		/// <summary>
		/// Adds the given Object to this Map.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		public void AddObjectNow(WorldObject obj, ref Vector3 pos)
		{
			obj.Position = pos;
			AddObjectNow(obj);
		}

		/// <summary>
		/// Adds the given Object to this Map.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		public void AddObjectNow(WorldObject obj)
		{
			try
			{
				EnsureNotUpdating();
				EnsureContext();

				if (IsDisposed)
				{
					// make sure, nothing gets in anymore
					if (!(obj is Character))
					{
						obj.Delete();
					}
					else
					{
						((Character)obj).TeleportToBindLocation();
					}
					return;
				}

				if (obj.IsDeleted)
				{
					// object has been deleted before it was added
					s_log.Warn("Tried to add deleted object \"{0}\" to Map: " + this);
					return;
				}

				// Add them to the quad-tree
				if (!m_root.AddObject(obj))
				{
					s_log.Error("Could not add Object to Map {0} at {1} (Map Bounds: {2})", this, obj.Position, Bounds);
					if (!(obj is Character))
					{
						obj.Delete();
					}
					else
					{
						((Character)obj).TeleportToBindLocation();
					}
					return;
				}

				obj.Map = this;

				m_objects.Add(obj.EntityId, obj);

				if (obj is Unit)
				{
					if (obj is Character)
					{
						var chr = (Character)obj;
						m_characters.Add(chr);
						if (chr.Role.Status == RoleStatus.Player)
						{
							AddPlayerCount(chr);
						}
						OnEnter(chr);
					}
					else if (obj is NPC)
					{
						// remember all SpiritHealers
						var npc = (NPC)obj;
						if (npc.IsSpiritHealer)
						{
							m_spiritHealers.Add(npc);
						}
					}
				}

				// TODO: For now enforce the MainZone and send the states of the default zone to force an update
				if (MainZoneCount == 1)
				{
					obj.SetZone(m_defaultZone);
				}
				else if (MainZoneCount > 1)
				{
					if (obj is Character)
					{
						MiscHandler.SendInitWorldStates((Character)obj, DefaultZone.WorldStates, DefaultZone);
					}
				}

				obj.OnEnterMap();
				obj.RequestUpdate();

				if (obj is Character)
				{
					m_MapTemplate.NotifyPlayerEntered(this, (Character)obj);
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Unable to add Object \"{0}\" to Map: {1}", obj, this);
				obj.DeleteNow();
			}
		}

		internal void AddPlayerCount(Character chr)
		{
			if (chr.Faction.IsHorde)
			{
				m_hordeCount++;
			}
			else
			{
				m_allyCount++;
			}
		}

		/// <summary>
		/// Gets a WorldObject by its ID.
		/// Requires map context.
		/// </summary>
		/// <param name="id">the ID of the WorldObject</param>
		/// <returns>The corresponding <see cref="WorldObject" /> if found; null otherwise</returns>
		public WorldObject GetObject(EntityId id)
		{
			WorldObject entity;
			m_objects.TryGetValue(id, out entity);
			return entity;
		}

		internal void RemovePlayerCount(Character chr)
		{
			if (chr.Faction.IsHorde)
			{
				m_hordeCount--;
			}
			else
			{
				m_allyCount--;
			}
		}

		/// <summary>
		/// Called when a Character object is added to the Map
		/// </summary>
		protected virtual void OnEnter(Character chr)
		{
		}

		/// <summary>
		/// Called when a Character object is removed from the Map
		/// </summary>
		protected virtual void OnLeave(Character chr)
		{
		}

		public virtual void OnSpawnedCorpse(Character chr)
		{
			chr.TeleportToNearestGraveyard();
		}

		public void RemoveObject(WorldObject obj)
		{
			if (m_isUpdating || !IsInContext)
			{
				// Cannot remove during Map Update
				RemoveObjectLater(obj);
			}
			else
			{
				RemoveObjectNow(obj);
			}
		}

		/// <summary>
		/// Removes an entity from this Map, instantly.
		/// Also make sure that the given Object is actually within this Map.
		/// Requires map context.
		/// </summary>
		/// <param name="obj">the entity to remove</param>
		/// <returns>true if the entity was removed; false otherwise</returns>
		public void RemoveObjectNow(WorldObject obj)
		{
			EnsureContext();
			EnsureNotUpdating();
			obj.EnsureContext();

			//if (m_isUpdating)
			//{
			//    throw new InvalidOperationException(string.Format(
			//        "Tried calling RemoveObjectNow() in {0} while updating: " + obj, this));
			//}

			if (obj is Character)
			{
				var chr = (Character)obj;
				if (m_characters.Remove(chr))
				{
					if (chr.Role.Status == RoleStatus.Player)
					{
						RemovePlayerCount(chr);
					}
					OnLeave(chr);
				}
				//if (chr.Zone != null)
				//{
				//    chr.Zone.LeaveZone(chr);
				//    chr.Zone = null;
				//}

				m_MapTemplate.NotifyPlayerLeft(this, (Character)obj);
			}

			obj.OnLeavingMap();				// call before actually removing

			// Remove object from the quadtree
			if (obj.Node != null && obj.Node.RemoveObject(obj))
			{
				m_objects.Remove(obj.EntityId);

				if (obj is Character)
				{
					m_characters.Remove((Character)obj);
				}
				else if (obj is NPC && ((NPC)obj).IsSpiritHealer)
				{
					m_spiritHealers.Remove((NPC)obj);
				}
			}
		}


		/// <summary>
		/// Returns all objects within the Map.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <returns>an array of all objects in the map</returns>
		public WorldObject[] CopyObjects()
		{
			EnsureContext();
			return m_objects.Values.ToArray();
		}

		public virtual bool CanEnter(Character chr)
		{
			return chr.Level >= MinLevel &&
				(MaxLevel == 0 || chr.Level <= MaxLevel) &&
				(m_MapTemplate.MayEnter(chr) &&
				(MaxPlayerCount == 0 || PlayerCount < MaxPlayerCount));
		}

		public virtual void TeleportOutside(Character chr)
		{
			chr.TeleportToBindLocation();
		}

		/// <summary>
		/// Returns the specified GameObject that is closest to the given point.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <returns>the closest GameObject to the given point, or null if none found.</returns>
		public GameObject GetNearestGameObject(Vector3 pos, GOEntryId goId, uint phase = WorldObject.DefaultPhase)
		{
			EnsureContext();

			GameObject closest = null;
			float distanceSq = int.MaxValue;

			IterateObjects(pos, WorldObject.BroadcastRange, phase, obj =>
			{
				if (obj is GameObject && ((GameObject)obj).Entry.GOId == goId)
				{
					var currentDistanceSq = obj.GetDistanceSq(ref pos);

					if (currentDistanceSq < distanceSq)
					{
						distanceSq = currentDistanceSq;
						closest = (GameObject)obj;
					}
				}
				return true;

			});

			return closest;
		}

		/// <summary>
		/// Returns the specified NPC that is closest to the given point.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <returns>the closest NPC to the given point, or null if none found.</returns>
		public NPC GetNearestNPC(ref Vector3 pos, NPCId id)
		{
			EnsureContext();

			NPC closest = null;
			float distanceSq = int.MaxValue;

			foreach (var obj in m_objects.Values)
			{
				if (obj is NPC && ((NPC)obj).Entry.NPCId == id)
				{
					continue;
				}

				var currentDistanceSq = obj.GetDistanceSq(ref pos);

				if (currentDistanceSq < distanceSq)
				{
					distanceSq = currentDistanceSq;
					closest = (NPC)obj;
				}
			}

			return closest;
		}

		/// <summary>
		/// Returns the spirit healer that is closest to the given point.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		/// <returns>the closest spirit healer, or null if none found.</returns>
		public virtual NPC GetNearestSpiritHealer(ref Vector3 pos)
		{
			EnsureContext();

			NPC closestHealer = null;
			int distanceSq = int.MaxValue, currentDistanceSq;
			foreach (var healer in m_spiritHealers)
			{
				currentDistanceSq = healer.GetDistanceSqInt(ref pos);

				if (currentDistanceSq < distanceSq)
				{
					distanceSq = currentDistanceSq;
					closestHealer = healer;
				}
			}

			return closestHealer;
		}
		#endregion

		#region GOs
		/// <summary>
		/// Gets the first GO with the given SpellFocus within the given radius around the given position.
		/// </summary>
		public GameObject GetGOWithSpellFocus(Vector3 pos, SpellFocus focus, float radius, uint phase)
		{
			foreach (GameObject go in GetObjectsInRadius(pos, radius, ObjectTypes.GameObject, phase, 0))
			{
				if (go.Entry is GOSpellFocusEntry && ((GOSpellFocusEntry)go.Entry).SpellFocus == focus)
				{
					return go;
				}
			}

			return null;
		}
		#endregion

		#region Move Objects
		/// <summary>
		/// Enqueues an object to be moved into this Map during the next Map-update
		/// </summary>
		/// <param name="obj">the entity to add</param>
		/// <returns>true if the entity was added, false otherwise</returns>
		protected internal bool AddObjectLater(WorldObject obj)
		{
			Start();

			var moveTask = new Message(() => AddObjectNow(obj));

			m_messageQueue.Enqueue(moveTask);

			return true;
		}

		/// <summary>
		/// Enqueues an object to be removed from its old map (if already added to one) and 
		/// moved into this one during the next Map-updates of the maps involved. 
		/// </summary>
		/// <param name="obj">the object to add</param>
		/// <returns>true if the entity was added, false otherwise</returns>
		internal bool TransferObjectLater(WorldObject obj, Vector3 newPos)
		{
			if (obj.IsTeleporting)
			{
				// this might take a while
				var pos2 = newPos;
				obj.AddMessage(() => TransferObjectLater(obj, pos2));
				return true;
			}

			if (obj.Map == this)
			{
				MoveObject(obj, ref newPos);
			}
			else
			{
				obj.IsTeleporting = true;
				var oldMap = obj.Map;
				if (oldMap != null)
				{
					var moveTask = new Message(() =>
					{
						oldMap.RemoveObjectNow(obj);
						obj.Map = this;

						var addTask = new Message(() =>
						{
							obj.IsTeleporting = false;
							obj.Position = newPos;
							AddObjectNow(obj);
						});

						AddMessage(addTask);
					});

					oldMap.AddMessage(moveTask);
				}
				else
				{
					var addTask = new Message(() =>
					{
						obj.IsTeleporting = false;
						obj.Position = newPos;
						AddObjectNow(obj);
					});

					AddMessage(addTask);
				}
			}

			return true;
		}

		public bool MoveObject(WorldObject obj, Vector3 newPos)
		{
			return MoveObject(obj, ref newPos);
		}

		/// <summary>
		/// Moves the given object to the given position (does not animate movement)
		/// </summary>
		/// <param name="newPos">the position to move the entity to</param>
		/// <param name="obj">the entity to move</param>
		/// <returns>true if the entity was moved, false otherwise</returns>
		public bool MoveObject(WorldObject obj, ref Vector3 newPos)
		{
			// Check for a partitioned map, and check the placement bounds
			if (m_root == null || !m_root.Bounds.Contains(ref newPos))
				return false;

			// Get the leaf node of the new position
			var newNode = m_root.GetLeafFromPoint(ref newPos);
			if (newNode == null)
				return false;

			// Get the leaf node of the current position
			var curNode = obj.Node;
			if (curNode == null)
				return false;

			// TODO: is this save?
			//m_messageQueue.Add(GetMoveObjectTask(obj, this, newPos));
			MoveObject(obj, ref newPos, newNode, curNode);

			if (obj is Unit)
			{
				((Unit)obj).OnMove();
			}
			return true;
		}


		static void MoveObject(WorldObject obj, ref Vector3 newPos,
			ZoneSpacePartitionNode newNode, ZoneSpacePartitionNode curNode)
		{
			// If the nodes aren't the same, 
			if (newNode != curNode)
			{
				curNode.RemoveObject(obj);

				if (newNode.AddObject(obj))
				{
					obj.Position = newPos;
				}
			}
			else
			{
				obj.Position = newPos;
			}
		}
		#endregion

        /// <summary>
        /// Checks if the event is currently active
        /// </summary>
        /// <param name="eventId">Id of the event to check</param>
        /// <returns></returns>
		public bool IsEventActive(uint eventId)
		{
			return WorldEventMgr.IsEventActive(eventId);
		}

		/// <summary>
		/// Enumerates all objects within the map.
		/// </summary>
		/// <remarks>Requires map context.</remarks>
		public IEnumerator<WorldObject> GetEnumerator()
		{
			EnsureContext();
			return m_objects.Values.GetEnumerator();
		}

		#region Waiting
		/// <summary>
		/// Adds the given message to the map's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the map is running before calling this method.</remarks>
		/// <remarks>Must not be called from the map context.</remarks>
		public void AddMessageAndWait(Action action)
		{
			AddMessageAndWait(new Message(action));
		}

		/// <summary>
		/// Adds the given message to the map's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the map is running before calling this method.</remarks>
		/// <remarks>Must not be called from the map context.</remarks>
		public void AddMessageAndWait(IMessage msg)
		{
			EnsureNoContext();

			Start();

			// to ensure that we are not exiting in the current message-loop, add an updatable
			// which again registers the message
			var updatable = new SimpleUpdatable();
			updatable.Callback = () =>
				AddMessage(new Message(() =>
				{
					msg.Execute();
					lock (msg)
					{
						Monitor.PulseAll(msg);
					}
					UnregisterUpdatable(updatable);
				}));

			lock (msg)
			{
				RegisterUpdatableLater(updatable);
				// int delay = this.GetWaitDelay();
				Monitor.Wait(msg);
				// Assert.IsTrue(added, string.Format(debugMsg, args));
			}
		}

		/// <summary>
		/// Waits for one map tick before returning.
		/// </summary>
		/// <remarks>Must not be called from the map context.</remarks>
		public void WaitOneTick()
		{
			EnsureNoContext();

			AddMessageAndWait(new Message(() =>
			{
				// do nothing
			}));
		}

		/// <summary>
		/// Waits for the given amount of ticks.
		/// One tick might take 0 until Map.UpdateSpeed milliseconds.
		/// </summary>
		/// <remarks>Make sure that the map is running before calling this method.</remarks>
		/// <remarks>Must not be called from the map context.</remarks>
		public void WaitTicks(int ticks)
		{
			EnsureNoContext();

			for (int i = 0; i < ticks; i++)
			{
				WaitOneTick();
			}
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0} (Id: {1}{2}, Players: {3}{4} (Alliance: {5}, Horde: {6}))",
								 Name,
								 (int)Id,
								 InstanceId != 0 ? ", #" + InstanceId : "",
								 PlayerCount,
								 MaxPlayerCount > 0 ? " / " + MaxPlayerCount : "",
								 m_allyCount,
								 m_hordeCount);
		}

		private bool m_IsDisposed;

		/// <summary>
		/// Indicates whether this Map is disposed. 
		/// Disposed Maps may not be used any longer.
		/// </summary>
		public bool IsDisposed
		{
			get { return m_IsDisposed; }
			protected set
			{
				m_IsDisposed = value;
				IsRunning = false;
			}
		}

		protected virtual void Dispose()
		{
			m_running = false;
			m_root = null;
			m_objects = null;
			m_characters = null;
			m_spiritHealers = null;
			m_updatables = null;
		}

		#region IGenericChatTarget Members

		public void SendMessage(string message)
		{
			AddMessage(new Message1<Map>(
							this, rgn => ChatMgr.SendSystemMessage(rgn.m_characters, message)
						));

			ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System, this);
		}

		#endregion

		/// <summary>
		/// Is called whenever a Character dies.
		/// </summary>
		/// <param name="action"></param>
		protected internal virtual void OnPlayerDeath(IDamageAction action)
		{
			if (action.Attacker.IsPvPing)
			{
				if (action.Victim.YieldsXpOrHonor)
				{
					var attacker = ((Character)action.Attacker);
					attacker.OnHonorableKill(action);
					OnHonorableKill(action);
				}
			}
			if (action.Victim is Character)
			{
				var chr = action.Victim as Character;
				chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathAtMap, (uint)MapId, 1);

				if (action.Attacker is Character)
				{
					var killer = action.Attacker as Character;
					chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByPlayer, (uint)killer.FactionGroup, 1);
				}
				else
				{
					chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByCreature, action.Attacker.EntryId, 1);
				}
			}
		}

		/// <summary>
		/// Is called whenevr an honorable character was killed by another character
		/// </summary>
		/// <param name="action"></param>
		protected internal virtual void OnHonorableKill(IDamageAction action)
		{
		}

		/// <summary>
		/// Is called whenever an NPC dies
		/// </summary>
		protected internal virtual void OnNPCDied(NPC npc)
		{
			if (npc.YieldsXpOrHonor)
			{
				var playerLooter = npc.FirstAttacker != null ? npc.FirstAttacker.PlayerOwner : null;
				if (playerLooter != null)
				{
					if (XpCalculator != null)
					{
						// distribute XP
						// TODO: Consider reductions if someone else killed the mob
						var chr = playerLooter;
						var baseXp = XpCalculator(playerLooter.Level, npc);
						if (npc.Entry.Rank >= CreatureRank.Elite)
						{
							// elites give double xp
							baseXp *= 2;
						}

						XpGenerator.CombatXpDistributer(chr, npc, baseXp);

						if (chr.Group != null)
						{
							chr.Group.OnKill(chr, npc);
						}
						else
						{
							chr.QuestLog.OnNPCInteraction(npc);
							chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KillCreature, npc.EntryId, 1);
						}
					}
				}
			}
		}
	}
}