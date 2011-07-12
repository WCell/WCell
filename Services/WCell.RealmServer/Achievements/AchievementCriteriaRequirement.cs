using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Titles;
using WCell.Util.Data;

namespace WCell.RealmServer.Achievements
{
    [DataHolder("Type")]
    [DependingProducer(AchievementCriteriaRequirementType.None, typeof(AchievementCriteriaRequirement))]
    [DependingProducer(AchievementCriteriaRequirementType.Creature, typeof(AchievementCriteriaRequirementCreature))]
    [DependingProducer(AchievementCriteriaRequirementType.PlayerClassRace, typeof(AchievementCriteriaRequirementPlayerClassRace))]
    [DependingProducer(AchievementCriteriaRequirementType.PlayerLessHealth, typeof(AchievementCriteriaRequirementPlayerLessHealth))]
    [DependingProducer(AchievementCriteriaRequirementType.PlayerDead, typeof(AchievementCriteriaRequirementPlayerDead))]
    [DependingProducer(AchievementCriteriaRequirementType.Aura1, typeof(AchievementCriteriaRequirementAura1))]
    [DependingProducer(AchievementCriteriaRequirementType.Area, typeof(AchievementCriteriaRequirementArea))]
    [DependingProducer(AchievementCriteriaRequirementType.Aura2, typeof(AchievementCriteriaRequirementAura2))]
    [DependingProducer(AchievementCriteriaRequirementType.Value, typeof(AchievementCriteriaRequirementValue))]
    [DependingProducer(AchievementCriteriaRequirementType.Level, typeof(AchievementCriteriaRequirementLevel))]
    [DependingProducer(AchievementCriteriaRequirementType.Gender, typeof(AchievementCriteriaRequirementGender))]
    [DependingProducer(AchievementCriteriaRequirementType.Disabled, typeof(AchievementCriteriaRequirementDisabled))]
    [DependingProducer(AchievementCriteriaRequirementType.MapDifficulty, typeof(AchievementCriteriaRequirementMapDifficulty))]
    [DependingProducer(AchievementCriteriaRequirementType.MapPlayerCount, typeof(AchievementCriteriaRequirementMapPlayerCount))]
    [DependingProducer(AchievementCriteriaRequirementType.Team, typeof(AchievementCriteriaRequirementTeam))]
    [DependingProducer(AchievementCriteriaRequirementType.Drunk, typeof(AchievementCriteriaRequirementDrunk))]
    [DependingProducer(AchievementCriteriaRequirementType.Holiday, typeof(AchievementCriteriaRequirementHoliday))]
    [DependingProducer(AchievementCriteriaRequirementType.BgLossTeamScore, typeof(AchievementCriteriaRequirementBgLossTeamScore))]
    [DependingProducer(AchievementCriteriaRequirementType.InstanceScript, typeof(AchievementCriteriaRequirementInstanceScript))]
    [DependingProducer(AchievementCriteriaRequirementType.EquippedItemLevel, typeof(AchievementCriteriaRequirementEquippedItemLevel))]
    [DependingProducer(AchievementCriteriaRequirementType.NthBirthday, typeof(AchievementCriteriaRequirementNthBirthday))]
    [DependingProducer(AchievementCriteriaRequirementType.KnownTitle, typeof(AchievementCriteriaRequirementKnownTitle))]
    public class AchievementCriteriaRequirement : IDataHolder
    {
        public uint CriteriaId;
        public AchievementCriteriaRequirementType Type;
        public uint Value1, Value2;

        public void FinalizeDataHolder()
        {
            var criteriaEntry = AchievementMgr.GetCriteriaEntryById(CriteriaId);
			if (criteriaEntry == null)
            {
                ContentMgr.OnInvalidDBData("{0} had an invalid criteria id.", this);
                return;
            }
			criteriaEntry.RequirementSet.Add(this);
        }

        public virtual bool Meets(Character chr, Unit target, uint miscValue)
        {
            return true;
        }
    }

    #region Criteria Requirement Type
    public class AchievementCriteriaRequirementCreature : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null)
                return false;
            else return Value1 == target.EntryId;
        }
    }
    public class AchievementCriteriaRequirementPlayerClassRace : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null || !(target is Character))
                return false;
            if (Value1 != 0 && (ClassId)Value1 != ((Character)target).Class)
                return false;
            if (Value2 != 0 && (RaceId)Value2 != ((Character)target).Race)
                return false;
            return true;
        }
    }

    
    public class AchievementCriteriaRequirementPlayerLessHealth : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null || !(target is Character))
                return false;
           return target.HealthPct == Value1;
        }
    }

    
    public class AchievementCriteriaRequirementPlayerDead : AchievementCriteriaRequirement
    {
        
    }

    
    public class AchievementCriteriaRequirementAura1 : AchievementCriteriaRequirement
    {
        //SpellId spellId;
        //uint effectIndex;
    }

    
    public class AchievementCriteriaRequirementArea : AchievementCriteriaRequirement
    {
        //public ZoneId zoneId;
        //public uint areaId;
    }

    
    public class AchievementCriteriaRequirementAura2 : AchievementCriteriaRequirement
    {
        //public SpellId spellId;
        //public uint effectIndex;
    }

    
    public class AchievementCriteriaRequirementValue : AchievementCriteriaRequirement
    {
        //public uint minValue;
    }

    
    public class AchievementCriteriaRequirementLevel : AchievementCriteriaRequirement
    {
        // 9
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null)
                return false;
            var charTarget = target as Character;
            if (charTarget != null)
            {
                if (charTarget.Class == ClassId.DeathKnight)
                    if (Value1 < 55)
                        return false; // Do not reward achievement for death knights when level < 55
            }
            return target.Level >= Value1;
        }
    }

    
    public class AchievementCriteriaRequirementGender : AchievementCriteriaRequirement
    {
        // 10
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null)
                return false;
            return target.Gender == (GenderType)Value1;
        }
    }

    
    public class AchievementCriteriaRequirementDisabled : AchievementCriteriaRequirement
    {
        // 11
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return false;
        }
    }

    
    public class AchievementCriteriaRequirementMapDifficulty : AchievementCriteriaRequirement
    {
        // 12
        //public uint difficulty;
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return chr.Map.DifficultyIndex == Value1;
        }
    }

    
    public class AchievementCriteriaRequirementMapPlayerCount : AchievementCriteriaRequirement
    {
        // 13
        //public uint MaxCount;
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return chr.Map.PlayerCount <= Value1;
        } 
    }

    
    public class AchievementCriteriaRequirementTeam : AchievementCriteriaRequirement
    {
        // 14
        //public FactionGroup Faction;
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null || !(target is Character))
                return false;
            return target.FactionId == (FactionId)Value1;
        }
    }

    
    public class AchievementCriteriaRequirementDrunk : AchievementCriteriaRequirement
    {
        // 15
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return chr.DrunkState >= Value1;
        }
    }

    
    public class AchievementCriteriaRequirementHoliday : AchievementCriteriaRequirement
    {
        // 16
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return WorldEventMgr.IsHolidayActive(Value1);
        }
    }

    
    public class AchievementCriteriaRequirementBgLossTeamScore : AchievementCriteriaRequirement
    {
        // 17
        //public uint minScore;
        //public uint maxScore;
    }

    
    public class AchievementCriteriaRequirementInstanceScript : AchievementCriteriaRequirement
    {
        // 18
    }

    
    public class AchievementCriteriaRequirementEquippedItemLevel : AchievementCriteriaRequirement
    {
        // 19
        //public uint itemLevel;
        //public ItemQuality itemQuality;
    }

    public class AchievementCriteriaRequirementNthBirthday : AchievementCriteriaRequirement
    {
        // 20
        // public uint N;
    }

    public class AchievementCriteriaRequirementKnownTitle : AchievementCriteriaRequirement
    {
        // 21
        //public uint knownTitleId;
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            CharacterTitleEntry title = TitleMgr.GetTitleEntry((TitleId)Value1);
            if (title == null)
                return false;
            return chr != null && chr.HasTitle(title.TitleId);
        }
    }
    #endregion

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
    public class AchievementCriteriaRequirementSet
    {
        public readonly uint CriteriaId;
        public readonly List<AchievementCriteriaRequirementCreator> Requirements = new List<AchievementCriteriaRequirementCreator>();

		public AchievementCriteriaRequirementSet(uint id)
		{
			CriteriaId = id;
		}

        public void Add(AchievementCriteriaRequirement requirement)
        {
            AchievementCriteriaRequirementCreator creator = AchievementMgr.GetCriteriaRequirementCreator(requirement.Type);
            Requirements.Add(creator);
        }

        public bool Meets(Character chr, Unit involved, uint miscValue)
        {
            foreach(AchievementCriteriaRequirementCreator requirementCreator in Requirements)
            {
                var requirement = requirementCreator();
                if (!(requirement.Meets(chr, involved, miscValue)))
                    return false;
                else
                    return true;
            }
            return true;
	    }
    }
}
