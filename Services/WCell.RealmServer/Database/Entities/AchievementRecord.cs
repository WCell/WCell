using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using WCell.Util.Logging;
using WCell.RealmServer.Entities;
using Utility = WCell.Util.Utility;

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
					AchievementId = achievementEntryId,
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
		public virtual long RecordId
		{
			get
			{
				return Utility.MakeLong(CharacterId, (int)AchievementId);
			}
			set
			{
				CharacterId = (int)value;
				AchievementId = (uint)(value >> 32);
			}
		}

		public virtual DateTime CompleteDate { get; set; }

		public virtual int CharacterId { get; set; }

		public virtual uint AchievementId { get; set; }

		public static IEnumerable<AchievementRecord> Load(int chrId)
		{
			return RealmWorldDBMgr.DatabaseProvider.Query<AchievementRecord>().Where(record => record.CharacterId == chrId);
		}

        public static IEnumerable<AchievementRecord> Load(IEnumerable<uint> achievementEntryIds)
        {
	        return RealmWorldDBMgr.DatabaseProvider.Query<AchievementRecord>().Where(record => achievementEntryIds.Contains(record.AchievementId));
        }

		public override string ToString()
		{
			return string.Format("{0} - Char: {1}, Achievement: {2}, RecordId: {3}", GetType(), CharacterId, AchievementId, RecordId);
		}
	}
}