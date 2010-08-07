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
	/// <summary>
	/// Represents the progress in one criterion of one Achievement.
	/// One Achievement can have many criteria.
	/// </summary>
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class AchievementProgressRecord : WCellRecord<AchievementProgressRecord>
    {
		#region Static
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Creates a new AchievementProgressRecord row in the database with the given information.
		/// </summary>
		/// <param name="account">the account this character is on</param>
		/// <param name="name">the name of the new character</param>
		/// <returns>the <seealso cref="AchievementProgressRecord"/> object</returns>
		public static AchievementProgressRecord CreateAchievementProgressRecord(Character chr, AchievementCriteriaId achievementCriteriaId, uint counter)
		{
			AchievementProgressRecord record;

			try
			{
				record = new AchievementProgressRecord
				{
					_characterGuid = (int)chr.EntityId.Low,
					_achievementCriteriaId = (int)achievementCriteriaId,
					_counter = (int)counter,
					StartOrUpdateTime = DateTime.Now,
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

		/// <summary>
		/// Encode char id and achievement id into RecordId
		/// </summary>
		[PrimaryKey(PrimaryKeyType.Assigned)]
		public long RecordId
		{
			get
			{
				return _characterGuid | ((long)_achievementCriteriaId << 32);
			}
			set
			{
				_characterGuid = (int)value;
				_achievementCriteriaId = (int)(value >> 32);
			}
		}

		[Field("CharacterId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _characterGuid;

		[Field("Criteria", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _achievementCriteriaId;

		[Field("Counter", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _counter;

		/// <summary>
		/// The time when this record was inserted or last updated (depends on the kind of criterion)
		/// </summary>
		[Property]
		public DateTime StartOrUpdateTime { get; set; }

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

		public static AchievementProgressRecord[] Load(int chrRecordId)
		{
			return FindAll(Restrictions.Eq("_characterGuid", chrRecordId));
		}


    }
}
