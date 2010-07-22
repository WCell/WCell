using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras.Passive
{
	public class UnattackableHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			Owner.Invulnerable++;
		}

		protected internal override void Remove(bool cancelled)
		{
			Owner.Invulnerable--;
		}
	}
}