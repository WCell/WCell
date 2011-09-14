using System;
using WCell.Constants.World;
using WCell.Core.Terrain;
using WCell.RealmServer.Global;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Waypoints
{
	public class RandomWaypointGenerator : WaypointGenerator
	{
		public static Vector3[] GenerateWaypoints(MapId map, Vector3 lastPos)
		{
			var terrain = TerrainMgr.GetTerrain(map);
			if (terrain != null)
			{
				var gen = new RandomWaypointGenerator();
				var wps = gen.GenerateWaypoints(terrain, lastPos);
				return wps;
			}
			return new Vector3[0];
		}

		public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos)
		{
			return GenerateWaypoints(terrain, lastPos, 5, 10, 5, 10);
		}

		public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, float radius)
		{
			var count = (int)(Math.Min(1.0f, radius) / 5) * 2;
			count = Math.Max(3, count);
			return GenerateWaypoints(terrain, lastPos, 1, radius, count);
		}

		public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, int min, int max, float minDist, float maxDist)
		{
			if (min < 1)
			{
				throw new ArgumentException("The minimum point count must be greater than 1", "min");
			}
			if (max < min)
			{
				throw new ArgumentException("The maximum point count must be greater than the minimum", "max");
			}

			var count = Utility.Random(min, max);
			return GenerateWaypoints(terrain, lastPos, minDist, maxDist, count);
		}

		public static Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, float minDist, float maxDist, int count)
		{
			if(maxDist < minDist)
			{
				throw new ArgumentException("The maximum waypoint distance must be greater than the minimum", "maxDist");
			}

			var wps = new Vector3[count];
			for (var i = 0; i < count; i++)
			{
				var direction = Utility.Random(0, MathUtil.TwoPI);
				var dist = Utility.Random(minDist, maxDist);
				var z = lastPos.Z;
				lastPos.GetPointYX(direction, dist, out lastPos);
				lastPos.Z = z;
				//consider flying units
				lastPos.Z = terrain.GetGroundHeightUnderneath(lastPos);
				wps[i] = lastPos;
			}
			return wps;
		}
	}
}