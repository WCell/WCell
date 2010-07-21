using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModAttackerSpellHitChanceHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.ModAttackerSpellHitChance(m_spellEffect.MiscBitSet, EffectValue);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.ModAttackerSpellHitChance(m_spellEffect.MiscBitSet, -EffectValue);
		}
	}

	public class ModAttackerMeleeHitChanceHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, EffectValue);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.ChangeModifier(StatModifierInt.AttackerMeleeHitChance, -EffectValue);
		}
	}

	public class ModAttackerRangedHitChanceHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.ChangeModifier(StatModifierInt.AttackerRangedHitChance, EffectValue);
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.ChangeModifier(StatModifierInt.AttackerRangedHitChance, -EffectValue);
		}
	}
}
