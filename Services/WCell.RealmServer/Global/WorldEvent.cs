using System;
using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Util.Data;

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// Holds all information regarding a World Event
	/// such as Darkmoon Faire
	/// </summary>
	[DataHolder]
	public class WorldEvent : IDataHolder
	{
        /// <summary>
        /// Id of the game event
        /// </summary>
		public uint Id;

        /// <summary>
        /// Absolute start date, the event will never start before
        /// </summary>
		public DateTime From;

        /// <summary>
        /// Absolute end date, the event will never start after
        /// </summary>
		public DateTime Until;

	    [NotPersistent]
        public TimeSpan? TimeUntilNextStart;

        [NotPersistent]
        public TimeSpan? TimeUntilEnd;

        /// <summary>
        /// Do not use, time in minutes read from database.
        /// Instead you should use TimeSpan <seealso cref="WorldEvent.Occurence"/>
        /// </summary>
        public long _Occurence;

        /// <summary>
        /// Do not use, time in minutes read from database.
        /// Instead you should use TimeSpan <seealso cref="WorldEvent.Duration"/>
        /// </summary>
        public long _Length;

        /// <summary>
        /// Delay in minutes between occurences of the event
        /// </summary>
        [NotPersistent]
	    public TimeSpan Occurence;

        /// <summary>
        /// Length in minutes of the event
        /// </summary>
        [NotPersistent]
	    public TimeSpan Duration;

        /// <summary>
        /// Client side holiday id
        /// </summary>
	    public uint HolidayId;

	    /// <summary>
	    /// Generally the event's name
	    /// </summary>
        public string Description;

        [NotPersistent]
        public List<WorldEventNPC> NPCSpawns = new List<WorldEventNPC>();
        [NotPersistent]
        public List<WorldEventGameObject> GOSpawns = new List<WorldEventGameObject>();
        [NotPersistent]
        public List<WorldEventNpcData> ModelEquips = new List<WorldEventNpcData>();
	    [NotPersistent]
        public List<uint> QuestIds = new List<uint>();

        public void FinalizeDataHolder()
        {
            Occurence = TimeSpan.FromMinutes(_Occurence);
            Duration = TimeSpan.FromMinutes(_Length);

            CalculateEventDelays(this);

            WorldEventMgr.AddEvent(this);
        }

	    public static void CalculateEventDelays(WorldEvent worldEvent)
	    {
	        var time = DateTime.Now;

	        //Only work out start times for events that will ever be able to start
            if (time < worldEvent.Until)
	        {
	            //If the first time this event can start is in the
	            //future then this is easy
                if (worldEvent.From > time)
                    worldEvent.TimeUntilNextStart = worldEvent.From - time;
	            else
	            {
	                //If the first time this event started was in the
	                //past work out which cycle of the event we are up
	                //to
                    DateTime timeToCheck = worldEvent.From;
	                while (timeToCheck < time)
	                {
	                    //if we are in the middle of a cycle
                        if ((timeToCheck + worldEvent.Duration) > time && (timeToCheck + worldEvent.Duration) < (timeToCheck + worldEvent.Occurence))
	                        break;

                        timeToCheck += worldEvent.Occurence;
	                }
                    worldEvent.TimeUntilNextStart = timeToCheck - time;
	            }

                worldEvent.TimeUntilEnd = worldEvent.TimeUntilNextStart + worldEvent.Duration;
	        }
            else
            {
                worldEvent.TimeUntilNextStart = null;
                worldEvent.TimeUntilEnd = null;
            }
	    }
	}

    /// <summary>
	/// Holds all information regarding a spawn
	/// involved in a WorldEvent
	/// </summary>
    public class WorldEventNPC
    {
        /// <summary>
        /// Spawn id of the object
        /// </summary>
        public uint Guid;

        /// <summary>
        /// ID of the world event relating to this entry
        /// as found in the database, negative values mean
        /// we should despawn
        /// </summary>
        public int _eventId;

        /// <summary>
        /// ID of the world event relating to this entry
        /// </summary>
        [NotPersistent]
        public uint EventId;

        /// <summary>
        /// True if we should spawn this entry
        /// False if we should despawn it
        /// </summary>
        [NotPersistent]
        public bool Spawn;

        public void FinalizeDataHolder()
        {
            Spawn = _eventId > 0;
            EventId = (uint)_eventId;
            var worldEvent = WorldEventMgr.GetEvent(EventId);
            if (worldEvent == null)
                return;

            worldEvent.NPCSpawns.Add(this);
        }
    }

    /// <summary>
    /// Holds all information regarding a spawn
    /// involved in a WorldEvent
    /// </summary>
    public class WorldEventGameObject
    {
        /// <summary>
        /// Spawn id of the object
        /// </summary>
        public uint Guid;

        /// <summary>
        /// ID of the world event relating to this entry
        /// as found in the database, negative values mean
        /// we should despawn
        /// </summary>
        public int _eventId;

        /// <summary>
        /// ID of the world event relating to this entry
        /// </summary>
        [NotPersistent]
        public uint EventId;

        /// <summary>
        /// True if we should spawn this entry
        /// False if we should despawn it
        /// </summary>
        [NotPersistent]
        public bool Spawn;

        public void FinalizeDataHolder()
        {
            Spawn = _eventId > 0;
            EventId = (uint)_eventId;
            var worldEvent = WorldEventMgr.GetEvent(EventId);
            if (worldEvent != null)
                worldEvent.GOSpawns.Add(this);
        }
    }

    /// <summary>
    /// Holds all information regarding a change of model
    /// or equipment for a WorldEvent
    /// </summary>
    [DataHolder]
    public class WorldEventNpcData : IDataHolder
    {
        /// <summary>
        /// Spawn id of the object
        /// </summary>
        public uint Guid;

        public NPCId EntryId;

        [NotPersistent]
        public NPCId OriginalEntryId;

        public uint ModelId;

        public uint EquipmentId;

        [NotPersistent]
        public uint OriginalEquipmentId;

        public SpellId SpellIdToCastAtStart;

        public SpellId SpellIdToCastAtEnd;

        /// <summary>
        /// ID of the world event relating to this entry
        /// </summary>
        public uint EventId;

        public void FinalizeDataHolder()
        {
            var worldEvent = WorldEventMgr.GetEvent(EventId);
            if (worldEvent != null)
                worldEvent.ModelEquips.Add(this);
        }
    }

        /// <summary>
    /// Holds all information regarding a quest
    /// involved in a WorldEvent
    /// </summary>
    [DataHolder]
    public class WorldEventQuest : IDataHolder
    {
        /// <summary>
        /// Id of the quest
        /// </summary>
        public uint QuestId;

        /// <summary>
        /// ID of the world event relating to this entry
        /// </summary>
        public uint EventId;

        public void FinalizeDataHolder()
        {
            WorldEventMgr.WorldEventQuests.Add(this);
            var worldEvent = WorldEventMgr.GetEvent(EventId);
            if(worldEvent != null)
                worldEvent.QuestIds.Add(QuestId);
        }
    }
}
