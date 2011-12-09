/*************************************************************************
 *
 *   file		: Duel.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-14 13:00:53 +0100 (to, 14 jan 2010) $

 *   revision		: $Rev: 1192 $
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
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// The caster requests a duel with the given target.
	/// Only used in: Duel (Id: 7266) (non-displayed Skill)
	/// </summary>
	public class DuelEffectHandler : SpellEffectHandler
	{
		public DuelEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason Initialize()
		{
			var rival = m_cast.SelectedTarget as Character;
			if (rival != null)
			{
				return Duel.CheckRequirements(m_cast.CasterChar, rival);
			}
			return SpellFailedReason.Ok;
		}

		public override bool HasOwnTargets
		{
			get
			{
				return false;
			}
		}

		public override void Apply()
		{
			Duel.InitializeDuel(m_cast.CasterChar, m_cast.SelectedTarget as Character);
		}

		public override ObjectTypes CasterType
		{
			get
			{
				return ObjectTypes.Player;
			}
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