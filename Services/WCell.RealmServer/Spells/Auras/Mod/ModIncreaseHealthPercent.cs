/*************************************************************************
 *
 *   file		: ModIncreaseHealthPercent.cs
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

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModIncreaseHealthPercentHandler : AuraEffectHandler
    {
        int health;

        protected override void Apply()
        {
            health = ((Owner.MaxHealth * EffectValue) + 50) / 100;	//rounded

            Owner.Health += health;
            Owner.MaxHealthModScalar += EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            Owner.Health -= health;
            Owner.MaxHealthModScalar -= EffectValue / 100f;
        }
    }
};