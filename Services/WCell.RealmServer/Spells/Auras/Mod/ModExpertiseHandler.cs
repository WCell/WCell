using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModExpertiseHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			m_aura.Auras.Owner.ChangeModifier(StatModifierInt.Expertise, EffectValue);
		}

		protected override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.ChangeModifier(StatModifierInt.Expertise, -EffectValue);
		}
	}
}