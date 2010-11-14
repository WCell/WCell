using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Achievement
{
	public delegate AchievementCriteriaEntry AchievementCriteriaEntryCreator();
    public delegate AchievementCriteriaRequirement AchievementCriteriaRequirementCreator();

	/// <summary>
	/// Global container for Achievement-related data
	/// </summary>
	public static class AchievementMgr
	{
		private static readonly AchievementCriteriaEntryCreator[] AchievementEntryCreators =
			new AchievementCriteriaEntryCreator[(int)AchievementCriteriaType.End];

        private static readonly AchievementCriteriaRequirementCreator[] AchievementCriteriaRequirementCreators =
            new AchievementCriteriaRequirementCreator[(int)AchievementCriteriaRequirementType.End];

		public static readonly List<AchievementCriteriaEntry>[] CriteriaEntriesByType = new List<AchievementCriteriaEntry>[(int)AchievementCriteriaType.End];
        public static readonly Dictionary<uint, AchievementCriteriaEntry> CriteriaEntriesById = new Dictionary<uint, AchievementCriteriaEntry>();
		public static readonly Dictionary<uint, AchievementEntry> AchievementEntries = new Dictionary<uint, AchievementEntry>();
		public static readonly Dictionary<AchievementCategoryEntryId, AchievementCategoryEntry> AchievementCategoryEntries = new Dictionary<AchievementCategoryEntryId, AchievementCategoryEntry>();
        public static readonly List<uint> CompletedRealmFirstAchievements = new List<uint>();
        public static readonly Dictionary<uint, AchievementCriteriaRequirementSet> CriteriaRequirements = new Dictionary<uint, AchievementCriteriaRequirementSet>();

		[Initialization(InitializationPass.Fifth, "Initialize Achievements")]
		public static void InitAchievements()
		{
			InitCriteria();
            InitCriteriaRequirements();
			LoadDBCs();
            ContentMgr.Load<AchievementReward>();
            ContentMgr.Load<AchievementCriteriaRequirement>();
		    LoadRealmFirstAchievements();
		}

        public static void LoadRealmFirstAchievements()
        {
            var allRealmFirstRecords = (from achievementEntry in AchievementEntries.Values
                                        where achievementEntry.IsRealmFirstType()
                                        select achievementEntry.ID).ToArray();

            var completedAchievements = AchievementRecord.Load(allRealmFirstRecords);

            foreach (var completedAchievement in completedAchievements)
            {
                CompletedRealmFirstAchievements.Add(completedAchievement.AchievementEntryId);
            }
        }

	    public static List<AchievementCriteriaEntry> GetCriteriaEntriesByType(AchievementCriteriaType type)
		{
			return CriteriaEntriesByType[(int)type];
		}

        public static AchievementCriteriaEntry GetCriteriaEntryById(uint id)
        {
            AchievementCriteriaEntry entry;
            CriteriaEntriesById.TryGetValue(id, out entry);
            return entry;
        }

        /// <summary>
        /// Returns the criteria requirements set
        /// </summary>
        /// <param name="id">the criteria id</param>
        /// <returns>the criteria requirements set</returns>
        public static AchievementCriteriaRequirementSet GetCriteriaRequirementSet(uint id)
        {
            AchievementCriteriaRequirementSet set;
            CriteriaRequirements.TryGetValue(id, out set);
            return set;
        }

		#region Dynamic Criteria Creation
		public static AchievementCriteriaEntryCreator GetCriteriaEntryCreator(AchievementCriteriaType criteria)
		{
			return AchievementEntryCreators[(int)criteria];
		}

		public static void SetEntryCreator(AchievementCriteriaType criteria, AchievementCriteriaEntryCreator creator)
		{
			AchievementEntryCreators[(int)criteria] = creator;
		}

		public static void InitCriteria()
		{
			// initialize criteria lists
			for (var i = 0; i < CriteriaEntriesByType.Length; i++)
			{
                CriteriaEntriesByType[i] = new List<AchievementCriteriaEntry>();
			}

			// initialize creator map
			SetEntryCreator(AchievementCriteriaType.KillCreature, () => new KillCreatureAchievementCriteriaEntry());                            // 0
			SetEntryCreator(AchievementCriteriaType.WinBg, () => new WinBattleGroundAchievementCriteriaEntry());                                // 1
			SetEntryCreator(AchievementCriteriaType.ReachLevel, () => new ReachLevelAchievementCriteriaEntry());                                // 5
			SetEntryCreator(AchievementCriteriaType.ReachSkillLevel, () => new ReachSkillLevelAchievementCriteriaEntry());                      // 7
			SetEntryCreator(AchievementCriteriaType.CompleteAchievement, () => new CompleteAchievementAchievementCriteriaEntry());              // 8
			SetEntryCreator(AchievementCriteriaType.CompleteQuestCount, () => new CompleteQuestCountAchievementCriteriaEntry());                // 9
			SetEntryCreator(AchievementCriteriaType.CompleteDailyQuestDaily, () => new CompleteDailyQuestDailyAchievementCriteriaEntry());      // 10
			SetEntryCreator(AchievementCriteriaType.CompleteQuestsInZone, () => new CompleteQuestsInZoneAchievementCriteriaEntry());            // 11
			SetEntryCreator(AchievementCriteriaType.CompleteDailyQuest, () => new CompleteDailyQuestAchievementCriteriaEntry());                // 14
			SetEntryCreator(AchievementCriteriaType.CompleteBattleground, () => new CompleteBattlegroundAchievementCriteriaEntry());            // 15
			SetEntryCreator(AchievementCriteriaType.DeathAtMap, () => new DeathAtMapAchievementCriteriaEntry());                                // 16
			SetEntryCreator(AchievementCriteriaType.DeathInDungeon, () => new DeathInDungeonAchievementCriteriaEntry());                        // 18
			SetEntryCreator(AchievementCriteriaType.CompleteRaid, () => new CompleteRaidAchievementCriteriaEntry());                            // 19
			SetEntryCreator(AchievementCriteriaType.KilledByCreature, () => new KilledByCreatureAchievementCriteriaEntry());                    // 20
			SetEntryCreator(AchievementCriteriaType.FallWithoutDying, () => new FallWithoutDyingAchievementCriteriaEntry());                    // 24
			SetEntryCreator(AchievementCriteriaType.DeathsFrom, () => new DeathsFromAchievementCriteriaEntry());                                // 26
			SetEntryCreator(AchievementCriteriaType.CompleteQuest, () => new CompleteQuestAchievementCriteriaEntry());                          // 27
			SetEntryCreator(AchievementCriteriaType.BeSpellTarget, () => new BeSpellTargetAchievementCriteriaEntry());                          // 28
			SetEntryCreator(AchievementCriteriaType.BeSpellTarget2, () => new BeSpellTargetAchievementCriteriaEntry());                         // 69
			SetEntryCreator(AchievementCriteriaType.CastSpell, () => new CastSpellAchievementCriteriaEntry());                                  // 29
			SetEntryCreator(AchievementCriteriaType.CastSpell2, () => new CastSpellAchievementCriteriaEntry());                                 // 110
			SetEntryCreator(AchievementCriteriaType.HonorableKillAtArea, () => new HonorableKillAtAreaAchievementCriteriaEntry());              // 31
			SetEntryCreator(AchievementCriteriaType.WinArena, () => new WinArenaAchievementCriteriaEntry());                                    // 32
			SetEntryCreator(AchievementCriteriaType.PlayArena, () => new PlayArenaAchievementCriteriaEntry());                                  // 33
			SetEntryCreator(AchievementCriteriaType.LearnSpell, () => new LearnSpellAchievementCriteriaEntry());                                // 34
			SetEntryCreator(AchievementCriteriaType.OwnItem, () => new OwnItemAchievementCriteriaEntry());                                      // 36
			SetEntryCreator(AchievementCriteriaType.WinRatedArena, () => new WinRatedArenaAchievementCriteriaEntry());                          // 37
			SetEntryCreator(AchievementCriteriaType.HighestTeamRating, () => new HighestTeamRatingAchievementCriteriaEntry());                  // 38
			SetEntryCreator(AchievementCriteriaType.ReachTeamRating, () => new ReachTeamRatingAchievementCriteriaEntry());                      // 39
            SetEntryCreator(AchievementCriteriaType.LearnSkillLevel, () => new LearnSkillLevelAchievementCriteriaEntry());                      // 40
            SetEntryCreator(AchievementCriteriaType.ExploreArea, () => new ExploreAreaAchievementCriteriaEntry());                              // 43
            SetEntryCreator(AchievementCriteriaType.MoneyFromVendors, () => new IncrementAtValue1AchievementCriteriaEntry());                   // 59
            SetEntryCreator(AchievementCriteriaType.GoldSpentForTalents, () => new IncrementAtValue1AchievementCriteriaEntry());                // 60
            SetEntryCreator(AchievementCriteriaType.MoneyFromQuestReward, () => new IncrementAtValue1AchievementCriteriaEntry());               // 62
            SetEntryCreator(AchievementCriteriaType.GoldSpentForTravelling, () => new IncrementAtValue1AchievementCriteriaEntry());             // 63
            SetEntryCreator(AchievementCriteriaType.GoldSpentAtBarber, () => new IncrementAtValue1AchievementCriteriaEntry());                  // 65
            SetEntryCreator(AchievementCriteriaType.GoldSpentForMail, () => new IncrementAtValue1AchievementCriteriaEntry());                   // 66                                      
            SetEntryCreator(AchievementCriteriaType.LootMoney, () => new IncrementAtValue1AchievementCriteriaEntry());                          // 67          
			SetEntryCreator(AchievementCriteriaType.WinDuel, () => new WinDuelLevelAchievementCriteriaEntry());									// 76
			SetEntryCreator(AchievementCriteriaType.LoseDuel, () => new LoseDuelLevelAchievementCriteriaEntry());								// 77					
			SetEntryCreator(AchievementCriteriaType.TotalDamageReceived, () => new IncrementAtValue1AchievementCriteriaEntry());                // 103
            SetEntryCreator(AchievementCriteriaType.TotalHealingReceived, () => new IncrementAtValue1AchievementCriteriaEntry());               // 105
            //TODO: Add more types.
		}

		static void LoadDBCs()
		{
			new DBCReader<AchievementEntryConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_ACHIEVEMENTS));
			new DBCReader<AchievementCategoryEntryConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_ACHIEVEMENT_CATEGORIES));
			new DBCReader<AchievementCriteriaConverter>(RealmServerConfiguration.GetDBCFile(WCellDef.DBC_ACHIEVEMENT_CRITERIAS));
		}
		#endregion

        #region Dynamic Criteria Requirement Creation
        public static AchievementCriteriaRequirementCreator GetCriteriaRequirementCreator(AchievementCriteriaRequirementType type)
        {
            return AchievementCriteriaRequirementCreators[(int)type];
        }

        public static void SetRequirementCreator(AchievementCriteriaRequirementType type, AchievementCriteriaRequirementCreator creator)
        {
            AchievementCriteriaRequirementCreators[(int)type] = creator;
        }

        public static void InitCriteriaRequirements()
        {
            // initialize creator map
            SetRequirementCreator(AchievementCriteriaRequirementType.None, () => new AchievementCriteriaRequirement());
            SetRequirementCreator(AchievementCriteriaRequirementType.Creature, () => new AchievementCriteriaRequirementCreature());
            SetRequirementCreator(AchievementCriteriaRequirementType.PlayerClassRace, () => new AchievementCriteriaRequirementPlayerClassRace());
            SetRequirementCreator(AchievementCriteriaRequirementType.PlayerLessHealth, () => new AchievementCriteriaRequirementPlayerLessHealth());
            SetRequirementCreator(AchievementCriteriaRequirementType.PlayerDead, () => new AchievementCriteriaRequirementPlayerDead());
            SetRequirementCreator(AchievementCriteriaRequirementType.Aura1, () => new AchievementCriteriaRequirementAura1());
            SetRequirementCreator(AchievementCriteriaRequirementType.Area, () => new AchievementCriteriaRequirementArea());
            SetRequirementCreator(AchievementCriteriaRequirementType.Aura2, () => new AchievementCriteriaRequirementAura2());
            SetRequirementCreator(AchievementCriteriaRequirementType.Value, () => new AchievementCriteriaRequirementValue());
            SetRequirementCreator(AchievementCriteriaRequirementType.Gender, () => new AchievementCriteriaRequirementGender());
            SetRequirementCreator(AchievementCriteriaRequirementType.Disabled, () => new AchievementCriteriaRequirementDisabled());
            SetRequirementCreator(AchievementCriteriaRequirementType.MapDifficulty, () => new AchievementCriteriaRequirementMapDifficulty());
            SetRequirementCreator(AchievementCriteriaRequirementType.MapPlayerCount, () => new AchievementCriteriaRequirementMapPlayerCount());
            SetRequirementCreator(AchievementCriteriaRequirementType.Team, () => new AchievementCriteriaRequirementTeam());
            SetRequirementCreator(AchievementCriteriaRequirementType.Drunk, () => new AchievementCriteriaRequirementDrunk());
            SetRequirementCreator(AchievementCriteriaRequirementType.Holiday, () => new AchievementCriteriaRequirementHoliday());
            SetRequirementCreator(AchievementCriteriaRequirementType.BgLossTeamScore, () => new AchievementCriteriaRequirementBgLossTeamScore());
            SetRequirementCreator(AchievementCriteriaRequirementType.InstanceScript, () => new AchievementCriteriaRequirementInstanceScript());
            SetRequirementCreator(AchievementCriteriaRequirementType.EquippedItemLevel, () => new AchievementCriteriaRequirementEquippedItemLevel());
        }
        #endregion

		public static AchievementEntry GetAchievementEntry(uint achievementEntryId)
		{
			AchievementEntry entry;
			AchievementEntries.TryGetValue(achievementEntryId, out entry);
			return entry;
		}

		public static AchievementCategoryEntry GetCategoryEntry(AchievementCategoryEntryId achievementCategoryEntryId)
		{
			AchievementCategoryEntry entry;
			AchievementCategoryEntries.TryGetValue(achievementCategoryEntryId, out entry);
			return entry;
		}

		public static AchievementCategoryEntry GetCriteria(AchievementCategoryEntryId achievementCategoryEntryId)
		{
			return AchievementCategoryEntries[achievementCategoryEntryId];
		}

        public static bool IsRealmFirst(uint achievementEntryId)
        {
            return (!CompletedRealmFirstAchievements.Contains(achievementEntryId));
        }
	}
}