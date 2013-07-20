using System;
using System.Collections.Generic;

namespace WCell.RealmServer.Database.Entities
{
	public class QuestRecord
	{
		//private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(QuestRecord), "QuestRecordId");

		/// <summary>
		/// Returns the next unique Id for a new Record
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		//[Field("QuestTemplateId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _questTemplateId;

		//[Field("OwnerId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _ownerId;

		public QuestRecord(uint qId, uint ownerId)
		{
			QuestTemplateId = qId;
			_ownerId = ownerId;
			QuestRecordId = NextId();
		}

		public QuestRecord()
		{
		}

		//[PrimaryKey(PrimaryKeyType.Assigned)]
		public long QuestRecordId
		{
			get;
			set;
		}

		public uint OwnerId
		{
			get { return (uint)_ownerId; }
			set { _ownerId = value; }
		}

		public uint QuestTemplateId
		{
			get { return (uint)_questTemplateId; }
			set { _questTemplateId = value; }
		}

		//[Property(NotNull = true)]
		public int Slot
		{
			get;
			set;
		}

		//[Property(NotNull = false)]
		public DateTime? TimeUntil
		{
			get;
			set;
		}

		//[Property(NotNull = false)]
		/// <summary>
		/// Amounts of interactions
		/// </summary>
		public uint[] Interactions
		{
			get;
			set;
		}

		//[Property(NotNull = false)]
		/// <summary>
		/// Visited AreaTriggers
		/// </summary>
		public bool[] VisitedATs
		{
			get;
			set;
		}

		public static IEnumerable<QuestRecord> GetQuestRecordForCharacter(uint chrId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<QuestRecord>(x => x.OwnerId == (long)chrId);
		}
	}
}