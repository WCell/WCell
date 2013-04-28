using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using WCell.Util.Logging;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Database.Entities
{
	public class AchievementRecord
	{
		#region Static
		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
					AchievementId = (int)achievementEntryId,
					CharacterId = (int)chr.EntityId.Low,
					CompleteDate = DateTime.Now
				};
				RealmWorldDBMgr.DatabaseProvider.Save(record);
			}
			catch (Exception ex)
			{
				Logger.ErrorException("AchievementRecord creation error (DBS: " + RealmServerConfiguration.DBType + "): ", ex);
				record = null;
			}


			return record;
		}

		#endregion

		/// <summary>
		/// Encode char id and achievement id into RecordId
		/// </summary>
		public long RecordId
		{
			get
			{
				return Utility.MakeLong(CharacterId, AchievementId);
			}
			set
			{
				CharacterId = (int)value;
				AchievementId = (int)(value >> 32);
			}
		}

		public DateTime CompleteDate { get; set; }

		public int CharacterId { get; set; }

		public int AchievementId { get; set; }

		public static IEnumerable<AchievementRecord> Load(int chrId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<AchievementRecord>(record => record.CharacterId == chrId);
		}

        public static IEnumerable<AchievementRecord> Load(IEnumerable<int> achievementEntryIds)
        {
	        return RealmWorldDBMgr.DatabaseProvider.FindAll<AchievementRecord>(record => achievementEntryIds.Contains(u => u == record.AchievementId));
        }

		public override string ToString()
		{
			return string.Format("{0} - Char: {1}, Achievement: {2}, RecordId: {3}", GetType(), CharacterId, AchievementId, RecordId);
		}
	}
}