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
			get { return m_TimeoutTicks / m_owner.Region.UpdateDelay; }
			set { m_TimeoutTicks = value / m_owner.Region.UpdateDelay; }
		}

		public int TimeoutTicks
		{
			get { return m_TimeoutTicks; }
			set { m_TimeoutTicks = value; }
		}

		public int RuntimeMillis
		{
			get { return m_RuntimeTicks / m_owner.Region.UpdateDelay; }
			set { m_RuntimeTicks = value / m_owner.Region.UpdateDelay; }
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

	public class AIMoveIntoAngleThenExecAction : AIMoveToThenExecAction
	{
		private const float ErrorMargin = MathUtil.PI/6;
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

		public override bool IsInRange(Unit target)
		{
			var angle = m_owner.GetAngleTowards(target);
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
}
