using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>
    /// For BeastMastery.  Adds 4 talent points to all controlled pets.
    /// </summary>
    public class ModPetTalentPointsHandler : AuraEffectHandler
    {
        protected internal override void CheckInitialize(ObjectReference casterReference, Unit target, ref SpellFailedReason failReason)
		{
			if (!(target is Character)) return;
            var chr = (Character)target;
            if (chr.Class != ClassId.Hunter)
            {
                failReason = SpellFailedReason.BadTargets;
            }
		}

        protected override void Apply()
		{
            var chr = m_aura.Auras.Owner as Character;
            if (chr != null) 
		    {
				chr.PetBonusTalentPoints += BonusPoints;
		    }
		}

		protected override void Remove(bool cancelled)
		{
		    var chr = m_aura.Auras.Owner as Character;
		    if (chr != null)
		    {
				chr.PetBonusTalentPoints -= BonusPoints;
		    }
		}

    	public int BonusPoints
    	{
			get { return m_aura.Spell.Effects[0].BasePoints + 1; }
    	}
    }
}