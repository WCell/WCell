using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModSpellDamageByPercentOfStatHandler : AuraEffectHandler
	{
		private int value;

		protected internal override void Apply()
		{
			var stat = (StatType)SpellEffect.MiscValueB;

			value = (Owner.GetStatValue(stat) * EffectValue + 50) / 100;

			// TODO: Update when stat changes
			// TODO: Apply as white bonus, if aura is passive?
			if (m_aura.Auras.Owner is Character)
			{
				((Character)m_aura.Auras.Owner).AddDamageMod(m_spellEffect.MiscBitSet, value);
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			if (m_aura.Auras.Owner is Character)
			{
				((Character)m_aura.Auras.Owner).RemoveDamageMod(m_spellEffect.MiscBitSet, value);
			}
		}
	}
}