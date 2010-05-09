using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using WCell.Constants.Talents;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Database
{
	public interface ITalentRecord
	{
		int TalentGroupId
		{
			get;
			set;
		}

		TalentId TalentId
		{
			get;
		}

		TalentEntry Entry
		{ 
			get;
		}

		int Rank
		{
			get;
		}
	}

	//[ActiveRecord("TalentRecords", Access = PropertyAccess.Property)]
	public class TalentRecord : // ActiveRecordBase<TalentRecord>,
		ITalentRecord
	{
		[PrimaryKey(PrimaryKeyType.Increment)]
		private long Id
		{
			get;
			set;
		}

		//[BelongsTo]
		public SpecProfile SpecProfile
		{
			get;
			set;
		}

		[Property]
		public int TalentGroupId
		{
			get;
			set;
		}

		[Field]
		private int talentId;

		public TalentId TalentId
		{
			get { return (TalentId)talentId; }
			set { talentId = (int)value; }
		}

		[Property]
		public int Rank
		{
			get;
			set;
		}

		public TalentEntry Entry
		{
			get { return TalentMgr.GetEntry(TalentId); }
		}

		private TalentRecord()
		{
		}

		public static ITalentRecord NewTalentRecord(SpecProfile profile, int talentGroupId, TalentId talentId, int rank)
		{
			var newRecord = new TalentRecord()
			{
				SpecProfile = profile,
				TalentGroupId = talentGroupId,
				TalentId = talentId,
				Rank = rank
			};

			// TODO: Only save in DB-queue context!
			//newRecord.CreateAndFlush();

			return newRecord;
		}
	}
}
