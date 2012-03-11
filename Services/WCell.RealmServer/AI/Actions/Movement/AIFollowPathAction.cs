using WCell.Constants.NPCs;
using WCell.Constants.Updates;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Actions.Movement
{
    public class AIFollowPathAction : AIAction
    {
        private Path _path;

        public AIFollowPathAction(Unit owner, Path path = null, AIMoveType moveType = AIMoveType.Walk)
            : base(owner)
        {
            Path = path;
            MoveType = moveType;
        }

        public Path Path
        {
            get { return _path; }
            set
            {
                _path = value;
            }
        }

        public AIMoveType MoveType { get; set; }

        private void MoveToNext()
        {
            var p = _path.Next();
            m_owner.Brain.SourcePoint = p;
            m_owner.Movement.MoveTo(p, false);
        }

        public override void Start()
        {
            m_owner.Movement.MoveType = MoveType;
            MoveToNext();
        }

        public override void Update()
        {
            if (Path == null)
            {
                return;
            }

            if (m_owner.Movement.Update() && _path.HasNext())
            {
                // next node in the path
                MoveToNext();
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
    }
}