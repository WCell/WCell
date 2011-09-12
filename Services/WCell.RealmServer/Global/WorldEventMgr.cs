using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.Util;
using WCell.Util.Variables;

namespace WCell.RealmServer.Global
{	
	/// <summary>
	/// Manages world events
	/// </summary>
	public static class WorldEventMgr
	{
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		internal static WorldEvent[] AllEvents = new WorldEvent[100];
		internal static WorldEvent[] ActiveEvents = new WorldEvent[100];

        #region Fields
        internal static uint _eventCount;

	    private static DateTime LastUpdateTime;

		[NotVariable]
        public static List<WorldEventQuest> WorldEventQuests = new List<WorldEventQuest>();
        #endregion

        #region Properties
        public static uint EventCount
        {
            get { return _eventCount; }
        }

	    public static bool Loaded
        { 
            get;
            private set;
        }
        #endregion

        #region Initialization and Loading
        [Initialization(InitializationPass.Fifth, "Initialize World Events")]
        public static void Initialize()
        {
            LoadAll();
        }

        /// <summary>
        /// Loads the world events.
        /// </summary>
        /// <returns></returns>
        public static bool LoadAll()
        {
            if (!Loaded)
            {
                ContentMgr.Load<WorldEvent>();
                ContentMgr.Load<WorldEventNpcData>();
                ContentMgr.Load<WorldEventQuest>();
                Loaded = true;
                LastUpdateTime = DateTime.Now;

                Log.Debug("{0} World Events loaded.", _eventCount);
                
                // Add the Update method to the world task queue
                World.TaskQueue.CallPeriodically(10000, Update);
            }
            return true;
        }

        public static bool UnloadAll()
        {
            if (Loaded)
            {
                Loaded = false;
                AllEvents = new WorldEvent[100];
		        ActiveEvents = new WorldEvent[100];
                return true;
            }
            return false;
        }

        public static bool Reload()
        {
            return UnloadAll() && LoadAll();
        }
        #endregion

        #region Timers

        public static void Update()
        {
            if (!Loaded || !QuestMgr.Loaded || !NPCMgr.Loaded || !GOMgr.Loaded)
            {
                return;
            }

            var updateInterval = DateTime.Now - LastUpdateTime;
            LastUpdateTime = DateTime.Now;
            foreach (var worldEvent in
                AllEvents.Where(worldEvent => worldEvent != null).Where(worldEvent => worldEvent.TimeUntilNextStart != null))
            {
                worldEvent.TimeUntilNextStart -= updateInterval;
                worldEvent.TimeUntilEnd -= updateInterval;

                if (worldEvent.TimeUntilEnd <= TimeSpan.Zero)
                    StopEvent(worldEvent);
                else if (worldEvent.TimeUntilNextStart <= TimeSpan.Zero)
                    StartEvent(worldEvent);
            }
        }

	    #endregion

        #region Manage Events
        public static WorldEvent GetEvent(uint id)
		{
			return AllEvents.Get(id);
		}

        public static void AddEvent(WorldEvent worldEvent)
        {
            if (AllEvents.Get(worldEvent.Id) == null)
            {
                _eventCount++;
            }
            ArrayUtil.Set(ref AllEvents, worldEvent.Id, worldEvent);
        }

        public static bool StartEvent(uint id)
        {
            var worldEvent = GetEvent(id);
            if (worldEvent == null)
                return false;

            StartEvent(worldEvent);
            return true;
        }

        public static void StartEvent(WorldEvent worldEvent)
        {
            //Log.Debug("Incrementing start event timer {0}: {1}", worldEvent.Id, worldEvent.Description);
            worldEvent.TimeUntilNextStart += worldEvent.Occurence;
            if (IsEventActive(worldEvent.Id))
                return;

            Log.Info("Starting event {0}: {1}", worldEvent.Id, worldEvent.Description);
            ArrayUtil.Set(ref ActiveEvents, worldEvent.Id, worldEvent);
            SpawnEvent(worldEvent);
            ApplyEventNPCData(worldEvent);
        }

        public static bool StopEvent(uint id)
        {
            var worldEvent = GetEvent(id);
            if (worldEvent == null)
                return false;

            StopEvent(worldEvent);
            return true;
        }

        public static void StopEvent(WorldEvent worldEvent)
        {
            //Log.Debug("Incrementing end event timer {0}: {1}", worldEvent.Id, worldEvent.Description);
            worldEvent.TimeUntilEnd += worldEvent.Occurence + worldEvent.Duration;
            if (!IsEventActive(worldEvent.Id))
                return;

            Log.Info("Stopping event {0}: {1}", worldEvent.Id, worldEvent.Description);
            ActiveEvents[worldEvent.Id] = null;

            if(worldEvent.QuestIds.Count != 0)
                ClearActiveQuests(worldEvent.QuestIds);

            DeSpawnEvent(worldEvent);
            ResetEventNPCData(worldEvent);
        }
		#endregion

        #region Event Actions

        private static void ClearActiveQuests(IEnumerable<uint> questIds)
        {
           
                World.CallOnAllChars(chr =>
                                         {
                                             foreach (var questId in questIds)
                                             {
                                                 var quest = chr.QuestLog.GetQuestById(questId);
                                                 if (quest != null)
                                                 {
                                                     quest.Cancel(false);
                                                 }
                                             }
                                         }
                    );
        }

        public static void SpawnEvent(uint eventId)
        {
            var worldEvent = GetEvent(eventId);
            SpawnEvent(worldEvent);
        }

        public static void DeSpawnEvent(uint eventId)
        {
            var worldEvent = GetEvent(eventId);
            DeSpawnEvent(worldEvent);
        }

        private static void SpawnEvent(WorldEvent worldEvent)
        {

            foreach (var worldEventNPC in worldEvent.NPCSpawns)
            {
                var spawnEntry = NPCMgr.GetSpawnEntry(worldEventNPC.Guid);
                var map = spawnEntry.Map;

                //if the map is null then this saves us some work
                //since the map will spawn any active events when
                //it is created
                if (map == null)
                    continue;

                if (worldEventNPC.Spawn)
                {
                    map.AddNPCSpawnPool(spawnEntry.PoolTemplate);
                }
                else
                {

                    foreach (var point in spawnEntry.SpawnPoints.ToArray())
                    {
                        point.Disable();
                    }
                }


            }

            foreach (var worldEventGO in worldEvent.GOSpawns)
            {
                var spawnEntry = GOMgr.GetSpawnEntry(worldEventGO.Guid);
                var map = spawnEntry.Map;

                //if the map is null then this saves us some work
                //since the map will spawn any active events when
                //it is created
                if (map == null)
                    continue;

                if (worldEventGO.Spawn)
                {
                    map.AddGOSpawnPoolLater(spawnEntry.PoolTemplate);
                }
                else
                {

                    foreach (var point in spawnEntry.SpawnPoints.ToArray())
                    {
                        point.Disable();
                    }
                }
            }
        }

        private static void DeSpawnEvent(WorldEvent worldEvent)
        {
            foreach (var worldEventNPC in worldEvent.NPCSpawns)
            {
                var spawnEntry = NPCMgr.GetSpawnEntry(worldEventNPC.Guid);
                var map = spawnEntry.Map;

                //if the map is null then this saves us some work
                //since the map wont spawn any inactive events when
                //it is created
                if (map == null)
                    continue;

                if (worldEventNPC.Spawn)
                {
                    map.RemoveNPCSpawnPool(spawnEntry.PoolTemplate);
                }
                else
                {

                    foreach (var point in spawnEntry.SpawnPoints.ToArray())
                    {
                        point.Respawn();
                    }
                }
            }

            foreach (var worldEventGO in worldEvent.GOSpawns)
            {
                var spawnEntry = GOMgr.GetSpawnEntry(worldEventGO.Guid);
                var map = spawnEntry.Map;

                //if the map is null then this saves us some work
                //since the map wont spawn any inactive events when
                //it is created
                if (map == null)
                    continue;

                if (worldEventGO.Spawn)
                {
                    map.RemoveGOSpawnPool(spawnEntry.PoolTemplate);
                }
                else
                {

                    foreach (var point in spawnEntry.SpawnPoints.ToArray())
                    {
                        point.Respawn();
                    }
                }
            }
        }

        private static void ApplyEventNPCData(WorldEvent worldEvent)
        {

            foreach (var eventNpcData in worldEvent.ModelEquips)
            {
                var spawnEntry = NPCMgr.GetSpawnEntry(eventNpcData.Guid);
                if(spawnEntry == null)
                {
                    Log.Warn("Invalid Spawn Entry in World Event NPC Data, Entry: {0}", eventNpcData.Guid);
                    continue;
                }

                if(eventNpcData.EntryId != 0)
                {
                    eventNpcData.OriginalEntryId = spawnEntry.EntryId;
                    spawnEntry.EntryId = eventNpcData.EntryId;

                    spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
                    if (spawnEntry.Entry == null)
                    {
                        Log.Warn("{0} had an invalid World Event EntryId.", spawnEntry);
                        spawnEntry.EntryId = eventNpcData.OriginalEntryId;
                        spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
                    }
                }

                if (eventNpcData.ModelId != 0)
                {
                    spawnEntry.DisplayIdOverride = eventNpcData.ModelId;
                }

                if(eventNpcData.EquipmentId != 0)
                {
                    eventNpcData.OriginalEquipmentId = spawnEntry.EquipmentId;
                    spawnEntry.EquipmentId = eventNpcData.EquipmentId;

                    spawnEntry.Equipment = NPCMgr.GetEquipment(spawnEntry.EquipmentId);
                }
                
                foreach (var point in spawnEntry.SpawnPoints.ToArray().Where(point => point.IsActive))
                {
                    point.Respawn();
                    if (eventNpcData.SpellIdToCastAtStart == 0)
                        continue;
                    
                    Spell spell = SpellHandler.Get(eventNpcData.SpellIdToCastAtStart);
                    if (spell == null)
                        continue;

                    SpellCast cast = point.ActiveSpawnling.SpellCast;
                    cast.Start(spell);
                }
            }
        }

        private static void ResetEventNPCData(WorldEvent worldEvent)
        {

            foreach (var eventNpcData in worldEvent.ModelEquips)
            {
                var spawnEntry = NPCMgr.GetSpawnEntry(eventNpcData.Guid);
                if (spawnEntry == null)
                {
                    Log.Warn("Invalid Spawn Entry in World Event NPC Data, Entry: {0}", eventNpcData.Guid);
                    continue;
                }

                if (eventNpcData.EntryId != 0)
                {
                    spawnEntry.EntryId = eventNpcData.OriginalEntryId;
                    spawnEntry.Entry = NPCMgr.GetEntry(spawnEntry.EntryId);
                }

                if (eventNpcData.ModelId != 0)
                {
                    spawnEntry.DisplayIdOverride = 0;
                }

                if (eventNpcData.EquipmentId != 0)
                {
                    spawnEntry.EquipmentId = eventNpcData.OriginalEquipmentId;

                    spawnEntry.Equipment = NPCMgr.GetEquipment(spawnEntry.EquipmentId);
                }
                
                foreach (var point in spawnEntry.SpawnPoints.ToArray().Where(point => point.IsActive))
                {
                    point.Respawn();

                    if (eventNpcData.SpellIdToCastAtEnd == 0)
                        continue;

                    Spell spell = SpellHandler.Get(eventNpcData.SpellIdToCastAtEnd);
                    if (spell != null)
                    {
                        SpellCast cast = point.ActiveSpawnling.SpellCast;
                        cast.Start(spell);
                    }
                }
            }
        }
        #endregion

        #region Active Events

        public static bool IsHolidayActive(uint id)
        {
            return id != 0 && ActiveEvents.Any(evnt => evnt != null && evnt.HolidayId == id);
        }

	    public static bool IsEventActive(uint id)
		{
            return id == 0 || ActiveEvents.Get(id) != null;
		}

		public static IEnumerable<WorldEvent> GetActiveEvents()
		{
			return ActiveEvents.Where(evt => evt != null);
		}
		#endregion
	}
}
