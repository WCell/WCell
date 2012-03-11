using WCell.Constants.Items;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Items
{
    /// <summary>
    /// Handles Items when moved to/from certain slots
    /// </summary>
    public interface IItemSlotHandler
    {
        /// <summary>
        /// Is called before adding to check whether the item may be added to the corresponding slot
        /// (given the case that the corresponding slot is valid and unoccupied)
        /// </summary>
        void CheckAdd(int slot, int amount, IMountableItem templ, ref InventoryError err);

        /// <summary>
        /// Is called before removing the given item to check whether it may actually be removed
        /// </summary>
        void CheckRemove(int slot, IMountableItem templ, ref InventoryError err);

        /// <summary>
        /// Is called after the given item is added to the given slot
        /// </summary>
        void Added(Item item);

        /// <summary>
        /// Is called after the given item is removed from the given slot
        /// </summary>
        void Removed(int slot, Item item);
    }
}