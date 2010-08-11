using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Auras.Misc;

namespace WCell.RealmServer.Auras.Effects
{
	/// <summary>
	/// Reduces victim armor by the given value in %
	/// </summary>
	public class ModArmorPenetrationHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			var owner = Owner as Character;
			if (owner != null)
				owner.ModCombatRating(CombatRating.ArmorPenetration, EffectValue);
		}

		protected override void Remove(bool cancelled)
		{
			var owner = Owner as Character;
			if (owner != null)
				owner.ModCombatRating(CombatRating.ArmorPenetration, -EffectValue);
		}
	}
}