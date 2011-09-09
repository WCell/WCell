using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Spells.Auras.Misc
{
	/// <summary>
	/// This handler is notified whenever an Item is equipped/unequipped
	/// </summary>
	public abstract class ItemEquipmentEventAuraHandler : AuraEffectHandler, IItemEquipmentEventHandler
	{
		protected override void Apply()
		{
			var chr = Owner as Character;
			if (chr != null)
			{
				chr.Inventory.AddEquipmentHandler(this);
			}
		}

		protected override void Remove(bool cancelled)
		{
			var chr = Owner as Character;
			if (chr != null)
			{
				chr.Inventory.RemoveEquipmentHandler(this);
			}
		}

		public abstract void OnEquip(Item item);

		public abstract void OnBeforeUnEquip(Item item);
	}
}
