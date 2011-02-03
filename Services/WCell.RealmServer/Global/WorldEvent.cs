using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Quests;
using WCell.Constants.Spells;
using WCell.RealmServer.Lang;
using WCell.Util;
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

            var time = DateTime.Now;

            //Only work out start times for events that will ever be able to start
            if (time < Until)
            {
                //If the first time this event can start is in the
                //future then this is easy
                if (From > time)
                    TimeUntilNextStart = From - time;
                else
                {
                    //If the first time this event started was in the
                    //past work out which cycle of the event we are up
                    //to
                    DateTime timeToCheck = From;
                    while (timeToCheck < time)
                    {
                        //if we are in the middle of a cycle
                        if ((timeToCheck + Duration) > time && (timeToCheck + Duration) < (timeToCheck + Occurence))
                            break;

                        timeToCheck += Occurence;
                    }
                    TimeUntilNextStart = timeToCheck - time;
                }
                
                TimeUntilEnd = TimeUntilNextStart + Duration;
            }

            WorldEventMgr.AddEvent(this);
        }
    }

    /// <summary>
	/// Holds all information regarding a spawn
	/// involved in a WorldEvent
	/// </summary>
    [DataHolder]
    public class WorldEventNPC : IDataHolder
    {
        /// <summary>
        /// Spawn id of the object
        /// </summary>
        public uint Guid;

        /// <summary>
        /// ID of the world event relating to this entry
        /// </summary>
        public uint EventId;

        public void FinalizeDataHolder()
        {
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
    [DataHolder]
    public class WorldEventGameObject : IDataHolder
    {
        /// <summary>
        /// Spawn id of the object
        /// </summary>
        public uint Guid;

        /// <summary>
        /// ID of the world event relating to this entry
        /// </summary>
        public uint EventId;

        public void FinalizeDataHolder()
        {
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

        public uint ModelId;

        public uint EquipmentId;

        public SpellId spell_start;

        public SpellId spell_end;

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
