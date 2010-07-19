using NLog;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOButtonEntry : GOEntry
    {
        /// <summary>
        /// Possibly whether or not this button is pressed?
        /// </summary>
        public uint StartOpen
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// LockId from Lock.dbc
        /// </summary>
        public uint LockId
        {
            get { return Fields[1]; }
        }
		
        /// <summary>
        /// Possibly the time delay before the door auto-closes?
        /// </summary>
        public uint AutoClose
        {
            get { return Fields[2]; }
        }

        /// <summary>
        /// ???
        /// </summary>
        public uint NoDamageImmune
        {
            get { return Fields[4]; }
        }
		
        /// <summary>
        /// ???
        /// </summary>
        public uint Large
        {
            get { return Fields[5]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon pressing the button?
        /// </summary>
        public uint OpenTextId
        {
            get { return Fields[6]; }
        }

        /// <summary>
        /// A reference to an object holding the Text to display upon unpressing the button?
        /// </summary>
        public uint CloseTextId
        {
            get { return Fields[7]; }
        }


		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get(LockId); 
			LinkedTrapId = Fields[3];
			LosOk = Fields[8] == 1;
		}
	}
}