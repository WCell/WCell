using System;
using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras
{
	public class ModStatPercentHandler : PeriodicallyUpdatedAuraEffectHandler
	{
		protected int[] m_vals;
		protected int m_singleVal;

		protected int GetModifiedValue(int value)
		{
			return (value * EffectValue + 50) / 100;
		}

		protected virtual int GetStatValue(StatType stat)
		{
			//return Owner.GetUnmodifiedBaseStatValue(stat);
			return Owner.GetBaseStatValue(stat);
		}

		protected override void Apply()
		{
			if (SpellEffect.MiscValue == -1)
			{
				// all stats
				m_vals = new int[(int)StatType.End];
				for (var stat = StatType.Strength; stat < StatType.End; stat++)
				{
					var val = GetStatValue(stat);
					var modVal = GetModifiedValue(val);
					Owner.AddStatMod(stat, modVal, m_aura.Spell.IsPassive);

					m_vals[(int)stat] = val;
				}
			}
			else
			{
				var stat = (StatType)SpellEffect.MiscValue;

				var val = GetStatValue(stat);
				var modVal = GetModifiedValue(val);
				Owner.AddStatMod(stat, modVal, m_aura.Spell.IsPassive);

				m_singleVal = val;
			}
		}

		protected override void Remove(bool cancelled)
		{
			if (SpellEffect.MiscValue == -1)
			{
				// all stats
				for (var stat = StatType.Strength; stat <= StatType.Spirit; stat++)
				{
					Owner.RemoveStatMod(stat, GetModifiedValue(m_vals[(int)stat]), m_aura.Spell.IsPassive);
				}
			}
			else
			{
				Owner.RemoveStatMod((StatType)SpellEffect.MiscValue, GetModifiedValue(m_singleVal), m_aura.Spell.IsPassive);
			}
		}

		/// <summary>
		/// Re-evaluate effect value, if stats changed
		/// </summary>
		public override void Update()
		{
			if (SpellEffect.MiscValue == -1)
			{
				// all stats
				for (var stat = StatType.Strength; stat <= StatType.Spirit; stat++)
				{
					if ((GetStatValue(stat) != m_vals[(int)stat]))
					{
						// re-apply
						Remove(false);
						Apply();
						break;
					}
				}
			}
			else
			{
				var stat = (StatType) SpellEffect.MiscValue;
				if (GetStatValue(stat) != m_singleVal)
				{
					// re-apply
					Remove(false);
					Apply();
				}
			}
		}
	}
}