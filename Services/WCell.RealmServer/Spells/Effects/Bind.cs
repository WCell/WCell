/*************************************************************************
 *
 *   file		: ForgetSpecialization.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 14:58:12 +0800 (Sat, 07 Mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Effects
{
	public class BindEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public BindEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		protected override void Apply(WorldObject target)
		{
			//((Character)target).BindTo(target, );
		}

		public override ObjectTypes TargetType
		{
			get
			{
				return ObjectTypes.Player;
			}
		}
	}
}
