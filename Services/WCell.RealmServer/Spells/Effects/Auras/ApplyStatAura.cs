/*************************************************************************
 *
 *   file		: ApplyStatAura.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-09-14 15:20:56 +0200 (ma, 14 sep 2009) $

 *   revision		: $Rev: 1096 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class ApplyStatAuraEffectHandler : ApplyAuraEffectHandler
	{
		public ApplyStatAuraEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			// TODO: ApplyStatAuraEffectHandler
		}
	}
}