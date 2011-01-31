using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Holds an exact World-Location, and also the Zone of the Location
	/// </summary>
	public struct NamedWorldZoneLocation : IWorldZoneLocation
	{
		public static readonly NamedWorldZoneLocation Zero = default(NamedWorldZoneLocation);

		public string Name;

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
			get
			{
				return World.GetMap(MapId);
			}
		}

		public uint Phase
		{
			get { return WorldObject.DefaultPhase; }
		}

		public ZoneId ZoneId
		{
			get;
			set;
		}

		public ZoneTemplate ZoneTemplate
		{
			get { return World.GetZoneInfo(ZoneId); }
		}

		public static bool operator ==(NamedWorldZoneLocation left, NamedWorldZoneLocation right)
		{
			return (left.Position == right.Position && left.MapId == right.MapId);
		}

		public static bool operator !=(NamedWorldZoneLocation left, NamedWorldZoneLocation right)
		{
			return !(left == right);
		}

		public bool IsValid
		{
			get { return MapId != MapId.End; }
		}

		public override bool Equals(object obj)
		{
			if (obj is NamedWorldZoneLocation)
			{
				return this == (NamedWorldZoneLocation)obj;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)((int)MapId * (Position.X * Position.Y * Position.Z));
		}
	}
}