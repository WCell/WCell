using System;
using WCell.Constants.World;
using WCell.Util.Data;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;
using WCell.Util.Variables;
using WCell.Constants;

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// World locations are specific game locations that can be accessed with teleport command.
	/// </summary>
	[DataHolder]
	public class WorldZoneLocation : IDataHolder, INamedWorldZoneLocation
	{
		public uint Id;
		private ZoneInfo m_ZoneInfo;

		private string[] m_Names = new string[(int)ClientLocale.End];

		[NotPersistent]
		public string[] Names
		{
			get { return m_Names; }
			set { m_Names = value; }
		}

		public WorldZoneLocation()
		{
		}

		public WorldZoneLocation(string name, MapId mapId, Vector3 pos)
			: this(mapId, pos)
		{
			DefaultName = name;
		}

		public WorldZoneLocation(string[] localizedNames, MapId mapId, Vector3 pos)
			: this(mapId, pos)
		{
			Names = localizedNames;
		}

		private WorldZoneLocation(MapId mapId, Vector3 pos)
		{
			RegionId = mapId;
			Position = pos;
		}

		public string DefaultName
		{
			get { return Names[(int)RealmServerConfiguration.DefaultLocale]; }
			set { Names[(int)RealmServerConfiguration.DefaultLocale] = value; }
		}

		public string EnglishName
		{
			get { return Names[(int)ClientLocale.English]; }
			set { Names[(int)ClientLocale.English] = value; }
		}

		public MapId RegionId
		{
			get;
			set;
		}

		public Region Region
		{
			get { return World.GetRegion(RegionId); }
		}

		public Vector3 Position
		{
			get;
			set;
		}

		private ZoneId m_ZoneId;

		public ZoneId ZoneId
		{
			get { return m_ZoneId; }
			set { m_ZoneId = value; }
		}

		/// <summary>
		/// The Zone to which this Location belongs (if any)
		/// </summary>
		[NotPersistent]
		public ZoneInfo ZoneInfo
		{
			get { return m_ZoneInfo; }
			set
			{
				m_ZoneInfo = value;
				ZoneId = m_ZoneInfo != null ? m_ZoneInfo.Id : 0;
			}
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

		// Method called when loading world locations. Its called for each element
		public void FinalizeDataHolder()
		{
			// Add loaded world location to world location dictionary
			WorldLocationMgr.WorldLocations[DefaultName] = this;
			var zone = World.GetZoneInfo(ZoneId);

			if (zone != null)
			{
				if (zone.Site is WorldZoneLocation)
				{
					// override
					((WorldZoneLocation)zone.Site).ZoneInfo = null;
				}
				else if (zone.Site != null)
				{
					return;
				}
				zone.Site = this;
				ZoneInfo = zone;
			}
		}

		// Used to compare current world location to another one
		// World locations are considered the same if have the same coords and map
		public override bool Equals(object obj)
		{
			if (obj is WorldZoneLocation)
			{
				return (
						((WorldZoneLocation)obj).Position.X == Position.X &&
						((WorldZoneLocation)obj).Position.Y == Position.Y &&
						((WorldZoneLocation)obj).Position.Z == Position.Z &&
						((WorldZoneLocation)obj).RegionId == RegionId
					   );
			}
			return false;
		}

		// Get a unique id for current world location
		public override int GetHashCode()
		{
			return (int)((int)RegionId * (Position.X * Position.Y * Position.Z));
		}

		/// <summary>
		/// Overload the ToString method to return a formated text with world location name and id
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0} (Id: {1})", DefaultName, Id);
		}
	}
}