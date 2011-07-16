using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Passive
{
	public class ForceAutoRunForwardHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			Owner.UnitFlags2 |= UnitFlags2.ForceAutoRunForward;
		}

		protected override void Remove(bool cancelled)
		{
			Owner.UnitFlags2 &= ~UnitFlags2.ForceAutoRunForward;
		}
	}
}
