/*************************************************************************
 *
 *   file		: EnchantItem.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 18:19:39 +0100 (ma, 25 jan 2010) $

 *   revision		: $Rev: 1222 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using NLog;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Items.Enchanting;

namespace WCell.RealmServer.Spells.Effects
{
	public class EnchantItemEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		ItemEnchantmentEntry enchantEntry;

		public EnchantItemEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason Initialize()
		{
			if (m_cast.TargetItem == null)
			{
				return SpellFailedReason.ItemGone;
			}

			if (m_cast.TargetItem.Template.Level < Effect.Spell.BaseLevel)
			{
				return SpellFailedReason.TargetLowlevel;
			}

			enchantEntry = EnchantMgr.GetEnchantmentEntry((uint)Effect.MiscValue);
			if (enchantEntry == null)
			{
				log.Error("Spell {0} refers to invalid EnchantmentEntry {1}", Effect.Spell, Effect.MiscValue);
				return SpellFailedReason.Error;
			}
			if (!enchantEntry.CheckRequirements(m_cast.CasterUnit))
			{
				return SpellFailedReason.MinSkill;
			}

			return SpellFailedReason.Ok;
		}

		public virtual EnchantSlot EnchantSlot
		{
			get { return EnchantSlot.Permanent; }
		}

		public override void Apply()
		{
			var item = m_cast.TargetItem;
			var duration = CalcEffectValue();
			if (duration < 0)
			{
				duration = 0;
			}
			item.ApplyEnchant(enchantEntry, EnchantSlot, duration, 0, true);
		}

		public override bool HasOwnTargets
		{
			get { return false; }
		}

		public override ObjectTypes CasterType
		{
			get { return ObjectTypes.Unit; }
		}
	}
}