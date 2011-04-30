using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Login;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Global
{

	public delegate Zone ZoneCreator(Map map, ZoneTemplate template);

    /// <summary>
    /// Holds information about a zone, an area within a map.
    /// </summary>
    public partial class ZoneTemplate
    {
        internal ZoneTemplate m_ParentZone;
		internal ZoneId m_parentZoneId;
		internal MapTemplate m_MapTemplate;
		internal MapId m_MapId;
		public readonly List<ZoneTemplate> ChildZones = new List<ZoneTemplate>(1);
        public readonly List<WorldMapOverlayId> WorldMapOverlays = new List<WorldMapOverlayId>();

		public ZoneId Id;

		/// <summary>
		/// All WorldStates that are active in this Zone
		/// </summary>
    	public WorldState[] WorldStates;

		/// <summary>
		/// The location of a significant site within this Zone.
		/// </summary>
		public IWorldLocation Site;

        public MapId MapId
        {
            get { return m_MapId; }
    		set
    		{
    		    m_MapId = value;
				m_MapTemplate = World.GetMapTemplate(value);
    		}
        }

        public MapTemplate MapTemplate
    	{
    		get { return m_MapTemplate; }
    		set
    		{
				m_MapTemplate = value;
				m_MapId = value != null ? value.Id : MapId.End;
    		}
    	}

        public ZoneId ParentZoneId
        {
            get { return m_parentZoneId; }
			//internal set
			//{
			//    m_parentZoneId = value;
			//    m_ParentZone = World.GetZoneInfo(value);
			//}
        }

        public ZoneTemplate ParentZone
        {
            get { return m_ParentZone; }
            internal set
            {
                m_ParentZone = value;
                m_parentZoneId = (value != null)? value.Id : ZoneId.None;
				if (value != null)
				{
					m_ParentZone.ChildZones.Add(this);
				}
            }
        }

        public int ExplorationBit;

        /// <summary>
        /// The flags for the zone.
        /// </summary>
        public ZoneFlags Flags;

        /// <summary>
        /// Who does this Zone belong to (if anyone)
        /// </summary>
        public FactionGroupMask Ownership;

        public int AreaLevel;

		public string Name;

    	public ZoneCreator Creator;

		/// <summary>
		/// Whether this is a PvP zone.
		/// Improve: http://www.wowwiki.com/PvP_flag
		/// </summary>
		public bool IsPvP
		{
			get { return Ownership != (FactionGroupMask.Alliance | FactionGroupMask.Horde); }
		}

		/// <summary>
		/// Whether or not the zone is an arena.
		/// </summary>
		public bool IsArena
		{
			get { return Flags.HasFlag(ZoneFlags.Arena); }
		}

		/// <summary>
		/// Whether or not the zone is a sanctuary.
		/// </summary>
		public bool IsSanctuary
		{
            get { return Flags.HasFlag(ZoneFlags.Sanctuary); }
		}

		/// <summary>
		/// Whether or not the zone is a city.
		/// </summary>
		public bool IsCity
		{
            get { return Flags.HasFlag(ZoneFlags.CapitalCity); }
		}

		/// <summary>
		/// Whether this Zone is hostile towards the given Character
		/// </summary>
		/// <param name="chr">The Character in question.</param>
		/// <returns>Whether or not to set the PvP flag.</returns>
		public bool IsHostileTo(Character chr)
		{
		    var ownership = Ownership;
            if (Ownership == FactionGroupMask.None && ParentZoneId != 0 && ParentZone.Ownership != FactionGroupMask.None)
                ownership = ParentZone.Ownership;

            switch (ownership)
            {
                case FactionGroupMask.Alliance:
                    return chr.FactionGroup != FactionGroup.Alliance && (RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.PVP | RealmServerType.RPPVP) || IsCity || IsArena);
                case FactionGroupMask.Horde:
                    return chr.FactionGroup != FactionGroup.Horde && (RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.PVP | RealmServerType.RPPVP) || IsCity || IsArena);
                case FactionGroupMask.None:
                    return RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.PVP | RealmServerType.RPPVP);
                default:
                    return false;
            }
		}

		#region Events
		/// <summary>
		/// Called when a player enters the zone.
		/// </summary>
		/// <param name="chr">the character entering the zone</param>
		/// <param name="oldZone">the zone the character came from</param>
		internal void OnPlayerEntered(Character chr, Zone oldZone)
		{
			var plrEnter = PlayerEntered;
			if (plrEnter != null)
			{
				plrEnter(chr, oldZone);
			}
		}

		/// <summary>
		/// Called when a player leaves the zone.
		/// </summary>
		/// <param name="chr">the character leaving the zone</param>
		/// <param name="oldZone">the zone the character just left</param>
		internal void OnPlayerLeft(Character chr, Zone oldZone)
		{
			var plrLeft = PlayerLeft;
			if (plrLeft != null)
			{
				plrLeft(chr, oldZone);
			}
		}

		public virtual void OnHonorableKill(Character victor, Character victim)
		{
			// For Hellfire and Halaa, this should be where the Honor Tokens are alotted.
		}
		#endregion

		/// <summary>
		/// Called after ZoneInfo was created
		/// </summary>
    	internal void FinalizeZone()
    	{
			if (Creator == null)
			{
				Creator = DefaultCreator;
			}

			if (ParentZoneId != ZoneId.None)
			{
				// WorldStates are currently only supported for major Zones, no sub-areas
				return;
			}

			WorldStates = Constants.World.WorldStates.GetStates(Id);
    	}

		public Zone DefaultCreator(Map map, ZoneTemplate templ)
		{
			return new Zone(map, templ);
		}
    }

    #region AreaTable.dbc

	public class AreaTableConverter : AdvancedDBCRecordConverter<ZoneTemplate>
	{
		public override ZoneTemplate ConvertTo(byte[] rawData, ref int id)
		{
			id = GetInt32(rawData, 0);
            var area = new ZoneTemplate {
				Id = (ZoneId)GetUInt32(rawData, 0),
				m_MapId = (MapId)GetUInt32(rawData, 1),
				m_parentZoneId = (ZoneId)GetUInt32(rawData, 2),
				ExplorationBit = GetInt32(rawData, 3),
				Flags = (ZoneFlags)GetUInt32(rawData, 4),
				AreaLevel = GetInt32(rawData, 10),
				Name = GetString(rawData, 11),
				Ownership = (FactionGroupMask)GetUInt32(rawData, 12)
			};
            return area;
		}
	}

	#endregion
}