/*************************************************************************
 *
 *   file		: Region.cs
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
using WCell.RealmServer.Res;
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

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// Represents a continent or Instance (including Battleground instances), that may or may not contain Zones.
	/// X-Axis: South -> North
	/// Y-Axis: East -> West
	/// Z-Axis: Down -> Up
	/// Orientation: North = 0, 2*Pi, counter-clockwise
	/// </summary>
	public partial class Region : IGenericChatTarget, IRegionId, IContextHandler, IWorldSpace
	{
		#region Global Variables

		/// <summary>
		/// Whether to spawn NPCs and GOs immediately, whenever a new Region gets activated the first time.
		/// For developing you might want to toggle this off and use the "Region Spawn" command ingame to spawn the region, if necessary.
		/// </summary>
		[Variable("AutoSpawnRegions")]
		public static bool AutoSpawn = true;

		/// <summary>
		/// Default update delay in milliseconds
		/// </summary>
		public static int DefaultUpdateDelay = 120;

		/// <summary>
		/// Every how many ticks to send UpdateField-changes to Characters
		/// </summary>
		public static int CharacterUpdateEnvironmentTicks = 5;

		[Variable("UpdateInactiveAreas")]
		public static bool UpdateInactiveAreasDefault = true;

		/// <summary>
		/// Whether to have NPCs scan inactive nodes for enemies
		/// </summary>
		[Variable("ScanInactiveAreas")]
		public static bool ScanInactiveAreasDefault;

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

		static Region()
		{
			SetUpdatePriorityTicks();
		}

		protected static Logger s_log = LogManager.GetCurrentClassLogger();

		private static int avgUpdateTime;

		public static string LoadAvgStr
		{
			get
			{
				return string.Format("{0}/{1} ({2} %)", avgUpdateTime, DefaultUpdateDelay, (avgUpdateTime / (float)DefaultUpdateDelay) * 100);
			}
		}

		#region Fields
		internal protected RegionInfo m_regionInfo;
		protected Dictionary<EntityId, WorldObject> m_objects;
		protected ZoneSpacePartitionNode m_root;
		protected ZoneTileSet m_zoneTileSet;
		protected ITerrain m_Terrain;

		protected volatile bool m_running;
		protected float m_lastRegionUpdate, m_lastUpdate;
		protected int m_tickCount;

		protected Stopwatch m_regionTime = new Stopwatch();
		protected int m_updateDelay;
		protected bool m_updateInactiveAreas, m_ScanInactiveAreas;
		protected bool m_canNPCsEvade;

		protected List<Character> m_characters = new List<Character>(50);
		int m_allyCount, m_hordeCount;
		protected List<NPC> m_spiritHealers = new List<NPC>();

		protected LockfreeQueue<IMessage> m_messageQueue = new LockfreeQueue<IMessage>();
		protected List<IUpdatable> m_updatables = new List<IUpdatable>();
		protected int m_currentThreadId;

		bool m_npcsSpawned, m_gosSpawned, m_isUpdating;
		private float m_lastUpdateDT;
		internal protected GameObject[] m_gos = new GameObject[0];
		internal protected SpawnPoint[] m_spawnPoints = new SpawnPoint[0];
		private int m_spawnPointCount;

		protected bool m_CanFly;

		Zone m_defaultZone;
		/// <summary>
		/// List of all Main zones (that have no parent-zone but only children)
		/// </summary>
		public readonly List<Zone> MainZones = new List<Zone>(5);

		/// <summary>
		/// All the Zone-instances within this region.
		/// </summary>
		public readonly IDictionary<ZoneId, Zone> Zones = new Dictionary<ZoneId, Zone>();

		/// <summary>
		/// The first TaxiPath-node of this Region (or null).
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
		protected Region()
		{
			m_objects = new Dictionary<EntityId, WorldObject>();

			m_updateDelay = DefaultUpdateDelay;

			m_updateInactiveAreas = UpdateInactiveAreasDefault;
			m_ScanInactiveAreas = ScanInactiveAreasDefault;
			m_canNPCsEvade = CanNPCsEvadeDefault;
		}

		/// <summary>
		/// Creates a region from the given region info.
		/// </summary>
		/// <param name="rgnInfo">the info for this region to use</param>
		public Region(RegionInfo rgnInfo) :
			this()
		{
			m_regionInfo = rgnInfo;
			m_CanFly = rgnInfo.Id == MapId.Outland || rgnInfo.Id == MapId.Northrend;
		}

		protected internal void InitRegion(RegionInfo info)
		{
			m_regionInfo = info;
			InitRegion();
		}

		/// <summary>
		/// Method is called after Creation of Region
		/// </summary>
		protected internal virtual void InitRegion()
		{
			// Set our region's bounds and Terrain
			m_Terrain = TerrainMgr.GetTerrain(m_regionInfo.Id);
			m_zoneTileSet = m_regionInfo.ZoneTileSet;
			m_root = new ZoneSpacePartitionNode(m_regionInfo.Bounds);
			//m_root = new ZoneSpacePartitionNode(new BoundingBox(
			//                                        new Vector3(-(TerrainConstants.MapLength / 2), -(TerrainConstants.MapLength / 2), -MAP_HEIGHT),
			//                                        new Vector3((TerrainConstants.MapLength / 2), (TerrainConstants.MapLength / 2), MAP_HEIGHT)
			//                                    ));
			PartitionSpace();

			var states = Constants.World.WorldStates.GetStates(m_regionInfo.Id) ?? WorldState.EmptyArray;
			WorldStates = new WorldStateCollection(this, states);

			CreateZones();
			World.AddRegion(this);

			var evt = Created;
			if (evt != null)
			{
				evt(this);
			}
			m_regionInfo.NotifyCreated(this);
		}

		private void CreateZones()
		{
			for (var i = 0; i < m_regionInfo.ZoneInfos.Count; i++)
			{
				var info = m_regionInfo.ZoneInfos[i];
				var zone = new Zone(this, info);
				if (zone.ParentZone == null)
				{
					MainZones.Add(zone);
				}
				Zones.Add(info.Id, zone);
			}

			m_defaultZone = MainZones.FirstOrDefault();
		}

		#endregion

		#region Properties
		public MapId RegionId
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

		public RegionInfo RegionInfo
		{
			get { return m_regionInfo; }
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
		/// The first MainZone (for reference and Regions that only contain one Zone)
		/// </summary>
		public Zone DefaultZone
		{
			get { return m_defaultZone; }
		}

		/// <summary>
		/// The bounds of this region.
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
		/// The display name of this region.
		/// </summary>
		public string Name
		{
			get { return m_regionInfo.Name; }
		}

		/// <summary>
		/// The map ID of this region.
		/// </summary>
		public MapId Id
		{
			get { return m_regionInfo.Id; }
		}

		/// <summary>
		/// The minimum required ClientId
		/// </summary>
		public ClientId RequiredClient
		{
			get { return m_regionInfo.RequiredClientId; }
		}

		/// <summary>
		/// Whether or not the region is instanced
		/// </summary>
		public bool IsInstance
		{
			get { return this is InstancedRegion; }
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
		/// The type of the region (normal, battlegrounds, instance, etc)
		/// </summary>
		public MapType Type
		{
			get { return m_regionInfo.Type; }
		}

		/// <summary>
		/// Whether or not the region (instance) is in heroic mode
		/// </summary>
		public virtual bool IsHeroic
		{
			get { return false; }
		}

		/// <summary>
		/// The minimum level a player has to be to enter the region (instance)
		/// </summary>
		public virtual int MinLevel
		{
			get { return m_regionInfo.MinLevel; }
		}

		/// <summary>
		/// The maximum level a player can be to enter the region (instance)
		/// </summary>
		public virtual int MaxLevel
		{
			get { return m_regionInfo.MaxLevel; }
		}

		/// <summary>
		/// Maximum number of players allowed in the region (instance)
		/// </summary>
		public int MaxPlayerCount
		{
			get { return m_regionInfo.MaxPlayerCount; }
		}

		/// <summary>
		/// Whether or not the region is currently processing object updates
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
		/// Indicates whether the current Thread is the Region's update-thread.
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
		/// The amount of all Players in this Region (excludes Staff members)
		/// </summary>
		public int PlayerCount
		{
			get { return m_allyCount + m_hordeCount; }
		}

		/// <summary>
		/// The amount of all Characters in this Region (includes Staff members)
		/// </summary>
		public int CharacterCount
		{
			get { return m_characters.Count; }
		}

		/// <summary>
		/// The number of Alliance Players currently in the region (not counting Staff)
		/// </summary>
		public int AllianceCount
		{
			get { return m_allyCount; }
		}

		/// <summary>
		/// The number of Alliance Players currently in the region (not counting Staff)
		/// </summary>
		public int HordeCount
		{
			get { return m_hordeCount; }
		}

		public int SpawnPointCount
		{
			get { return m_spawnPointCount; }
		}

		/// <summary>
		/// Amount of passed ticks in this Region
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
		/// Called whenever an object is removed from the region to determine whether it may stop now.
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
		/// one Region-Update and the next.
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
		/// Total amount of objects within this Region
		/// </summary>
		public int ObjectCount
		{
			get { return m_objects.Count; }
		}

		/// <summary>
		/// Whether NPCs in this Region will try to evade after Combat
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
		/// Starts the Region's update-, message- and timer- loop
		/// </summary>
		public void Start()
		{
			if (!m_running)
			{
				// lock and check again, so Region won't be started twice or start while stoping
				lock (m_regionTime)
				{
					if (m_running)
						return;

					// global sync
					lock (World.PauseLock)
					{
						if (World.Paused)
						{
							// must ensure that IsRunning does not change during World Pause
							throw new InvalidOperationException("Tried to start Region while World is paused.");
						}

						m_running = true;
					}

					s_log.Debug(Resources.RegionStarted, m_regionInfo.Id);

					m_regionTime.Start();

					// start updating
					Task.Factory.StartNewDelayed(m_updateDelay, RegionUpdateCallback, this);

					if (AutoSpawn)
					{
						SpawnRegionLater();
					}

					m_regionInfo.NotifyStarted(this);
				}
			}
		}

		/// <summary>
		/// Stops region updating and stops the update delta measuring
		/// </summary>
		public void Stop()
		{
			if (m_running)
			{
				if (!m_regionInfo.NotifyStopping(this))
				{
					return;
				}

				// lock and check again, so region won't be stopped twice or stopped while starting
				lock (m_regionTime)
				{
					if (!m_running)
					{
						return;
					}
					m_running = false;

					s_log.Debug(Resources.RegionStopped, m_regionInfo.Id);

					m_regionTime.Reset();
					m_lastRegionUpdate = 0;
					m_lastUpdate = 0;

					m_regionInfo.NotifyStopped(this);
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
		/// Ensures execution within the region.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the calling thread isn't the region thread</exception>
		public void EnsureContext()
		{
			if (Thread.CurrentThread.ManagedThreadId != m_currentThreadId && IsRunning)
			{
				Stop();
				throw new InvalidOperationException(string.Format(Resources.RegionContextNeeded, this));
			}
		}

		/// <summary>
		/// Ensures execution outside the Region-context.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the calling thread is the region thread</exception>
		public void EnsureNoContext()
		{
			if (Thread.CurrentThread.ManagedThreadId == m_currentThreadId)
			{
				Stop();
				throw new InvalidOperationException(string.Format(Resources.RegionContextProhibited, this));
			}
		}

		/// <summary>
		/// Ensures that Region is not updating.
		/// </summary>
		/// <exception cref="InvalidOperationException">thrown if the Region is currently updating</exception>
		public void EnsureNotUpdating()
		{
			if (m_isUpdating)
			{
				Stop();
				throw new InvalidOperationException(string.Format(Resources.RegionUpdating, this));
			}
		}
		#endregion

		#region Spawning

		/// <summary>
		/// Whether this Region's NPCs and GOs have been fully spawned
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

		public SpawnPoint AddSpawn(SpawnEntry entry)
		{
			var point = new SpawnPoint(entry, this);
			AddSpawn(point);
			point.Active = true;
			return point;
		}

		void AddSpawn(SpawnPoint point)
		{
			ArrayUtil.Set(ref m_spawnPoints, point.Id, point);
			m_spawnPointCount++;
		}

		internal void RemoveSpawn(SpawnPoint spawn)
		{
			m_spawnPoints[spawn.Id] = null;
			m_spawnPointCount--;
		}

		/// <summary>
		/// Adds a message to the Region to clear it
		/// </summary>
		public void ClearLater()
		{
			AddMessage(RemoveAll);
		}

		/// <summary>
		/// Removes all Objects, NPCs and Spawns from this Region.
		/// </summary>
		/// <remarks>Requires region context</remarks>
		public virtual void RemoveAll()
		{
			RemoveObjects();
		}

		/// <summary>
		/// Removes all Objects and NPCs from the Region
		/// </summary>
		public void RemoveObjects()
		{
			EnsureContext();

			if (!IsUpdating)
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
			m_spawnPointCount = 0;
			for (var i = 0; i < m_spawnPoints.Length; i++)
			{
				var spawnPoint = m_spawnPoints[i];
				if (spawnPoint != null)
				{
					spawnPoint.RemoveSpawnNow();
				}
			}

			var objs = CopyObjects();
			for (var i = 0; i < objs.Length; i++)
			{
				var obj = objs[i];
				if (!(obj is Character) && (!(obj is Unit) || obj.MasterChar != null))
				{
					// only delete things that are not Characters or belong to Characters
					obj.DeleteNow();
				}
			}

			m_gosSpawned = false;
			m_npcsSpawned = false;
			m_gos = new GameObject[100];
		}

		/// <summary>
		/// Clears all Objects and NPCs and spawns the default ones again
		/// </summary>
		public virtual void Reset()
		{
			RemoveAll();

			SpawnRegion();

			for (var i = 0; i < m_spawnPoints.Length; i++)
			{
				var spawnPoint = m_spawnPoints[i];
				if (spawnPoint != null)
				{
					spawnPoint.RespawnFull();
				}
			}
		}

		/// <summary>
		/// If not added already, this method adds all default GameObjects and NPC spawnpoints to this region.
		/// </summary>
		public void SpawnRegionLater()
		{
			AddMessage(SpawnRegion);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="SpawnRegionLater"/>
		/// <remarks>Requires region context</remarks>
		public virtual void SpawnRegion()
		{
			EnsureContext();

			if (!IsSpawned)
			{
				if (!m_regionInfo.NotifySpawning(this))
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
							s_log.Debug("Added {0} Objects to Region: {1}", ObjectCount - count, this);
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
						if (count > 0)
						{
							s_log.Debug("Added {0} NPC Spawnpoints to Region: {1}", ObjectCount - count, this);
						}
						m_npcsSpawned = true;
					}
				}

				if (IsSpawned)
				{
					m_regionInfo.NotifySpawned(this);
				}
			}
		}

		protected virtual void SpawnGOs()
		{
			var gos = GOMgr.TemplatesByMap[(int)Id];
			if (gos != null)
			{
				m_gos = new GameObject[gos.Count + 1];
				for (var i = 0; i < gos.Count; i++)
				{
					var template = gos[i];
					if (template.Entry != null && template.AutoSpawn && IsEventActive(template.EventId))
					{
						template.Spawn(this);
					}
				}
			}
		}

		protected virtual void SpawnNPCs()
		{
			var entries = NPCMgr.SpawnEntriesByMap[(int)Id];
			if (entries != null)
			{
				foreach (var entry in entries)
				{
					if (entry != null && entry.AutoSpawn && IsEventActive(entry.EventId))
					{
						AddSpawn(entry);
					}
				}
			}
		}

		public void ForeachSpawnPoint(Action<SpawnPoint> func)
		{
			ForeachSpawnPoint(Vector3.Zero, 0, func);
		}

		public void ForeachSpawnPoint(Vector3 pos, float radius, Action<SpawnPoint> func)
		{
			var radiusSq = radius * radius;
			foreach (var spawn in m_spawnPoints)
			{
				if (spawn != null && (radius <= 0 || spawn.GetDistSq(pos) < radiusSq))
				{
					func(spawn);
				}
			}
		}

		public void RespawnInRadius(Vector3 pos, float radius)
		{
			ForeachSpawnPoint(pos, radius, spawn => spawn.RespawnFull());
		}

		public void SpawnZone(ZoneId id)
		{
			// TODO: Add Zone spawning
		}
		#endregion

		#region Messages & Updates
		/// <summary>
		/// The time delay in seconds between the last 2 updates
		/// </summary>
		public float LastUpdateDT
		{
			get { return m_lastUpdateDT; }
		}

		public float CurrentTime
		{
			get { return m_regionTime.ElapsedMilliseconds / 1000f; }
		}

		/// <summary>
		/// The time in seconds when the last 
		/// Update started.
		/// </summary>
		public float LastUpdateTime
		{
			get { return m_lastUpdate; }
		}

		/// <summary>
		/// Adds a new Updatable right away.
		/// Requires Region context.
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
		/// In region context.
		/// <see cref="UnregisterUpdatableLater"/>
		/// </summary>
		public void UnregisterUpdatable(IUpdatable updatable)
		{
			EnsureContext();
			m_updatables.Remove(updatable);
		}

		/// <summary>
		/// Registers the given Updatable during the next Region Tick
		/// </summary>
		public void RegisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => RegisterUpdatable(updatable)));
		}

		/// <summary>
		/// Unregisters the given Updatable during the next Region Update
		/// </summary>
		public void UnregisterUpdatableLater(IUpdatable updatable)
		{
			m_messageQueue.Enqueue(new Message(() => UnregisterUpdatable(updatable)));
		}

		/// <summary>
		/// Executes the given action after the given delay within this Region's context.
		/// </summary>
		/// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Region's context.</remarks>
		public TimerEntry CallDelayed(float seconds, Action action)
		{
			var timer = new TimerEntry();
			timer.Action = delay =>
			{
				action();
				UnregisterUpdatableLater(timer);
			};
			timer.Start(seconds, 0f);
			RegisterUpdatableLater(timer);
			return timer;
		}

		/// <summary>
		/// Executes the given action after the given delay within this Region's context.
		/// </summary>
		/// <remarks>Make sure that once the timeout is hit, the given action is executed in the correct Region's context.</remarks>
		public TimerEntry CallPeriodically(float seconds, Action action)
		{
			var timer = new TimerEntry
							{
								Action = (delay => { action(); })
							};
			timer.Start(seconds, 0f);
			RegisterUpdatableLater(timer);
			return timer;
		}

		/// <summary>
		/// Adds a message to the message queue for this region.
		/// TODO: Consider extra-message for Character that checks whether Char is still in Region?
		/// </summary>
		/// <param name="action">the action to be enqueued</param>
		public void AddMessage(Action action)
		{
			AddMessage((Message)action);
		}

		/// <summary>
		/// Adds a message to the message queue for this region.
		/// </summary>
		/// <param name="msg">the message</param>
		public void AddMessage(IMessage msg)
		{
			// make sure, Region is running
			Start();
			m_messageQueue.Enqueue(msg);
		}

		/// <summary>
		/// Callback for executing updates of the region, which includes updates for all inhabiting objects.
		/// </summary>
		/// <param name="state">the <see cref="Region" /> to update</param>
		private void RegionUpdateCallback(object state)
		{
			// we need some locking here because for some reason sometimes RegionUpdateCallback would be called twice
			if (Interlocked.CompareExchange(ref m_currentThreadId, Thread.CurrentThread.ManagedThreadId, 0) == 0)
			//lock (m_regionTime)
			{
				// get the time at the start of our callback
				var updateStart = m_regionTime.ElapsedMilliseconds;

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

				for (var i = 0; i < m_characters.Count; i++)
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

				// check to see if it's time to run a region update yet again
				//if (m_lastRegionUpdate + DefaultUpdateDelay <= now)

				m_isUpdating = true;
				// Updatables
				var updatablesStart = m_regionTime.ElapsedMilliseconds / 1000f;

				// see if we have any pending updates

				// update delta in seconds
				m_lastUpdateDT = updatablesStart - m_lastUpdate;

				foreach (var updatable in m_updatables)
				{
					try
					{
						updatable.Update(m_lastUpdateDT);
					}
					catch (Exception e)
					{
						LogUtil.ErrorException(e, "Exception raised when updating Updatable: " + updatable);
					}
				}

				m_lastUpdate = updatablesStart;

				// update delta in seconds
				var objUpdateDelta = updatablesStart - m_lastRegionUpdate;

				// update all Objects in the Region
				// copy objects, so object-internal message processing won't interfere with updating
				foreach (var obj in m_objects.Values)
				{
					if (obj.IsTeleporting)
					{
						// dont update while teleporting
						continue;
					}

					try
					{
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
							priority = obj.UpdatePriority;
						}

						var tickMatch = m_tickCount + obj.GetUInt32(ObjectFields.GUID_2);

						// Update Object
						var ticks = UpdatePriorityTicks[(int)priority];
						if (tickMatch % ticks == 0)
						{
							if (ticks > 1)
							{
								obj.Update(objUpdateDelta + (((ticks - 1) * m_updateDelay) / 1000f));
							}
							else
							{
								obj.Update(objUpdateDelta);
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

				UpdateRegion();

				// we updated the region, so set our last update time to now
				m_lastRegionUpdate = updatablesStart;
				m_isUpdating = false;

				// get the time, now that we've finished our update callback
				var updateEnd = m_regionTime.ElapsedMilliseconds;

				// weigh old update-time 9 times and new update-time once
				avgUpdateTime = (int)((avgUpdateTime * 9) + (updateEnd - updateStart)) / 10;

				m_tickCount++;

				// make sure to unset the ID *before* enqueuing the task in the ThreadPool again
				Interlocked.Exchange(ref m_currentThreadId, 0);
				if (m_running)
				{
					var callbackTimeout = (updateStart + m_updateDelay) - updateEnd;
					if (callbackTimeout < 0)
					{
						// even if we are in a hurry: For the sake of load-balance we have to give control back to the ThreadPool
						callbackTimeout = 0;
					}

					Task.Factory.StartNewDelayed((int)callbackTimeout, RegionUpdateCallback, this);
				}
				else
				{
					if (IsDisposed)
					{
						// thats it
						Dispose();
					}
				}
			}
		}

		/// <summary>
		/// Can be overridden to add any kind of to be executed every Region-tick
		/// </summary>
		protected virtual void UpdateRegion()
		{
		}

		internal void UpdateCharacters()
		{
			var updatedObjs = new HashSet<WorldObject>();
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
		internal void PartitionSpace()
		{
			// Start partitioning the region space
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
		public bool IsPointInRegion(ref Vector3 point)
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
			if (m_regionInfo.ZoneTileSet != null)
			{
				var zoneId = m_regionInfo.ZoneTileSet.GetZoneId(x, y);
				return GetZone(zoneId);
			}
			//return DefaultZone;
			return null;
		}

		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="filter">the entities to return</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public IList<WorldObject> GetObjectsInRadius(ref Vector3 origin, float radius, ObjectTypes filter, uint phase, int limit)
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
		/// <remarks>Requires region context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<T> GetObjectsInRadius<T>(ref Vector3 origin, float radius, uint phase, int limit) where T : WorldObject
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
		/// <remarks>Requires region context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="filter">the entities to return</param>
		/// <param name="limit">Max amount of objects to search for</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public ICollection<T> GetObjectsInRadius<T>(ref Vector3 origin, float radius, Func<T, bool> filter, uint phase, int limit) where T : WorldObject
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
		/// <remarks>Requires region context.</remarks>
		/// <param name="origin">the point to search from</param>
		/// <param name="radius">the area to check in</param>
		/// <param name="filter">a delegate to filter search results</param>
		/// <returns>a linked list of the entities which were found in the search area</returns>
		public IList<WorldObject> GetObjectsInRadius(ref Vector3 origin, float radius, Func<WorldObject, bool> filter, uint phase, int limit)
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
		/// Iterates over all objects in all phases in the given radius around the given origin.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		/// <param name="predicate">Returns whether to continue iteration.</param>
		/// <returns>Whether Iteration was not cancelled (usually indicating that we did not find what we were looking for).</returns>
		public bool IterateObjects(ref Vector3 origin, float radius, Func<WorldObject, bool> predicate)
		{
			return IterateObjects(ref origin, radius, predicate, uint.MaxValue);
		}

		/// <summary>
		/// Iterates over all objects in the given radius around the given origin.
		/// </summary>
		/// <param name="origin"></param>
		/// <param name="radius"></param>
		/// <param name="predicate">Returns whether to continue iteration.</param>
		/// <returns>Whether Iteration was not cancelled (usually indicating that we did not find what we were looking for).</returns>
		public bool IterateObjects(ref Vector3 origin, float radius, Func<WorldObject, bool> predicate, uint phase)
		{
			EnsureContext();

			var sphere = new BoundingSphere(origin, radius);
			return m_root.Iterate(ref sphere, predicate, phase);
		}

		/// <summary>
		/// Gets all entities in a radius from the origin, according to the search filter.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
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
		/// <remarks>Requires region context.</remarks>
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
		/// <remarks>Requires region context.</remarks>
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
		/// <remarks>Requires region context.</remarks>
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
				// Cannot add during Region Update
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
				// Cannot add during Region Update
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
				// Cannot add during Region Update
				TransferObjectLater(obj, pos);
			}
			else
			{
				AddObjectNow(obj, ref pos);
			}
		}

		/// <summary>
		/// Adds the given Object to this Region.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		public void AddObjectNow(WorldObject obj, Vector3 pos)
		{
			obj.Position = pos;
			AddObjectNow(obj);
		}

		/// <summary>
		/// Adds the given Object to this Region.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		public void AddObjectNow(WorldObject obj, ref Vector3 pos)
		{
			obj.Position = pos;
			AddObjectNow(obj);
		}

		/// <summary>
		/// Adds the given Object to this Region.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
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

				// Add them to the quad-tree
				if (!m_root.AddObject(obj))
				{
					s_log.Error("Could not add Object to Region {0} at {1} (Region Bounds: {2})", this, obj.Position, Bounds);
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

				obj.Region = this;

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
				else if (obj is GameObject)
				{
					var go = obj as GameObject;

					ArrayUtil.Set(ref m_gos, go.EntityId.Low, go);
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

				obj.OnEnterRegion();
				obj.RequestUpdate();

				if (obj is Character)
				{
					m_regionInfo.NotifyPlayerEntered(this, (Character)obj);
				}
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Unable to add Object \"{0}\" to Region: {1}", obj, this);
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
		/// Requires region context.
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
		/// Called when a Character object is added to the Region
		/// </summary>
		protected virtual void OnEnter(Character chr)
		{
		}

		/// <summary>
		/// Called when a Character object is removed from the Region
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
				// Cannot remove during Region Update
				RemoveObjectLater(obj);
			}
			else
			{
				RemoveObjectNow(obj);
			}
		}

		/// <summary>
		/// Removes an entity from this Region, instantly.
		/// Also make sure that the given Object is actually within this Region.
		/// Requires region context.
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

				m_regionInfo.NotifyPlayerLeft(this, (Character)obj);
			}

			obj.OnLeavingRegion();				// call before actually removing

			// Remove object from the quadtree
			if (obj.Node != null && obj.Node.RemoveObject(obj))
			{
				m_objects.Remove(obj.EntityId);
				if (obj is GameObject)
				{
					m_gos[obj.EntityId.Low] = null;
				}

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
		/// Returns all objects within the Region.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		/// <returns>an array of all objects in the region</returns>
		public WorldObject[] CopyObjects()
		{
			EnsureContext();
			return m_objects.Values.ToArray();
		}

		public virtual bool CanEnter(Character chr)
		{
			return chr.Level >= MinLevel &&
				(MaxLevel == 0 || chr.Level <= MaxLevel) &&
				(m_regionInfo.MayEnter(chr) &&
				(MaxPlayerCount == 0 || PlayerCount < MaxPlayerCount));
		}

		public virtual void TeleportOutside(Character chr)
		{
			chr.TeleportToBindLocation();
		}

		/// <summary>
		/// Returns the specified GameObject that is closest to the given point.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		/// <returns>the closest GameObject to the given point, or null if none found.</returns>
		public GameObject GetNearestGameObject(ref Vector3 pos, GOEntryId goId)
		{
			EnsureContext();

			GameObject closest = null;
			float distanceSq = int.MaxValue, currentDistanceSq;

			foreach (var go in m_gos)
			{
				if (go == null || go.Entry.GOId != goId)
				{
					continue;
				}

				currentDistanceSq = go.GetDistanceSq(ref pos);

				if (currentDistanceSq < distanceSq)
				{
					distanceSq = currentDistanceSq;
					closest = go;
				}
			}

			return closest;
		}

		/// <summary>
		/// Returns the specified GameObject that is closest to the given point.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		/// <returns>the closest GameObject to the given point, or null if none found.</returns>
		public NPC GetNearestNPC(ref Vector3 pos, NPCId id)
		{
			EnsureContext();

			NPC closest = null;
			float distanceSq = int.MaxValue, currentDistanceSq;

			foreach (var obj in m_objects.Values)
			{
				if (obj is NPC && ((NPC)obj).Entry.NPCId == id)
				{
					continue;
				}

				currentDistanceSq = obj.GetDistanceSq(ref pos);

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
		/// <remarks>Requires region context.</remarks>
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
		public GameObject GetGO(uint lowId)
		{
			return m_gos.Get(lowId);
		}

		/// <summary>
		/// Returns all GOs of this Region
		/// </summary>
		/// <returns></returns>
		public GameObject[] GetAllGOs()
		{
			return m_gos;
		}

		/// <summary>
		/// Gets the first GO with the given SpellFocus within the given radius around the given position.
		/// </summary>
		public GameObject GetGOWithSpellFocus(Vector3 pos, SpellFocus focus, float radius, uint phase)
		{
			foreach (GameObject go in GetObjectsInRadius(ref pos, radius, ObjectTypes.GameObject, phase, 0))
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
		/// Enqueues an object to be moved into this Region during the next Region-update
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
		/// Enqueues an object to be removed from its old region (if already added to one) and 
		/// moved into this one during the next Region-updates of the regions involved. 
		/// </summary>
		/// <param name="obj">the object to add</param>
		/// <returns>true if the entity was added, false otherwise</returns>
		internal bool TransferObjectLater(WorldObject obj, Vector3 newPos)
		{
			// We can't add anything we already have
			var context = obj.ContextHandler;

			if (obj.IsTeleporting)
			{
				// this might take a while
				var pos2 = newPos;
				obj.AddMessage(() => TransferObjectLater(obj, pos2));
				return true;
			}

			if (obj.Region == this)
			{
				MoveObject(obj, ref newPos);
			}
			else
			{
				obj.IsTeleporting = true;
				var oldRegion = obj.Region;
				if (oldRegion != null)
				{
					var moveTask = new Message(() =>
					{
						oldRegion.RemoveObjectNow(obj);
						obj.Region = this;

						var addTask = new Message(() =>
						{
							obj.IsTeleporting = false;
							obj.Position = newPos;
							AddObjectNow(obj);
						});

						AddMessage(addTask);
					});

					oldRegion.AddMessage(moveTask);
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
					obj.Region = this;
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
			// Check for a partitioned region, and check the placement bounds
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
		/// TODO: Implement event support
		/// </summary>
		public bool IsEventActive(uint eventId)
		{
			return eventId == 0;
		}

		/// <summary>
		/// Enumerates all objects within the region.
		/// </summary>
		/// <remarks>Requires region context.</remarks>
		public IEnumerator<WorldObject> GetEnumerator()
		{
			EnsureContext();
			return m_objects.Values.GetEnumerator();
		}

		#region Waiting
		/// <summary>
		/// Adds the given message to the region's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the region is running before calling this method.</remarks>
		/// <remarks>Must not be called from the region context.</remarks>
		public void AddMessageAndWait(Action action)
		{
			AddMessageAndWait(new Message(action));
		}

		/// <summary>
		/// Adds the given message to the region's message queue and does not return 
		/// until the message is processed.
		/// </summary>
		/// <remarks>Make sure that the region is running before calling this method.</remarks>
		/// <remarks>Must not be called from the region context.</remarks>
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
		/// Waits for one region tick before returning.
		/// </summary>
		/// <remarks>Must not be called from the region context.</remarks>
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
		/// One tick might take 0 until Region.UpdateSpeed milliseconds.
		/// </summary>
		/// <remarks>Make sure that the region is running before calling this method.</remarks>
		/// <remarks>Must not be called from the region context.</remarks>
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
		/// Indicates whether this Region is disposed. 
		/// Disposed Regions may not be used any longer.
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
			m_gos = null;
			m_updatables = null;
		}

		#region IGenericChatTarget Members

		public void SendMessage(string message)
		{
			AddMessage(new Message1<Region>(
							this, rgn => ChatMgr.SendSystemMessage(rgn.m_characters, message)
						));

			ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System, this);
		}

		#endregion

		protected internal virtual void OnHonorableKill(IDamageAction action)
		{
		}

		protected internal virtual void OnDeath(IDamageAction action)
		{
		}
	}
}
