using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer;
using WCell.RealmServer.Lang;
using WCell.Util.Graphics;
using WCell.RealmServer.Global;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Teleport
{
	public class TeleportNode : INamedWorldZoneLocation
	{
		public delegate WorldObject TeleporterCreatorFunc(TeleportNode node, Map map, Vector3 pos);

		private string[] m_Names = new string[(int)ClientLocale.End];

		public readonly List<INamedWorldZoneLocation> Destinations = new List<INamedWorldZoneLocation>(5);

		public TeleporterCreatorFunc TeleportCreator = TeleportNetwork.CreateDefaultPortal;

		private Vector3 m_Position;

		public TeleportNode(string defaultName, MapId id, Vector3 pos)
		{
			DefaultName = defaultName;
			Map = World.GetMap(id);
			if (Map == null)
			{
				throw new ArgumentException("Map is not a continent: " + id);
			}
			Position = pos;
		}

		public WorldObject TeleporterObject;

		public TeleportNode(string defaultName, Map rgn, Vector3 pos)
		{
			DefaultName = defaultName;
			Map = rgn;
			Position = pos;
		}

		public string[] Names
		{
			get { return m_Names; }
			set { m_Names = value; }
		}

		public string DefaultName
		{
			get { return Names.LocalizeWithDefaultLocale(); }
			set { Names[(int)RealmServerConfiguration.DefaultLocale] = value; }
		}

		public MapId MapId
		{
			get { return Map.Id; }
		}

		public Map Map
		{
			get;
			set;
		}

		public Vector3 Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		public ZoneId ZoneId
		{
			get { return ZoneId.None; }
		}

		public ZoneTemplate ZoneTemplate
		{
			get { return null; }
		}

		public void Spawn()
		{
			if (TeleporterObject == null || !TeleporterObject.IsInWorld)
			{
				TeleporterObject = TeleportCreator(this, Map, m_Position);
			}
		}
	}
}