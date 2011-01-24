using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Data;
using WCell.Util.Graphics;
using Zone = WCell.RealmServer.Global.Zone;

namespace WCell.RealmServer.Entities
{
	public interface IWorldLocation : IHasPosition
	{
		MapId MapId { get; }

		[NotPersistent]
		Map Map { get; }
	}

	public interface IWorldZoneLocation : IWorldLocation
	{
		ZoneId ZoneId { get; }

		[NotPersistent]
		ZoneTemplate ZoneTemplate { get; }
	}

	public interface INamedWorldZoneLocation : IWorldZoneLocation
	{
		string[] Names
		{
			get;
			set;
		}

		string DefaultName { get; }
	}

	public class WorldLocation : IWorldLocation
	{
		public WorldLocation(MapId map, Vector3 pos)
		{
			Position = pos;
			Map = World.GetMap(map);
			if (Map == null)
			{
				throw new Exception("Invalid Map in WorldLocation: " + map);
			}
		}

		public WorldLocation(Map map, Vector3 pos)
		{
			Position = pos;
			Map = map;
		}

		public Vector3 Position { get; set; }
		public MapId MapId
		{
			get { return Map.Id; }
		}

		public Map Map { get; set; }
	}

	public struct WorldLocationStruct : IWorldLocation
	{
		private Vector3 m_Position;
		private Map m_Map;

		public WorldLocationStruct(MapId map, Vector3 pos)
		{
			m_Position = pos;
			m_Map = World.GetMap(map);
			if (m_Map == null)
			{
				throw new Exception("Invalid Map in WorldLocationStruct: " + map);
			}
		}

		public WorldLocationStruct(Map map, Vector3 pos)
		{
			m_Position = pos;
			m_Map = map;
		}

		public Vector3 Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		public Map Map
		{
			get { return m_Map; }
			set { m_Map = value; }
		}

		public MapId MapId
		{
			get { return Map.Id; }
		}
	}

	public class SimpleWorldLocation : IWorldLocation
	{
		public SimpleWorldLocation(MapId map, Vector3 pos)
		{
			Position = pos;
			MapId = map;
		}

		public Vector3 Position { get; set; }
		public MapId MapId
		{
			get;
			set;
		}

		public Map Map
		{
			get { return World.GetMap(MapId); }
		}
	}

	public class ZoneWorldLocation : WorldLocation, IWorldZoneLocation
	{
		public ZoneWorldLocation(MapId map, Vector3 pos, ZoneTemplate zone)
			: base(map, pos)
		{
			ZoneTemplate = zone;
		}

		public ZoneWorldLocation(Map map, Vector3 pos, ZoneTemplate zone)
			: base(map, pos)
		{
			ZoneTemplate = zone;
		}

		public ZoneWorldLocation(IWorldZoneLocation location)
			: base(location.Map, location.Position)
		{
			ZoneTemplate = location.ZoneTemplate;
		}

		public ZoneWorldLocation(MapId map, Vector3 pos, ZoneId zone)
			: base(map, pos)
		{
			if (Map != null)
			{
				ZoneTemplate = World.GetZoneInfo(zone);
			}
		}

		public ZoneId ZoneId
		{
			get { return ZoneTemplate != null ? ZoneTemplate.Id : ZoneId.None; }
		}

		public ZoneTemplate ZoneTemplate { get; set; }
	}

	public static class LocationUtil
	{
		public static bool IsValid(this IWorldLocation location, Unit user)
		{
			return !location.Position.Equals(default(Vector3)) &&
				(location.Map != null || user.Map.Id == location.MapId);
		}

		public static Zone GetZone(this IWorldZoneLocation loc)
		{
			return loc.Map.GetZone(loc.ZoneId);
		}
	}
}