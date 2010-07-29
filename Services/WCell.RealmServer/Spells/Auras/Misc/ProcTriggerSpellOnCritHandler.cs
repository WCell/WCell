using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells.Auras.Handlers;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	public class ProcTriggerSpellOnCritHandler : ProcTriggerSpellHandler
	{
		public override bool CanProcBeTriggeredBy(IUnitAction action)
		{
			// only allow critical procs to trigger this
			return action.IsCritical;
		}
	}
}
