using System.Collections.Generic;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.AI.Brains
{
	public class TestBrain : BaseBrain
	{
		//public TestBrain(NPC owner, IAIActionCollectionFactory collectionFactory, BrainState defaultState)
		//    : base(owner, collectionFactory, defaultState)
		//{
		//}

		private LinkedList<WaypointEntry> m_waypoints;

		public TestBrain(NPC owner) : base(owner)
		{
			var spawn = owner.SpawnPoint;
			if (spawn != null)
			{
				var spawnEntry = spawn.SpawnEntry;

				if (spawnEntry != null)
				{
					m_waypoints = spawnEntry.Waypoints;
				}
			}
		}

		private LinkedListNode<WaypointEntry> m_cur;
		private bool m_moveForward = true;

		public void GoToNextWP()
		{
			if (m_waypoints == null || m_waypoints.Count < 2)
				return;

			if (m_cur == null)
			{
				m_cur = m_waypoints.First;
			}
			else
			{
				m_cur = m_cur.Next;
			}

			if (m_moveForward)
			{
				m_owner.Movement.MoveTo(m_cur.Value.Position, false);

				if (m_cur.Next == null)
				{
					m_moveForward = false;
				}
			}
		}

		public int WaypointsCount()
		{
			return m_waypoints.Count;
		}

		public void GoToWaypoint(WaypointEntry waypointEntry)
		{
			m_owner.Movement.MoveTo(waypointEntry.Position, false);
		}

		public void EnqueueAllWaypoints()
		{
			foreach (var wp in m_waypoints)
			{
				GoToWaypoint(wp);
			}
		}
	}
}