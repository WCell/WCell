using System;
using WCell.Constants.World;
using WCell.RealmServer.Lang;
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
		private ZoneTemplate m_ZoneTemplate;

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
			MapId = mapId;
			Position = pos;
		}

		public string DefaultName
		{
			get { return Names.LocalizeWithDefaultLocale(); }
			set { Names[(int)RealmServerConfiguration.DefaultLocale] = value; }
		}

		public string EnglishName
		{
			get { return Names.LocalizeWithDefaultLocale(); }
			set { Names[(int)ClientLocale.English] = value; }
		}

		public MapId MapId
		{
			get;
			set;
		}

		public Map Map
		{
			get { return World.GetMap(MapId); }
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
		public ZoneTemplate ZoneTemplate
		{
			get { return m_ZoneTemplate; }
			set
			{
				m_ZoneTemplate = value;
				ZoneId = m_ZoneTemplate != null ? m_ZoneTemplate.Id : 0;
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
					((WorldZoneLocation)zone.Site).ZoneTemplate = null;
				}
				else if (zone.Site != null)
				{
					return;
				}
				zone.Site = this;
				ZoneTemplate = zone;
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
						((WorldZoneLocation)obj).MapId == MapId
					   );
			}
			return false;
		}

		// Get a unique id for current world location
		public override int GetHashCode()
		{
			return (int)((int)MapId * (Position.X * Position.Y * Position.Z));
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