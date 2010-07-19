using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer;
using WCell.Util.Graphics;
using WCell.RealmServer.Global;
using WCell.Constants.World;
using WCell.RealmServer.Entities;

namespace WCell.Addons.Default.Teleport
{
	public class TeleportNode : INamedWorldZoneLocation
	{
		private string[] m_Names = new string[(int)ClientLocale.End];

		public readonly List<INamedWorldZoneLocation> Destinations = new List<INamedWorldZoneLocation>(5);

		public Func<TeleportNode, WorldObject> ObjectCreator = TeleportNetwork.CreateDefaultPortal;

		private Vector3 m_Position;

		public TeleportNode(string defaultName, MapId id, Vector3 pos)
		{
			DefaultName = defaultName;
			Region = World.GetRegion(id);
			if (Region == null)
			{
				throw new ArgumentException("Map is not a continent: " + id);
			}
			Position = pos;
		}

		public WorldObject TeleporterObject;

		public TeleportNode(string defaultName, Region rgn, Vector3 pos)
		{
			DefaultName = defaultName;
			Region = rgn;
			Position = pos;
		}

		public string[] Names
		{
			get { return m_Names; }
			set { m_Names = value; }
		}

		public string DefaultName
		{
			get { return Names[(int)RealmServerConfiguration.DefaultLocale]; }
			set { Names[(int)RealmServerConfiguration.DefaultLocale] = value; }
		}

		public MapId RegionId
		{
			get { return Region.Id; }
		}

		public Region Region
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

		public ZoneInfo ZoneInfo
		{
			get { return null; }
		}

		public void Spawn()
		{
			if (TeleporterObject == null || !TeleporterObject.IsInWorld)
			{
				var go = TeleporterObject = ObjectCreator(this);
				Region.AddObject(go, ref m_Position);
			}
		}
	}
}