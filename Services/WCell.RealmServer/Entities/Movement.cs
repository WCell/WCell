using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.Handlers;
using WCell.Util;
using WCell.Constants.NPCs;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
	public class Movement
	{
		private const float SPEED_FACTOR = 0.001f;

		//public static int MoveUpdateDelay = ;

		/// <summary>
		/// Starting time of movement
		/// </summary>
		protected uint m_lastMoveTime;

		/// <summary>
		/// Total time of movement
		/// </summary>
		protected uint m_totalMovingTime;

		/// <summary>
		/// Time at which movement should end
		/// </summary>
		protected uint m_desiredEndMovingTime;

		/// <summary>
		/// The target of the current (or last) travel
		/// </summary>
		protected Vector3 m_destination;

		protected Vector3[] _currentPath;
		protected int _currentPathIndex;

		/// <summary>
		/// The movement type (walking, running or flying)
		/// </summary>
		protected AIMoveType m_moveType;

		protected internal Unit m_owner;

		protected bool m_moving, m_MayMove;

		protected PathQuery m_currentQuery;

		#region Constructors

		public Movement(Unit owner)
			: this(owner, AIMoveType.Run)
		{
		}

		public Movement(Unit owner, AIMoveType moveType)
		{
			m_owner = owner;
			m_moveType = moveType;
			m_MayMove = true;
		}

		#endregion

		#region Properties

		public Vector3 Destination
		{
			get { return m_destination; }
		}

		/// <summary>
		/// AI-controlled Movement setting
		/// </summary>
		public bool MayMove
		{
			get { return m_MayMove; }
			set { m_MayMove = value; }
		}

		public bool IsMoving
		{
			get { return m_moving; }
		}

		/// <summary>
		/// Whether the owner is within 1 yard of the Destination
		/// </summary>
		public bool IsAtDestination
		{
			get
			{
				return m_owner.Position.DistanceSquared(ref m_destination) < 1f;
			}
		}

		/// <summary>
		/// Get movement flags for the packet
		/// </summary>
		/// <returns></returns>
		public virtual MonsterMoveFlags MovementFlags
		{
			get
			{
				MonsterMoveFlags moveFlags;

				switch (m_moveType)
				{
					case AIMoveType.Fly:
						moveFlags = MonsterMoveFlags.Fly;
						break;
					case AIMoveType.Run:
					case AIMoveType.Walk:
						moveFlags = MonsterMoveFlags.Walk;
						break;
					case AIMoveType.Sprint:
						moveFlags = MonsterMoveFlags.DefaultMask;
						break;
					default:
						moveFlags = MonsterMoveFlags.DefaultMask;
						break;
				}

				return moveFlags;
			}
		}

		public AIMoveType MoveType
		{
			get
			{
				return m_moveType;
			}
			set
			{
				m_moveType = value;
				//if (!IsAtDestination)
				//{
				//    // re-compute route
				//    MoveToDestination();
				//}
			}
		}

		/// <summary>
		/// Remaining movement time to current Destination (in millis)
		/// </summary>
		public uint RemainingTime
		{
			get
			{
				float speed, distanceToTarget;

				if (m_owner.IsFlying)
				{
					speed = m_owner.FlightSpeed;
					distanceToTarget = m_owner.GetDistance(ref m_destination);
				}
				else if (m_moveType == AIMoveType.Run)
				{
					speed = m_owner.RunSpeed;
					distanceToTarget = m_owner.GetDistanceXY(ref m_destination);
				}
				else // walk
				{
					speed = m_owner.WalkSpeed;
					distanceToTarget = m_owner.GetDistanceXY(ref m_destination);
				}

				speed *= SPEED_FACTOR;

				return (uint)(distanceToTarget / speed);
			}
		}

		#endregion

		#region MoveTo / Update / Stop
		/// <summary>
		/// Starts the MovementAI
		/// </summary>
		/// <returns>Whether already arrived</returns>
		public bool MoveTo(Vector3 destination, bool findPath = true)
		{
			m_destination = destination;
			if (IsAtDestination)
			{
				return true;
			}

			if (findPath)
			{
				m_currentQuery = new PathQuery(m_owner.Position, ref destination, m_owner.ContextHandler, OnPathQueryReply);

				// TODO: Consider flying units & liquid levels
				m_owner.Map.Terrain.FindPath(m_currentQuery);
			}
			else if (m_owner.CanMove)
			{
				// start moving
				MoveToDestination();
			}
			// cannot move
			return false;
		}

		/// <summary>
		/// Interpolates the current Position
		/// </summary>
		/// <returns>Whether we arrived</returns>
		public bool Update()
		{
			if (!m_moving)
			{
				// is not moving
				return false;
			}

			if (!MayMove)
			{
				// cannot continue moving
				Stop();
				return false;
			}

			if (UpdatePosition())
			{
				// arrived
				return true;
			}

			// still going
			return false;
		}

		/// <summary>
		/// Stops at the current position
		/// </summary>
		public void Stop()
		{
			if (m_moving)
			{
				UpdatePosition();
				MovementHandler.SendStopMovementPacket(m_owner);
				m_moving = false;
			}
		}

		#endregion

		/// <summary>
		/// Starts moving to current Destination
		/// </summary>
		/// <remarks>Sends movement packet to client</remarks>
		protected void MoveToDestination()
		{
			m_moving = true;

			m_totalMovingTime = RemainingTime;
			m_owner.SetOrientationTowards(ref m_destination);
			MovementHandler.SendMoveToPacket(m_owner, ref m_destination, 0f, m_totalMovingTime, MovementFlags);

			m_lastMoveTime = Utility.GetSystemTime();
			m_desiredEndMovingTime = m_lastMoveTime + m_totalMovingTime;
		}

		protected void OnPathQueryReply(PathQuery query)
		{
			if (query != m_currentQuery)
			{
				// deprecated query
				return;
			}
			m_currentQuery = null;

			if (query.Path != null)
			{
				_currentPathIndex = 0;
				_currentPath = query.Path;

				m_destination = _currentPath[_currentPathIndex];
			}
			MoveToDestination();
		}

		/// <summary>
		/// Updates position of unit
		/// </summary>
		/// <returns>true if target point is reached</returns>
		protected bool UpdatePosition()
		{
			var currentTime = Utility.GetSystemTime();

			// ratio between time passed since last update and total movement time
			var delta = (currentTime - m_lastMoveTime)/(float) m_totalMovingTime;

			// if the ratio is more than 1, then we have reached the target
			if (currentTime >= m_desiredEndMovingTime || delta >= 1f)
			{
				// move target directly to the destination
				m_owner.Map.MoveObject(m_owner, ref m_destination);
				if (_currentPath != null)
				{
					if (++_currentPathIndex < _currentPath.Length - 1)
					{
						// go to next destination
						m_destination = _currentPath[_currentPathIndex];
						MoveToDestination();
					}
					else
					{
						_currentPath = null;
					}
					return false;
				}
				else
				{
					m_moving = false;
					return true;
				}
			}

			// otherwise we've passed delta part of the path
			var currentPos = m_owner.Position;
			var newPosition = currentPos + (m_destination - currentPos)*delta;

			m_lastMoveTime = currentTime;
			m_owner.Map.MoveObject(m_owner, ref newPosition);
			return false;
		}
	}
}