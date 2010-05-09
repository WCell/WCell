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
		MapId RegionId { get; }

		[NotPersistent]
		Region Region { get; }
	}

	public interface IWorldZoneLocation : IWorldLocation
	{
		ZoneId ZoneId { get; }

		[NotPersistent]
		ZoneInfo ZoneInfo { get; }
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
		public WorldLocation(MapId region, Vector3 pos)
		{
			Position = pos;
			Region = World.GetRegion(region);
			if (Region == null)
			{
				throw new Exception("Invalid Region in WorldLocation: " + region);
			}
		}

		public WorldLocation(Region region, Vector3 pos)
		{
			Position = pos;
			Region = region;
		}

		public Vector3 Position { get; set; }
		public MapId RegionId
		{
			get { return Region.Id; }
		}

		public Region Region { get; set; }
	}

	public class SimpleWorldLocation : IWorldLocation
	{
		public SimpleWorldLocation(MapId region, Vector3 pos)
		{
			Position = pos;
			RegionId = region;
		}

		public Vector3 Position { get; set; }
		public MapId RegionId
		{
			get;
			set;
		}

		public Region Region
		{
			get { return World.GetRegion(RegionId); }
		}
	}

	public class ZoneWorldLocation : WorldLocation, IWorldZoneLocation
	{
		public ZoneWorldLocation(MapId region, Vector3 pos, ZoneInfo zone)
			: base(region, pos)
		{
			ZoneInfo = zone;
		}

		public ZoneWorldLocation(Region region, Vector3 pos, ZoneInfo zone)
			: base(region, pos)
		{
			ZoneInfo = zone;
		}

		public ZoneWorldLocation(IWorldZoneLocation location)
			: base(location.Region, location.Position)
		{
			ZoneInfo = location.ZoneInfo;
		}

		public ZoneWorldLocation(MapId region, Vector3 pos, ZoneId zone)
			: base(region, pos)
		{
			if (Region != null)
			{
				ZoneInfo = World.GetZoneInfo(zone);
			}
		}

		public ZoneId ZoneId
		{
			get { return ZoneInfo != null ? ZoneInfo.Id : ZoneId.None; }
		}

		public ZoneInfo ZoneInfo { get; set; }
	}

	public static class LocationUtil
	{
		public static bool IsValid(this IWorldLocation location, Unit user)
		{
			return !location.Position.Equals(default(Vector3)) &&
				(location.Region != null || user.Region.Id == location.RegionId);
		}

		public static Zone GetZone(this IWorldZoneLocation loc)
		{
			return loc.Region.GetZone(loc.ZoneId);
		}
	}
}
