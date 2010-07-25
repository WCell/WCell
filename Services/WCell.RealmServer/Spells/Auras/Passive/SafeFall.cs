/*************************************************************************
 *
 *   file		: SafeFall.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-13 22:54:37 +0100 (fr, 13 mar 2009) $
 *   last author	: $LastChangedBy: meshok $
 *   revision		: $Rev: 804 $
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
	/// Reduces the falling damage and allows to fall longer without taking any damage at all.
	/// The amount of yards the has been falling for is reduced by this number for the damage formular
	/// </summary>
	public class SafeFallHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			m_aura.Auras.Owner.SafeFall += EffectValue;
		}

		protected override void Remove(bool cancelled)
		{
			m_aura.Auras.Owner.SafeFall -= EffectValue;
		}

	}
};