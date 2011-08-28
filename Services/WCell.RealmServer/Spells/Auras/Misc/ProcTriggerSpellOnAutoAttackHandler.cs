using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public class ProcTriggerSpellOnAutoAttackHandler : ProcTriggerSpellHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			// only allow auto attack to trigger this
			return action.Spell == null || action.Spell.IsAutoRepeating;
		}
	}
}
