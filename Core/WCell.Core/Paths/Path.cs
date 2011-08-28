using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Util.Graphics;

namespace WCell.Core.Paths
{
	/// <summary>
	/// TODO: Recycle
	/// A nicely recyclable iterable Vector3 collection
	/// </summary>
	public class Path : IEnumerable<Vector3>
	{
		private Vector3[] points;
		private int last, index;

		public Path()
		{
		}

		public Vector3 this[int i]
		{
			get { return points[i]; }
		}

		public Vector3 Next()
		{
			return points[index++];
		}

		public bool HasNext()
		{
			return index < points.Length;
		}

		public void Reset(int newSize)
		{
			if (points == null)
			{
				points = new Vector3[newSize];
			}
			else if (points.Length < newSize)
			{
				Array.Resize(ref points, newSize);
			}
			last = points.Length;
		}

		public void Add(Vector3 v)
		{
			points[--last] = v;
			index = last;
		}

		public IEnumerator<Vector3> GetEnumerator()
		{
			for (var i = last; i < points.Length; i++)
			{
				yield return points[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}