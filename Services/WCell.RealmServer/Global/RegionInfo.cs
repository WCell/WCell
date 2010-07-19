using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Util;
using WCell.Util.Collections;
using WCell.Util.Data;
using WCell.Constants.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Battlegrounds;
using System;
using WCell.RealmServer.Instances;
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
		/// Might be 0 (have to use RegionInfo.MaxPlayerCount)
		/// </summary>
    	public int MaxPlayerCount;
    }

	/// <summary>
	/// 
	/// </summary>
	[DataHolder]
	public partial class RegionInfo : IDataHolder, IComparable
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
        /// Maximum amount of players allowed in the region.
        /// See Difficulties for more information.
        /// </summary>
        [NotPersistent]
		public int MaxPlayerCount;
		public Vector3 RepopPosition;
		public MapId RepopRegionId;
		public uint AreaTableId;
		public string HordeText;
		public string AllianceText;
		public int HeroicLevelDiff;
		public uint RequiredQuestId;
		public uint RequiredItemId;
		public ClientId RequiredClientId;
		public int DefaultResetTime;

		/// <summary>
		/// The default BattlegroundTemplate, associated with this RegionInfo
		/// </summary>
		[NotPersistent]
		public BattlegroundTemplate BGTemplate;

		/// <summary>
		/// The default InstanceTemplate, associated with this RegionInfo
		/// </summary>
		[NotPersistent]
		public InstanceTemplate InstanceTemplate;

		/// <summary>
		/// The BoundingBox around the entire Region
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
		/// All zone ids within the Region
		/// </summary>
		public bool IsInstance
		{
			get { return Type == MapType.Dungeon || Type == MapType.Raid; }
		}

		public bool IsPvP
		{
			get { return Type == MapType.Battleground || Type == MapType.Arena; }
		}

		public Region RepopRegion
		{
			get
			{
				if (RepopRegionId != MapId.End)
				{
					return World.GetRegion(RepopRegionId);
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
		#endregion

		#region Zones
		[NotPersistent]
		/// <summary>
		/// All the ZoneInfos within this region.
		/// </summary>
		public readonly IList<ZoneInfo> ZoneInfos = new List<ZoneInfo>();

		[NotPersistent]
		public ZoneTileSet ZoneTileSet;

		public ZoneInfo GetZoneInfo(float x, float y)
		{
			if (ZoneTileSet != null)
			{
				var zoneId = ZoneTileSet.GetZoneId(x, y);
				return World.GetZoneInfo(zoneId);
			}
			return null;
		}
		#endregion

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
				RepopRegionId = MapId.End;
			}

			if (IsInstance)
			{
				InstanceTemplate = new InstanceTemplate(this);
				InstanceMgr.InstanceInfos.Add(this);
			}

			//ArrayUtil.Set(ref World.s_regionInfos, (uint)Id, this);
		}

		#region Events

		internal void NotifyCreated(Region region)
		{
			var evt = Created;
			if (evt != null)
			{
				evt(region);
			}
		}

		public void NotifyStarted(Region region)
		{
			var evt = Started;
			if (evt != null)
			{
				evt(region);
			}
		}

		public bool NotifySpawning(Region region)
		{
			var evt = Spawning;
			if (evt != null)
			{
				return evt(region);
			}
			return true;
		}

		public void NotifySpawned(Region region)
		{
			var evt = Spawned;
			if (evt != null)
			{
				evt(region);
			}
		}

		public bool NotifyStopping(Region region)
		{
			var evt = Stopping;
			if (evt != null)
			{
				return evt(region);
			}
			return true;
		}

		public void NotifyStopped(Region region)
		{
			var evt = Started;
			if (evt != null)
			{
				evt(region);
			}
		}

		public void NotifyPlayerEntered(Region region, Character chr)
		{
			var evt = PlayerEntered;
			if (evt != null)
			{
				evt(region, chr);
			}
		}

		public void NotifyPlayerLeft(Region region, Character chr)
		{
			var evt = PlayerLeft;
			if (evt != null)
			{
				evt(region, chr);
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
			action.Victim.Region.OnDeath(action);
		}

		public void NotifyPlayerResurrected(Character chr)
		{
			var evt = PlayerResurrected;
			if (evt != null)
			{
				evt(chr);
			}
		}
		#endregion

		#region Misc
		//public static IEnumerable<RegionInfo> GetAllDataHolders()
		//{
		//    return World.RegionInfos;
		//}

		public int CompareTo(object obj)
		{
			var info = obj as RegionInfo;
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
		#endregion
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
//    /// Holds information about a region, an area of the world that requires a teleport to get to.
//    /// </summary>
//    public class RegionInfo
//    {
//        /// <summary>
//        /// The name of the region.
//        /// </summary>
//        public string Name
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The map type of the region.
//        /// </summary>
//        public MapType MapType
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The ID of the region.
//        /// </summary>
//        public MapId Id
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Whether or not this region is Burning Crusade only.
//        /// </summary>
//        public bool IsBurningCrusadeOnly
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The minimum level to enter the region.
//        /// </summary>
//        public uint MinimumLevel
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The maximum level to enter the region.
//        /// </summary>
//        public uint MaximumLevel
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The maximum amount of players that can enter the region.
//        /// </summary>
//        public uint MaximumPlayers
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The raid reset timer for this region, in second.
//        /// </summary>
//        public uint RaidResetTimer
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// The heroic raid reset timer for this region, in second.
//        /// </summary>
//        public uint HeroicResetTimer
//        {
//            get;
//            set;
//        }

//        /// <summary>
//        /// Whether or not this region can be set to Heroic mode.
//        /// </summary>
//        public bool IsHeroicCapable
//        {
//            get { return HeroicResetTimer > 0; }
//        }


//        /// <summary>
//        /// The description of the heroic mode for this region.
//        /// </summary>
//        public string HeroicDescription
//        {
//            get;
//            set;
//        }
//    }
//}