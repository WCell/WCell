using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
	public class AIGuardMasterAction : AIFollowMasterAction, IAIStateAction
	{
		public AIGuardMasterAction(Unit owner) : base(owner)
		{
		}

		public override void Update()
		{
			if (!m_owner.Brain.CheckCombat())
			{
				base.Update();
			}
		}

		protected override void OnLostTarget()
		{
			m_target = m_owner.Master;
			if (m_target == null)
			{
				//log.Warn(GetType().Name + ": " + m_owner + " has no Master.");
				Stop();
				m_owner.Brain.EnterDefaultState();
				return;
			}
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.LowPriority; }
		}
	}
}