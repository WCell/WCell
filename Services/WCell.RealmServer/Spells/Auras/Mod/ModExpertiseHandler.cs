using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
	public class ModExpertiseHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			var owner = Owner as Character;
			if (owner != null)
			{
				owner.IntMods[(int)StatModifierInt.Expertise] += EffectValue;
			}
		}

		protected internal override void Remove(bool cancelled)
		{
			var owner = Owner as Character;
			if (owner != null)
			{
				owner.IntMods[(int)StatModifierInt.Expertise] -= EffectValue;
			}
		}
	}
}