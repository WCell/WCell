using WCell.Constants.Updates;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    /// <summary>
    /// Moves to the Target and then enters Idle mode
    /// </summary>
    public class AIMoveToTargetAction : AITargetMoveAction, IAIStateAction
    {
        public AIMoveToTargetAction(Unit owner)
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
                m_target = m_owner.Target;
                base.Start();
            }
        }

        protected override void OnArrived()
        {
            m_owner.Brain.CurrentAction = null;
            m_owner.Brain.State = BrainState.Idle;
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