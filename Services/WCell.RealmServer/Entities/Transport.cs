using System;
using System.Collections.Generic;
using WCell.Constants.GameObjects;
using WCell.Constants.Pathing;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.GameObjects.Spawns;
using WCell.RealmServer.Misc;
using WCell.Core.Paths;
using WCell.RealmServer.Paths;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.Transports;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	/// <summary>
	/// Ships, Zeppelins etc
	/// </summary>
	public partial class Transport : GameObject, ITransportInfo
	{
		private List<Unit> m_passengers = new List<Unit>();

		private uint m_pathTime;

		// these are all MOTransports only
		private LinkedList<TransportPathVertex> m_transportPathVertices;
		private LinkedListNode<TransportPathVertex> m_CurrentPathVertex, m_NextPathVertex;
		private MapId[] m_mapIds;

		private GOMOTransportEntry m_goTransportEntry;
		private TransportEntry m_transportEntry;

		private bool m_isMOTransport;

		public List<Unit> Passengers
		{
			get { return m_passengers; }
		}

		public MapId[] MapIds
		{
			get { return m_mapIds; }
		}

		internal protected Transport()
		{
			AnimationProgress = 255;
			m_passengers = new List<Unit>();
		}

		internal override void Init(GOEntry entry, GOSpawnEntry spawnEntry, GOSpawnPoint spawnPoint)
		{
			base.Init(entry, spawnEntry, spawnPoint);

			m_goTransportEntry = Entry as GOMOTransportEntry;
			TransportMgr.TransportEntries.TryGetValue(m_entry.GOId, out m_transportEntry);
			m_isMOTransport = m_goTransportEntry != null && m_transportEntry != null;
		}

		public void AddPassenger(Unit unit)
		{
			Passengers.Add(unit);
			unit.Transport = this;
		}

		public void RemovePassenger(Unit unit)
		{
			Passengers.Remove(unit);
			unit.Transport = null;
		}

		internal void GenerateWaypoints(TaxiPath path)
		{
			GenerateWaypoints(path, out m_mapIds);
		}

		private void GenerateWaypoints(TaxiPath path, out MapId[] mapIds)
		{
			mapIds = null;

			if (path == null)
				return;

			// Pass 1. Initialize tempVertices. Find first and last stop of the path.
			LinkedListNode<TransportPathTempVertex> pathFirstStop, pathLastStop;
			LinkedList<TransportPathTempVertex> tempVertices = GetTempVertices(path, out pathFirstStop, out pathLastStop, out mapIds);

			// Pass 2. Fill DistFromFirstStop
			FillDistFromStop(tempVertices, pathLastStop);

			// Pass 3. Fill DistToLastStop
			FillDistToStop(tempVertices, pathFirstStop);

			// Pass 4. Calculate time
			var node = tempVertices.First;

			while (node != null)
			{
				//node.Value.MoveTimeFromFirstStop = GetTimeByDistance(node.Value.DistFromFirstStop);
				//node.Value.MoveTimeToLastStop = GetTimeByDistance(node.Value.DistToLastStop);

				node = node.Next;
			}

			// Pass 5. Calculate time between neighboring vertices

			//node = tempVertices.First;
			//var prevNode = tempVertices.Last;

			//while (node != null)
			//{
			//    node.Value.MoveTimeFromPrevious = node.Value.MoveTimeFromFirstStop - prevNode.Value.MoveTimeFromFirstStop;

			//    prevNode = node;
			//    node = node.Next;
			//}

			node = tempVertices.First;
			var prevNode = tempVertices.Last;

			m_transportPathVertices = new LinkedList<TransportPathVertex>();
			uint time = 0;

			while (node != null)
			{
			    bool teleport = (node.Value.Vertex.MapId != prevNode.Value.Vertex.MapId) ||
			                    node.Value.Vertex.Flags.HasFlag(TaxiPathNodeFlags.IsTeleport);

				var pathVertice = new TransportPathVertex
				{
					Time = time,
					MapId = node.Value.Vertex.MapId,
					Position = node.Value.Vertex.Position,
					Teleport = teleport
				};

				m_transportPathVertices.AddLast(pathVertice);

				if (node.Value.MoveTimeFromFirstStop < node.Value.MoveTimeToLastStop)
					time += (uint)((Math.Abs(node.Value.MoveTimeFromFirstStop - prevNode.Value.MoveTimeFromFirstStop)) * 1000f);
				else
					time += (uint)((Math.Abs(node.Value.MoveTimeToLastStop - prevNode.Value.MoveTimeToLastStop)) * 1000f);

				time += node.Value.Vertex.Delay * 1000;

				prevNode = node;
				node = node.Next;
			}

			m_pathTime = time;
		}

		private LinkedList<TransportPathTempVertex> GetTempVertices(TaxiPath path, out LinkedListNode<TransportPathTempVertex> pathStart,
			out LinkedListNode<TransportPathTempVertex> pathStop, out MapId[] mapIds)
		{
			var mapIdsList = new List<MapId>();
			pathStop = null;
			pathStart = null;
			var node = path.Nodes.First;
			var tempVertices = new LinkedList<TransportPathTempVertex>();
			while (node != null)
			{
				if (!mapIdsList.Contains(node.Value.MapId))
					mapIdsList.Add(node.Value.MapId);

			    var tempVertice = new TransportPathTempVertex(0.0f, 0.0f, 0.0f, node.Value);
				var tempVerticeNode = tempVertices.AddLast(tempVertice);

				if (node.Value.IsStoppingPoint)
				{
					pathStop = tempVerticeNode;

					if (pathStart == null)
						pathStart = tempVerticeNode;
				}

				node = node.Next;
			}

			if (pathStart == null)
			{
				throw new Exception("TaxiPath provided does not have any stops");
			}

			mapIds = mapIdsList.ToArray();

			return tempVertices;
		}

		private void FillDistFromStop(LinkedList<TransportPathTempVertex> tempVertices, LinkedListNode<TransportPathTempVertex> pathStop)
		{
			var currentNode = pathStop;

			float distance = 0;

			while (currentNode != pathStop && currentNode != null)
			{
				if (currentNode.Value.Vertex.Flags.HasFlag(TaxiPathNodeFlags.IsTeleport))
					distance = 0;
				else
					distance += currentNode.Value.Vertex.DistFromPrevious;

				currentNode.Value.DistFromFirstStop = distance;

				currentNode = currentNode.Next ?? tempVertices.First;
			}
		}

		private void FillDistToStop(LinkedList<TransportPathTempVertex> tempVertices, LinkedListNode<TransportPathTempVertex> pathStart)
		{
			var currentNode = pathStart;

			float distance = 0;

			while (currentNode != pathStart && currentNode != null)
			{
				var nextNode = currentNode.Next ?? tempVertices.First;

				distance += nextNode.Value.Vertex.DistFromPrevious;

				currentNode.Value.DistToLastStop = distance;

				if (currentNode.Value.Vertex.Flags.HasFlag(TaxiPathNodeFlags.IsTeleport))
					distance = 0;

				currentNode = currentNode.Previous ?? tempVertices.Last;
			}
		}


		public override void Update(int dt)
		{
			base.Update(dt);

			// no transports moving across maps
			//if (m_transportPathVertices == null || m_transportPathVertices.Count <= 1 || m_mapIds.Length > 1)
			//    return;

			if (!m_isMOTransport)
			{
				return;
			}

			//uint time = Utility.GetSystemTime() % m_pathTime;

			//while ((time - m_CurrentPathVertex.Value.Time) % m_pathTime >
			//       (m_NextPathVertex.Value.Time - m_CurrentPathVertex.Value.Time) % m_pathTime)
			//{
			//    AdvanceByOneWaypoint();

			//    MoveTransport(m_CurrentPathVertex.Value.MapId, m_CurrentPathVertex.Value.Position);
			//}
		}

		private void AdvanceByOneWaypoint()
		{
			m_CurrentPathVertex = m_NextPathVertex;

			m_NextPathVertex = m_NextPathVertex.Next ?? m_transportPathVertices.First;
		}

		private void MoveTransport(MapId mapId, Vector3 position)
		{
			var newMapId = m_CurrentPathVertex.Value.MapId;

			if (newMapId != Map.Id || m_CurrentPathVertex.Value.Teleport)
			{
				//foreach (Unit unit in Passengers)
				//{
				//    if (unit is Character)
				//    {
				//        Character chr = (Character) unit;
				//        if (!chr.IsAlive && !chr.IsGhost)
				//        {
				//            chr.Resurrect();
				//        }
				//    }

				//    unit.TeleportTo(newMapId, m_currentPathVertex.Value.Position); // TODO: not leave transport
				//}
			}
			else
			{
				m_position = m_CurrentPathVertex.Value.Position;
			}
		}
	}

	class TransportPathTempVertex
	{
        public TransportPathTempVertex(float fromFirstStop, float toLastStop,
            float fromPrevious, PathVertex v)
        {
            MoveTimeFromFirstStop = fromFirstStop;
            MoveTimeToLastStop = toLastStop;
            MoveTimeFromPrevious = fromPrevious;
            Vertex = v;
        }

	    public PathVertex Vertex;

		public float DistFromFirstStop, DistToLastStop;

		public float MoveTimeFromFirstStop, MoveTimeToLastStop, MoveTimeFromPrevious;
	}
}