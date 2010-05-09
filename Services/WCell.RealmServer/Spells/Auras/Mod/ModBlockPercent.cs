/*************************************************************************
 *
 *   file		: ModBlockPercent.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-05-08 23:23:29 +0200 (lø, 08 maj 2010) $
 *   last author	: $LastChangedBy: XTZGZoReX $
 *   revision		: $Rev: 1290 $
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
	/// Increases Chance to block
	/// </summary>
	public class ModBlockPercentHandler : AuraEffectHandler
	{
        protected internal override void Apply()
        {
            m_aura.Auras.Owner.ChangeModifier(StatModifierInt.BlockChance, EffectValue);
        }

        protected internal override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.ChangeModifier(StatModifierInt.BlockChance, -EffectValue);
        }
	}
};
