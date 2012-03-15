using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.States
{
    public class AIIdleAction : AIAction, IAIStateAction
    {
        public AIIdleAction(Unit owner) : base(owner) { }

        public override void Start()
        {
            if (m_owner.IsAlive)
            {
                // make sure we don't have Target nor Attacker
                m_owner.FirstAttacker = null;
            }
            m_owner.Target = null;
        }

        public override void Update()
        {
        }

        public override void Stop()
        {
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.Active; }
        }
    }
}