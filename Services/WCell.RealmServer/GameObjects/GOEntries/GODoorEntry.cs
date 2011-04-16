using NLog;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GODoorEntry : GOEntry
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

	    public int StartOpen
	    {
            get { return Fields[0]; }
	    }

	    /// <summary>
	    /// LockId from Lock.dbc
	    /// "open" in the fields list
	    /// </summary>
	    public int LockId
	    {
            get { return Fields[1]; }
	    }

	    /// <summary>
	    /// Possibly the time delay before the door closes?
	    /// </summary>
	    public int AutoClose
	    {
            get { return Fields[2]; }
	    }

	    public int NoDamageImmune
	    {
            get { return Fields[3]; }
	    }
		
	    /// <summary>
	    /// A reference to an object holding the Text to display upon opening the door?
	    /// </summary>
	    public int OpenTextId
	    {
            get { return Fields[4]; }
	    }

	    /// <summary>
	    /// A reference to an object holding the Text to display upon closing the door?
	    /// </summary>
	    public int CloseTextId
	    {
            get { return Fields[5]; }
	    }

	    public int IgnoredByPathing
	    {
            get { return Fields[6]; }
	    }


		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get((uint)LockId);
		}
	}
}