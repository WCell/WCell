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

		public override UpdatePriority Priority
		{
			get { return UpdatePriority.LowPriority; }
		}
	}
}
