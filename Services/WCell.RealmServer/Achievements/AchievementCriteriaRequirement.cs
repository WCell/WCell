using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Content;
using WCell.RealmServer.Entities;
using WCell.Util.Data;

namespace WCell.RealmServer.Achievements
{
    [DataHolder]
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

        public virtual bool Meets(Character chr, Unit target = null, uint miscValue = 0u)
        {
            return true;
        }
    }

    #region Criteria Requirement Type
    public class AchievementCriteriaRequirementCreature : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            if (target == null || !(target is Unit))
                return false;
            else return (Value1 == target.EntryId);
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
        //public uint minLevel;
    }

    
    public class AchievementCriteriaRequirementGender : AchievementCriteriaRequirement
    {
        //public GenderType gender;
    }

    
    public class AchievementCriteriaRequirementDisabled : AchievementCriteriaRequirement
    {
        public override bool Meets(Character chr, Unit target, uint miscValue)
        {
            return false;
        }
    }

    
    public class AchievementCriteriaRequirementMapDifficulty : AchievementCriteriaRequirement
    {
        //public uint difficulty;
    }

    
    public class AchievementCriteriaRequirementMapPlayerCount : AchievementCriteriaRequirement
    {
        //public uint MaxCount;
    }

    
    public class AchievementCriteriaRequirementTeam : AchievementCriteriaRequirement
    {
        //public FactionGroup Faction;
    }

    
    public class AchievementCriteriaRequirementDrunk : AchievementCriteriaRequirement
    {
        //public uint State;
    }

    
    public class AchievementCriteriaRequirementHoliday : AchievementCriteriaRequirement
    {
        //public uint holidayId;
    }

    
    public class AchievementCriteriaRequirementBgLossTeamScore : AchievementCriteriaRequirement
    {
        //public uint minScore;
        //public uint maxScore;
    }

    
    public class AchievementCriteriaRequirementInstanceScript : AchievementCriteriaRequirement
    {
    }

    
    public class AchievementCriteriaRequirementEquippedItemLevel : AchievementCriteriaRequirement
    {
        //public uint itemLevel;
        //public ItemQuality itemQuality;
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

        public bool Meets(Character chr, uint miscValue1 = 0u, Unit involved = null)
        {
            foreach(AchievementCriteriaRequirementCreator requirementCreator in Requirements)
            {
                var requirement = requirementCreator();
                if (!(requirement.Meets(chr, involved, miscValue1)))
                    return false;
                else
                    return true;
            }
            return true;
	    }
    }
}
