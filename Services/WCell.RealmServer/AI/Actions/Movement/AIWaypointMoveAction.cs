using System.Collections.Generic;
using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Util;

namespace WCell.RealmServer.AI.Actions.Movement
{
	/// <summary>
	/// Movement AI action for moving through a set of waypoints
	/// </summary>
	public class AIWaypointMoveAction : AIAction
	{
		protected LinkedList<WaypointEntry> m_waypoints;
		protected LinkedListNode<WaypointEntry> m_currentWaypoint;
		protected LinkedListNode<WaypointEntry> m_targetWaypoint;

		public AIWaypointMoveAction(Unit owner)
			: this(owner, AIMovementType.ForwardThenStop)
		{
		}

		public AIWaypointMoveAction(Unit owner, AIMovementType wpMovementType)
			: base(owner)
		{
			m_waypoints = new LinkedList<WaypointEntry>();

			m_WPmovementType = wpMovementType;
		}

		public AIWaypointMoveAction(Unit owner, AIMovementType wpMovementType,
			LinkedList<WaypointEntry> waypoints)
			: this(owner, wpMovementType)
		{
			if (waypoints == null)
			{
				m_waypoints = WaypointEntry.EmptyList;
			}
			else
			{
				m_waypoints = waypoints;
			}
		}

		/// <summary>
		/// The direction of movement on waypoints. True - moving backwards, false - moving forward
		/// Only used for AIMovementType.ForwardThenBack
		/// </summary>
		protected bool m_goingBack;

		/// <summary>
		/// Whether we are staying on a waypoint (pause)
		/// </summary>
		protected bool m_stayingOnWaypoint;

		/// <summary>
		/// When to start moving again
		/// </summary>
		protected uint m_desiredStartMovingTime;

		protected AIMovementType m_WPmovementType;

		/// <summary>
		/// Amount of Waypoints in queue
		/// </summary>
		public int Count { get { return m_waypoints.Count; } }

		public bool IsStayingOnWaypoint
		{
			get { return m_stayingOnWaypoint; }
		}

		public override void Start()
		{
			m_stayingOnWaypoint = true;
			m_desiredStartMovingTime = 0;
			m_owner.Movement.MoveType = AIMoveType.Walk;
		}

		public override void Update()
		{
			if (m_waypoints.Count == 0)
			{
				return;
			}

			if (m_stayingOnWaypoint)
			{
				if (Utility.GetSystemTime() >= m_desiredStartMovingTime)
				{
					m_targetWaypoint = GetNextWaypoint();
					MoveToTargetWaypoint();
				}
			}
			else
			{
				if (m_owner.Movement.Update())
				{
					m_currentWaypoint = m_targetWaypoint;
					if (m_currentWaypoint.Value.Orientation != 0)
					{
						m_owner.Face(m_currentWaypoint.Value.Orientation);
					}

					var waitTime = m_targetWaypoint.Value.WaitTime;

					// need to wait on this waypoint
					m_stayingOnWaypoint = true;
					m_desiredStartMovingTime = Utility.GetSystemTime() + waitTime;
				}
				//else if (m_owner.MayMove)
				//{
				//    MoveToTargetWaypoint();
				//}
			}
		}

		public override void Stop()
		{
			m_owner.Movement.Stop();
		}

		protected void MoveToTargetWaypoint()
		{
			if (m_targetWaypoint == null)
			{
				// there is no next Waypoint (usually means there are no waypoints at all)
				m_owner.Movement.MoveTo(m_owner.Position, false);
			}
			else
			{
				m_stayingOnWaypoint = false;

				m_owner.Brain.SourcePoint = m_targetWaypoint.Value.Position;
				m_owner.Movement.MoveTo(m_targetWaypoint.Value.Position, false);
			}
		}

		protected LinkedListNode<WaypointEntry> GetNextWaypoint()
		{
			if (m_waypoints.Count == 0)
				return null;

			if (m_currentWaypoint == null)
				return m_waypoints.First;

			switch (m_WPmovementType)
			{
				case AIMovementType.ForwardThenStop:
					return m_currentWaypoint.Next;

				case AIMovementType.ForwardThenBack:
					if (!m_goingBack)
					{
						if (m_currentWaypoint.Next != null)
							return m_currentWaypoint.Next;

						if (m_currentWaypoint.Previous != null)
						{
							m_goingBack = true;
							return m_currentWaypoint.Previous;
						}
					}

					if (m_goingBack)
					{
						if (m_currentWaypoint.Previous != null)
							return m_currentWaypoint.Previous;

						if (m_currentWaypoint.Next != null)
						{
							m_goingBack = false;
							return m_currentWaypoint.Next;
						}
					}

					return null;

				case AIMovementType.ForwardThenFirst:
					if (m_currentWaypoint.Next != null)
						return m_currentWaypoint.Next;

					return m_waypoints.First;
			}

			return null;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.LowPriority; }
		}
	}
}