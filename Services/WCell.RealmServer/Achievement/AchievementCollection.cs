using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;

namespace WCell.RealmServer.Achievement
{
    // AchievementCollection class which each Character has an instance of
    /// <summary>
    /// Represents the Player's Achievements.
    /// </summary>
    public class AchievementCollection
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

		internal Dictionary<AchievementEntryId, AchievementRecord> m_completedAchievements = new Dictionary<AchievementEntryId, AchievementRecord>();
		internal Dictionary<AchievementCriteriaId, AchievementProgressRecord> m_achivement_progress = new Dictionary<AchievementCriteriaId, AchievementProgressRecord>();
        internal Character m_owner;
        
        public AchievementCollection(Character chr)
        {
            m_owner = chr;
        }

        #region Props

		public bool HasCompleted(AchievementEntryId achievementEntry)
		{
			return m_completedAchievements.ContainsKey(achievementEntry);
		}

		public AchievementProgressRecord GetAchievementProgress(AchievementCriteriaId achievementCriteriaId)
		{
			AchievementProgressRecord entry;
			m_achivement_progress.TryGetValue(achievementCriteriaId, out entry);
			return entry;
		}

        /// <summary>
        /// Returns the Achievement's Owner.
        /// </summary>
        public Character Owner
        {
            get { return m_owner; }
        }

        /// <summary>
        /// Returns the amount of completed achievements.
        /// </summary>
        public int AchievementsCount
        {
            get { return m_completedAchievements.Count; }
        }

		public bool IsCompletedAchievement(AchievementEntry achievementEntry)
		{
			// Counter achievement were never meant to be completed.
			if (achievementEntry.Flags.HasFlag(AchievementFlags.AchievementFlagCounter))
				return false;

			AchievementEntryId achievementForTestId = (achievementEntry.RefAchievement != 0)
														? achievementEntry.RefAchievement
														: achievementEntry.ID;
			uint achievementForTestCount = achievementEntry.Count;


			List<AchievementCriteriaEntry> achievementCriteriaIds = achievementEntry.Criteria;

			if (achievementCriteriaIds.Count == 0)
				return false;

			uint count = 0;



			// Default case
			bool completedAll = true;

			foreach (var achievementCriteriaEntry in achievementCriteriaIds)
			{
				if (IsCompletedCriteria(achievementCriteriaEntry))
					++count;
				else
					completedAll = false;

				if (achievementForTestCount > 0 && achievementForTestCount <= count)
					return true;
			}
			// all criterias completed requirement
			return (completedAll && achievementForTestCount == 0);
		}

		public bool IsCompletedCriteria(AchievementCriteriaEntry achievementCriteriaEntry)
		{
			AchievementEntry achievementEntry = achievementCriteriaEntry.AchievementEntry;

			// Counter achievement were never meant to be completed.
			if (achievementEntry.Flags.HasFlag(AchievementFlags.AchievementFlagCounter))
				return false;

			//TODO: Add support for realm first.
			
			// We never completed the criteria befoer.
			AchievementProgressRecord achievementProgressRecord =
				m_owner.Achievements.GetAchievementProgress(achievementCriteriaEntry.AchievementCriteriaId);
			if (achievementProgressRecord == null)
				return false;

			return achievementCriteriaEntry.HasCompleted(achievementProgressRecord);

		}

        #endregion

        #region Add / Set
		/// <summary>
		/// Adds a new achievement to the list.
		/// </summary>
		/// <param name="achievementRecord"></param>
		public void AddAchievement(AchievementRecord achievementRecord)
		{
			m_completedAchievements.Add(achievementRecord.AchievementEntryId, achievementRecord);
		}

    	/// <summary>
    	/// Adds a new achievement to the list, when achievement is earned.
    	/// </summary>
    	/// <param name="achievementEntry"></param>
		public void EarnAchievement(AchievementEntryId achievementEntryId)
    	{
    		EarnAchievement(AchievementMgr.GetAchievementEntry(achievementEntryId));
    	}

    	/// <summary>
		/// Adds a new achievement to the list, when achievement is earned.
		/// </summary>
		/// <param name="achievementEntry"></param>
		public void EarnAchievement(AchievementEntry achievement)
		{
			AddAchievement(AchievementRecord.CreateNewAchievementRecord(m_owner, achievement.ID));
			AchievementHandler.SendAchievementEarned(achievement.ID, m_owner);
		}

		public void AddAchievementProgress(AchievementProgressRecord achievementProgressRecord)
		{
			RemoveAchievementProgress(achievementProgressRecord);
			m_achivement_progress.Add(achievementProgressRecord.AchievementCriteriaId, achievementProgressRecord);
		}

        #endregion

        #region Remove

		public bool RemoveAchievement(AchievementEntryId achievementEntryId)
		{
			AchievementRecord achievementRecord;
			if (m_completedAchievements.TryGetValue(achievementEntryId, out achievementRecord))
			{
				RemoveAchievement(achievementRecord);
				return true;
			}
			return false;
		}

		public void RemoveAchievement(AchievementRecord achievementRecord)
		{
			m_completedAchievements.Remove(achievementRecord.AchievementEntryId);
		}

		public bool RemoveAchievementProgress(AchievementCriteriaId achievementCriteriaId)
		{
			AchievementProgressRecord achievementProgressRecord;
			if (m_achivement_progress.TryGetValue(achievementCriteriaId, out achievementProgressRecord))
			{
				RemoveAchievementProgress(achievementProgressRecord);
				return true;
			}
			return false;
		}

		public void RemoveAchievementProgress(AchievementProgressRecord achievementProgressRecord)
		{
			m_achivement_progress.Remove(achievementProgressRecord.AchievementCriteriaId);
		}

        #endregion

		#region Update
		internal void Update(AchievementCriteriaType type, uint value1 = 0u, uint value2 = 0u, ObjectBase involved = null)
		{
			var list = AchievementMgr.GetEntriesByCriterion(type);
			if (list != null)
			{
				foreach (var entry in list)
				{
					// TODO: Add preliminary checks, if necessary
					entry.OnUpdate(this, value1, value2, involved);
					if (IsCompletedAchievement(entry.AchievementEntry) && !HasCompleted(entry.AchievementEntryId))
						EarnAchievement(entry.AchievementEntry);
				}
			}
		}

		internal void SetCriteriaProgress(AchievementCriteriaEntry entry, uint newValue)
		{
			AchievementProgressRecord achievementProgressRecord = GetAchievementProgress(entry.AchievementCriteriaId);
			if (achievementProgressRecord == null)
			{
				if (newValue == 0)
					return;
				achievementProgressRecord = AchievementProgressRecord.CreateAchievementProgressRecord(Owner,
																					  entry.AchievementCriteriaId,
																					  newValue);
			}
			else
			{
				achievementProgressRecord.Counter = newValue;
			}

			if (entry.TimeLimit > 0)
			{
				DateTime now = DateTime.Now;
				if (achievementProgressRecord.StartOrUpdateTime.AddSeconds(entry.TimeLimit) < now)
				{
					achievementProgressRecord.Counter = 1;
				}
				achievementProgressRecord.StartOrUpdateTime = now;
			}
			AchievementHandler.SendAchievmentStatus(achievementProgressRecord, Owner);
			AddAchievementProgress(achievementProgressRecord);
		}
		#endregion

		#region Save & Load
		public void SaveNow()
		{
			foreach (var mCompletedAchievement in m_completedAchievements.Values)
			{
				mCompletedAchievement.Save();
			}
			foreach (var mAchivementProgress in m_achivement_progress.Values)
			{
				mAchivementProgress.Save();
			}
		}

		public void Load()
		{
			foreach (var mCompletedAchievement in AchievementRecord.Load((int)Owner.EntityId.Low))
			{
				var achievement = AchievementMgr.GetAchievementEntry(mCompletedAchievement.AchievementEntryId);
				if (achievement != null)
				{
					if (m_completedAchievements.ContainsKey(achievement.ID))
					{
						log.Warn("Character {0} had Achievement {1} more than once.", m_owner, achievement.ID);
					}
					else
					{
						AddAchievement(mCompletedAchievement);
					}
				}
				else
				{
					log.Warn("Character {0} has invalid Achievement: {1}", m_owner, mCompletedAchievement.AchievementEntryId);
				}
			}

			foreach (var achivementProgress in AchievementProgressRecord.Load((int)Owner.EntityId.Low))
			{
				// how to check if there's no criteria
				//if (achievement != null)
				{
					if (m_achivement_progress.ContainsKey(achivementProgress.AchievementCriteriaId))
					{
						log.Warn("Character {0} had Achievement {1} more than once.", m_owner, achivementProgress.AchievementCriteriaId);
					}
					else
					{
						m_achivement_progress.Add(achivementProgress.AchievementCriteriaId, achivementProgress);
					}
				}
				//else
				//{
				//    log.Warn("Character {0} has invalid Achievement: {1}", m_owner, achivementProgress.AchievementCriteriaId);
				//}
			}
		}
		#endregion
    }
}
