/*************************************************************************
 *
 *   file		: ModAllCooldownDuration.cs
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
	/// Only in: Noxious Breath (Id: 24818)
	/// </summary>
	public class ModAllCooldownDurationHandler : AuraEffectHandler
	{
		protected internal override void Apply()
		{
			// value is probably %
		}
	
		/// <summary>
		/// If the amount of duration to be applied is negative, we have a positive effect 
		/// (because we decrease cooldown)
		/// </summary>
		public override bool IsPositive
		{
			get
			{
				return EffectValue <= 0;
			}
		}
	}
};