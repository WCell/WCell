using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	/// <summary>
	/// Increases crit damage in %
	/// </summary>
	public class ModMeleeCritDamageBonusHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.ChangeModifier(StatModifierInt.CritDamageBonusPct, EffectValue);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.ChangeModifier(StatModifierInt.CritDamageBonusPct, -EffectValue);
		}
	}
}