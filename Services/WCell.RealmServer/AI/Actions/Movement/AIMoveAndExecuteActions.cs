using System;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveToThenExecAction : AIMoveToTargetAction
    {
        protected int m_TimeoutTicks, m_RuntimeTicks;

        public AIMoveToThenExecAction(Unit owner, UnitActionCallback actionCallback)
            : base(owner)
        {
            ActionCallback = actionCallback;
        }

        /// <summary>
        /// The Action to execute, once the Target has been approached
        /// </summary>
        public UnitActionCallback ActionCallback
        {
            get;
            set;
        }

        public int TimeoutMillis
        {
            get { return m_TimeoutTicks / m_owner.Map.UpdateDelay; }
            set { m_TimeoutTicks = value / m_owner.Map.UpdateDelay; }
        }

        public int TimeoutTicks
        {
            get { return m_TimeoutTicks; }
            set { m_TimeoutTicks = value; }
        }

        public int RuntimeMillis
        {
            get { return m_RuntimeTicks / m_owner.Map.UpdateDelay; }
            set { m_RuntimeTicks = value / m_owner.Map.UpdateDelay; }
        }

        public int RuntimeTicks
        {
            get { return m_RuntimeTicks; }
            set { m_RuntimeTicks = value; }
        }

        public override void Start()
        {
            m_RuntimeTicks = 0;
            base.Start();
        }

        public override void Update()
        {
            m_RuntimeTicks++;
            if (m_TimeoutTicks > 0 && m_RuntimeTicks >= m_TimeoutTicks)
            {
                Stop();
                OnTimeout();
            }
            else
            {
                base.Update();
            }
        }

        public override void Stop()
        {
            m_RuntimeTicks = 0;
            base.Stop();
        }

        protected override void OnArrived()
        {
            base.OnArrived();
            // Arrived, now execute:
            if (ActionCallback != null)
            {
                ActionCallback(m_owner);
            }
        }
    }

    public class AIMoveIntoRangeThenExecAction : AIMoveToThenExecAction
    {
        private SimpleRange m_Range;

        public AIMoveIntoRangeThenExecAction(Unit owner, SimpleRange range, UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            m_Range = range;
        }

        public override float DistanceMin
        {
            get { return m_Range.MinDist; }
        }

        public override float DistanceMax
        {
            get { return m_Range.MaxDist; }
        }

        public override float DesiredDistance
        {
            get { return m_Range.Average; }
        }
    }

    public class AIMoveIntoRangeOfGOThenExecAction : AIMoveToThenExecAction
    {
        private SimpleRange m_Range;
        private GameObject _gameObject;

        public AIMoveIntoRangeOfGOThenExecAction(Unit owner, GameObject go, SimpleRange range, UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            _gameObject = go;
            m_Range = range;
        }

        public override float DistanceMin
        {
            get { return m_Range.MinDist; }
        }

        public override float DistanceMax
        {
            get { return m_Range.MaxDist; }
        }

        public override float DesiredDistance
        {
            get { return m_Range.Average; }
        }

        public override void Start()
        {
            if (_gameObject == null)
            {
                log.Error("Started " + GetType().Name + " without Target set: " + m_owner);
                m_owner.Brain.EnterDefaultState();
                return;
            }
            Update();
        }

        /// <summary>
        /// Gets a preferred point, close to the current target and walks towards it
        /// </summary>
        /// <returns></returns>
        protected override void MoveToTargetPoint()
        {
            var direction = _gameObject.Position - m_owner.Position;

            if (direction == Vector3.Zero)
            {
                direction = Vector3.Right;
            }
            else
            {
                direction.Normalize();
            }

            m_owner.Movement.MoveTo(_gameObject.Position - direction * DesiredDistance);
        }

        public override void Update()
        {
            if (_gameObject == null || !_gameObject.IsInWorld)
            {
                // lost target
                OnLostTarget();
                if (_gameObject == null)
                {
                    return;
                }
            }

            if (!m_owner.Movement.Update() && !m_owner.CanMove)
            {
                return;
            }

            if (!m_owner.CanSee(_gameObject))
            {
                m_owner.Movement.Stop();
            }

            if (IsInRange(_gameObject))
            {
                OnArrived();
            }
            else if (!m_owner.IsMoving || m_owner.CheckTicks(UpdatePositionTicks))
            {
                MoveToTargetPoint();
            }
        }
    }

    public class AIMoveIntoAngleThenExecAction : AIMoveToThenExecAction
    {
        private const float ErrorMargin = MathUtil.PI / 6;
        private readonly float m_Angle;

        public AIMoveIntoAngleThenExecAction(Unit owner, float angle, UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            m_Angle = angle;
        }

        public float Angle
        {
            get { return m_Angle; }
        }

        public override bool IsInRange(WorldObject target)
        {
            var angle = Math.Abs(m_owner.Orientation - m_owner.GetAngleTowards(target));
            if (angle >= m_Angle - ErrorMargin && angle <= m_Angle + ErrorMargin)
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
            target.GetPointXY(m_Angle, bodySize + DesiredDistance, out pos);
            m_owner.Movement.MoveTo(pos);
        }
    }

    public class AIMoveToGameObjectIntoAngleThenExecAction : AIMoveToThenExecAction
    {
        private const float ErrorMargin = MathUtil.PI / 6;
        private readonly float _angle;
        private readonly GameObject _gameObject;

        public AIMoveToGameObjectIntoAngleThenExecAction(Unit owner, GameObject go, float angle, UnitActionCallback actionCallback)
            : base(owner, actionCallback)
        {
            _angle = angle;
            _gameObject = go;
        }

        public float Angle
        {
            get { return _angle; }
        }

        public override void Start()
        {
            if (_gameObject == null)
            {
                log.Error("Started " + GetType().Name + " without Target set: " + m_owner);
                m_owner.Brain.EnterDefaultState();
                return;
            }
            Update();
        }

        public override bool IsInRange(WorldObject target)
        {
            var angleTowards = Math.Abs(m_owner.Orientation - m_owner.GetAngleTowards(target));
            if (angleTowards >= _angle - ErrorMargin && angleTowards <= _angle + ErrorMargin)
            {
                return base.IsInRange(target);
            }
            return false;
        }

        protected override void MoveToTargetPoint()
        {
            Vector3 pos;
            _gameObject.GetPointXY(_angle, DesiredDistance, out pos);
            pos.Z = _gameObject.Position.Z;

            m_owner.Movement.MoveTo(pos);
        }

        public override void Update()
        {
            if (_gameObject == null || !_gameObject.IsInWorld)
            {
                // lost target
                OnLostTarget();
                if (_gameObject == null)
                {
                    return;
                }
            }

            if (!m_owner.Movement.Update() && !m_owner.CanMove)
            {
                return;
            }

            if (!m_owner.CanSee(_gameObject))
            {
                m_owner.Movement.Stop();
            }

            if (IsInRange(_gameObject))
            {
                //if (moved)
                {
                    OnArrived();
                }
            }
            else if (!m_owner.IsMoving || m_owner.CheckTicks(UpdatePositionTicks))
            {
                MoveToTargetPoint();
            }
        }
    }
}