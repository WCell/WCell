using NLog;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOFishingHoleEntry : GOEntry, IGOLootableEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The activation radius (?)
        /// </summary>
        public int Radius
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// Id of an object that stores the object's loot.
        /// </summary>
        public uint LootId
        {
            get { return (uint)Fields[1]; }
            set { }
        }

        /// <summary>
        /// Minimum number of consecutive times this object can be "fished".
        /// </summary>
        public int MinRestock
        {
            get { return Fields[2]; }
            set { }
        }

        /// <summary>
        /// Maximum number of consecutive times this object can be "fished".
        /// </summary>
        public int MaxRestock
        {
            get { return Fields[3]; }
            set { }
        }

        protected internal override void InitEntry()
        {
            Lock = LockEntry.Entries.Get((uint)Fields[4]);
        }
    }
}