using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NHibernate.Criterion;
using WCell.Util;

namespace WCell.RealmServer.Database.Entities
{
	public class QuestRecord
	{
		//private static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(QuestRecord), "QuestRecordId");

		/// <summary>
		/// Returns the next unique Id for a new Record
		/// </summary>
		//public static long NextId()
		//{
		//	return _idGenerator.Next();
		//}

		//[Field("QuestTemplateId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		//private long _questTemplateId;

		//[Field("OwnerId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		//private long _ownerId;

		private static bool _idGeneratorInitialised;
		private static long _highestId;

		private static void Init()
		{
			//long highestId;
			try
			{
				QuestRecord highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<QuestRecord>().OrderBy(record => record.QuestRecordId).Desc.Take(1).SingleOrDefault();
				_highestId = highestItem != null ? highestItem.QuestRecordId : 0;
				//_highestId = RealmWorldDBMgr.DatabaseProvider.Query<QuestRecord>().Max(questRecord => questRecord.QuestRecordId);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				QuestRecord highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<QuestRecord>().OrderBy(record => record.QuestRecordId).Desc.Take(1).SingleOrDefault();
				_highestId = highestItem != null ? highestItem.QuestRecordId : 0;
			}

			//_highestId = (long)Convert.ChangeType(highestId, typeof(long));

			_idGeneratorInitialised = true;
		}

		/// <summary>
		/// Returns the next unique Id for a new Item
		/// </summary>
		public static long NextId()
		{
			if (!_idGeneratorInitialised)
				Init();

			return Interlocked.Increment(ref _highestId);
		}

		public static long LastId
		{
			get
			{
				if (!_idGeneratorInitialised)
					Init();
				return Interlocked.Read(ref _highestId);
			}
		}

		public QuestRecord(uint qId, uint ownerId)
		{
			QuestTemplateId = qId;
			//_ownerId = ownerId;
			OwnerId = ownerId;
			QuestRecordId = NextId();
		}

		public QuestRecord()
		{
		}

		//[PrimaryKey(PrimaryKeyType.Assigned)]
		public long QuestRecordId;

		public uint OwnerId;

		public uint QuestTemplateId;

		//[Property(NotNull = true)]
		public int Slot;

		//[Property(NotNull = false)]
		public DateTime? TimeUntil;

		//[Property(NotNull = false)]
		/// <summary>
		/// Amounts of interactions
		/// </summary>
		public uint[] Interactions;

		//[Property(NotNull = false)]
		/// <summary>
		/// Visited AreaTriggers
		/// </summary>
		public bool[] VisitedATs;

		public static IEnumerable<QuestRecord> GetQuestRecordForCharacter(uint chrId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<QuestRecord>(x => x.OwnerId == (long)chrId); // TODO: Find out why this cast is required for comparing 2 uints
		}
	}
}