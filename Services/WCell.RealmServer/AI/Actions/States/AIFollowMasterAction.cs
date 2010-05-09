using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.Movement;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
	public class AIFollowMasterAction : AITargetMoveAction, IAIStateAction
	{
		public AIFollowMasterAction(Unit owner) : base(owner)
		{
		}

		public override void Start()
		{
			if (!m_owner.HasMaster)
			{
				m_owner.Say("I have no Master to follow.");
				m_owner.Brain.EnterDefaultState();
			}
			else
			{
				m_owner.Target = Target = m_owner.Master;
				base.Start();
			}
		}

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.LowPriority; }
		}
	}
}