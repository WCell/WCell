using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModDamageDoneVersusCreatureTypeHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var chr = Owner as Character;
			if (chr != null)
			{
				chr.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, EffectValue);
			}
		}

		protected override void Remove(bool cancelled)
		{
			var chr = Owner as Character;
			if (chr != null)
			{
				chr.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, -EffectValue);
			}
		}
	}
}
