/*************************************************************************
 *
 *   file		: CreateItem.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-24 17:50:44 +0100 (sø, 24 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1216 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Util.Logging;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Spells.Effects
{
	/// <summary>
	/// Creates a new Item and puts it in the caster's backpack
	/// </summary>
	public class CreateItemEffectHandler : SpellEffectHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		private SimpleSlotId slotId;
		private int amount;
		private ItemTemplate templ;

		public CreateItemEffectHandler(SpellCast cast, SpellEffect effect)
			: base(cast, effect)
		{
		}

		public override SpellFailedReason InitializeTarget(WorldObject target)
		{
			var templId = Effect.ItemId;
			templ = ItemMgr.GetTemplate(templId);
			amount = CalcEffectValue();

			if (templ == null)
			{
				log.Warn("Spell {0} referred to invalid Item {1}", Effect.Spell, templId);
				return SpellFailedReason.ItemNotFound;
			}

			// find a free slot
			// TODO: Add & use HoldFreeSlotCheck instead, so slot won't get occupied
			InventoryError error;
			slotId = ((Character)target).Inventory.FindFreeSlotCheck(templ, amount, out error);
			if (error != InventoryError.OK)
			{
				ItemHandler.SendInventoryError((Character)target, error);
				return SpellFailedReason.DontReport;
			}

			return SpellFailedReason.Ok;
		}

		protected override void Apply(WorldObject target)
		{
			if (slotId.Container[slotId.Slot] == null)
			{
				slotId.Container.DistributeUnchecked(templ, amount, true);
			}
			else
			{
				// slot got occupied in the meantime (should usually not happen)
				((Character)target).Inventory.TryAdd(templ, ref amount, true);
			}
		}

		public override ObjectTypes TargetType
		{
			get { return ObjectTypes.Player; }
		}
	}
}