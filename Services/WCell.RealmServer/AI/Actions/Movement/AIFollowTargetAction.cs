using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIFollowTargetAction : AITargetMoveAction, IAIStateAction
    {
        public AIFollowTargetAction(Unit owner)
            : base(owner)
        {
        }

        public override void Start()
        {
            if (m_owner.Target == null)
            {
                m_owner.Say("I have no Target to follow.");
                m_owner.Brain.EnterDefaultState();
            }
            else
            {
                base.Start();
            }
        }

        public override void Stop()
        {
            m_owner.Target = null;
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }
    }
}