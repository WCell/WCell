using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
    public class MapDifficultyDBCEntry
    {
        public uint Id;
        public MapId MapId;
        public uint Index;
        /// <summary>
        /// You must have level...
        /// You must have Key of...
        /// Heroid Difficulty requires completion of...
        /// You must complete the quest...
        /// </summary>
        public string RequirementString;
        /// <summary>
        /// Automatic reset-time in seconds.
        /// 0 for non-raid Dungeons
        /// </summary>
        public int ResetTime;
        /// <summary>
        /// Might be 0 (have to use MapInfo.MaxPlayerCount)
        /// </summary>
        public int MaxPlayerCount;
    }

    /// <summary>
    /// The template of a Map
    /// </summary>
    [DataHolder]
    public partial class MapTemplate : IDataHolder, IComparable
    {
        public MapId Id;
        public string Name;
        public bool HasTwoSides;
        public uint LoadScreen;
        public MapId ParentMapId;
        public MapType Type;
        public int MinLevel;
        public int MaxLevel;

        /// <summary>
        /// Maximum amount of players allowed in the map.
        /// See Difficulties for more information.
        /// </summary>
        [NotPersistent]
        public int MaxPlayerCount;
        public Vector3 RepopPosition;
        public MapId RepopMapId;
        public uint AreaTableId;
        public string HordeText;
        public string AllianceText;
        public int HeroicLevelDiff;
        public uint RequiredQuestId;
        public uint RequiredItemId;
        public ClientId RequiredClientId;
        public int DefaultResetTime;

        /// <summary>
        /// The default BattlegroundTemplate, associated with this MapTemplate
        /// </summary>
        public BattlegroundTemplate BattlegroundTemplate
        {
            get;
            internal set;
        }

        /// <summary>
        /// The default InstanceTemplate, associated with this MapInfo
        /// </summary>
        [NotPersistent]
        public InstanceTemplate InstanceTemplate;

        /// <summary>
        /// The BoundingBox around the entire Map
        /// </summary>
        [NotPersistent]
        public BoundingBox Bounds;

        [NotPersistent]
        public MapDifficultyEntry[] Difficulties;

        public MapDifficultyEntry GetDifficulty(uint index)
        {
            var diff = Difficulties.Get(index);
            if (diff == null)
            {
                return Difficulties[0];
            }
            return diff;
        }

        public uint GetId()
        {
            return (uint)Id;
        }

        public bool IsRaid
        {
            get { return Type == MapType.Raid; }
        }

        /// <summary>
        /// All zone ids within the Map
        /// </summary>
        public bool IsInstance
        {
            get { return Type == MapType.Dungeon || Type == MapType.Raid; }
        }

        /// <summary>
        /// Battleground or Arena
        /// </summary>
        public bool IsBattleground
        {
            get
            {
                //return Type == MapType.Battleground || Type == MapType.Arena;
                return BattlegroundTemplate != null;
            }
        }

        public Map RepopMap
        {
            get
            {
                if (RepopMapId != MapId.End)
                {
                    return World.GetNonInstancedMap(RepopMapId);
                }
                return null;
            }
        }

        [NotPersistent]
        public Vector3[] EntrancePositions = new Vector3[0];

        [NotPersistent]
        /// <summary>
        ///
        /// </summary>
        public Vector3 FirstEntrance
        {
            get { return EntrancePositions.Length > 0 ? EntrancePositions[0] : Vector3.Right; }
        }

        #region Ids

        IdQueue Ids;

        public uint NextId()
        {
            if (Ids == null)
            {
                Ids = new IdQueue();
            }
            return Ids.NextId();
        }

        public void RecycleId(uint id)
        {
            Ids.RecycleId(id);
        }

        #endregion Ids

        #region Zones

        [NotPersistent]
        /// <summary>
        /// All the ZoneInfos within this map.
        /// </summary>
        public readonly IList<ZoneTemplate> ZoneInfos = new List<ZoneTemplate>();

        [NotPersistent]
        public ZoneTileSet ZoneTileSet;

        public ZoneTemplate GetZoneInfo(float x, float y)
        {
            if (ZoneTileSet != null)
            {
                var zoneId = ZoneTileSet.GetZoneId(x, y);
                return World.GetZoneInfo(zoneId);
            }
            return null;
        }

        #endregion Zones

        /// <summary>
        /// Does all the default checks
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        public bool MayEnter(Character chr)
        {
            if (Type == MapType.Normal)
            {
                return true;
            }

            return
                (RequiredQuestId == 0 || chr.QuestLog.FinishedQuests.Contains(RequiredQuestId)) &&
                RequiredClientId <= chr.Account.ClientId;
            //&& (RequiredItemId == 0 || chr.Inventory.Contains(RequiredItemId)) &&
            //chr.Inventory.ContainsAll(RequiredKeys);
        }

        public void FinalizeDataHolder()
        {
            //if (NormalResetDelay > 0 && HeroicResetDelay == 0)
            //{
            //    HeroicResetDelay = NormalResetDelay;
            //}

            if (RepopPosition == default(Vector3))
            {
                RepopMapId = MapId.End;
            }

            if (IsInstance)
            {
                InstanceTemplate = new InstanceTemplate(this);
                InstanceMgr.InstanceInfos.Add(this);
            }

            //ArrayUtil.Set(ref World.s_mapInfos, (uint)Id, this);
        }

        #region Events

        internal void NotifyCreated(Map map)
        {
            var evt = Created;
            if (evt != null)
            {
                evt(map);
            }
        }

        public void NotifyStarted(Map map)
        {
            var evt = Started;
            if (evt != null)
            {
                evt(map);
            }
        }

        public bool NotifySpawning(Map map)
        {
            var evt = Spawning;
            if (evt != null)
            {
                return evt(map);
            }
            return true;
        }

        public void NotifySpawned(Map map)
        {
            var evt = Spawned;
            if (evt != null)
            {
                evt(map);
            }
        }

        public bool NotifyStopping(Map map)
        {
            var evt = Stopping;
            if (evt != null)
            {
                return evt(map);
            }
            return true;
        }

        public void NotifyStopped(Map map)
        {
            var evt = Started;
            if (evt != null)
            {
                evt(map);
            }
        }

        public void NotifyPlayerEntered(Map map, Character chr)
        {
            var evt = PlayerEntered;
            if (evt != null)
            {
                evt(map, chr);
            }
        }

        public void NotifyPlayerLeft(Map map, Character chr)
        {
            var evt = PlayerLeft;
            if (evt != null)
            {
                evt(map, chr);
            }
        }

        public bool NotifyPlayerBeforeDeath(Character chr)
        {
            var evt = PlayerBeforeDeath;
            if (evt != null)
            {
                return evt(chr);
            }
            return true;
        }

        public void NotifyPlayerDied(IDamageAction action)
        {
            var evt = PlayerDied;
            if (evt != null)
            {
                evt(action);
            }
            action.Victim.Map.OnPlayerDeath(action);
        }

        public void NotifyPlayerResurrected(Character chr)
        {
            var evt = PlayerResurrected;
            if (evt != null)
            {
                evt(chr);
            }
        }

        #endregion Events

        #region Misc

        //public static IEnumerable<MapInfo> GetAllDataHolders()
        //{
        //    return World.MapInfos;
        //}

        public int CompareTo(object obj)
        {
            var info = obj as MapTemplate;
            if (info != null)
            {
                return Id.CompareTo(info.Id);
            }
            return -1;
        }

        public override string ToString()
        {
            return Type + " " + Name + " (" + Id + " #" + (uint)Id + ")";
        }

        #endregion Misc
    }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using WCell.Core;
//using WCell.Constants;
//using WCell.Constants.World;

//namespace WCell.RealmServer.Global
//{
//    /// <summary>
//    /// Holds information about a map, an area of the world that requires a teleport to get to.
//    /// </summary>
//    public class MapInfo
//    {
//        /// <summary>
//        /// The name of the map.
//        /// </summary>
//        public string Name
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The map type of the map.
//        /// </summary>
//        public MapType MapType
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The ID of the map.
//        /// </summary>
//        public MapId Id
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Whether or not this map is Burning Crusade only.
//        /// </summary>
//        public bool IsBurningCrusadeOnly
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The minimum level to enter the map.
//        /// </summary>
//        public uint MinimumLevel
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The maximum level to enter the map.
//        /// </summary>
//        public uint MaximumLevel
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The maximum amount of players that can enter the map.
//        /// </summary>
//        public uint MaximumPlayers
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The raid reset timer for this map, in second.
//        /// </summary>
//        public uint RaidResetTimer
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The heroic raid reset timer for this map, in second.
//        /// </summary>
//        public uint HeroicResetTimer
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Whether or not this map can be set to Heroic mode.
//        /// </summary>
//        public bool IsHeroicCapable
//        {
//            get { return HeroicResetTimer > 0; }
//        }

//        /// <summary>
//        /// The description of the heroic mode for this map.
//        /// </summary>
//        public string HeroicDescription
//        {
//            get;
//            set;
//        }
//    }
//}