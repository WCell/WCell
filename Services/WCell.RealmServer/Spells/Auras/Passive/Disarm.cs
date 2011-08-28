/*************************************************************************
 *
 *   file		: Disarmed.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants.Items;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
	/// <summary>
	/// Simply disarm melee and ranged for now
	/// </summary>
	public abstract class DisarmHandler : AuraEffectHandler
	{
		public abstract InventorySlotType DisarmType { get; }

		protected override void Apply()
		{
			Owner.IncMechanicCount(SpellMechanic.Disarmed);
			Owner.SetDisarmed(DisarmType);
		}

		protected override void Remove(bool cancelled)
		{
			Owner.DecMechanicCount(SpellMechanic.Disarmed);
			Owner.UnsetDisarmed(DisarmType);
		}
	}

	public class DisarmMainHandHandler : DisarmHandler
	{
		public override InventorySlotType DisarmType
		{
			get { return InventorySlotType.WeaponMainHand; }
		}
	}

	public class DisarmOffHandHandler : DisarmHandler
	{
		public override InventorySlotType DisarmType
		{
			get { return InventorySlotType.WeaponOffHand; }
		}
	}

	public class DisarmRangedHandler : DisarmHandler
	{
		public override InventorySlotType DisarmType
		{
			get { return InventorySlotType.WeaponRanged; }
		}
	}
};