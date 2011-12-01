using System.Collections.Generic;

namespace WCell.Util.Graphics
{
	public struct Triangle
	{
		public Vector3 Point1;
		public Vector3 Point2;
		public Vector3 Point3;

		public Triangle(Vector3 point1, Vector3 point2, Vector3 point3)
		{
			Point1 = point1;
			Point2 = point2;
			Point3 = point3;
		}

		public Vector3 Min
		{
			get { return Vector3.Min(Vector3.Min(Point1, Point2), Point3); }
		}

		public Vector3 Max
		{
			get { return Vector3.Max(Vector3.Max(Point1, Point2), Point3); }
		}

		public List<int> Indices
		{
			get
			{
				return new List<int> { 0, 1, 2 };
			}
		}

		public IEnumerable<Vector3> Vertices
		{
			get
			{
				yield return Point1;
				yield return Point2;
				yield return Point3;
			}
		}

		/// <summary>
		/// Computes the normal of this triangle *without normalizing it*
		/// </summary>
		public Vector3 CalcNormal()
		{
			return Vector3.Cross(Point3 - Point1, Point2 - Point1);
		}

		/// <summary>
		/// Computes the normalized normal of this triangle
		/// </summary>
		public Vector3 CalcNormalizedNormal()
		{
			var normal = Vector3.Cross(Point3 - Point1, Point2 - Point1);
			normal.Normalize();
			return normal;
		}
	}
}
