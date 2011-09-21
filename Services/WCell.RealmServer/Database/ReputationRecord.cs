using Castle.ActiveRecord;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Database
{
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class ReputationRecord : ActiveRecordBase<ReputationRecord>
	{
	    [PrimaryKey(PrimaryKeyType.Native)]
		public long RecordId
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public long OwnerId
		{
			get;
			set;
		}

        [Property(NotNull = true)]
        public FactionReputationIndex ReputationIndex
        {
            get; 
            set;
        }

		[Property]
		public int Value
		{
			get;
			set;
		}

        [Property(NotNull = true)]
        public ReputationFlags Flags
        {
            get; 
            set;
        }

	    public static ReputationRecord[] Load(long chrRecordId)
		{
			return FindAllByProperty("OwnerId", chrRecordId);
		}
	}
}