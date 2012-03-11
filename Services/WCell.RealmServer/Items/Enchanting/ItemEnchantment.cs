using System;
using WCell.Constants.Items;

namespace WCell.RealmServer.Items.Enchanting
{
    public class ItemEnchantment
    {
        /// <summary>
        /// The Enchantment slot (0-ItemConstants.MaxEnchantsPerItem).
        /// Not to confuse with Item-slots.
        /// </summary>
        public EnchantSlot Slot;
        public ItemEnchantmentEntry Entry;
        public DateTime ApplyTime;

        /// <summary>
        /// The total duration of the Enchantment in seconds.
        /// </summary>
        public int Duration;

        public ItemEnchantment(ItemEnchantmentEntry entry, EnchantSlot slot, DateTime applyTime, int duration)
        {
            Entry = entry;
            Slot = slot;
            ApplyTime = applyTime;
            Duration = duration;
        }

        public TimeSpan RemainingTime
        {
            get
            {
                if (Duration == 0)
                {
                    return TimeSpan.Zero;
                }
                return (ApplyTime.AddSeconds(Duration)) - DateTime.Now;
            }
        }
    }
}