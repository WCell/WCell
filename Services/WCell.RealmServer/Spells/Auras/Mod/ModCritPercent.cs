/*************************************************************************
 *
 *   file		: ModCritPercent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $

 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.RealmServer.Modifiers;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Modifies MeleeCritHitRating
    /// </summary>
    public class ModCritPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            ModValues(EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            ModValues(-EffectValue);
        }

        private void ModValues(int delta)
        {
            if (SpellEffect.Spell.HasItemRequirements)
            {
                if (SpellEffect.Spell.EquipmentSlot == Constants.Items.EquipmentSlot.ExtraWeapon)
                {
                    Owner.ChangeModifier(StatModifierInt.RangedCritChance, delta);
                }
                else
                {
                    Owner.ModCritMod(DamageSchool.Physical, delta);
                }
            }
            else
            {
                // all crit chances
                if (SpellEffect.Spell.SchoolMask == DamageSchoolMask.Physical)
                {
                    // all physical
                    Owner.ModCritMod(DamageSchool.Physical, delta);
                    Owner.ChangeModifier(StatModifierInt.RangedCritChance, delta);
                }
                else
                {
                    // given school
                    Owner.ModCritMod(SpellEffect.Spell.Schools, delta);
                }
            }
        }
    }
};