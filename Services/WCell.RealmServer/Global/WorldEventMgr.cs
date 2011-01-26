using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.Util;
using WCell.Util.Threading.TaskParallel;

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
                Loaded = true;
                LastUpdateTime = DateTime.Now;

                Log.Debug("{0} World Events loaded.", _eventCount);

                // start updating
                //TODO: Enable this
                //Disabled for now, no point updating nothing!!!
                //Task.Factory.StartNewDelayed(30000, Update);
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
            if (!Loaded)
                return;

            var updateInterval = DateTime.Now - LastUpdateTime;
            LastUpdateTime = DateTime.Now;
            foreach (var worldEvent in AllEvents.Where(worldEvent => worldEvent != null))
            {
                worldEvent.TimeUntilNextStart -= updateInterval;
                worldEvent.TimeUntilEnd -= updateInterval;

                if (worldEvent.TimeUntilEnd <= TimeSpan.Zero)
                    StopEvent(worldEvent);

                if (worldEvent.TimeUntilNextStart <= TimeSpan.Zero)
                    StartEvent(worldEvent);
            }

            Task.Factory.StartNewDelayed(1000, Update);
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
            worldEvent.TimeUntilNextStart = worldEvent.Occurence;
            ActiveEvents[worldEvent.Id] = worldEvent;
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
            worldEvent.TimeUntilEnd = worldEvent.Occurence + worldEvent.Duration;
            ActiveEvents[worldEvent.Id] = null;
        }
		#endregion

		#region Active Events

        public static bool IsHolidayActive(uint id)
        {
            return id != 0 && ActiveEvents.Any(evnt => evnt.HolidayId == id);
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
