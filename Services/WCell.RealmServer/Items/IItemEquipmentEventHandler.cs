using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    public interface IItemEquipmentEventHandler
    {
        void OnEquip(Item item);

        void OnBeforeUnEquip(Item item);
    }
}
