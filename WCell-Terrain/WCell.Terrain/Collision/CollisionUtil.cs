using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision
{
	public static class CollisionUtil
	{
		public static Triangle GetTriangle(this IShape shape, int index)
		{
			var vertices = shape.Vertices;
			var indices = shape.Indices;
			return new Triangle(vertices[indices[index]], vertices[indices[index + 1]], vertices[indices[index + 2]]);
		}

		/// <summary>
		/// Returns the first triangle that is directly underneath the given position.
		/// The value is -1, if none was found.
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		public static int FindFirstTriangleUnderneath(this IShape shape, Vector3 pos)
		{
			// shoot a ray straight down
			pos.Z += 0.001f;						// make sure, it is slightly above the surface
			var ray = new Ray(pos, Vector3.Down);
			return shape.FindFirstHitTriangle(ray);
		}

		public static int FindFirstTriangleUnderneath(this IShape shape, Vector3 pos, out float dist)
		{
			// shoot a ray straight down
			pos.Z += 0.001f;						// make sure, it is slightly above the surface
			var ray = new Ray(pos, Vector3.Down);
			return shape.FindFirstHitTriangle(ray, out dist);
		}

		public static int IntersectFirstTriangle(this IShape shape, Ray selectRay, out Vector3 v)
		{
			float distance;
			var hit = shape.FindFirstHitTriangle(selectRay, out distance);
			if (hit == -1)
			{
				v = Vector3.Zero;
			}
			else
			{
				v = selectRay.Position + selectRay.Direction*distance;
			}
			return hit;
		}

		/// <summary>
		/// Returns the first triangle that is directly underneath the given position.
		/// The value is -1, if none was found.
		/// </summary>
		/// <returns></returns>
		public static int FindFirstHitTriangle(this IShape shape, Ray selectRay)
		{
			// shoot a ray straight down
			float distance;
			return shape.FindFirstHitTriangle(selectRay, out distance);
		}

		/// <summary>
		/// Intersects the given ray with the tile and sets triangle to the first one that got hit
		/// <param name="distance">The distance from Ray origin to, where it hits the returned triangle</param>
		/// </summary>
		public static int FindFirstHitTriangle(this IShape shape, Ray selectRay, out float distance)
		{
			distance = float.MaxValue;

			var triangle = -1;

			foreach (var trii in shape.GetPotentialColliders(selectRay))
			{
				var tri = shape.GetTriangle(trii);

				float newDist;
				if (!Intersection.RayTriangleIntersect(selectRay, tri.Point1, tri.Point2, tri.Point3, out newDist)) continue;
				if (newDist > distance) continue;

				distance = newDist;
				triangle = trii;
			}

			return triangle;
		}
	}

}
