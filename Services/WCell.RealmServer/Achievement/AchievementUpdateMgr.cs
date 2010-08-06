using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Achievements;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.Util;

namespace WCell.RealmServer.Achievement
{
	/// <summary>
	/// Callback to be called to check the requirements for the given possible achievement
	/// </summary>
	/// <param name="type"></param>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	/// <param name="involved">The object that is involved in this achievement (e.g. a slain creature or acquired Item etc)</param>
	public delegate void AchievementUpdater(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved);

	/// <summary>
	/// Hook to InitializationPass.Seven to customize the Updater delegates.
	/// </summary>
	public static class AchievementUpdateMgr
	{
		public static readonly AchievementUpdater[] Updaters = new AchievementUpdater[(int)AchievementCriteriaType.End];

		public static AchievementUpdater GetUpdater(AchievementCriteriaType type)
		{
			return Updaters[(int)type];
		}

		public static void SetUpdater(AchievementCriteriaType type, AchievementUpdater updater)
		{
			Updaters[(int)type] = updater;
		}

		#region InitializeUpdater
		[Initialization(InitializationPass.Sixth)]
		public static void InitializeUpdater()
		{
			SetUpdater(AchievementCriteriaType.KillCreature, OnKillCreature);
			SetUpdater(AchievementCriteriaType.WinBg, OnWinBg);
			SetUpdater(AchievementCriteriaType.ReachLevel, OnReachLevel);
			SetUpdater(AchievementCriteriaType.ReachSkillLevel, OnReachSkillLevel);
			SetUpdater(AchievementCriteriaType.CompleteAchievement, OnCompleteAchievement);
			SetUpdater(AchievementCriteriaType.CompleteQuestCount, OnCompleteQuestCount);
			// you have to complete a daily quest x times in a row  
			SetUpdater(AchievementCriteriaType.CompleteDailyQuestDaily, OnCompleteDailyQuestDaily);
			SetUpdater(AchievementCriteriaType.CompleteQuestsInZone, OnCompleteQuestsInZone);
			SetUpdater(AchievementCriteriaType.DamageDone, OnDamageDone);
			SetUpdater(AchievementCriteriaType.CompleteDailyQuest, OnCompleteDailyQuest);
			SetUpdater(AchievementCriteriaType.CompleteBattleground, OnCompleteBattleground);
			SetUpdater(AchievementCriteriaType.DeathAtMap, OnDeathAtMap);
			SetUpdater(AchievementCriteriaType.Death, OnDeath);
			SetUpdater(AchievementCriteriaType.DeathInDungeon, OnDeathInDungeon);
			SetUpdater(AchievementCriteriaType.CompleteRaid, OnCompleteRaid);
			SetUpdater(AchievementCriteriaType.KilledByCreature, OnKilledByCreature);
			SetUpdater(AchievementCriteriaType.KilledByPlayer, OnKilledByPlayer);
			SetUpdater(AchievementCriteriaType.FallWithoutDying, OnFallWithoutDying);
			SetUpdater(AchievementCriteriaType.DeathsFrom, OnDeathsFrom);
			SetUpdater(AchievementCriteriaType.CompleteQuest, OnCompleteQuest);
			SetUpdater(AchievementCriteriaType.BeSpellTarget, OnBeSpellTarget);
			SetUpdater(AchievementCriteriaType.CastSpell, OnCastSpell);
			SetUpdater(AchievementCriteriaType.BgObjectiveCapture, OnBgObjectiveCapture);
			SetUpdater(AchievementCriteriaType.HonorableKillAtArea, OnHonorableKillAtArea);
			SetUpdater(AchievementCriteriaType.WinArena, OnWinArena);
			SetUpdater(AchievementCriteriaType.PlayArena, OnPlayArena);
			SetUpdater(AchievementCriteriaType.LearnSpell, OnLearnSpell);
			SetUpdater(AchievementCriteriaType.HonorableKill, OnHonorableKill);
			SetUpdater(AchievementCriteriaType.OwnItem, OnOwnItem);
			SetUpdater(AchievementCriteriaType.WinRatedArena, OnWinRatedArena);
			SetUpdater(AchievementCriteriaType.HighestTeamRating, OnHighestTeamRating);
			SetUpdater(AchievementCriteriaType.ReachTeamRating, OnReachTeamRating);
			SetUpdater(AchievementCriteriaType.LearnSkillLevel, OnLearnSkillLevel);
			SetUpdater(AchievementCriteriaType.UseItem, OnUseItem);
			SetUpdater(AchievementCriteriaType.LootItem, OnLootItem);
			SetUpdater(AchievementCriteriaType.ExploreArea, OnExploreArea);
			SetUpdater(AchievementCriteriaType.OwnRank, OnOwnRank);
			SetUpdater(AchievementCriteriaType.BuyBankSlot, OnBuyBankSlot);
			SetUpdater(AchievementCriteriaType.GainReputation, OnGainReputation);
			SetUpdater(AchievementCriteriaType.GainExaltedReputation, OnGainExaltedReputation);
			SetUpdater(AchievementCriteriaType.VisitBarberShop, OnVisitBarberShop);
			SetUpdater(AchievementCriteriaType.EquipEpicItem, OnEquipEpicItem);
			SetUpdater(AchievementCriteriaType.RollNeedOnLoot, OnRollNeedOnLoot);
			SetUpdater(AchievementCriteriaType.RollGreedOnLoot, OnRollGreedOnLoot);
			SetUpdater(AchievementCriteriaType.HkClass, OnHkClass);
			SetUpdater(AchievementCriteriaType.HkRace, OnHkRace);
			SetUpdater(AchievementCriteriaType.DoEmote, OnDoEmote);
			SetUpdater(AchievementCriteriaType.HealingDone, OnHealingDone);
			SetUpdater(AchievementCriteriaType.GetKillingBlows, OnGetKillingBlows);
			SetUpdater(AchievementCriteriaType.EquipItem, OnEquipItem);
			SetUpdater(AchievementCriteriaType.MoneyFromVendors, OnMoneyFromVendors);
			SetUpdater(AchievementCriteriaType.GoldSpentForTalents, OnGoldSpentForTalents);
			SetUpdater(AchievementCriteriaType.NumberOfTalentResets, OnNumberOfTalentResets);
			SetUpdater(AchievementCriteriaType.MoneyFromQuestReward, OnMoneyFromQuestReward);
			SetUpdater(AchievementCriteriaType.GoldSpentForTravelling, OnGoldSpentForTravelling);
			SetUpdater(AchievementCriteriaType.GoldSpentAtBarber, OnGoldSpentAtBarber);
			SetUpdater(AchievementCriteriaType.GoldSpentForMail, OnGoldSpentForMail);
			SetUpdater(AchievementCriteriaType.LootMoney, OnLootMoney);
			SetUpdater(AchievementCriteriaType.UseGameobject, OnUseGameobject);
			SetUpdater(AchievementCriteriaType.BeSpellTarget2, OnBeSpellTarget2);
			SetUpdater(AchievementCriteriaType.SpecialPvpKill, OnSpecialPvpKill);
			SetUpdater(AchievementCriteriaType.FishInGameobject, OnFishInGameobject);
			SetUpdater(AchievementCriteriaType.EarnedPvpTitle, OnEarnedPvpTitle);
			SetUpdater(AchievementCriteriaType.LearnSkilllineSpells, OnLearnSkilllineSpells);
			SetUpdater(AchievementCriteriaType.WinDuel, OnWinDuel);
			SetUpdater(AchievementCriteriaType.LoseDuel, OnLoseDuel);
			SetUpdater(AchievementCriteriaType.KillCreatureType, OnKillCreatureType);
			SetUpdater(AchievementCriteriaType.GoldEarnedByAuctions, OnGoldEarnedByAuctions);
			SetUpdater(AchievementCriteriaType.CreateAuction, OnCreateAuction);
			SetUpdater(AchievementCriteriaType.HighestAuctionBid, OnHighestAuctionBid);
			SetUpdater(AchievementCriteriaType.WonAuctions, OnWonAuctions);
			SetUpdater(AchievementCriteriaType.HighestAuctionSold, OnHighestAuctionSold);
			SetUpdater(AchievementCriteriaType.HighestGoldValueOwned, OnHighestGoldValueOwned);
			SetUpdater(AchievementCriteriaType.GainReveredReputation, OnGainReveredReputation);
			SetUpdater(AchievementCriteriaType.GainHonoredReputation, OnGainHonoredReputation);
			SetUpdater(AchievementCriteriaType.KnownFactions, OnKnownFactions);
			SetUpdater(AchievementCriteriaType.LootEpicItem, OnLootEpicItem);
			SetUpdater(AchievementCriteriaType.ReceiveEpicItem, OnReceiveEpicItem);
			SetUpdater(AchievementCriteriaType.RollNeed, OnRollNeed);
			SetUpdater(AchievementCriteriaType.RollGreed, OnRollGreed);
			SetUpdater(AchievementCriteriaType.HighestHealth, OnHighestHealth);
			SetUpdater(AchievementCriteriaType.HighestPower, OnHighestPower);
			SetUpdater(AchievementCriteriaType.HighestStat, OnHighestStat);
			SetUpdater(AchievementCriteriaType.HighestSpellpower, OnHighestSpellpower);
			SetUpdater(AchievementCriteriaType.HighestArmor, OnHighestArmor);
			SetUpdater(AchievementCriteriaType.HighestRating, OnHighestRating);
			SetUpdater(AchievementCriteriaType.HighestHitDealt, OnHighestHitDealt);
			SetUpdater(AchievementCriteriaType.HighestHitReceived, OnHighestHitReceived);
			SetUpdater(AchievementCriteriaType.TotalDamageReceived, OnTotalDamageReceived);
			SetUpdater(AchievementCriteriaType.HighestHealCasted, OnHighestHealCasted);
			SetUpdater(AchievementCriteriaType.TotalHealingReceived, OnTotalHealingReceived);
			SetUpdater(AchievementCriteriaType.HighestHealingReceived, OnHighestHealingReceived);
			SetUpdater(AchievementCriteriaType.QuestAbandoned, OnQuestAbandoned);
			SetUpdater(AchievementCriteriaType.FlightPathsTaken, OnFlightPathsTaken);
			SetUpdater(AchievementCriteriaType.LootType, OnLootType);
			SetUpdater(AchievementCriteriaType.CastSpell2, OnCastSpell2);
			SetUpdater(AchievementCriteriaType.LearnSkillLine, OnLearnSkillLine);
			SetUpdater(AchievementCriteriaType.EarnHonorableKill, OnEarnHonorableKill);
			SetUpdater(AchievementCriteriaType.AcceptedSummonings, OnAcceptedSummonings);
			SetUpdater(AchievementCriteriaType.EarnAchievementPoints, OnEarnAchievementPoints);
			SetUpdater(AchievementCriteriaType.UseLfdToGroupWithPlayers, OnUseLfdToGroupWithPlayers);
		}
		#endregion

		private static void SetCriteriaProgress(AchievementCriteriaEntry entry, Character chr, uint newValue)
		{
			AchievementProgressRecord achievementProgressRecord = chr.Achievements.GetAchievementProgress(entry.AchievementCriteriaId);
			if (achievementProgressRecord == null)
			{
				if(newValue == 0)
					return;
				achievementProgressRecord = AchievementProgressRecord.CreateAchievementProgressRecord(chr,
																					  entry.AchievementCriteriaId,
																					  newValue);
			}
			else
			{
				achievementProgressRecord.Counter = newValue;
			}

			if(entry.TimeLimit > 0)
			{
				DateTime now = DateTime.Now;
				if(Utility.GetEpochTimeFromDT(achievementProgressRecord.Date) + entry.TimeLimit < Utility.GetEpochTimeFromDT(now))					
					achievementProgressRecord.Counter = 1;
				achievementProgressRecord.Date = now;
			}
			AchievementHandler.SendAchievmentStatus(achievementProgressRecord.AchievementCriteriaId, chr);
			chr.Achievements.AddAchievementProgress(achievementProgressRecord);
		}

		#region Default Achievement Event Updaters
		private static void OnKillCreature(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
			var killCreatureAchievementCriteriaEntry = entry as KillCreatureAchievementCriteriaEntry;
			if (killCreatureAchievementCriteriaEntry == null || killCreatureAchievementCriteriaEntry.CreatureId != (NPCId)value1)
			{
				return;
			}
			SetCriteriaProgress(entry, chr, value2);

		}
		private static void OnWinBg(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnReachLevel(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
			SetCriteriaProgress(entry, chr, value1);
		}
		private static void OnReachSkillLevel(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteAchievement(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteQuestCount(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteDailyQuestDaily(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteQuestsInZone(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnDamageDone(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteDailyQuest(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteBattleground(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnDeathAtMap(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnDeath(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnDeathInDungeon(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteRaid(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnKilledByCreature(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnKilledByPlayer(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnFallWithoutDying(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnDeathsFrom(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCompleteQuest(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnBeSpellTarget(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCastSpell(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnBgObjectiveCapture(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHonorableKillAtArea(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnWinArena(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnPlayArena(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLearnSpell(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHonorableKill(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnOwnItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnWinRatedArena(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestTeamRating(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnReachTeamRating(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLearnSkillLevel(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnUseItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLootItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnExploreArea(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnOwnRank(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnBuyBankSlot(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGainReputation(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGainExaltedReputation(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnVisitBarberShop(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnEquipEpicItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnRollNeedOnLoot(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnRollGreedOnLoot(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHkClass(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHkRace(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnDoEmote(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHealingDone(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGetKillingBlows(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnEquipItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnMoneyFromVendors(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGoldSpentForTalents(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnNumberOfTalentResets(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnMoneyFromQuestReward(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGoldSpentForTravelling(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGoldSpentAtBarber(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGoldSpentForMail(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLootMoney(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnUseGameobject(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnBeSpellTarget2(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnSpecialPvpKill(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnFishInGameobject(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnEarnedPvpTitle(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLearnSkilllineSpells(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnWinDuel(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLoseDuel(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		// todo: creature type (demon, undead etc.) is not stored in dbc  
		private static void OnKillCreatureType(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGoldEarnedByAuctions(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCreateAuction(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestAuctionBid(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnWonAuctions(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestAuctionSold(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestGoldValueOwned(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGainReveredReputation(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnGainHonoredReputation(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnKnownFactions(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLootEpicItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnReceiveEpicItem(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnRollNeed(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnRollGreed(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestHealth(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestPower(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestStat(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestSpellpower(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestArmor(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestRating(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestHitDealt(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestHitReceived(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnTotalDamageReceived(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestHealCasted(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnTotalHealingReceived(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnHighestHealingReceived(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnQuestAbandoned(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnFlightPathsTaken(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLootType(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnCastSpell2(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnLearnSkillLine(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnEarnHonorableKill(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnAcceptedSummonings(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnEarnAchievementPoints(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		private static void OnUseLfdToGroupWithPlayers(AchievementCriteriaEntry entry, Character chr, uint value1, uint value2, ObjectBase involved)
		{
		}
		#endregion
	}
}
