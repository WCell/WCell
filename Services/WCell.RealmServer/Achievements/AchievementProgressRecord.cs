using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Achievements
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
		public static AchievementProgressRecord CreateAchievementProgressRecord(Character chr, uint achievementCriteriaId, uint counter)
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
					State = RecordState.New
				};
			}
			catch (Exception ex)
			{
				s_log.Error("AchievementProgressRecord creation error (DBS: " + RealmServerConfiguration.DatabaseType + "): ", ex);
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
				return Utility.MakeLong(_characterGuid, _achievementCriteriaId);
			}
			set
			{
                Utility.UnpackLong(value, ref _characterGuid, ref _achievementCriteriaId);
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

		public uint AchievementCriteriaId
		{
			get { return (uint)_achievementCriteriaId; }
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

		public override string ToString()
		{
			return string.Format("{0} (Char: {1}, Criteria: {2})", GetType().Name, _characterGuid, _achievementCriteriaId);
		}
    }
}
