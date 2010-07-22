using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// TODO: Reapply when AP changes
	/// </summary>
	public class ModSpellPowerByAPPctHandler : AuraEffectHandler
	{
		private int[] values;

		protected internal override void Apply()
		{
			var owner = Owner as Character;
			if (owner == null)
			{
				return;
			}

			values = new int[m_spellEffect.MiscBitSet.Length];
			for (var i = 0; i < m_spellEffect.MiscBitSet.Length; i++)
			{
				var school = m_spellEffect.MiscBitSet[i];
				var sp = owner.GetDamageDoneMod((DamageSchool) school);
				var val = (sp*EffectValue + 50)/100;
				values[i] = val;
				owner.AddDamageMod((DamageSchool) school, val);
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			var owner = Owner as Character;
			if (owner == null)
			{
				return;
			}

			for (var i = 0; i < m_spellEffect.MiscBitSet.Length; i++)
			{
				var school = m_spellEffect.MiscBitSet[i];
				owner.RemoveDamageMod((DamageSchool) school, values[i]);
			}
		}
	}
}
