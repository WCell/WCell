using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras
{
	public class ModStatPercentHandler : AuraEffectHandler
	{
		int[] vals;
		private int val;

		protected override void Apply()
		{
			if (SpellEffect.MiscValue == -1)
			{
				// all stats
				vals = new int[(int)StatType.End];
				for (var stat = StatType.Strength; stat < StatType.End; stat++)
				{
					val = (Owner.GetUnmodifiedBaseStatValue(stat) * EffectValue + 50) / 100;
					vals[(int)stat] = val;
					Owner.AddStatMod(stat, val, m_aura.Spell.IsPassive);
				}
			}
			else
			{
				var stat = (StatType)SpellEffect.MiscValue;
				val = (Owner.GetUnmodifiedBaseStatValue(stat) * EffectValue + 50) / 100;
				Owner.AddStatMod(stat, val, m_aura.Spell.IsPassive);
			}
		}

		protected override void Remove(bool cancelled)
		{
			if (SpellEffect.MiscValue == -1)
			{
				// all stats
				for (var stat = StatType.Strength; stat <= StatType.Spirit; stat++)
				{
					Owner.RemoveStatMod(stat, vals[(int)stat], m_aura.Spell.IsPassive);
				}
			}
			else
			{
				Owner.RemoveStatMod((StatType)SpellEffect.MiscValue, val, m_aura.Spell.IsPassive);
			}
		}
	}
}