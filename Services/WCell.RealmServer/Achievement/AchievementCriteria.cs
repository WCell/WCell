using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.RealmServer.Entities;
using WCell.Constants.Factions;

namespace WCell.RealmServer.Achievement
{
	[StructLayout(LayoutKind.Sequential)]
	public abstract class AchievementCriteriaEntry
	{
		public AchievementCriteriaType Criteria;
		public AchievementCriteriaId AchievementCriteriaId;
		public AchievementEntryId AchievementEntryId;

		public uint CompletionFlag;							// 26
		public AchievementCriteriaGroupFlags GroupFlag;		// 27
		public uint TimeLimit;								// 29

		public AchievementEntry AchievementEntry
		{
			get { return AchievementMgr.GetAchievementEntry(AchievementEntryId); }
		}

		public virtual bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
		{
			return false;
		}

		public virtual void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
		{
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class KillCreatureAchievementCriteriaEntry : AchievementCriteriaEntry
	{
		// 0
		public NPCId CreatureId;
		public int CreatureCount;

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
		{
			return achievementProgressRecord.Counter >= Level;
		}

		public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
		{
			achievements.SetCriteriaProgress(this, value1);
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
			if(value1== 0 || (SkillId)value1!=SkillId)
				return;
			achievements.SetCriteriaProgress(this,value2);
		}

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
		{
			return achievementProgressRecord.Counter >= SkillValue;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CompleteAchievementAchievementCriteriaEntry : AchievementCriteriaEntry
	{
		// 8
		public AchievementEntryId AchievementToCompleteId;

		public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
		{
			if(AchievementToCompleteId == (AchievementEntryId)value1)
				achievements.SetCriteriaProgress(this, value2);
		}

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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
			if(value1 == 0)
				return;
			achievements.SetCriteriaProgress(this, value1, ProgressType.ProgressAccumulate);
		}

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
		{
			return achievementProgressRecord.Counter >= CompletedQuestCount;
		}

		public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
		{
			if(ZoneId == (ZoneId)value1) 
				achievements.SetCriteriaProgress(this, value2);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CompleteDailyQuestAchievementCriteriaEntry : AchievementCriteriaEntry
	{
		// 14
		public uint Unused;
		public uint CompletedQuestCount;

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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
			if(value1 == 0)
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
			achievements.SetCriteriaProgress(this, value2, ProgressType.ProgressAccumulate);
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
			if(value1 == 0)
				return;
			if((NPCId)value1 != CreatureId)
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
			if(value1 == 0)
				return;
			if(achievements.Owner.FactionGroup == (FactionGroup)value1)
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
			if(value1 == 0)
				return;
			achievements.SetCriteriaProgress(this, value1);
		}

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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
			if ((EnviromentalDamageType) value1 != EnviromentalDamageType)
				return;

			achievements.SetCriteriaProgress(this, value2, ProgressType.ProgressAccumulate);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class CompleteQuestAchievementCriteriaEntry : AchievementCriteriaEntry
	{
		// 27
		public uint QuestId;

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
		{
			return achievementProgressRecord.Counter >= 1;
		}

		public override void OnUpdate(AchievementCollection achievements, uint value1, uint value2, ObjectBase involved)
		{
			if(value1 != QuestId)
				return;
			achievements.SetCriteriaProgress(this,1);
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
		// 110
		public SpellId SpellId;
		public uint SpellCount;
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
	}

	[StructLayout(LayoutKind.Sequential)]
	public class OwnItemAchievementCriteriaEntry : AchievementCriteriaEntry
	{
		// 36
		public ItemId ItemId;
		public uint ItemCount;
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

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
		{
			return achievementProgressRecord.Counter >= (uint)SkillTierId;
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

		public override bool HasCompleted(AchievementProgressRecord achievementProgressRecord)
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


}