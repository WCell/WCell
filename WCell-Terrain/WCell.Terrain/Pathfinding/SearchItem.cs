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
		private float distFromStart;
		private float directDistToDest;
		private Vector3 enterPos;
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
			enterPos = Vector3.Zero;
			triangle = tri;
			directDistToDest = 0;
			previous = null;

			distFromStart = 0;
			nodeCount = 0;
		}

		public SearchItem(int triangle, float directDistToDest, Vector3 enterPos, SearchItem previous)
		{
			this.enterPos = enterPos;
			this.triangle = triangle;
			this.directDistToDest = directDistToDest;
			this.previous = previous;

			distFromStart = previous.distFromStart + Vector3.Distance(previous.EnterPos, enterPos);
			nodeCount = previous.nodeCount + 1;
		}

		public int Triangle
		{
			get { return triangle; }
			set { triangle = value; }
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
		public Vector3 EnterPos
		{
			get { return enterPos; }
			set { enterPos = value; }
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
		/// Travelled distance + direct distance to destination
		/// </summary>
		public float GetSearchValue()
		{
			return distFromStart + directDistToDest;
		}
	}
}
