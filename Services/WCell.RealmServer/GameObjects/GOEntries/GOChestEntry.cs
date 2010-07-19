using NLog;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOChestEntry : GOEntry, IGOLootableEntry
	{
		/// <summary>
		/// The Id of the Loot that can be looted from this Chest
		/// </summary>
        public uint LootId
        {
            get { return Fields[1]; }
            set { }
        }

		/// <summary>
		/// Minimum number of consecutive times this object can be opened.
		/// </summary>
        public uint MinRestock
        {
            get { return Fields[4]; }
            set { }
        }

		/// <summary>
		/// Maximum number of consecutive times this object can be opened.
		/// </summary>
        public uint MaxRestock
        {
            get { return Fields[5]; }
            set { }
        }

	    /// <summary>
	    /// The time it takes until the chest restocks its loot.
	    /// </summary>
	    public uint ChestRestockTime
	    {
            get { return Fields[2]; }
	    }

	    /// <summary>
	    ///The event-id of a Quest to be triggered upon looting this kind of GO
	    /// </summary>
	    public uint LootedEventId
	    {
            get { return Fields[6]; }
	    }

	    /// <summary>
	    /// The Id of the quest this chest is associated with.
	    /// </summary>
	    public uint QuestId
	    {
            get { return Fields[8]; }
	    }

	    /// <summary>
	    /// The minimum level a character can be in order to open this chest.
	    /// </summary>
	    public uint MinLevel
	    {
            get { return Fields[9]; }
	    }

	    /// <summary>
	    /// Possibly, don't trigger a restock event unless the chest is looted completely (?)
	    /// </summary>
	    public bool LeaveLoot
	    {
            get { return Fields[11] > 0; }
	    }

	    /// <summary>
	    /// Possibly, whether this chest can be looted during combat (?)
	    /// </summary>
	    public bool NotInCombat
	    {
            get { return Fields[12] != 0; }
	    }

	    /// <summary>
	    /// Possibly whether or not to log the looting of this chest (?)
	    /// </summary>
	    public bool LogLoot
	    {
            get { return Fields[13] != 0; }
	    }

	    /// <summary>
	    /// The Id of a text object to display upon opening this chest.
	    /// </summary>
	    public uint OpenTextId
	    {
            get { return Fields[14]; }
	    }

	    public uint FloatingTooltip
	    {
            get { return Fields[16]; }
	    }

		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get(Fields[0]);

			IsConsumable = Fields[3] > 0;

			LinkedTrapId = Fields[7];

			LosOk = Fields[10] > 0;
			UseGroupLoot = Fields[15] > 0;
		}
	}
}