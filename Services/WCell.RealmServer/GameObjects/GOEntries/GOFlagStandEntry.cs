using WCell.Constants.Spells;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOFlagStandEntry : GOFlagEntry
    {
		/// <summary>
		/// SpellId from Spell.dbc
		/// </summary>
    	public override SpellId PickupSpellId
    	{
    		get { return (SpellId)Fields[ 1 ]; }
    	}

		/// <summary>
		/// Activation radius (?)
		/// </summary>
    	public int Radius
    	{
    		get { return Fields[ 2 ]; }
    	}

		/// <summary>
		/// SpellId from Spells.dbc
		/// </summary>
    	public int ReturnAuraId
    	{
    		get { return Fields[ 3 ]; }
    	}

        public override bool NoDamageImmune
        {
            get { return Fields[5] != 0; }
        }

        /// <summary>
		/// SpellId from Spells.dbc
		/// </summary>
    	public SpellId ReturnSpellId
    	{
    		get { return (SpellId)Fields[ 4 ]; }
    	}

		/// <summary>
		/// Id of a text object that is shown when the object is activated (?)
		/// </summary>
    	public override int OpenTextId
    	{
    		get { return Fields[ 6 ]; }
    	}

		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get((uint)LockId);
		    LosOk = Fields[7] != 0;
		}
	}
}