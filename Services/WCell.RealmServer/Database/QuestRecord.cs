using System;
using Castle.ActiveRecord;
using WCell.Core.Database;

namespace WCell.RealmServer.Database
{
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class QuestRecord : WCellRecord<QuestRecord>
	{
		private static readonly NHIdGenerator _idGenerator =
			new NHIdGenerator(typeof(QuestRecord), "QuestRecordId");

		/// <summary>
		/// Returns the next unique Id for a new Record
		/// </summary>
		public static long NextId()
		{
			return _idGenerator.Next();
		}

		[Field("QuestTemplateId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private long _questTemplateId;

		[Field("OwnerId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
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

		[PrimaryKey(PrimaryKeyType.Assigned)]
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

		[Property(NotNull = true)]
		public int Slot
		{
			get;
			set;
		}

		[Property(NotNull = false)]
		public DateTime? TimeUntil
		{
			get;
			set;
		}

		[Property(NotNull = false)]
		public uint[] KilledNPCs
		{
			get;
			set;
		}

		[Property(NotNull = false)]
		/// <summary>
		/// Amounts of interacted GameObjects
		/// </summary>
		public uint[] UsedGOs
		{
			get;
			set;
		}

		[Property(NotNull = false)]
		/// <summary>
		/// Spells casted
		/// </summary>
		public int[] CastedSpells
		{
			get;
			set;
		}

		[Property(NotNull = false)]
		/// <summary>
		/// Visited AreaTriggers
		/// </summary>
		public bool[] VisitedATs
		{
			get;
			set;
		}

		public static QuestRecord[] GetQuestRecordForCharacter(uint chrId)
		{
			return FindAllByProperty("_ownerId", (long)chrId);
		}
	}
}