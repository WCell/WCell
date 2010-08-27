using System;
using System.Collections.Generic;
using NLog;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Achievement
{
	public enum AchievementFactionGroup
	{
		Horde,
		Alliance
	}

	/// <summary>
	/// Represents the Player's Achievements.
	/// </summary>
	public class AchievementCollection
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		internal Dictionary<AchievementEntryId, AchievementRecord> m_completedAchievements = new Dictionary<AchievementEntryId, AchievementRecord>();
		internal Dictionary<AchievementCriteriaId, AchievementProgressRecord> m_progressRecords = new Dictionary<AchievementCriteriaId, AchievementProgressRecord>();
		internal Character m_owner;

		public AchievementCollection(Character chr)
		{
			m_owner = chr;
		}

		#region Props

		/// <summary>
		/// Checks if player has completed the given achievement.
		/// </summary>
		/// <param name="achievementEntry"></param>
		/// <returns></returns>
		public bool HasCompleted(AchievementEntryId achievementEntry)
		{
			return m_completedAchievements.ContainsKey(achievementEntry);
		}

		/// <summary>
		/// Returns progress with given achievement's criteria
		/// </summary>
		/// <param name="achievementCriteriaId"></param>
		/// <returns></returns>
		public AchievementProgressRecord GetAchievementCriteriaProgress(AchievementCriteriaId achievementCriteriaId)
		{
			AchievementProgressRecord entry;
			m_progressRecords.TryGetValue(achievementCriteriaId, out entry);
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

		/// <summary>
		/// Checks if the given achievement is completable.
		/// </summary>
		/// <param name="achievementEntry"></param>
		/// <returns></returns>
		public bool IsAchievementCompletable(AchievementEntry achievementEntry)
		{
			// Counter achievement were never meant to be completed.
			if (achievementEntry.Flags.HasFlag(AchievementFlags.Counter))
				return false;

            // The method will return false only if the achievement has RealmFirst flags
            // and already achieved by someone.
            if (!AchievementMgr.IsRealmFirst(achievementEntry.ID))
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
				if (IsCriteriaCompletable(achievementCriteriaEntry))
					++count;
				else
					completedAll = false;

				if (achievementForTestCount > 0 && achievementForTestCount <= count)
					return true;		// TODO: Why return true here?
			}
			// all criterias completed requirement
			return (completedAll && achievementForTestCount == 0);
		}

		/// <summary>
		/// Checks if the given criteria is completable
		/// </summary>
		/// <param name="achievementCriteriaEntry"></param>
		/// <returns></returns>
		public bool IsCriteriaCompletable(AchievementCriteriaEntry achievementCriteriaEntry)
		{
			AchievementEntry achievementEntry = achievementCriteriaEntry.AchievementEntry;

			// Counter achievement were never meant to be completed.
			if (achievementEntry.Flags.HasFlag(AchievementFlags.Counter))
				return false;

			//TODO: Add support for realm first.

			// We never completed the criteria befoer.
			AchievementProgressRecord achievementProgressRecord =
				m_owner.Achievements.GetAchievementCriteriaProgress(achievementCriteriaEntry.AchievementCriteriaId);
			if (achievementProgressRecord == null)
				return false;
			return achievementCriteriaEntry.IsAchieved(achievementProgressRecord);

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
		    var achievementEntry = AchievementMgr.GetAchievementEntry(achievementEntryId);
            if(achievementEntry!= null)
                EarnAchievement(achievementEntry);
		}

		/// <summary>
		/// Adds a new achievement to the list, when achievement is earned.
		/// </summary>
		/// <param name="achievementEntry"></param>
		public void EarnAchievement(AchievementEntry achievement)
		{
			AddAchievement(AchievementRecord.CreateNewAchievementRecord(m_owner, achievement.ID));
			CheckPossibleAchievementUpdates(AchievementCriteriaType.CompleteAchievement, (uint)achievement.ID, 1);
			RemoveAchievementProgress(achievement);

            foreach (var achievementReward in achievement.Rewards)
                achievementReward.GiveReward(Owner);

            if (m_owner.IsInGuild)
                m_owner.Guild.Broadcast(AchievementHandler.CreateAchievementEarnedToGuild(achievement.ID, m_owner));

            if (achievement.IsRealmFirstType())
                AchievementHandler.SendServerFirstAchievement(achievement.ID, m_owner);

		    AchievementHandler.SendAchievementEarned(achievement.ID, m_owner);
		}

		/// <summary>
		/// Returns the corresponding ProgressRecord. Creates a new one if the player doesn't have progress record.
		/// </summary>
		/// <param name="achievementCriteriaId"></param>
		/// <returns></returns>
		internal AchievementProgressRecord GetOrCreateProgressRecord(AchievementCriteriaId achievementCriteriaId)
		{
			AchievementProgressRecord achievementProgressRecord;
			if (!m_progressRecords.TryGetValue(achievementCriteriaId, out achievementProgressRecord))
			{
				achievementProgressRecord = AchievementProgressRecord.CreateAchievementProgressRecord(Owner, achievementCriteriaId, 0);
				AddProgressRecord(achievementProgressRecord);
			}
			return achievementProgressRecord;
		}

		/// <summary>
		/// Adds a new progress record to the list.
		/// </summary>
		/// <param name="achievementProgressRecord"></param>
		void AddProgressRecord(AchievementProgressRecord achievementProgressRecord)
		{
			m_progressRecords.Add(achievementProgressRecord.AchievementCriteriaId, achievementProgressRecord);
		}

		#endregion

		#region Remove

		/// <summary>
		/// Removes achievement from the player.
		/// </summary>
		/// <param name="achievementEntryId"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Removes achievement from the player.
		/// </summary>
		/// <param name="achievementRecord"></param>
		public void RemoveAchievement(AchievementRecord achievementRecord)
		{
			m_completedAchievements.Remove(achievementRecord.AchievementEntryId);
		}

		/// <summary>
		/// Removes criteria progress from the player.
		/// </summary>
		/// <param name="achievementCriteriaId"></param>
		/// <returns></returns>
		public bool RemoveProgress(AchievementCriteriaId achievementCriteriaId)
		{
			AchievementProgressRecord achievementProgressRecord;
			if (m_progressRecords.TryGetValue(achievementCriteriaId, out achievementProgressRecord))
			{
				RemoveProgress(achievementProgressRecord);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Removes criteria progress from the player.
		/// </summary>
		/// <param name="achievementProgressRecord"></param>
		public void RemoveProgress(AchievementProgressRecord achievementProgressRecord)
		{
			m_progressRecords.Remove(achievementProgressRecord.AchievementCriteriaId);
		}

		/// <summary>
		/// Removes all the progress of a given achievement.
		/// </summary>
		/// <param name="achievementEntry"></param>
		public void RemoveAchievementProgress(AchievementEntry achievementEntry)
		{
			foreach (var achievementCriteriaEntry in achievementEntry.Criteria)
			{
				RemoveProgress(achievementCriteriaEntry.AchievementCriteriaId);
			}
		}

		#endregion

		#region Update
		/// <summary>
		/// Checks if the player can ever complete the given achievement.
		/// </summary>
		/// <param name="achievementCriteriaEntry"></param>
		/// <returns></returns>
		private bool IsAchieveable(AchievementCriteriaEntry achievementCriteriaEntry)
		{
			// Skip achievements we have completed
			if (HasCompleted(achievementCriteriaEntry.AchievementEntryId))
				return false;

			// Skip achievements that have different faction requirement then the player faction.
			if (achievementCriteriaEntry.AchievementEntry.FactionFlag == (int)AchievementFactionGroup.Alliance && Owner.FactionGroup != FactionGroup.Alliance)
				return false;
			if (achievementCriteriaEntry.AchievementEntry.FactionFlag == (int)AchievementFactionGroup.Horde && Owner.FactionGroup != FactionGroup.Horde)
				return false;

			// Skip achievements that require to be groupfree and 
			if (achievementCriteriaEntry.GroupFlag.HasFlag(AchievementCriteriaGroupFlags.AchievementCriteriaGroupNotInGroup) && Owner.IsInGroup)
				return false;

			return true;
		}

		/// <summary>
		/// A method that will try to update the progress of all the related criterias.
		/// </summary>
		/// <param name="type">The Criteria Type.</param>
		/// <param name="value1"></param>
		/// <param name="value2"></param>
		/// <param name="involved"></param>
		internal void CheckPossibleAchievementUpdates(AchievementCriteriaType type, uint value1 = 0u, uint value2 = 0u, ObjectBase involved = null)
		{
			// Get all the related criterions.
			var list = AchievementMgr.GetEntriesByCriterion(type);
			if (list != null)
			{
				foreach (var entry in list)
				{
					if (IsAchieveable(entry))
					{
						entry.OnUpdate(this, value1, value2, involved);
					}
				}
			}
		}

		/// <summary>
		/// Sets the progress with a given Criteria entry.
		/// </summary>
		/// <param name="entry"></param>
		/// <param name="newValue"></param>
		/// <param name="progressType"></param>
		internal void SetCriteriaProgress(AchievementCriteriaEntry entry, uint newValue, ProgressType progressType = ProgressType.ProgressSet)
		{
			// not create record for 0 counter
			if (newValue == 0)
				return;

			var achievementProgressRecord = GetOrCreateProgressRecord(entry.AchievementCriteriaId);
			uint updateValue;

			switch (progressType)
			{
				case ProgressType.ProgressAccumulate:
					updateValue = newValue + achievementProgressRecord.Counter;
					break;
				case ProgressType.ProgressHighest:
					updateValue = achievementProgressRecord.Counter < newValue ? newValue : achievementProgressRecord.Counter;
					break;
				default:
					updateValue = newValue;
					break;
			}

			if (updateValue == achievementProgressRecord.Counter)
				return;

			achievementProgressRecord.Counter = updateValue;


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



			if (IsAchievementCompletable(entry.AchievementEntry))
			{
				EarnAchievement(entry.AchievementEntry);
			}
		}

		/*public void CheckAchievementsAfterLoading()
		{
			CheckPossibleAchievementUpdates(AchievementCriteriaType.ReachLevel, (uint)Owner.Level);
			CheckPossibleAchievementUpdates(AchievementCriteriaType.ReachLevel, (uint)Owner.Level);
		}*/

		#endregion

		#region Save & Load
		public void SaveNow()
		{
			foreach (var mCompletedAchievement in m_completedAchievements.Values)
			{
				mCompletedAchievement.Save();
			}
			foreach (var mAchivementProgress in m_progressRecords.Values)
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
					if (m_progressRecords.ContainsKey(achivementProgress.AchievementCriteriaId))
					{
						log.Warn("Character {0} had progress for Achievement Criteria {1} more than once.", m_owner, achivementProgress.AchievementCriteriaId);
					}
					else
					{
						AddProgressRecord(achivementProgress);
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
