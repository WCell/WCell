/*************************************************************************
 *
 *   file		: TameCreature.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-16 21:33:51 +0100 (lø, 16 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1197 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Pets;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Updates;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Tames the target
	/// </summary>
	public class TameCreatureEffectHandler : SpellEffectHandler
	{
		public TameCreatureEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			if (!(target is NPC))
			{
				return SpellFailedReason.BadTargets;
			}

			if (SpellCast.CheckTame(m_cast.CasterObject as Character, (NPC)target) != TameFailReason.Ok)
			{
				return SpellFailedReason.DontReport;
			}
			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			var caster = (Unit)m_cast.CasterObject;
			if (caster is Character)
			{
				((Character)caster).MakePet((NPC)target);
			}
			else
			{
				caster.Enslave((NPC)target);

				((NPC)target).Summoner = caster;
				((NPC)target).Creator = caster.EntityId;
			}
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Unit; }
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}