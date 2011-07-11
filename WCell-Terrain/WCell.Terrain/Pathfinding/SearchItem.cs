using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Pathfinding
{
	public class SearchItem
	{
		public static SearchItem Null = new SearchItem(-1);

		private int triangle;
		private int edge;
		private float distFromStart;
		private float directDistToDest;
		private readonly SearchItem previous;

		/// <summary>
		/// Amount of nodes traversed
		/// </summary>
		private int nodeCount;

		/// <summary>
		/// Root
		/// </summary>
		private SearchItem(int tri)
		{
			triangle = tri;
			directDistToDest = 0;
			previous = null;

			distFromStart = 0;
			nodeCount = 0;
		}

		public SearchItem(int triangle, int edge, float directDistToDest, ref Vector3 pointOfReference)
		{
			this.triangle = triangle;
			this.edge = edge;
			this.directDistToDest = directDistToDest;
			this.previous = Null;
			this.PointOfReference = pointOfReference;

			distFromStart = 0;
			nodeCount = previous.nodeCount + 1;
		}

		public SearchItem(int triangle, int edge, float directDistToDest, ref Vector3 pointOfReference, SearchItem previous)
		{
			this.triangle = triangle;
			this.edge = edge;
			this.directDistToDest = directDistToDest;
			this.previous = previous;
			this.PointOfReference = pointOfReference;

			distFromStart = previous.distFromStart + Vector3.Distance(previous.PointOfReference, pointOfReference);
			nodeCount = previous.nodeCount + 1;
		}

		public int Triangle
		{
			get { return triangle; }
			set { triangle = value; }
		}

		/// <summary>
		/// The edge of the previous Triangle that was traversed to get to this (0, 1 or 2)
		/// </summary>
		public int Edge
		{
			get { return edge; }
			set { edge = value; }
		}

		public float DistFromStart
		{
			get { return distFromStart; }
			set { distFromStart = value; }
		}

		public float DirectDistToDest
		{
			get { return directDistToDest; }
			set { directDistToDest = value; }
		}

		/// <summary>
		/// The position where we enter this triangle
		/// </summary>
		public Vector3 PointOfReference
		{
			get;
			set;
		}

		public SearchItem Previous
		{
			get { return previous; }
		}

		public bool IsNull
		{
			get { return Triangle == -1; }
		}

		/// <summary>
		/// Amount of nodes traversed, including this one
		/// </summary>
		public int NodeCount
		{
			get { return nodeCount; }
		}

		/// <summary>
		/// Typical A*: Already traversed cost + Estimated remaining cost
		/// </summary>
		public float GetSearchValue()
		{
			return distFromStart + directDistToDest;
		}
	}
}
