﻿using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Chat;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Global;
using WCell.Util.Data;

namespace WCell.RealmServer.Achievements
{
    /// <summary>
    /// TODO: Correct the values & move to constants
    /// </summary>
    public enum AchievementCriteriaGroupFlags
    {
        AchievementCriteriaGroupNotInGroup
    }

    /// <summary>
    /// Do not change the layout of this class!
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public abstract class AchievementCriteriaEntry
    {
        [NotPersistent]
        public AchievementCriteriaType Criteria;
        [NotPersistent]
        public uint AchievementCriteriaId;
        [NotPersistent]
        public uint AchievementEntryId;

        [NotPersistent]
        public AchievementCriteriaRequirementSet RequirementSet
        {
            get;
            internal set;
        }

        [NotPersistent]
        public uint CompletionFlag;
        [NotPersistent]						// 26
        public AchievementCriteriaGroupFlags GroupFlag;		// 27
        [NotPersistent]
        public uint TimeLimit;								// 29

        public AchievementEntry AchievementEntry
        {
            get { return AchievementMgr.GetAchievementEntry(AchievementEntryId); }
        }

        public virtual bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return false;
        }

        public virtual void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
        }

        public override string ToString()
        {
            return Criteria.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class KillCreatureAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 0
        public NPCId CreatureId;
        public int CreatureCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= CreatureCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (CreatureId != (NPCId)value1)
            {
                return;
            }
            achievements.SetCriteriaProgress(this, value2, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WinBattleGroundAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 1
        public MapId MapId;
        public uint WinCount;
        public uint AdditionalRequirement1Type;
        public uint AdditionalRequirement1Value;
        public uint AdditionalRequirement2Type;
        public uint AdditionalRequirement2Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ReachLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 5
        public uint Unused;
        public uint Level;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= Level;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (!AchievementEntry.IsRealmFirstType() || AchievementCollection.ClassSpecificAchievementId[(int)achievements.Owner.Class] == AchievementEntryId
                || AchievementCollection.RaceSpecificAchievementId[(int)achievements.Owner.Race] == AchievementEntryId)
            {
                achievements.SetCriteriaProgress(this, value1);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ReachSkillLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 7
        public SkillId SkillId;
        public uint SkillValue;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0 || (SkillId)value1 != SkillId)
                return;
            achievements.SetCriteriaProgress(this, value2);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= SkillValue;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteAchievementAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 8
        public uint AchievementToCompleteId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (AchievementToCompleteId == value1)
                achievements.SetCriteriaProgress(this, value2);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteQuestCountAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 9
        public uint Unused;
        public uint CompletedQuestCount;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= CompletedQuestCount;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteDailyQuestDailyAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 10
        public uint Unused;
        public uint NumberOfDays;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= NumberOfDays;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteQuestsInZoneAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 11
        public ZoneId ZoneId;
        public uint CompletedQuestCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= CompletedQuestCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (ZoneId == (ZoneId)value1)
                achievements.SetCriteriaProgress(this, value2);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteDailyQuestAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 14
        public uint Unused;
        public uint CompletedQuestCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= CompletedQuestCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteBattlegroundAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 15
        public MapId MapId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            if (MapId != (MapId)value1)
                return;
            achievements.SetCriteriaProgress(this, value2, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DeathAtMapAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 16
        public MapId MapId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            if (MapId != (MapId)value1)
                return;
            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DeathInDungeonAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 18
        public uint ManLimit;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteRaidAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 19
        public uint RaidSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class KilledByCreatureAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 20
        public NPCId CreatureId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            if ((NPCId)value1 != CreatureId)
                return;
            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class KilledByPlayerAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 23
        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            if (achievements.Owner.FactionGroup == (FactionGroup)value1)
                return;
            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class FallWithoutDyingAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 24
        public uint Unused;
        public uint Height;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressHighest);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= Height;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DeathsFromAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 26
        public EnviromentalDamageType EnviromentalDamageType;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            if ((EnviromentalDamageType)value1 != EnviromentalDamageType)
                return;

            achievements.SetCriteriaProgress(this, value2, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CompleteQuestAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 27
        public uint QuestId;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 != QuestId)
                return;
            achievements.SetCriteriaProgress(this, 1);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class BeSpellTargetAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 28
        // 69
        public SpellId SpellId;
        public uint SpellCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class CastSpellAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 29
        public SpellId SpellId;
        public uint SpellCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= SpellCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 != (uint)SpellId)
            {
                return;
            }
            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class HonorableKillAtAreaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 31
        public uint AreaId;
        public uint KillCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WinArenaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 32
        public MapId MapId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PlayArenaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 33
        public MapId MapId;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class LearnSpellAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 34
        public SpellId SpellId;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if ((SpellId)value1 != SpellId)
                return;

            if (!achievements.Owner.PlayerSpells.Contains((uint)SpellId))
                return;

            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class OwnItemAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 36
        public ItemId ItemId;
        public uint ItemCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= ItemCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (ItemId != (ItemId)value1)
                return;
            var itemCount = (uint)achievements.Owner.Inventory.GetAmount(ItemId);
            if (itemCount == 0)
                return;
            achievements.SetCriteriaProgress(this, itemCount, ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WinRatedArenaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 37
        public uint Unused;
        public uint Count;
        public uint Flag;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class HighestTeamRatingAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 38
        public uint TeamSize; //{2,3,5}
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ReachTeamRatingAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 39
        public uint TeamSize; //{2,3,5}
        public uint TeamRating;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class LearnSkillLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 40
        public SkillId SkillId;
        public SkillTierId SkillTierId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0 || (SkillId)value1 != SkillId)
                return;
            achievements.SetCriteriaProgress(this, value2);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= (uint)SkillTierId;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class UseItemAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 41
        public ItemId ItemId;
        public uint ItemCount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= ItemCount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 != (uint)ItemId)
                return;

            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class LootItemAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 42
        public ItemId ItemId;
        public uint ItemCount;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (ItemId != (ItemId)value1)
                return;
            achievements.SetCriteriaProgress(this, value2, ProgressType.ProgressAccumulate);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= ItemCount;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ExploreAreaAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 43
        public WorldMapOverlayId WorldMapOverlayId;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 != (uint)WorldMapOverlayId)
                return;
            var worldMapOverlayEntry = World.s_WorldMapOverlayEntries[value1];
            if (worldMapOverlayEntry == null)
                return;

            bool matchFound = false;
            foreach (var zoneTemplateId in worldMapOverlayEntry.ZoneTemplateId)
            {
                if (zoneTemplateId == 0)        // no more areaids to come, let's stop the search
                    break;

                var zoneTemplate = World.GetZoneInfo(zoneTemplateId);
                if (zoneTemplate.ExplorationBit < 0)
                    continue;

                if (achievements.Owner.IsZoneExplored(zoneTemplate.ExplorationBit))
                {
                    matchFound = true;
                    break;
                }
            }

            if (!matchFound)
                return;

            achievements.SetCriteriaProgress(this, 1);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class BuyBankSlotAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 45
        public uint Unused;
        public uint NumberOfBankSlots;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= NumberOfBankSlots;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, achievements.Owner.BankBagSlots, ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GainReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 46
        public FactionId FactionId;
        public uint ReputationAmount;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= ReputationAmount;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 != (uint)FactionId)
                return;

            int reputation = achievements.Owner.Reputations.GetValue(FactionMgr.GetFactionIndex(FactionId));
            achievements.SetCriteriaProgress(this, (uint)reputation, ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GainExaltedReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 47
        public uint Unused;
        public uint NumberOfExaltedReputations;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= NumberOfExaltedReputations;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, achievements.Owner.Reputations.GetExaltedReputations(), ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class VisitBarberShopAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 48
        public uint Unused;
        public uint NumberOfVisitsAtBarberShop;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= NumberOfVisitsAtBarberShop;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class DoEmoteAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 54
        public TextEmote emoteId;
        public uint countOfEmotes;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= countOfEmotes;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0 || value1 != (uint)emoteId)
                return;

            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class IncrementAtValue1AchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 60, 62, 63, 65, 66, 67, 103, 105
        public uint unused;
        public uint value; // gold in copper for 60, 62, 63, 65, 66, 67 and damage/heal received for 103, 105

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= value;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            if (value1 == 0)
                return;
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WinDuelLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 76
        public uint Unused;
        public uint DuelCount;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
        }

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= DuelCount;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class LoseDuelLevelAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 77
        public uint Unused;
        public uint DuelCount;

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GainReveredReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 87
        public uint Unused;
        public uint Unused2;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, achievements.Owner.Reputations.GetReveredReputations(), ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GainHonoredReputationAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 88
        public uint Unused;
        public uint Unused2;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, achievements.Owner.Reputations.GetHonoredReputations(), ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class KnownFactionsAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 89
        public uint Unused;
        public uint Unused2;

        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, achievements.Owner.Reputations.GetVisibleReputations(), ProgressType.ProgressHighest);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class FlightPathsTakenAchievementCriteriaEntry : AchievementCriteriaEntry
    {
        // 108
        public override bool IsAchieved(AchievementProgressRecord achievementProgressRecord)
        {
            return achievementProgressRecord.Counter >= 1;
        }

        public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
        {
            achievements.SetCriteriaProgress(this, 1, ProgressType.ProgressAccumulate);
        }
    }
}