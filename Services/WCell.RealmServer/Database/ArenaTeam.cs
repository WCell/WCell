using System;
using Castle.ActiveRecord;
using WCell.Core.Database;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.ArenaTeams
{
	[ActiveRecord("ArenaTeam", Access = PropertyAccess.Property)]
	public partial class ArenaTeam
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
    }
}
