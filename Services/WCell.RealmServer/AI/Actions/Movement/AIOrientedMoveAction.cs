using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    /// <summary>
    /// Lets the owner stay in a specific angle and distance towards a Target
    /// </summary>
    public class AIOrientedTargetMoveAction : AITargetMoveAction
    {
        public AIOrientedTargetMoveAction(Unit owner)
            : base(owner)
        {
        }

        /// <summary>
        /// Whether the Owner should have the same orientation as the Target
        /// </summary>
        public bool SameOrientation
        {
            get;
            set;
        }

        public float MinAngle
        {
            get;
            set;
        }

        public float MaxAngle
        {
            get;
            set;
        }

        public float DesiredAngle
        {
            get
            {
                return MinAngle + ((MaxAngle - MinAngle) / 2);
            }
        }

        public override void Start()
        {
            if (m_target == null)
            {
                m_target = m_owner.Target;
                if (m_target == null)
                {
                    log.Error(GetType().Name + " is being started without a Target set: " + m_owner);
                    m_owner.Brain.EnterDefaultState();
                }
            }
        }

        protected override void OnArrived()
        {
            base.OnArrived();
            if (SameOrientation)
            {
                m_owner.Face(m_target.Orientation);
            }
        }

        public override bool IsInRange(WorldObject target)
        {
            var angle = m_owner.GetAngleTowards(target);
            if (angle >= MinAngle && angle <= MaxAngle)
            {
                return base.IsInRange(target);
            }
            return false;
        }

        protected override void MoveToTargetPoint()
        {
            var target = m_owner.Target;
            var bodySize = target.BoundingRadius + m_owner.BoundingRadius;

            Vector3 pos;
            target.GetPointXY(DesiredAngle, bodySize + DesiredDistance, out pos);

            m_owner.Movement.MoveTo(pos);
        }

        /// <summary>
        /// Creates a Movement action that lets the owner stay behind its Target
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static AIOrientedTargetMoveAction CreateStayBehindAction(Unit owner)
        {
            return new AIOrientedTargetMoveAction(owner) { MinAngle = WorldObject.BehindAngleMin, MaxAngle = WorldObject.BehindAngleMax };
        }

        /// <summary>
        /// Creates a Movement action that lets the owner stay in front of its Target
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static AIOrientedTargetMoveAction CreateStayInFrontAction(Unit owner)
        {
            return new AIOrientedTargetMoveAction(owner) { MinAngle = WorldObject.InFrontAngleMin, MaxAngle = WorldObject.InFrontAngleMax };
        }
    }
}