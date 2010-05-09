using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Paths;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Transports
{
	public class TransportMovement
	{
		private GOMOTransportEntry m_entry;
        //private uint m_period;

        //private MapId[] m_mapIds;
		private LinkedListNode<PathVertex>[] m_stops;
		//private List<uint> m_accelerationVertexIds, m_decelerationVertexIds;

        //private bool m_ready;

		public TransportMovement(GOMOTransportEntry entry, uint period)
		{
			m_entry = entry;
			//m_period = period;
			Initialize();
		}

		public MapId GetMap(int time)
		{
			return MapId.EasternKingdoms;
		}

		public Vector3 GetPosition(int time)
		{
			return Vector3.Zero;
		}

		private void Initialize()
		{
			InitializeConstants();

			var path = m_entry.Path;

			FindStopsAndMapIds(path.Nodes.First);

			if (m_stops.Length != 2)
			{
				// Northrend transports with many stops
				// Deal with them later
				return;
			}

			var startNode = m_stops[0];
			var stopNode = m_stops[1];

			AppendAccelerationNodes(startNode);
			//AppendDecelerationNodes(stopNode);
		}

		private void FindStopsAndMapIds(LinkedListNode<PathVertex> node)
		{
			var mapIdsList = new List<MapId>();
			var stopsList = new List<LinkedListNode<PathVertex>>();

			while (node != null)
			{
				if (!mapIdsList.Contains(node.Value.MapId))
					mapIdsList.Add(node.Value.MapId);

				if (node.Value.IsStoppingPoint)
				{
					stopsList.Add(node);
				}

				node = node.Next;
			}

			//m_mapIds = mapIdsList.ToArray();
			m_stops = stopsList.ToArray();
		}

		private void AppendAccelerationNodes(LinkedListNode<PathVertex> stopVertexNode)
        {
            // TODO: ...
			//var stopVertex = stopVertexNode.Value;

			//var node = stopVertexNode;
			//uint time = 0;

			//while (time <= m_accelerationTime)
			//{
			//    m_accelerationVertexIds.Add(node.Value.Id);

			//    var nextNode = node.Next ?? node.List.First;
			//}
		}

		private void InitializeConstants()
		{
			m_accelerationRate = m_entry.AccelRate;
			m_moveSpeed = m_entry.MoveSpeed;
			m_accelerationTime = m_moveSpeed / m_accelerationRate;
			m_accelerationDist = m_accelerationRate * m_accelerationTime * m_accelerationTime * 0.5f;
		}

		private float m_accelerationRate;
		private float m_moveSpeed;

		private float m_accelerationTime;
		private float m_accelerationDist;

		private float GetTimeByDistance(float distance)
		{
			if (distance < m_accelerationDist)
				return (float)Math.Sqrt(2 * distance / m_accelerationRate);

			return (distance - m_accelerationDist) / m_moveSpeed + m_accelerationTime;
		}

		private float GetDistanceByTime(float time)
		{
			if (time <= m_accelerationTime)
				return m_accelerationRate * time * time * 0.5f;

			return (time - m_accelerationTime) * m_moveSpeed + m_accelerationDist;
		}
	}
}
