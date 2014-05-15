using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Database;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Guilds
{
    public partial class GuildBankTab
    {

        public ItemRecord this[int slot]
        {
            get
            {
                if (slot > GuildMgr.MAX_BANK_TAB_SLOTS)
                    return null;
                return slot < ItemRecords.Count - 1 ? ItemRecords[slot] : null;
            }
            set
            {
                if (slot > GuildMgr.MAX_BANK_TAB_SLOTS)
                    return;

                if(value == null)
                {
                    Items[slot] = null;
                    ItemRecords[slot] = null;
                    return;
                }

                value.Slot = slot;

                Items[slot] = new GuildBankTabItem{
                    Guid = value.Guid,
                    TabSlot = (byte)slot
                };

                ItemRecords[slot] = value;
            }
        }

        /// <summary>
        /// Iterates over all slots in this GuildBankTab.
        /// </summary>
        /// <param name="callback">Function that is called on each Slot. This should return true to continue iterating.</param>
        public void Iterate(Func<ItemRecord, bool> callback)
        {
            for (var i = 0; i < GuildMgr.MAX_BANK_TAB_SLOTS; i++)
            {
                if (!callback(ItemRecords[i]))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Applies a function to all ItemRecords in this GuildBankTab.
        /// </summary>
        /// <param name="callback"></param>
        public void ForeachItem(Action<ItemRecord> callback)
        {
            foreach (var item in ItemRecords)
            {
                callback(item);
            }
        }

        /// <summary>
        /// Tries to store the given item in the given slot. No merge attempted.
        /// </summary>
        /// <param name="item">The item to store.</param>
        /// <param name="slot">The slot to store it in.</param>
        /// <returns>The <see cref="ItemRecord" /> that was in the slot (or the original <see cref="ItemRecord" /> minus the items that were merged) 
        /// or null if the store/merge was successful.</returns>
        public ItemRecord StoreItemInSlot(ItemRecord item, int slot)
        {
            return StoreItemInSlot(item, slot, false);
        }
        
        /// <summary>
        /// Places the given item in the given slot (or tries mergeing at slot> if indicated).
        /// Make sure that the depositer has deposit rights to this BankTab!
        /// </summary>
        /// <param name="item">The <see cref="ItemRecord" /> to store.</param>
        /// <param name="slot">The slotId where you want to store.</param>
        /// <param name="allowMerge">Whether or not to try and merge the stacks.</param>
        /// <returns>The <see cref="ItemRecord" /> that was in the slot (or the original <see cref="ItemRecord" /> minus the items that were merged) 
        /// or null if the store/merge was successful.</returns>
        public ItemRecord StoreItemInSlot(ItemRecord item, int slot, bool allowMerge)
        {
            return StoreItemInSlot(item, item.Amount, slot, allowMerge);
        }

        /// <summary>
        /// Places the given amount of the given item stack in the given slot (or tries mergeing at slot if indicated).
        /// Make sure that the depositer has deposit rights to this BankTab!
        /// </summary>
        /// <param name="item">The <see cref="ItemRecord" /> to store.</param>
        /// <param name="amount">The amount of items from the stack to store.</param>
        /// <param name="slot">The slotId where you want to store.</param>
        /// <param name="allowMerge">Whether or not to try and merge the stacks.</param>
        /// <returns>The <see cref="ItemRecord" /> that was in the slot (or the original <see cref="ItemRecord" /> minus the items that were merged) 
        /// or null if the store/merge was successful.</returns>
        public ItemRecord StoreItemInSlot(ItemRecord item, int amount, int slot, bool allowMerge)
        {
            if (slot >= GuildMgr.MAX_BANK_TAB_SLOTS) return item;
            if (item.Amount < amount) return item; // Cheater!

            var curItem = this[slot];
            if (curItem == null)
            {
                // the slot is empty, rock on 
                this[slot] = item;
                return null;
            }

            if (allowMerge && this[slot].EntryId == item.EntryId)
            {
                // try to merge the stacks.
                var freeSpace = (int)curItem.Template.MaxAmount - curItem.Amount;
                freeSpace = Math.Min(freeSpace, amount);

                curItem.Amount += freeSpace;
                item.Amount -= freeSpace;

                return item.Amount > 0 ? item : null;
            }

            // gonna swap
            this[slot] = item;
            return curItem;
        }

        /// <summary>
        /// Checks if the given amount of the given item stack can be stored in the given slot (optionally with merging),
        /// without necessitating an Item swap. No actual store takes place.
        /// </summary>
        /// <param name="item">The <see cref="ItemRecord" /> to store.</param>
        /// <param name="amount">The amount of items from the stack to store.</param>
        /// <param name="slot">The slotId where you want to store.</param>
        /// <param name="allowMerge">Whether or not to try and merge the stacks.</param>
        /// <returns>true if no Item swap is needed.</returns>
        public bool CheckStoreItemInSlot(ItemRecord item, int amount, int slot, bool allowMerge)
        {
            if (slot >= GuildMgr.MAX_BANK_TAB_SLOTS) return false;
            if (item.Amount < amount) return false; // Cheater!

            var curItem = this[slot];
            if (curItem == null) return true;
            
            if (allowMerge && this[slot].EntryId == item.EntryId)
            {
                // try to merge the stacks.
                var freeSpace = (int)curItem.Template.MaxAmount - curItem.Amount;
                return (freeSpace >= amount);
            }

            // gonna swap
            return false;
        }

        /// <summary>
        /// Attempts to find an available Slot in this BankTab.
        /// </summary>
        /// <returns>The slot's id or -1</returns>
        public int FindFreeSlot()
        {
            for (var i = 0; i < GuildMgr.MAX_BANK_TAB_SLOTS; ++i)
            {
                if (ItemRecords[i] == null)
                    return i;
            }
            return -1;
        }
    }
}