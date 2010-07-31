using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	public static class DeathKnightFixes
	{
		[Initialization(InitializationPass.Second)]
		public static void FixDeathKnight()
		{
			//Forceful Reflection is missing stat values
			SpellHandler.Apply(spell =>
			{
				spell.Effects[0].MiscValue = (int)CombatRating.Parry;
				spell.Effects[0].MiscValueB = (int)StatType.Strength;
			},
			SpellId.ClassSkillForcefulDeflectionPassive);

			// Only one Presence may be active at a time
			AuraHandler.AddAuraGroup(SpellLineId.DeathKnightBloodPresence, SpellLineId.DeathKnightFrostPresence, SpellLineId.DeathKnightUnholyPresence);
		}
	}
}