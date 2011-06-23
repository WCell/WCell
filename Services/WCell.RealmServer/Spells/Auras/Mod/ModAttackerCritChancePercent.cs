/*************************************************************************
 *
 *   file		: ModAttackerCritChancePercent.cs
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
	public class ModAttackerCritChancePercentHandler : AuraEffectHandler
	{
		float amount;

		protected override void Apply()
		{
			Owner.AttackerPhysicalCritChancePercentMod += EffectValue;
		}

		protected override void Remove(bool cancelled)
		{
			Owner.AttackerPhysicalCritChancePercentMod -= EffectValue;
		}
	}
};