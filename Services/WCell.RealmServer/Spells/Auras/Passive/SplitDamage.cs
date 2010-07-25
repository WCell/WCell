/*************************************************************************
 *
 *   file		: SplitDamage.cs
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

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Allows to split all received damage with the caster
	/// Usually comes with a Dummy Aura.
	/// Description often points at at effect-values of: Soul Link (Id: 25228)
	/// </summary>
	public class SplitDamageHandler : AuraEffectHandler
	{

		protected override void Apply()
		{
			// DamageTypeMask types = (DamageTypeMask)m_spellEffect.MiscValue;
			// EffectValue = % of damage to transfer to caster
		}

	}
};