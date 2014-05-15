using System;
using System.Collections.Generic;
using WCell.Util.Logging;
using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Database.Entities
{
	/// <summary>
	/// Represents the progress in one criterion of one Achievement.
	/// One Achievement can have many criteria.
	/// </summary>
	public class AchievementProgressRecord
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
					CharacterGuid = (int)chr.EntityId.Low,
					AchievementCriteriaId = achievementCriteriaId,
					Counter = counter,
					StartOrUpdateTime = DateTime.Now,
				};//TODO: Should we not be adding this to the database here?
				RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(record);
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
		public virtual long RecordId
		{
			get
			{
				return Utility.MakeLong(CharacterGuid, AchievementCriteriaId);
			}
			set
			{
				CharacterGuid = (int)value;
				AchievementCriteriaId = (int)(value >> 32);
			}
		}

		/// <summary>
		/// The time when this record was inserted or last updated (depends on the kind of criterion)
		/// </summary>
		public virtual DateTime StartOrUpdateTime { get; set; }

		public virtual int CharacterGuid { get; set; }

		public virtual long AchievementCriteriaId { get; set; }

		public virtual uint Counter { get; set; }

		public static IEnumerable<AchievementProgressRecord> Load(int chrRecordId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<AchievementProgressRecord>(record => record.CharacterGuid == chrRecordId);
		}

		public override string ToString()
		{
			return string.Format("{0} (Char: {1}, Criteria: {2})", GetType().Name, CharacterGuid, AchievementCriteriaId);
		}
	}
}
