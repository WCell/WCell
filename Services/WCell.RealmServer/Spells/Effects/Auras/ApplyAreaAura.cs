/*************************************************************************
 *
 *   file		: ApplyAreaAura.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1192 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Represents a mobile Aura-Effect that is applied to everyone in the area.
	/// The SpellCast-object creates AreaAuras explicitely.
	/// </summary>
	public class ApplyAreaAuraEffectHandler : ApplyAuraEffectHandler
	{
		public ApplyAreaAuraEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override void Apply()
		{
			// do nothing (AreaAuras will be created upon SpellCast)
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}
