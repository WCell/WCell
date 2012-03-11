using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIMoveThenEnterAction : AIAction
    {
        public AIMoveThenEnterAction(Unit owner)
            : this(owner, BrainState.Idle)
        {
        }

        public AIMoveThenEnterAction(Unit owner, BrainState arrivedState)
            : base(owner)
        {
            ArrivedState = arrivedState;
        }

        /// <summary>
        /// The State to switch to, once arrived
        /// </summary>
        public BrainState ArrivedState
        {
            get;
            set;
        }

        #region Overrides of AIAction

        public override void Start()
        {
        }

        public override void Update()
        {
            if (m_owner.Movement.Update())
            {
                m_owner.Brain.State = ArrivedState;
            }
        }

        public override void Stop()
        {
            m_owner.Movement.Stop();
        }

        public override UpdatePriority Priority
        {
            get { return UpdatePriority.LowPriority; }
        }

        #endregion Overrides of AIAction
    }
}