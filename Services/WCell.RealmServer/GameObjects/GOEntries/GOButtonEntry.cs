using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOButtonEntry : GOEntry
    {
        /// <summary>
        /// Possibly whether or not this button is pressed?
        /// </summary>
        public int StartOpen
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// LockId from Lock.dbc
        /// </summary>
        public int LockId
        {
            get { return Fields[1]; }
        }
		
        /// <summary>
        /// Possibly the time delay before the door auto-closes?
        /// </summary>
        public int AutoClose
        {
            get { return Fields[2]; }
        }

        /// <summary>
        /// ???
        /// </summary>
        public int NoDamageImmune
        {
            get { return Fields[4]; }
        }
		
        /// <summary>
        /// ???
        /// </summary>
        public int Large
        {
            get { return Fields[5]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon pressing the button?
        /// </summary>
        public int OpenTextId
        {
            get { return Fields[6]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon unpressing the button?
        /// </summary>
        public int CloseTextId
        {
            get { return Fields[7]; }
        }


		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get((uint)LockId); 
			LinkedTrapId = (uint) Fields[3];
			LosOk = Fields[8] == 1;
		}
	}
}