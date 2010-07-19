using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Increases crit damage in %
	/// </summary>
	public class ModMeleeCritDamageBonusHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.ChangeModifier(StatModifierInt.CritDamageBonusPct, EffectValue);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.ChangeModifier(StatModifierInt.CritDamageBonusPct, -EffectValue);
		}
	}
}