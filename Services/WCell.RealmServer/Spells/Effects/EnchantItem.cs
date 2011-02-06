/*************************************************************************
 *
 *   file		: EnchantItem.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-25 18:19:39 +0100 (ma, 25 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
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

		public override void Initialize(ref SpellFailedReason failReason)
		{
			if (m_cast.TargetItem.Template.Level < Effect.Spell.BaseLevel)
			{
				failReason = SpellFailedReason.Lowlevel;
				return;
			}

			enchantEntry = EnchantMgr.GetEnchantmentEntry((uint)Effect.MiscValue);
			if (enchantEntry == null)
			{
				log.Error("Spell {0} refers to invalid EnchantmentEntry {1}", Effect.Spell, Effect.MiscValue);
				failReason = SpellFailedReason.Error;
			}
			else if (!enchantEntry.CheckRequirements(m_cast.CasterUnit))
			{
				failReason = SpellFailedReason.MinSkill;
			}
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