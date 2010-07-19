/*************************************************************************
 *
 *   file		: RetainComboPoints.cs
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
	/// Retains combo points, applied through another effect of this SpellCast (removes them if not used in time)
	/// Only in: Rogue Subtlety Premeditation (Id: 14183)
	/// </summary>
	public class RetainComboPointsHandler : AuraEffectHandler
	{

		protected internal override void Apply()
		{
			
		}

	}
};