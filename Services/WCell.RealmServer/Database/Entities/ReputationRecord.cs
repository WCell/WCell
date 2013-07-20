using System;
using System.Collections.Generic;
using WCell.Constants.Factions;
using WCell.Database;

namespace WCell.RealmServer.Database.Entities
{
	public class ReputationRecord
	{
		public long RecordId
		{
			get;
			set;
		}

		public long OwnerId
		{
			get;
			set;
		}

        public FactionReputationIndex ReputationIndex
        {
            get; 
            set;
        }

		public int Value
		{
			get;
			set;
		}

        public ReputationFlags Flags
        {
            get; 
            set;
        }

	    public static IEnumerable<ReputationRecord> Load(long chrRecordId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<ReputationRecord>(x => x.OwnerId == chrRecordId);
		}
	}
}