using System;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.RealmServer.Global;
using WCell.Util.Data;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	#region IWorldLocation
	public interface IWorldLocation : IHasPosition
	{
		MapId MapId { get; }

		[NotPersistent]
		Map Map { get; }

		[NotPersistent]
		uint Phase { get; }
	}
	#endregion

	#region IWorldZoneLocation
	public interface IWorldZoneLocation : IWorldLocation
	{
		ZoneId ZoneId { get; }

		[NotPersistent]
		ZoneTemplate ZoneTemplate { get; }
	}
	#endregion

	#region INamedWorldZoneLocation
	public interface INamedWorldZoneLocation : IWorldZoneLocation
	{
		string[] Names
		{
			get;
			set;
		}

		string DefaultName { get; }
	}
	#endregion

	#region WorldLocation
	public class WorldLocation : IWorldLocation
	{
		public WorldLocation(MapId map, Vector3 pos, uint phase = WorldObject.DefaultPhase)
		{
			Position = pos;
			Map = World.GetNonInstancedMap(map);
			if (Map == null)
			{
				throw new ArgumentException("map", "Invalid Map in WorldLocation: " + map);
			}
			Phase = phase;
		}

		public WorldLocation(Map map, Vector3 pos, uint phase = WorldObject.DefaultPhase)
		{
			Position = pos;
			Map = map;
			Phase = phase;
		}

		public Vector3 Position { get; set; }
		public MapId MapId
		{
			get { return Map.Id; }
		}

		public Map Map { get; set; }

		public uint Phase { get; set; }
	}
	#endregion

	#region WorldLocationStruct
	public struct WorldLocationStruct : IWorldLocation
	{
		private Vector3 m_Position;
		private Map m_Map;
		private uint m_Phase;

		public WorldLocationStruct(MapId map, Vector3 pos, uint phase = WorldObject.DefaultPhase)
		{
			m_Position = pos;
			m_Map = World.GetNonInstancedMap(map);
			if (m_Map == null)
			{
				throw new Exception("Invalid Map in WorldLocationStruct: " + map);
			}
			m_Phase = phase;
		}

		public WorldLocationStruct(Map map, Vector3 pos, uint phase = WorldObject.DefaultPhase)
		{
			m_Position = pos;
			m_Map = map;
			m_Phase = phase;
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

		public uint Phase
		{
			get { return m_Phase; }
			set { m_Phase = value; }
		}

		public MapId MapId
		{
			get { return Map.Id; }
		}
	}
	#endregion

	#region SimpleWorldLocation
	public class SimpleWorldLocation : IWorldLocation
	{
		public SimpleWorldLocation(MapId map, Vector3 pos, uint phase = WorldObject.DefaultPhase)
		{
			Position = pos;
			MapId = map;
			Phase = phase;
		}

		public Vector3 Position
		{
			get;
			set;
		}

		public MapId MapId
		{
			get;
			set;
		}

		public Map Map
		{
			get { return World.GetNonInstancedMap(MapId); }
		}

		public uint Phase
		{
			get;
			set;
		}
	}
	#endregion

	#region WorldZoneLocation
	public class WorldZoneLocation : WorldLocation, IWorldZoneLocation
	{
		public WorldZoneLocation(MapId map, Vector3 pos, ZoneTemplate zone)
			: base(map, pos)
		{
			ZoneTemplate = zone;
		}

		public WorldZoneLocation(Map map, Vector3 pos, ZoneTemplate zone)
			: base(map, pos)
		{
			ZoneTemplate = zone;
		}

		public WorldZoneLocation(IWorldZoneLocation location)
			: base(location.Map, location.Position)
		{
			ZoneTemplate = location.ZoneTemplate;
		}

		public WorldZoneLocation(MapId map, Vector3 pos, ZoneId zone)
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
	#endregion

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