using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

	}
}
