using System;
using Castle.ActiveRecord;
using NHibernate.Criterion;
using NLog;
using WCell.Core.Database;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Achievements
{

	[ActiveRecord(Access = PropertyAccess.Property)]
	public class AchievementRecord : WCellRecord<AchievementRecord>
	{
		#region Static
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Creates a new AchievementRecord row in the database with the given information.
		/// </summary>
		/// <param name="account">the account this character is on</param>
		/// <param name="name">the name of the new character</param>
		/// <returns>the <seealso cref="AchievementRecord"/> object</returns>
		public static AchievementRecord CreateNewAchievementRecord(Character chr, uint achievementEntryId)
		{
			AchievementRecord record;

			try
			{
				record = new AchievementRecord
				{
					_achievementEntryId = (int)achievementEntryId,
					_characterGuid = (int)chr.EntityId.Low,
					CompleteDate = DateTime.Now,
					State = RecordState.New
				};
			}
			catch (Exception ex)
			{
				s_log.ErrorException("AchievementRecord creation error (DBS: " + RealmServerConfiguration.DatabaseType + "): ", ex);
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
				return Utility.MakeLong(_characterGuid, _achievementEntryId);
			}
			set
			{
                Utility.UnpackLong(value, ref _characterGuid, ref _achievementEntryId);
			}
		}

		[Field("CharacterId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _characterGuid;

		[Field("Achievement", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _achievementEntryId;

		[Property]
		public DateTime CompleteDate { get; set; }

		public uint CharacterGuid
		{
			get { return (uint)_characterGuid; }
			set { _characterGuid = (int)value; }
		}

		public uint AchievementEntryId
		{
			get { return (uint)_achievementEntryId; }
			set { _achievementEntryId = (int)value; }
		}

		public static AchievementRecord[] Load(int chrId)
		{
			return FindAll(Restrictions.Eq("_characterGuid", chrId));
		}

        public static AchievementRecord[] Load(uint[] achievementEntryIds)
        {
            return FindAll(Restrictions.In("_achievementEntryId", achievementEntryIds));
        }

		public override string ToString()
		{
			return string.Format("{0} - Char: {1}, Achievement: {2}, RecordId: {3}", GetType(), _characterGuid, _achievementEntryId, RecordId);
		}
	}
}