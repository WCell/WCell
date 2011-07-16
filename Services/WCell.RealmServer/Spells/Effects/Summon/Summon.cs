/*************************************************************************
 *
 *   file		: Summon.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-17 17:38:11 +0100 (s? 17 jan 2010) $

 *   revision		: $Rev: 1198 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.NPCs;
using WCell.Constants.Pets;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;
using WCell.Util.Logging;


namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Summons a friendly companion, Pets, Guardians or Totems
	/// TODO: Handle Totems
	/// </summary>
	public class SummonEffectHandler : SpellEffectHandler
	{
		protected NPCEntry entry;

		public SummonEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
			// MiscValueB:
			// 41 -> NonCombat Companion
			// 
		}

		public override void Initialize(ref SpellFailedReason failReason)
		{
			var id = (NPCId)Effect.MiscValue;
			entry = NPCMgr.GetEntry(id);
			if (entry == null)
			{
				LogManager.GetCurrentClassLogger().Warn("The NPC for Summon-Spell {0} does not exist: {1} (Are NPCs loaded?)", Effect.Spell, id);
				failReason = SpellFailedReason.Error;
				return;
			}
		}

		public virtual SummonType SummonType
		{
			get { return (SummonType)Effect.MiscValueB; }
		}

		public override void Apply()
		{
			var handler = SpellHandler.GetSummonEntry(SummonType);
			Summon(handler);
		}

		protected virtual void Summon(SpellSummonEntry summonEntry)
		{
			var caster = m_cast.CasterUnit;

			Vector3 targetLoc;
			if (m_cast.TargetLoc.X != 0)
			{
				targetLoc = m_cast.TargetLoc;
			}
			else
			{
				targetLoc = caster.Position;
			}
			
			var effectValue = CalcEffectValue();
			int amount;
			if (summonEntry.DetermineAmountBySpellEffect)
			{
				amount = effectValue > 0 ? effectValue : 1;
			}
			else
			{
				amount = 1;
			}

			for (var i = 0; i < amount; i++)
			{
				var minion = summonEntry.Handler.Summon(m_cast, ref targetLoc, entry);
				minion.CreationSpellId = Effect.Spell.SpellId;
				if (!summonEntry.DetermineAmountBySpellEffect && effectValue > 1)
				{
					// effectValue represents the health
					minion.Health = minion.BaseHealth = effectValue;
				}
			}
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}
	}
}