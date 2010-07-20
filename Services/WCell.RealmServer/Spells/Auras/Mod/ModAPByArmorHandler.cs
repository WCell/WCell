using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// Increases your attack power by $s2 for every ${$m1*$m2} armor value you have.
	/// </summary>
	public class ModAPByArmorHandler : AuraEffectHandler
	{
		private int amt;

		protected internal override void Apply()
		{
			//var modEffect = SpellEffect.Spell.GetEffect(SpellEffectType.Dummy);
			//if (modEffect == null)
			//{
			//    LogManager.GetCurrentClassLogger().Error("ModAPByArmorHandler is missing dummy mod effect in Spell {0}", SpellEffect.Spell);
			//    return;
			//}
			//var apFactor = modEffect.CalcEffectValue();

			//var armorStep = apFactor*EffectValue;

			amt = (Owner.Armor + EffectValue - 1) / EffectValue;
			if (amt > 0)
			{
				Owner.MeleeAttackPowerModsPos += amt;
			}
			else
			{
				Owner.MeleeAttackPowerModsNeg -= amt;
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			if (amt > 0)
			{
				Owner.MeleeAttackPowerModsPos -= amt;
			}
			else
			{
				Owner.MeleeAttackPowerModsNeg += amt;
			}
		}
	}
}
