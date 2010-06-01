using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
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
	public class ModArmorPenetrationHandler : AttackModifierHandler
	{
		public override void ModAttack(AttackAction action)
		{
			var weapon = action.Weapon as Item;
			var spell = m_spellEffect.Spell;

			// Only for physical damage
			// Weapon must match Spell requirements (if restricted)
			if (action.UsedSchool == DamageSchool.Physical && (spell.RequiredItemClass == 0 || (weapon != null &&
					(weapon.Template.Class == m_spellEffect.Spell.RequiredItemClass && 
						(spell.RequiredItemSubClassMask == 0 || spell.RequiredItemSubClassMask.HasAnyFlag(weapon.Template.SubClassMask))))))
			{
				action.ResistPct -= EffectValue;
			}
		}
	}
}
