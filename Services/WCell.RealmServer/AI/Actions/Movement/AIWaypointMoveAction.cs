using System;
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
		protected LinkedList<WaypointEntry> Waypoints;
		protected LinkedListNode<WaypointEntry> CurrentWaypoint;
		protected LinkedListNode<WaypointEntry> TargetWaypoint;


		/// <summary>
		/// The direction of movement on waypoints. True - moving backwards, false - moving forward
		/// Only used for AIMovementType.ForwardThenBack
		/// </summary>
		protected bool GoingBack;

		/// <summary>
		/// Whether we are staying on a waypoint (pause)
		/// </summary>
		protected bool StayingOnWaypoint;

		/// <summary>
		/// When to start moving again
		/// </summary>
		protected uint DesiredStartMovingTime;

		protected AIMovementType WaypointSequence;

		public AIWaypointMoveAction(Unit owner)
			: this(owner, AIMovementType.ForwardThenStop)
		{
		}

		public AIWaypointMoveAction(Unit owner, AIMovementType waypointSequence)
			: base(owner)
		{
			Waypoints = new LinkedList<WaypointEntry>();

			WaypointSequence = waypointSequence;
		}

		public AIWaypointMoveAction(Unit owner, AIMovementType waypointSequence,
			LinkedList<WaypointEntry> waypoints)
			: this(owner, waypointSequence)
		{
			Waypoints = waypoints ?? WaypointEntry.EmptyList;
		}

		/// <summary>
		/// Amount of Waypoints
		/// </summary>
		public int Count { get { return Waypoints.Count; } }

		public bool IsStayingOnWaypoint
		{
			get { return StayingOnWaypoint; }
		}

		public override void Start()
		{
			StayingOnWaypoint = true;
			DesiredStartMovingTime = 0;
			m_owner.Movement.MoveType = AIMoveType.Walk;
		}

		public override void Update()
		{
			if (Waypoints.Count == 0)
			{
				return;
			}

			if (StayingOnWaypoint)
			{
				if (Utility.GetSystemTime() >= DesiredStartMovingTime)
				{
					TargetWaypoint = GetNextWaypoint();
					MoveToTargetWaypoint();
				}
			}
			else
			{
				if (m_owner.Movement.Update())
				{
					CurrentWaypoint = TargetWaypoint;
					if (Math.Abs(CurrentWaypoint.Value.Orientation - 0) > float.Epsilon)
					{
						m_owner.Face(CurrentWaypoint.Value.Orientation);
					}

					var waitTime = TargetWaypoint.Value.WaitTime;

					// need to wait on this waypoint
					StayingOnWaypoint = true;
					DesiredStartMovingTime = Utility.GetSystemTime() + waitTime;
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
			if (TargetWaypoint == null)
			{
				// there is no next Waypoint (usually means there are no waypoints at all)
				//m_owner.Movement.MoveTo(m_owner.Position, false);
			}
			else
			{
				StayingOnWaypoint = false;

				m_owner.Brain.SourcePoint = TargetWaypoint.Value.Position;
				m_owner.Movement.MoveTo(TargetWaypoint.Value.Position, false);
			}
		}

		protected LinkedListNode<WaypointEntry> GetNextWaypoint()
		{
			if (Waypoints.Count == 0)
				return null;

			if (CurrentWaypoint == null)
				return Waypoints.First;

			switch (WaypointSequence)
			{
				case AIMovementType.ForwardThenStop:
					return CurrentWaypoint.Next;

				case AIMovementType.ForwardThenBack:
					if (!GoingBack)
					{
						if (CurrentWaypoint.Next != null)
							return CurrentWaypoint.Next;

						if (CurrentWaypoint.Previous != null)
						{
							GoingBack = true;
							return CurrentWaypoint.Previous;
						}
					}

					if (GoingBack)
					{
						if (CurrentWaypoint.Previous != null)
							return CurrentWaypoint.Previous;

						if (CurrentWaypoint.Next != null)
						{
							GoingBack = false;
							return CurrentWaypoint.Next;
						}
					}

					return null;

				case AIMovementType.ForwardThenFirst:
					if (CurrentWaypoint.Next != null)
						return CurrentWaypoint.Next;

					return Waypoints.First;
			}

			return null;
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.LowPriority; }
		}
	}
}