using System;
using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.World;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Spawns;
using WCell.Util;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.GameObjects.Spawns
{
    /// <summary>
    /// Spawn-information for GameObjects
    /// </summary>
    [DataHolder]
    public class GOSpawnEntry : SpawnEntry<GOSpawnPoolTemplate, GOSpawnEntry, GameObject, GOSpawnPoint, GOSpawnPool>, IDataHolder
    {
        #region GO-Specific Data

        public uint Id;
        public GOEntryId EntryId;

        [NotPersistent]
        public uint EntryIdRaw;
        public GameObjectState State;

        public float Scale = 1;

        [Persistent(GOConstants.MaxRotations)]
        public float[] Rotations;

        public byte AnimProgress;

        #endregion GO-Specific Data

        [NotPersistent]
        public GOEntry Entry;

        [NotPersistent]
        public LootItemEntry LootEntry;

        public GOSpawnEntry()
        {
        }

        public GOSpawnEntry(GOEntry entry, GameObjectState state,
            MapId mapId, ref Vector3 pos, float orientation, float scale, float[] rotations, int respawnTimeSecs = 600)
        {
            //Id = id;
            Entry = entry;
            EntryId = entry.GOId;
            State = state;
            MapId = mapId;
            Position = pos;
            Orientation = orientation;
            Scale = scale;
            Rotations = rotations;
            RespawnSeconds = respawnTimeSecs;
        }

        public uint LootMoney
        {
            get { return 0; }
        }

        /// <summary>
        /// Spawns and returns a new GameObject from this template into the given map
        /// </summary>
        /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
        public GameObject Spawn(Map map)
        {
            if (Entry == null)
            {
                return null;
            }
            var go = GameObject.Create(Entry, new WorldLocationStruct(map, Position), this);
            return go;
        }

        /// <summary>
        /// Spawns and returns a new GameObject from this template at the given location
        /// </summary>
        /// <returns>The newly spawned GameObject or null, if the Template has no Entry associated with it.</returns>
        public GameObject Spawn(IWorldLocation pos)
        {
            return GameObject.Create(Entry, pos, this);
        }

        public override GameObject SpawnObject(GOSpawnPoint point)
        {
            return GameObject.Create(Entry, point, this, point);
        }

        public uint GetId()
        {
            return Id;
        }

        public DataHolderState DataHolderState
        {
            get;
            set;
        }

        #region FinalizeDataHolder

        /// <summary>
        /// Finalize this GOSpawnEntry
        /// </summary>
        /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom GOSpawnEntry that share a pool)</param>
        public void FinalizeDataHolder()
        {
            FinalizeDataHolder(true);
        }

        private void AddToPoolTemplate()
        {
            m_PoolTemplate = GOMgr.GetOrCreateSpawnPoolTemplate(PoolId);
            m_PoolTemplate.AddEntry(this);
        }

        /// <summary>
        /// Finalize this GOSpawnEntry
        /// </summary>
        /// <param name="addToPool">If set to false, will not try to add it to any pool (recommended for custom GOSpawnEntry that share a pool)</param>
        public override void FinalizeDataHolder(bool addToPool)
        {
            // get Entry
            if (Entry == null)
            {
                Entry = GOMgr.GetEntry(EntryId, false);
                if (Entry == null)
                {
                    ContentMgr.OnInvalidDBData("{0} had an invalid EntryId.", this);
                    return;
                }
            }

            // fix data inconsistencies
            if (Scale == 0)
            {
                Scale = 1;
            }

            if (EntryId == 0)
            {
                EntryId = (GOEntryId)EntryIdRaw;
            }
            else
            {
                EntryIdRaw = (uint)EntryId;
            }

            if (Rotations == null)
            {
                Rotations = new float[GOConstants.MaxRotations];
            }

            //GOMgr.Templates.Add(Id, this);

            // do the default thing
            base.FinalizeDataHolder(addToPool);

            if (MapId != MapId.End)
            {
                // valid map
                Entry.SpawnEntries.Add(this);

                // add to list of GOSpawnEntries
                ArrayUtil.Set(ref GOMgr.SpawnEntries, SpawnId, this);

                if (addToPool)
                {
                    // add to pool
                    AddToPoolTemplate();
                }
            }

            // Is this GO associated with an event
            if (_eventId != 0)
            {
                // The event id loaded can be negative if this
                // entry is expected to despawn during an event
                var eventId = (uint)Math.Abs(_eventId);

                //Check if the event is valid
                var worldEvent = WorldEventMgr.GetEvent(eventId);
                if (worldEvent != null)
                {
                    // Add this GO to the list of related spawns
                    // for the given world event
                    var eventGO = new WorldEventGameObject() { _eventId = _eventId, EventId = eventId, Guid = SpawnId, Spawn = _eventId > 0 };

                    worldEvent.GOSpawns.Add(eventGO);
                }
                EventId = eventId;
            }
        }

        #endregion FinalizeDataHolder

        public override string ToString()
        {
            return (Entry != null ? Entry.DefaultName : "") + " (EntryId: " + EntryId + " (" + (int)EntryId + ")" //+ ", Id: " + Id
                + ")";
        }

        /// <summary>
        /// Do not remove: Used internally for caching
        /// </summary>
        public static IEnumerable<GOSpawnEntry> GetAllDataHolders()
        {
            var list = new List<GOSpawnEntry>(10000);
            foreach (var entry in GOMgr.Entries.Values)
            {
                if (entry != null)
                {
                    list.AddRange(entry.SpawnEntries);
                }
            }
            return list;
        }
    }
}