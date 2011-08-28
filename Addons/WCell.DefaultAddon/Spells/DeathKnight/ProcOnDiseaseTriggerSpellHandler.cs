using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.Addons.Default.Spells.DeathKnight
{
	/// <summary>
	/// CF may only proc on diseases
	/// </summary>
	public class ProcOnDiseaseTriggerSpellHandler : ProcTriggerSpellHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			return action.Spell != null && action.Spell.DispelType == DispelType.Disease &&
				action.Spell != m_spellEffect.TriggerSpell;	// prevent infinite loop
		}
	}
}
