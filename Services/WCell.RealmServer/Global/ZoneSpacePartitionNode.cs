/*************************************************************************
 *
 *   file		: ZoneSpacePartitionNode.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 458 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Global
{
	/// <summary>
	/// Represents a division of region space (a node in any Region's quadtree).
	/// </summary>
	public class ZoneSpacePartitionNode
	{
		private const int Two = 2;
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// The max depth of the QuadTrees used for space partitioning within Regions
		/// </summary>
		public static int DefaultPartitionThreshold = 6;

		public static float MinNodeLength = 250;

		private const int WEST = 0;
		private const int EAST = 1;
		private const int NORTH = 0;
		private const int SOUTH = 1;

		private const int HOR_EAST = 0;
		private const int HOR_CENTER = 1;
		private const int HOR_WEST = 2;
		private const int VER_NORTH = 0;
		private const int VER_CENTER = 1;
		private const int VER_SOUTH = 2;

		private BoundingBox m_bounds;
		private ZoneSpacePartitionNode[,] m_children;
		private Dictionary<EntityId, WorldObject> m_objects;

		//private ZoneSpacePartitionNode[,] m_neighbors;

		/// <summary>
		/// Whether or not this node is a leaf node. (contains objects)
		/// </summary>
		public bool IsLeaf
		{
			get { return m_children == null; }
		}

		/// <summary>
		/// The dimensional bounds of this node.
		/// </summary>
		public BoundingBox Bounds
		{
			get { return m_bounds; }
		}

		/// <summary>
		/// The origin X of this node's bounds.
		/// </summary>
		public float X
		{
			get { return m_bounds.Min.X; }
		}

		/// <summary>
		/// The origin Y of this node's bounds.
		/// </summary>
		public float Y
		{
			get { return m_bounds.Min.Y; }
		}

		/// <summary>
		/// The length of this node.
		/// </summary>
		public float Length
		{
			get { return m_bounds.Max.X - m_bounds.Min.X; }
		}

		/// <summary>
		/// The width of this node.
		/// </summary>
		public float Width
		{
			get { return m_bounds.Max.Y - m_bounds.Min.Y; }
		}

		/// <summary>
		/// Whether or not this node has objects.
		/// </summary>
		public bool HasObjects
		{
			get { return m_objects != null && m_objects.Count > 0; }
		}

		/// <summary>
		/// Creates a node with the given bounds.
		/// </summary>
		/// <param name="bounds"></param>
		public ZoneSpacePartitionNode(BoundingBox bounds)
		{
			m_bounds = bounds;
		}

		/// <summary>
		/// Partitions the node, dividing it into subnodes until the desired depth is reached.
		/// </summary>
		/// <param name="maxLevels">the maximum depth to partition</param>
		/// <param name="startingDepth">the depth to start partitioning from</param>
		internal void PartitionSpace(ZoneSpacePartitionNode parent, int maxLevels, int startingDepth)
		{
			var width = Length / Two;
			var height = Width / Two;

			if (startingDepth < maxLevels && width > MinNodeLength && height > MinNodeLength)
			{
				m_children = new ZoneSpacePartitionNode[Two, Two];

				// Bottom left
				m_children[SOUTH, WEST] =
					new ZoneSpacePartitionNode(
						new BoundingBox(new Vector3(m_bounds.Min.X, m_bounds.Min.Y, m_bounds.Min.Z),
										new Vector3(m_bounds.Min.X + width, m_bounds.Min.Y + height, m_bounds.Max.Z)));

				// Top left
				m_children[NORTH, WEST] =
					new ZoneSpacePartitionNode(
						new BoundingBox(new Vector3(m_bounds.Min.X, m_bounds.Min.Y + height, m_bounds.Min.Z),
										new Vector3(m_bounds.Min.X + width, m_bounds.Max.Y, m_bounds.Max.Z)));

				// Bottom right
				m_children[SOUTH, EAST] =
					new ZoneSpacePartitionNode(
						new BoundingBox(new Vector3(m_bounds.Min.X + width, m_bounds.Min.Y, m_bounds.Min.Z),
										new Vector3(m_bounds.Max.X, m_bounds.Min.Y + height, m_bounds.Max.Z)));

				// Top right
				m_children[NORTH, EAST] =
					new ZoneSpacePartitionNode(
						new BoundingBox(new Vector3(m_bounds.Min.X + width, m_bounds.Min.Y + height, m_bounds.Min.Z),
										new Vector3(m_bounds.Max.X, m_bounds.Max.Y, m_bounds.Max.Z)));

				// Set the next level's depth
				startingDepth++;

				// Partition any further depths 
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						node.PartitionSpace(this, maxLevels, startingDepth);
					}
			}
			else
			{
				m_objects = new Dictionary<EntityId, WorldObject>(0);
			}
		}		
        
        //private static Dictionary<int, ZoneSpacePartitionNode[,]> m_raster = new Dictionary<int, ZoneSpacePartitionNode[,]>();
	
		// private static Dictionary<int, ZoneSpacePartitionNode[,]> m_raster = new Dictionary<int, ZoneSpacePartitionNode[,]>();

		///// <summary>
		///// Partitions the node, dividing it into subnodes until the desired depth is reached.
		///// </summary>
		///// <param name="maxLevels">the maximum depth to partition</param>
		///// <param name="startingDepth">the depth to start partitioning from</param>
		//internal void PartitionSpace(int maxLevels, int startingDepth, int nodeY, int nodeX, int nodesCountAtCurrentDepth)
		//{
		//    float incX = Length / 2;
		//    float incY = Width / 2;

		//    if (!m_raster.ContainsKey(startingDepth))
		//    {
		//        m_raster.Add(startingDepth, new ZoneSpacePartitionNode[nodesCountAtCurrentDepth, nodesCountAtCurrentDepth]);
		//    }

		//    if (m_raster[startingDepth][nodeY, nodeX] != null)
		//    {
		//        throw new Exception();
		//    }

		//    m_raster[startingDepth][nodeY, nodeX] = this;

		//    if (startingDepth < maxLevels)
		//    {
		//        m_children = new ZoneSpacePartitionNode[2, 2];

		//        // Bottom left
		//        m_children[SOUTH, WEST] =
		//            new ZoneSpacePartitionNode(
		//                new BoundingBox(new Vector3(m_bounds.Min.X, m_bounds.Min.Y, m_bounds.Min.Z),
		//                                new Vector3(m_bounds.Min.X + incX, m_bounds.Min.Y + incY, m_bounds.Max.Z)));

		//        // Top left
		//        m_children[NORTH, WEST] =
		//            new ZoneSpacePartitionNode(
		//                new BoundingBox(new Vector3(m_bounds.Min.X, m_bounds.Min.Y + incY, m_bounds.Min.Z),
		//                                new Vector3(m_bounds.Min.X + incX, m_bounds.Max.Y, m_bounds.Max.Z)));

		//        // Bottom right
		//        m_children[SOUTH, EAST] =
		//            new ZoneSpacePartitionNode(
		//                new BoundingBox(new Vector3(m_bounds.Min.X + incX, m_bounds.Min.Y, m_bounds.Min.Z),
		//                                new Vector3(m_bounds.Max.X, m_bounds.Min.Y + incY, m_bounds.Max.Z)));

		//        // Top right
		//        m_children[NORTH, EAST] =
		//            new ZoneSpacePartitionNode(
		//                new BoundingBox(new Vector3(m_bounds.Min.X + incX, m_bounds.Min.Y + incY, m_bounds.Min.Z),
		//                                new Vector3(m_bounds.Max.X, m_bounds.Max.Y, m_bounds.Max.Z)));



		//        // Set the next level's depth
		//        startingDepth++;

		//        int newNodesCount = nodesCountAtCurrentDepth * 2;

		//        m_children[SOUTH, WEST].PartitionSpace(maxLevels, startingDepth, nodeY * 2 + SOUTH, nodeX * 2 + WEST, newNodesCount);
		//        m_children[NORTH, WEST].PartitionSpace(maxLevels, startingDepth, nodeY * 2 + NORTH, nodeX * 2 + WEST, newNodesCount);
		//        m_children[SOUTH, EAST].PartitionSpace(maxLevels, startingDepth, nodeY * 2 + SOUTH, nodeX * 2 + EAST, newNodesCount);
		//        m_children[NORTH, EAST].PartitionSpace(maxLevels, startingDepth, nodeY * 2 + NORTH, nodeX * 2 + EAST, newNodesCount);

		//        //// Partition any further depths 
		//        //foreach (var node in m_children)
		//        //{
		//        //    node.PartitionSpace(maxLevels, startingDepth, nodesCountAtCurrentDepth * 2);
		//        //}

		//        //FindNeighbors(parent, 0, 0);
		//    }
		//    else
		//    {
		//        m_objects = new Dictionary<EntityId, WorldObject>();
		//    }
		//}





		///// <summary>
		///// Partitions the space of the zone.
		///// </summary>
		//public void PartitionSpace()
		//{
		//    // Start partitioning the region space
		//    m_root.PartitionSpace(ZoneSpacePartitionNode.DefaultPartitionThreshold, 0, 0, 0, 1);
		//}

		/// <summary>
		/// TODO: Find all intermediate neighbors
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="vertical"></param>
		/// <param name="horizontal"></param>
		internal void FindNeighbors(ZoneSpacePartitionNode parent, int vertical, int horizontal)
		{
			//m_neighbors = new ZoneSpacePartitionNode[3, 3];
			//if (parent != null)
			//{
			//    if (vertical == NORTH)
			//    {
			//        if (horizontal == EAST)
			//        {
			//            m_neighbors[VER_CENTER, HOR_WEST] = parent.m_children[NORTH, WEST];
			//            m_neighbors[VER_CENTER, HOR_WEST] = parent.m_children[NORTH, WEST];
			//        }
			//    }
			//}
		}

		//ZoneSpacePartitionNode GetNode(int horizontal, int vertical, int depth)
		//{

		//}

		#region Box-based searches

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <typeparam name="T">the specific type of the objects to retrieve</typeparam>
		/// <param name="entities">the list to append retrieved objects to</param>
		internal void GetObjectsOfSpecificType<T>(ref BoundingBox box, List<T> entities, uint phase, ref int limitCounter) where T : WorldObject
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var worldObj in m_objects.Values)
					{
						var objPos = worldObj.Position;

						if (box.Contains(ref objPos) && worldObj.IsInPhase(phase))
						{
							if (worldObj.GetType() == typeof(T))
							{
								entities.Add(worldObj as T);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref box) & IntersectionType.Touches) != 0)
						{
							node.GetObjectsOfSpecificType(ref box, entities, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
		/// <param name="box">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		internal void GetEntitiesInArea<T>(ref BoundingBox box, List<T> entities, uint phase, ref int limitCounter) where T : WorldObject
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (box.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (obj is T)
							{
								entities.Add(obj as T);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref box) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref box, entities, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
		/// <param name="box">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		internal void GetEntitiesInArea<T>(ref BoundingBox box, List<T> entities, Func<T, bool> filter, uint phase, ref int limitCounter)
			where T : WorldObject
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (box.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (obj is T)
							{
								T castedObj = obj as T;

								if (filter(castedObj))
								{
									entities.Add(castedObj);

									if (--limitCounter == 0)
										break;

									continue;
								}
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref box) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref box, entities, filter, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <param name="box">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		/// <param name="filter">the type (in respect to the WoW client) that the object must be</param>
		internal void GetEntitiesInArea(ref BoundingBox box, List<WorldObject> entities, ObjectTypes filter, uint phase, ref int limitCounter)
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (box.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if ((obj.Type & filter) != 0)
							{
								entities.Add(obj);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref box) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref box, entities, filter, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <param name="box">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		/// <param name="filter">a predicate to determin whether or not to add specific objects</param>
		internal void GetEntitiesInArea(ref BoundingBox box, List<WorldObject> entities, Func<WorldObject, bool> filter, uint phase, ref int limitCounter)
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (box.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (filter(obj))
							{
								entities.Add(obj);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref box) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref box, entities, filter, phase, ref limitCounter);
						}
					}
			}
		}

		#endregion

		#region Sphere-based searches

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <typeparam name="T">the specific type of the objects to retrieve</typeparam>
		/// <param name="entities">the list to append retrieved objects to</param>
		internal void GetObjectsOfSpecificType<T>(ref BoundingSphere sphere, List<T> entities, uint phase, ref int limitCounter) where T : WorldObject
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (sphere.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (obj.GetType() == typeof(T))
							{
								entities.Add(obj as T);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref sphere) & IntersectionType.Touches) != 0)
						{
							node.GetObjectsOfSpecificType(ref sphere, entities, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
		/// <param name="sphere">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		internal void GetEntitiesInArea<T>(ref BoundingSphere sphere, List<T> entities, uint phase, ref int limitCounter) where T : WorldObject
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (sphere.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (obj is T)
							{
								entities.Add(obj as T);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref sphere) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref sphere, entities, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <typeparam name="T">the minimum type of the objects to retrieve</typeparam>
		/// <param name="sphere">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		internal void GetEntitiesInArea<T>(ref BoundingSphere sphere, List<T> entities, Func<T, bool> filter, uint phase, ref int limitCounter)
			where T : WorldObject
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (sphere.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (obj is T)
							{
								T castedObj = obj as T;

								if (filter(castedObj))
								{
									entities.Add(castedObj);

									if (--limitCounter == 0)
										break;

									continue;
								}
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref sphere) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <param name="sphere">the area to search</param>
		/// <param name="entities">the list to append retrieved objects to</param>
		/// <param name="filter">the type (in respect to the WoW client) that the object must be</param>
		internal void GetEntitiesInArea(ref BoundingSphere sphere, List<WorldObject> entities, ObjectTypes filter, uint phase, ref int limitCounter)
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (sphere.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if ((obj.Type & filter) != 0)
							{
								entities.Add(obj);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref sphere) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Gets all objects within a specified radius.
		/// </summary>
		/// <param name="entities">the list to append retrieved objects to</param>
		/// <param name="filter">a predicate to determin whether or not to add specific objects</param>
		internal void GetEntitiesInArea(ref BoundingSphere sphere, List<WorldObject> entities, Func<WorldObject, bool> filter, uint phase, ref int limitCounter)
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (sphere.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (filter(obj))
							{
								entities.Add(obj);

								if (--limitCounter == 0)
									break;

								continue;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref sphere) & IntersectionType.Touches) != 0)
						{
							node.GetEntitiesInArea(ref sphere, entities, filter, phase, ref limitCounter);
						}
					}
			}
		}

		/// <summary>
		/// Iterates over all objects in this Node.
		/// </summary>
		/// <param name="predicate">Returns whether to continue iteration.</param>
		/// <returns>Whether Iteration was not cancelled (usually indicating that we did not find what we were looking for).</returns>
		internal bool Iterate(ref BoundingSphere sphere, Func<WorldObject, bool> predicate, uint phase)
		{
			if (IsLeaf)
			{
				if (HasObjects)
				{
					foreach (var obj in m_objects.Values)
					{
						var objPos = obj.Position;

						if (sphere.Contains(ref objPos) && obj.IsInPhase(phase))
						{
							if (!predicate(obj))
							{
								return false;
							}
						}
					}
				}
			}
			else
			{
				for (var i0 = 0; i0 < Two; i0++)
					for (var i1 = 0; i1 < Two; i1++)
					{
						var node = m_children[i0, i1];
						if ((node.Bounds.Intersects(ref sphere) & IntersectionType.Touches) != 0)
						{
							if (!node.Iterate(ref sphere, predicate, phase))
							{
								return false;
							}
						}
					}
			}
			return true;
		}
		#endregion


		#region Object Management
		/// <summary>
		/// Adds an object to the node.
		/// </summary>
		/// <param name="obj">the object to add</param>
		/// <returns>whether or not the object was added successfully</returns>
		internal bool AddObject(WorldObject obj)
		{
			if (IsLeaf)
			{
				if (m_objects.ContainsKey(obj.EntityId))
				{
					throw new ArgumentException(string.Format(
						"Tried to add Object \"{0}\" with duplicate EntityId {1} to Region.", obj, obj.EntityId));
				}

				//if (obj is Character)
				//{
				//    m_areaCharCount += 1;
				//    if (m_areaCharCount == 1)
				//    {
				//        OnActivated();
				//    }

				//    foreach (var node in m_neighbors)
				//    {
				//        if (node != null)
				//        {
				//            node.m_areaCharCount += 1;
				//            if (node.m_areaCharCount == 1)
				//            {
				//                node.OnActivated();
				//            }
				//        }
				//    }
				//}

				m_objects.Add(obj.EntityId, obj);
				obj.Node = this;
				return true;
			}

			var pos = obj.Position;

			for (var i0 = 0; i0 < Two; i0++)
				for (var i1 = 0; i1 < Two; i1++)
				{
					var node = m_children[i0, i1];
					if (node.Bounds.Contains(ref pos))
					{
						return node.AddObject(obj);
					}
				}

			return false;
		}

		/// <summary>
		/// Gets a leaf node from a given point.
		/// </summary>
		/// <param name="pt">the point to retrieve the parent node from</param>
		/// <returns>the node which contains the given point; null if the point is invalid</returns>
		internal ZoneSpacePartitionNode GetLeafFromPoint(ref Vector3 pt)
		{
			if (IsLeaf)
			{
				if (m_bounds.Contains(ref pt))
				{
					return this;
				}

				return null;
			}

			for (var i0 = 0; i0 < Two; i0++)
				for (var i1 = 0; i1 < Two; i1++)
				{
					var node = m_children[i0, i1];
					if (node.Bounds.Contains(ref pt))
					{
						return node.GetLeafFromPoint(ref pt);
					}
				}

			return null;
		}

		/// <summary>
		/// Removes an object from the node.
		/// </summary>
		/// <param name="obj">the object to remove</param>
		/// <returns>whether or not the object was removed successfully</returns>
		internal bool RemoveObject(WorldObject obj)
		{
			if (IsLeaf)
			{
				if (m_objects.Remove(obj.EntityId))
				{
					//if (obj is Character)
					//{
					//    m_areaCharCount -= 1;
					//    if (m_areaCharCount == 0)
					//    {
					//        OnDeactivated();
					//    }
					//    foreach (var node in m_neighbors)
					//    {
					//        if (node != null)
					//        {
					//            node.m_areaCharCount -= 1;
					//            if (node.m_areaCharCount == 0)
					//            {
					//                OnDeactivated();
					//            }
					//        }
					//    }
					//}
					return true;
				}

				return false;
			}

			var pos = obj.Position;

			for (var i0 = 0; i0 < Two; i0++)
				for (var i1 = 0; i1 < Two; i1++)
				{
					var node = m_children[i0, i1];
					if (node.Bounds.Contains(ref pos))
					{
						return node.RemoveObject(obj);
					}
				}

			return false;
		}

		///// <summary>
		///// Called when a new Char enters the Area
		///// </summary>
		//private void OnActivated()
		//{

		//}

		///// <summary>
		///// Called when the last Char leaves the Area
		///// </summary>
		//private void OnDeactivated()
		//{

		//}
		#endregion

		public override string ToString()
		{
			string count;
			if (IsLeaf)
			{
				count = " (" + m_objects.Count + " Objects)";
			}
			else
			{
				count = "";
			}
			return GetType().Name + count + m_bounds;
		}
	}
}