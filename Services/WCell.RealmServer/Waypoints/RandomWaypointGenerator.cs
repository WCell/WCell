using System;
using WCell.Constants.World;
using WCell.RealmServer.Global;
using WCell.Util.Graphics;
using WCell.Util;
using WCell.Core.Terrain;

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

		public Vector3[] GenerateWaypoints(ITerrain terrain, Vector3 lastPos, int min, int max, float minDist, float maxDist)
		{
			if (min < 1 || max < min || maxDist < minDist)
			{
				throw new ArgumentException("Invalid arguments");
			}

			var count = Utility.Random(min, max);
			var wps = new Vector3[count];
			for (var i = 0; i < count; i++)
			{
				var direction = Utility.Random(0, MathUtil.TwoPI);
				var dist = Utility.Random(minDist, maxDist);
				lastPos.GetPointYX(direction, dist, out lastPos);
				lastPos.Z = terrain.GetGroundHeightUnderneath(lastPos);
				wps[i] = lastPos;
			}
			return wps;
		}
	}
}