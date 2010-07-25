/*************************************************************************
 *
 *   file		: WorldMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-29 16:55:24 +0800 (Sun, 29 Jun 2008) $
 *   last author	: $LastChangedBy: nosferatus99 $
 *   revision		: $Rev: 538 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.Util.Threading;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Formulas;
using WCell.Util;
using WCell.Util.Variables;
using WCell.RealmServer.Instances;

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// Delegate used for events when a <see cref="Character" /> is changed, like logging in or out.
	/// </summary>
	/// <param name="chr">the <see cref="Character" /> being changed</param>
	public delegate void CharacterChangedHandler(Character chr);

	/// <summary>
	/// Manages the areas and maps that make up the game world, and tracks all entities in all maps.
	/// </summary>
	public partial class World : IWorldSpace
	{
		#region Fields
		/// <summary>
		/// Only used for implementing interfaces
		/// </summary>
		public static readonly World Instance = new World();

		private static readonly ReaderWriterLockSlim m_worldLock = new ReaderWriterLockSlim();

		/// <summary>
		/// While pausing, resuming and saving, the World locks against this Lock, 
		/// so resuming cannot start before all Contexts have been paused
		/// </summary>
		public static readonly object PauseLock = new object();

		/// <summary>
		/// Global PauseObject that all Contexts wait for when pausing
		/// </summary>
		public static readonly object PauseObject = new object();

		private static readonly Dictionary<string, INamedEntity> s_entitiesByName =
			new Dictionary<string, INamedEntity>(StringComparer.InvariantCultureIgnoreCase);

		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
		private static readonly Dictionary<uint, INamedEntity> s_namedEntities = new Dictionary<uint, INamedEntity>();

		private static bool m_paused;
		private static int m_pauseThreadId;
		private static bool m_saving;

		internal static RegionInfo[] s_regionInfos = new RegionInfo[(int)MapId.End];
		internal static Region[] s_Regions = new Region[(int)MapId.End];
		internal static ZoneTemplate[] s_ZoneTemplates = new ZoneTemplate[(int)ZoneId.End];

		internal static InstancedRegion[][] s_instances = new InstancedRegion[(int)MapId.End][];

		private static int s_totalPlayerCount, s_hordePlayerCount, s_allyPlayerCount, s_staffMemberCount;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the collection of regions.
		/// </summary>
		public static RegionInfo[] RegionInfos
		{
			get { return s_regionInfos; }
		}

		/// <summary>
		/// Gets the collection of zones.
		/// </summary>
		public static ZoneTemplate[] ZoneTemplates
		{
			get { return s_ZoneTemplates; }
		}

		/// <summary>
		/// The number of characters in the game.
		/// </summary>
		public static int CharacterCount
		{
			get
			{
				return s_totalPlayerCount;
			}
		}

		/// <summary>
		/// The number of online characters that belong to the Horde
		/// </summary>
		public static int HordeCharCount
		{
			get
			{
				return s_hordePlayerCount;
			}
		}

		/// <summary>
		/// The number of online characters that belong to the Alliance
		/// </summary>
		public static int AllianceCharCount
		{
			get
			{
				return s_allyPlayerCount;
			}
		}

		/// <summary>
		/// The number of online staff characters
		/// </summary>
		public static int StaffMemberCount
		{
			get
			{
				return s_staffMemberCount;
			}
			internal set
			{
				s_staffMemberCount = value;
			}
		}

		#endregion

		#region Pausing
		public static int PauseThreadId
		{
			get { return m_pauseThreadId; }
		}

		public static bool IsInPauseContext
		{
			get { return Thread.CurrentThread.ManagedThreadId == m_pauseThreadId; }
		}

		/// <summary>
		/// Pauses the World, executes the given Action, unpauses the world again and blocks while doing so
		/// </summary>
		public static void ExecuteWhilePaused(Action onPause)
		{
			RealmServer.Instance.AddMessageAndWait(true, () =>
			{
				Paused = true;
				onPause();
				Paused = false;
			});
		}

		[NotVariable]
		/// <summary>
		/// Pauses/unpauses all Regions.
		/// Setting Paused to true blocks until all regions have been paused.
		/// Setting Paused to false blocks during world-safe.
		/// 
		/// TODO: Freeze all players before pausing to make sure, movement is not out of sync
		/// (TODO: Also sync the RealmServer queue)
		/// </summary>
		// [MethodImpl(MethodImplOptions.Synchronized)]
		internal static bool Paused
		{
			get { return m_paused; }
			set
			{
				if (m_paused != value)
				{
					// lock the pausing, so you cannot start resuming before everything has been paused
					// also ensures that Regions don't start while other are still being paused
					lock (PauseLock)
					{
						if (m_paused != value) // check again to make sure that we are not pausing/unpausing twice
						{
							lock (PauseObject)
							{
								m_paused = value;
							}

							if (!value)
							{
								// resume
								lock (PauseObject)
								{
									// tell all waiting threads to continue normal execution
									Monitor.PulseAll(PauseObject);
								}
							}
							else
							{
								// pause
								m_pauseThreadId = Thread.CurrentThread.ManagedThreadId;
								var activeRegions = s_Regions.Where((region) => region != null && region.IsRunning);
								var pauseCount = activeRegions.Count();
								foreach (var region in activeRegions)
								{
									if (region.IsInContext)
									{
										if (!m_saving && !RealmServer.IsShuttingDown)
										{
											lock (PauseObject)
											{
												// unpause all
												Monitor.PulseAll(PauseObject);
											}
											throw new InvalidOperationException("Cannot pause World from within a Region's context - Use the Pause() method instead.");
										}
										pauseCount--;
									}
									else
									{
										if (region.IsRunning)
										{
											region.AddMessage(new Message(() =>
											{
												pauseCount--;
												lock (PauseObject)
												{
													Monitor.Wait(PauseObject);
												}
											}));
										}
									}
								}

								while (pauseCount > 0)
								{
									Thread.Sleep(50);
								}
							}

							var evt = WorldPaused;
							if (evt != null)
							{
								evt(value);
							}
							m_pauseThreadId = 0;
						}
					}
				}
			}
		}
		#endregion

		#region Initialization/teardown

		[Initialization(InitializationPass.Third, "Initializing World")]
		public static void InitializeWorld()
		{
			if (s_regionInfos[(uint)MapId.Kalimdor] == null)
			{
				LoadMapData();
				LoadZoneInfos();
				GameTables.LoadGtDBCs();
				LoadChatChannelsDBC();

				TerrainMgr.InitTerrain();
				LoadDefaultRegions();
			}
		}

		#endregion

		#region Save

		/// <summary>
		/// Indicates whether the world is currently saving.
		/// </summary>
		public static bool IsSaving
		{
			get { return m_saving; }
		}

		public static void Save()
		{
			Save(false);
		}

		/// <summary>
		/// Blocks until all pending changes to dynamic Data have been saved.
		/// </summary>
		/// <param name="beforeShutdown">Whether the server is about to shutdown.</param>
		public static void Save(bool beforeShutdown)
		{
			// only save if save was not already initialized (eg when trying to shutdown while saving)
			var needsSave = !m_saving;
			lock (PauseLock)
			{
				if (needsSave)
				{
					m_saving = true;
					// pause the world so nothing else can happen anymore
					//Paused = true;

					// save everything
					var chars = GetAllCharacters();
					var saveCount = chars.Count;
					RealmServer.Instance.ExecuteInContext(() =>
					{
						for (var i = 0; i < chars.Count; i++)
						{
							var chr = chars[i]; ;
							if (chr.IsInWorld)
							{
								if (beforeShutdown)
								{
									chr.Record.LastLogout = DateTime.Now;
								}
								chr.SaveNow();
							}

							if (beforeShutdown)
							{
								// Game over
								chr.Record.CanSave = false;
								chr.Client.Disconnect();
							}
							saveCount--;
						}

						saveCount = 0;
					});

					while (saveCount > 0)
					{
						Thread.Sleep(50);
					}

					m_saving = false;

					var evt = Saved;
					if (evt != null)
					{
						Saved();
					}
				}
			}
		}

		#endregion

		#region DBCs
		private static void LoadMapData()
		{
			Instance.WorldStates = new WorldStateCollection(Instance, Constants.World.WorldStates.GlobalStates);

			new DBCReader<MapConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_MAPS));
			new DBCReader<MapDifficultyConverter>(RealmServerConfiguration.GetDBCFile("MapDifficulty.dbc"));

			// add existing RegionInfo objects to mapper
			var mapper = ContentHandler.GetMapper<RegionInfo>();
			mapper.AddObjectsUInt(s_regionInfos);

			// Add additional data from DB
			ContentHandler.Load<RegionInfo>();

			// when only updating, it won't call FinalizeAfterLoad automatically:
			foreach (var rgn in s_regionInfos)
			{
				if (rgn != null)
				{
					rgn.FinalizeDataHolder();
				}
			}

			SetupBoundaries();
		}

		private static void SetupBoundaries()
		{
			var regionBounds = RegionBoundaries.GetRegionBoundaries();
			var zoneTileSets = ZoneBoundaries.GetZoneTileSets();

			for (var i = 0; i < s_regionInfos.Length; i++)
			{
				var regionInfo = s_regionInfos[i];
				if (regionInfo != null)
				{
					if (regionBounds != null && regionBounds.Length > i)
					{
						regionInfo.Bounds = regionBounds[i];
					}
					if (zoneTileSets != null && zoneTileSets.Length > i)
					{
						regionInfo.ZoneTileSet = zoneTileSets[i];
					}
				}
			}
		}

		private static void LoadZoneInfos()
		{
			var atDbcPath = RealmServerConfiguration.GetDBCFile(WCellDef.DBC_AREATABLE);
			var dbcRdr = new MappedDBCReader<ZoneTemplate, AreaTableConverter>(atDbcPath);

			foreach (var zone in dbcRdr.Entries.Values)
			{
				ArrayUtil.Set(ref s_ZoneTemplates, (uint)zone.Id, zone);

				var region = s_regionInfos.Get((uint)zone.RegionId);
				if (region != null)
				{
					zone.RegionInfo = region;
					region.ZoneInfos.Add(zone);
				}
			}

			// Set ParentZone and ChildZones
			foreach (var zone in s_ZoneTemplates)
			{
				if (zone != null)
				{
					zone.ParentZone = s_ZoneTemplates.Get((uint)zone.ParentZoneId);
				}
			}

			foreach (var zone in s_ZoneTemplates)
			{
				if (zone != null)
				{
					zone.FinalizeZone();
				}
			}
		}

		private static void LoadChatChannelsDBC()
		{
			var ccDbcPath = RealmServerConfiguration.GetDBCFile(WCellDef.DBC_CHATCHANNELS);
			var reader = new MappedDBCReader<ChatChannelEntry, ChatChannelConverter>(ccDbcPath);
			foreach (var entry in reader.Entries.Values)
			{
				ChatChannelGroup.DefaultChannelFlags.Add(entry.Id, new ChatChannelFlagsEntry
				{
					Flags = entry.ChannelFlags,
					ClientFlags = ChatMgr.Convert(entry.ChannelFlags)
				});
			}

			return;
		}

		#endregion

		#region Characters
		/// <summary>
		/// Does some things to get the World back into sync
		/// </summary>
		public static void Resync()
		{
			m_worldLock.EnterWriteLock();
			try
			{
				s_totalPlayerCount = s_staffMemberCount = s_hordePlayerCount = s_allyPlayerCount = 0;
				foreach (var entity in s_namedEntities.Values)
				{
					if (!(entity is Character))
					{
						continue;
					}

					var chr = (Character)entity;
					s_totalPlayerCount++;
					if (chr.Role.IsStaff)
					{
						s_staffMemberCount++;
					}

					if (chr.Faction.IsHorde)
					{
						s_hordePlayerCount++;
					}
					else
					{
						s_allyPlayerCount++;
					}
				}
			}
			finally
			{
				m_worldLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Add a NamedEntity
		/// </summary>
		public static void AddNamedEntity(INamedEntity entity)
		{
			if (entity is Character)
			{
				AddCharacter((Character)entity);
				return;
			}

			m_worldLock.EnterWriteLock();
			try
			{
				s_namedEntities.Add(entity.EntityId.Low, entity);
				s_entitiesByName.Add(entity.Name, entity);
			}
			finally
			{
				m_worldLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Add a character to the world manager.
		/// </summary>
		/// <param name="chr">the character to add</param>
		public static void AddCharacter(Character chr)
		{
			m_worldLock.EnterWriteLock();

			try
			{
				s_namedEntities.Add(chr.EntityId.Low, chr);
				s_entitiesByName.Add(chr.Name, chr);
				s_totalPlayerCount++;
				if (chr.Role.IsStaff)
				{
					s_staffMemberCount++;
				}

				if (chr.Faction.IsHorde)
				{
					s_hordePlayerCount++;
				}
				else
				{
					s_allyPlayerCount++;
				}
			}
			finally
			{
				m_worldLock.ExitWriteLock();
			}
		}

		/// <summary>
		/// Removes a character from the world manager.
		/// </summary>
		/// <param name="chr">the character to stop tracking</param>
		public static bool RemoveCharacter(Character chr)
		{
			m_worldLock.EnterWriteLock();

			try
			{
				s_entitiesByName.Remove(chr.Name);
				if (s_namedEntities.Remove(chr.EntityId.Low))
				{
					s_totalPlayerCount--;
					if (chr.Role.IsStaff)
					{
						s_staffMemberCount--;
					}
					if (chr.Faction.IsHorde)
					{
						s_hordePlayerCount--;
					}
					else
					{
						s_allyPlayerCount--;
					}
					return true;
				}
			}
			finally
			{
				m_worldLock.ExitWriteLock();
			}
			return false;
		}

		/// <summary>
		/// Gets a <see cref="Character" /> by entity ID.
		/// </summary>
		/// <param name="lowEntityId">EntityId.Low of the Character to be looked up</param>
		/// <returns>the <see cref="Character" /> of the given ID; null otherwise</returns>
		public static Character GetCharacter(uint lowEntityId)
		{
			INamedEntity chr;

			m_worldLock.EnterReadLock();
			s_namedEntities.TryGetValue(lowEntityId, out chr);
			m_worldLock.ExitReadLock();

			return chr as Character;
		}

		public static INamedEntity GetNamedEntity(uint lowEntityId)
		{
			INamedEntity entity;

			m_worldLock.EnterReadLock();
			s_namedEntities.TryGetValue(lowEntityId, out entity);
			m_worldLock.ExitReadLock();

			return entity;
		}

		/// <summary>
		/// Gets a character by name.
		/// </summary>
		/// <param name="name">the name of the character to get</param>
		/// <returns>the <see cref="Character" /> object representing the character; null if not found</returns>
		public static Character GetCharacter(string name, bool caseSensitive)
		{
			if (name.Length == 0)
				return null;

			INamedEntity chr;

			m_worldLock.EnterReadLock();

			try
			{
				s_entitiesByName.TryGetValue(name, out chr);
				if (caseSensitive && chr.Name != name)
				{
					return null;
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}

			return chr as Character;
		}


		/// <summary>
		/// Gets a character by name.
		/// </summary>
		/// <param name="name">the name of the character to get</param>
		/// <returns>the <see cref="Character" /> object representing the character; null if not found</returns>
		public static INamedEntity GetNamedEntity(string name, bool caseSensitive)
		{
			if (name.Length == 0)
				return null;

			INamedEntity entity;

			m_worldLock.EnterReadLock();

			try
			{
				s_entitiesByName.TryGetValue(name, out entity);
				if (caseSensitive && entity.Name != name)
				{
					return null;
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}

			return entity;
		}

		/// <summary>
		/// Gets all current characters.
		/// </summary>
		/// <returns>a list of <see cref="Character" /> objects</returns>
		public static List<Character> GetAllCharacters()
		{
			var list = new List<Character>(s_namedEntities.Count);

			m_worldLock.EnterReadLock();
			try
			{
				foreach (var chr in s_namedEntities.Values)
				{
					if (chr is Character)
					{
						list.Add((Character)chr);
					}
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}

			return list;
		}

		/// <summary>
		/// Gets all current characters.
		/// </summary>
		/// <returns>a list of <see cref="Character" /> objects</returns>
		public static ICollection<INamedEntity> GetAllNamedEntities()
		{
			m_worldLock.EnterReadLock();

			try
			{
				return s_namedEntities.Values.ToList();
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}
		}

		/// <summary>
		/// Gets an enumerator of characters based on their race.
		/// </summary>
		/// <param name="entRace">the race to search for</param>
		/// <returns>a list of <see cref="Character" /> objects belonging to the given race</returns>
		public static ICollection<Character> GetCharactersOfRace(RaceId entRace)
		{
			var list = new List<Character>(s_namedEntities.Count);
			m_worldLock.EnterReadLock();
			try
			{
				foreach (INamedEntity chr in s_namedEntities.Values)
				{
					if (chr is Character && ((Character)chr).Race == entRace)
					{
						list.Add((Character)chr);
					}
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}
			return list;
		}

		/// <summary>
		/// Gets an enumerator of characters based on their class.
		/// </summary>
		/// <param name="entClass">the class to search for</param>
		/// <returns>a list of <see cref="Character" /> objects who are of the given class</returns>
		public static ICollection<Character> GetCharactersOfClass(ClassId entClass)
		{
			var list = new List<Character>(s_namedEntities.Count);
			m_worldLock.EnterReadLock();
			try
			{
				foreach (INamedEntity chr in s_namedEntities.Values)
				{
					if (chr is Character && ((Character)chr).Class == entClass)
					{
						list.Add((Character)chr);
					}
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}
			return list;
		}

		/// <summary>
		/// Gets an enumerator of characters based on their level.
		/// </summary>
		/// <param name="level">the level to search for</param>
		/// <returns>a list of <see cref="Character" /> objects who are of the given level</returns>
		public static ICollection<Character> GetCharactersOfLevel(uint level)
		{
			var list = new List<Character>(s_namedEntities.Count);
			m_worldLock.EnterReadLock();
			try
			{
				foreach (INamedEntity chr in s_namedEntities.Values)
				{
					if (chr is Character && ((Character)chr).Level == level)
					{
						list.Add((Character)chr);
					}
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}
			return list;
		}

		/// <summary>
		/// Gets an enumerator of characters based on what their name starts with.
		/// </summary>
		/// <param name="nameStarts">the starting part of the name to search for</param>
		/// <returns>a list of <see cref="Character" /> objects whose name starts with the given string</returns>
		public static ICollection<Character> GetCharactersStartingWith(string nameStarts)
		{
			var list = new List<Character>(s_namedEntities.Count);
			m_worldLock.EnterReadLock();
			try
			{
				foreach (INamedEntity chr in s_namedEntities.Values)
				{
					if (chr is Character && chr.Name.StartsWith(nameStarts, StringComparison.InvariantCultureIgnoreCase))
					{
						list.Add((Character)chr);
					}
				}
			}
			finally
			{
				m_worldLock.ExitReadLock();
			}
			return list;
		}

		#endregion

		#region Region-Management
		public static Region Kalimdor
		{
			get { return s_Regions[(int)MapId.Kalimdor]; }
		}

		public static Region EasternKingdoms
		{
			get { return s_Regions[(int)MapId.EasternKingdoms]; }
		}

		public static Region Outland
		{
			get { return s_Regions[(int)MapId.Outland]; }
		}

		public static Region Northrend
		{
			get { return s_Regions[(int)MapId.Northrend]; }
		}

		/// <summary>
		/// Gets all default (non-instanced) Regions
		/// </summary>
		/// <returns>a collection of all current regions</returns>
		public static Region[] Regions
		{
			get { return s_Regions; }
		}

		public static int RegionCount
		{
			get;
			private set;
		}

		/// <summary>
		/// All Continents and Instances, including Battlegrounds
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Region> GetAllRegions()
		{
			for (var i = 0; i < s_Regions.Length; i++)
			{
				var rgn = s_Regions[i];
				if (rgn != null)
				{
					yield return rgn;
				}
			}

			for (var j = 0; j < s_instances.Length; j++)
			{
				var instances = s_instances[j];
				if (instances != null)
				{
					for (var i = 0; i < instances.Length; i++)
					{
						var instance = instances[i];
						if (instance != null)
						{
							yield return instance;
						}
					}
				}
			}
		}

		/// <summary>
		/// Adds a region, associated with its unique Id (and InstanceId).
		/// </summary>
		/// <param name="region">the region to add</param>
		internal static void AddRegion(Region region)
		{
			RegionCount++;

			if (!region.IsInstance)
			{
				ArrayUtil.Set(ref s_Regions, (uint)region.Id, region);
			}
			else
			{
				var instances = GetInstances(region.Id);
				if (region.InstanceId >= instances.Length)
				{
					Array.Resize(ref instances, (int)(region.InstanceId * ArrayUtil.LoadConstant));
					s_instances[(uint)region.Id] = instances;
				}
				instances[region.InstanceId] = (InstancedRegion)region;
			}
		}

		internal static void RemoveInstance(InstancedRegion region)
		{
			var instances = GetInstances(region.Id);
			RegionCount--;
			ArrayUtil.Set(ref instances, region.InstanceId, null);
		}

		public static InstancedRegion[][] GetAllInstances()
		{
			return s_instances.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="region"></param>
		/// <returns></returns>
		/// <remarks>Never returns null</remarks>
		public static InstancedRegion[] GetInstances(MapId region)
		{
			var instances = s_instances.Get((uint)region);
			if (instances == null)
			{
				s_instances[(uint)region] = instances = new InstancedRegion[10];
			}
			return instances;
		}

		/// <summary>
		/// Gets an instance
		/// </summary>
		/// <returns>the <see cref="Region" /> object; null if the ID is not valid</returns>s
		public static InstancedRegion GetInstance(MapId mapId, uint instanceId)
		{
			var instances = GetInstances(mapId);
			if (instances != null)
			{
				return GetInstances(mapId).Get(instanceId);
			}
			return null;
		}

		/// <summary>
		/// Gets an instance
		/// </summary>
		/// <returns>the <see cref="Region" /> object; null if the ID is not valid</returns>s
		public static InstancedRegion GetInstance(IRegionId mapId)
		{
			var instances = GetInstances(mapId.RegionId);
			if (instances != null)
			{
				return instances.Get(mapId.InstanceId);
			}
			return null;
		}

		/// <summary>
		/// Gets a normal Region by its Id
		/// </summary>
		/// <returns>the <see cref="Region" /> object; null if the ID is not valid</returns>
		public static Region GetRegion(MapId mapId)
		{
			return s_Regions.Get((uint)mapId);
		}

		/// <summary>
		/// Gets a normal Region by its Id
		/// </summary>
		/// <returns>the <see cref="Region" /> object; null if the ID is not valid</returns>
		public static Region GetRegion(IRegionId mapId)
		{
			if (mapId.InstanceId > 0)
			{
				return GetInstance(mapId);
			}
			return s_Regions.Get((uint)mapId.RegionId);
		}

		/// <summary>
		/// Gets region info by ID.
		/// </summary>
		/// <param name="regionID">the ID to the region to get</param>
		/// <returns>the <see cref="RegionInfo" /> object for the given region ID</returns>
		public static RegionInfo GetRegionInfo(MapId regionID)
		{
			if (s_ZoneTemplates == null)
			{
				LoadMapData();
			}
			return s_regionInfos.Get((uint)regionID);
		}

		/// <summary>
		/// Gets zone info by ID.
		/// </summary>
		/// <param name="zoneID">the ID to the zone to get</param>
		/// <returns>the <see cref="Zone" /> object for the given zone ID</returns>
		public static ZoneTemplate GetZoneInfo(ZoneId zoneID)
		{
			return s_ZoneTemplates.Get((uint)zoneID);
		}

		/// <summary>
		/// Gets ZoneInfo by Name
		/// </summary>
		/// <returns>the <see cref="Zone" /> object for the given zone ID</returns>
		//public static ZoneInfo GetZoneInfo(string name)
		//{
		//    name = name.Replace(" ", "");
		//    ZoneId id;
		//    if (EnumUtil.TryParse(name, out id))
		//    {
		//        return s_zoneInfos.Get((uint)id);
		//    }
		//    return null;
		//}

		/// <summary>
		/// Gets the first significant location within the Zone with the given Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static IWorldLocation GetSite(ZoneId id)
		{
			var zone = GetZoneInfo(id);
			if (zone != null)
			{
				return zone.Site;
			}
			return null;
		}

		internal static void LoadDefaultRegions()
		{
			foreach (var rgnInfo in s_regionInfos)
			{
				if (rgnInfo != null && rgnInfo.Type == MapType.Normal)
				{
					var region = new Region(rgnInfo);

					if (region.Id == MapId.Outland)
					{
						region.XpCalculator = XpGenerator.CalcOutlandXp;
					}
					else
					{
						region.XpCalculator = XpGenerator.CalcDefaultXp;
					}

					region.InitRegion();
				}
			}
		}

		#endregion

		/// <summary>
		/// Calls the given Action on all currently logged in Characters
		/// </summary>
		/// <param name="action"></param>
		/// <param name="doneCallback">Called after the action was called on everyone.</param>
		public static void CallOnAllChars(Action<Character> action, Action doneCallback)
		{
			var regions = GetAllRegions();
			var rgnCount = regions.Count();
			var i = 0;
			foreach (var rgn in regions)
			{
				if (rgn.CharacterCount > 0)
				{
					rgn.AddMessage(new Message1<Region>(rgn, (region) =>
					{
						foreach (var chr in region.Characters)
						{
							action(chr);
						}
						i++;

						if (i == rgnCount)
						{
							// this was the last one
							doneCallback();
						}
					}));
				}
			}
		}

		/// <summary>
		/// Calls the given Action on all currently logged in Characters
		/// </summary>
		/// <param name="action"></param>
		/// <param name="doneCallback">Called after the action was called on everyone.</param>
		public static void CallOnAllChars(Action<Character> action)
		{
			var regions = GetAllRegions();
			foreach (var rgn in regions)
			{
				if (rgn.CharacterCount > 0)
				{
					rgn.AddMessage(new Message1<Region>(rgn, (region) =>
					{
						foreach (var chr in region.Characters)
						{
							action(chr);
						}
					}));
				}
			}
		}

		/// <summary>
		/// Calls the given Action on all currently existing Regions within each Region's context
		/// </summary>
		/// <param name="action"></param>
		/// <param name="doneCallback">Called after the action was called on all Regions.</param>
		public static void CallOnAllRegions(Action<Region> action, Action doneCallback)
		{
			var regions = GetAllRegions();
			var rgnCount = regions.Count();
			var i = 0;
			foreach (var rgn in regions)
			{
				rgn.AddMessage(() =>
				{
					action(rgn);
					i++;

					if (i == rgnCount)
					{
						// this was the last one
						doneCallback();
					}
				});
			}
		}

		/// <summary>
		/// Calls the given Action on all currently existing Regions within each Region's context
		/// </summary>
		/// <param name="action"></param>
		public static void CallOnAllRegions(Action<Region> action)
		{
			var regions = GetAllRegions();
			foreach (var rgn in regions)
			{
				rgn.AddMessage(() =>
				{
					action(rgn);
				});
			}
		}

		public static void Broadcast(string message, params object[] args)
		{
			Broadcast(string.Format(message, args));
		}

		public static void Broadcast(IChatter broadCaster, string message, params object[] args)
		{
			Broadcast(broadCaster, string.Format(message, args));
		}

		public static void Broadcast(string message)
		{
			Broadcast(null, message);
		}

		public static void Broadcast(IChatter broadCaster, string message)
		{
			if (broadCaster != null)
			{
				message = broadCaster.Name + ": ";
			}

			GetAllCharacters().SendSystemMessage(message);
			//ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System, null);


			s_log.Info("[Broadcast] " + ChatUtility.Strip(message));

			var evt = Broadcasted;
			if (evt != null)
			{
				evt(broadCaster, message);
			}
		}

		#region Events
		#endregion

		#region Instance members
		public IWorldSpace ParentSpace
		{
			get { return null; }
		}

		public WorldStateCollection WorldStates
		{
			get;
			private set;
		}

		public void CallOnAllCharacters(Action<Character> action)
		{
			CallOnAllChars(action);
		}
		#endregion
	}
}