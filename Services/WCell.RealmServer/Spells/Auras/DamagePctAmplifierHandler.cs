using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells.Auras
{
	public class DamagePctAmplifierHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var owner = Owner;
			if (owner.Auras.DamagePctAmplifiers == null)
			{
				owner.Auras.DamagePctAmplifiers = new List<SpellEffect>(3);
			}
			owner.Auras.DamagePctAmplifiers.Add(SpellEffect);
		}

		protected internal override void Remove(bool cancelled)
		{
			var owner = Owner;
			if (owner.Auras.DamagePctAmplifiers != null)
			{
				owner.Auras.DamagePctAmplifiers.Remove(SpellEffect);
			}
		}
	}
}