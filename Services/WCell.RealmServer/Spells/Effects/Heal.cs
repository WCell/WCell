/*************************************************************************
 *
 *   file		: Heal.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-23 15:13:50 +0200 (fr, 23 apr 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1282 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Spells.Auras;

namespace WCell.RealmServer.Spells.Effects
{

	/// <summary>
	/// Either just heals or trades one Rejuvenation or Regrowth for a lot of healing
	/// </summary>
	public class HealEffectHandler : SpellEffectHandler
	{
		Aura toConsume;

		public HealEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason CheckValidTarget(WorldObject target)
		{
			if (Effect.Spell.RequiredTargetAuraState == AuraState.RejuvenationOrRegrowth)
			{
				// consume Reju or Regrowth and apply its full effect at once
				toConsume = ((Unit)target).Auras.FindFirst((aura) =>
				{
					return aura.Spell.IsRejuvenationOrRegrowth && toConsume.TimeLeft > 100;
				});
				if (toConsume == null)
				{
					return SpellFailedReason.TargetAurastate;
				}
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			int effectValue;
			if (toConsume != null)
			{
				// TODO: Correct effect-value
				effectValue = toConsume.MaxApplications;
			}
			else
			{
				effectValue = CalcEffectValue();
			}
			((Unit)target).Heal(m_cast.Caster, effectValue, Effect);
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}
