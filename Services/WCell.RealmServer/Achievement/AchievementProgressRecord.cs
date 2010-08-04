using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Constants.Achievements;
using WCell.Core.Database;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievement
{
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class AchievementProgressRecord : WCellRecord<AchievementProgressRecord>
    {
		#region Static
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
		private static readonly Order CreatedOrder = new Order("Created", true);

		/// <summary>
		/// Character will not have Ids below this threshold. 
		/// You can use those unused ids for self-implemented mechanisms, eg to fake participants in chat-channels etc.
		/// </summary>
		/// <remarks>
		/// Do not change this value once the first Character exists.
		/// If you want to change this value to reserve more (or less) ids for other use, make sure
		/// that none of the ids below this threshold are in the DB.
		/// </remarks>

		protected static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(AchievementRecord), "RecordId");

		/// <summary>
		/// Creates a new AchievementRecord row in the database with the given information.
		/// </summary>
		/// <param name="account">the account this character is on</param>
		/// <param name="name">the name of the new character</param>
		/// <returns>the <seealso cref="AchievementRecord"/> object</returns>
		public static AchievementProgressRecord CreateAchievementProgressRecord(Character chr, AchievementCriteriaId achievementCriteriaId, uint counter)
		{
			AchievementProgressRecord record;

			try
			{
				record = new AchievementProgressRecord
				{
					RecordId = _idGenerator.Next(),
					_characterGuid = (int)chr.EntityId.Low,
					_achievementCriteriaId = (int)achievementCriteriaId,
					_counter = (int)counter,
					Date = DateTime.Now,
					New = true
				};
			}
			catch (Exception ex)
			{
				s_log.Error("AchievementProgressRecord creation error (DBS: " + RealmServerConfiguration.DBType + "): ", ex);
				record = null;
			}


			return record;
		}

		#endregion

		[PrimaryKey(PrimaryKeyType.Assigned)]
		public long RecordId { get; set; }

		[Field("Guid", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _characterGuid;

		[Field("Criteria", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _achievementCriteriaId;

		[Field("Counter", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _counter;

		[Property]
		public DateTime Date { get; set; }


		public uint CharacterGuid
		{
			get { return (uint)_characterGuid; }
			set { _characterGuid = (int)value; }
		}

		public AchievementCriteriaId AchievementCriteriaId
		{
			get { return (AchievementCriteriaId)_achievementCriteriaId; }
			set { _achievementCriteriaId = (int)value; }
		}

		public uint Counter
		{
			get { return (uint)_counter; }
			set { _counter = (int)value; }
		}

		public static AchievementProgressRecord[] Load(long chrRecordId)
		{
			return FindAllByProperty("Guid", chrRecordId);
		}


    }
}
