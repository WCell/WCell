﻿/*************************************************************************
 *
 *   file		: ModManaRegenInterrupt.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
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
    /// Boosts interrupted Mana regen.
    /// See: http://www.wowwiki.com/Formulas:Mana_Regen#Five_Second_Rule
    /// </summary>
    public class ModManaRegenInterruptHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ManaRegenInterruptPct, EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.ChangeModifier(StatModifierInt.ManaRegenInterruptPct, -EffectValue);
        }
    }
};