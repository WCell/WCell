using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using WCell.Util;
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

        public Path(ICollection<Vector3> points)
        {
            Reset(points.Count);

            foreach (var point in points)
            {
                Add(point);
            }
        }

        /// <summary>
        /// Creates a new path that re-uses the given array of points.
        /// </summary>
        /// <param name="points"></param>
        public Path(Vector3[] points)
        {
            this.points = points;
            last = points.Length;
        }

        public Path(Path path)
        {
            points = path.points;
            index = path.index;
            last = path.last;
        }

		public Vector3 this[int i]
		{
			get { return points[i]; }
		}

		public Vector3 First
		{
			get { return points[0]; }
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
            
		    for (var i = 0; i < points.Length; i++)
            {
                points[i] = new Vector3(float.MinValue);
		    }

            last = points.Length;
		}

	    /// <summary>
        /// Adds a new vector to the Path in reverse order
        /// i.e. the first added vector goes onto the end of the array, the next goes to (end - 1), etc.
        /// </summary>
        /// <param name="v"></param>
		public void Add(Vector3 v)
		{
			points[--last] = v;
			index = last;
		}

        /// <summary>
        /// Adds a list of new vectors to the Path in reverse order
        /// i.e. the first added vector goes onto the end of the array, the next goes onto (end - 1), etc.
        /// </summary>
        public void Add(IEnumerable<Vector3> list)
        {
            foreach (var v in list)
            {
                points[--last] = v;
                index = last;
            }
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

        public Path Copy()
        {
            return new Path();
        }
	}
}