using System;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.ArenaTeams
{
	[ActiveRecord("ArenaTeam", Access = PropertyAccess.Property)]
	public partial class ArenaTeam : WCellRecord<ArenaTeam>
	{
        private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(ArenaTeam), "_id");

        [PrimaryKey(PrimaryKeyType.Assigned, "Id")]
        private long _id
        {
            get;
            set;
        }

        [Field("Name", NotNull = true, Unique = true)]
        private string _name;

        [Field("LeaderLowId", NotNull = true)]
        private int _leaderLowId;

        public uint LeaderLowId
        {
            get { return (uint)_leaderLowId; }
        }

        [Field("Type", NotNull = true)]
        private int _type;

        public override void Save()
        {
            base.Update();
        }

        public override void SaveAndFlush()
        {
            UpdateAndFlush();
        }

        public void UpdateLater()
        {
            RealmServer.Instance.AddMessage(Update);
        }

        public void UpdateLater(Action triggerAction)
        {
            triggerAction();
            RealmServer.Instance.AddMessage(Update);
        }

        public override void Create()
        {
            try
            {
                base.Create();
                OnCreate();
            }
            catch (Exception e)
            {
                RealmDBUtil.OnDBError(e);
            }
        }

        public override void CreateAndFlush()
        {
            try
            {
                base.CreateAndFlush();
                OnCreate();
            }
            catch (Exception e)
            {
                RealmDBUtil.OnDBError(e);
            }
        }

        private void OnCreate()
        {
            m_syncRoot.Enter();
            try
            {
                foreach (var member in Members.Values)
                {
                    member.Create();
                }
                Stats.Create();
            }
            finally
            {
                m_syncRoot.Exit();
            }
        }

        protected override void OnDelete()
        {
            base.OnDelete();
        }
    }
}
