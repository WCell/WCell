using NLog;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOFlagDropEntry : GOFlagEntry
    {

		/// <summary>
		/// Id for an Event that is triggered upon activating this object (?)
		/// </summary>
    	public uint EventId
    	{
    		get { return Fields[ 1 ]; }
    	}

        /// <summary>
        /// SpellId from Spells.dbc
        /// </summary>
        public override SpellId PickupSpellId
        {
            get { return (SpellId)Fields[2]; }
        }

		/// <summary>
		/// ???
		/// </summary>
    	public override bool NoDamageImmune
    	{
            get { return Fields[3] != 0; }
    	}

		/// <summary>
		/// Id for a text object that is displayed when activating this object (?)
		/// </summary>
    	public override uint OpenTextId
    	{
            get { return Fields[4]; }
    	}

		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get(LockId);
		}
	}
}
