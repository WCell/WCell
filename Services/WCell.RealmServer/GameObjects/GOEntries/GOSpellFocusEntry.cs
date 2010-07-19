using NLog;
using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOSpellFocusEntry : GOEntry
    {
		private static Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The type of SpellFocus this is.
        /// </summary>
        public SpellFocus SpellFocus
        {
            get { return (SpellFocus)Fields[0]; }
        }

		/// <summary>
		/// Caster must be within this distance of the object in order to cast the associated spell
		/// </summary>
        public uint Radius
        {
            get { return Fields[1]; }

        }

        /// <summary>
        /// TOOD: find out what this means and possibly change its type to bool or enum or whatever.
        ///  </summary>
        public uint ServerOnly
        {
            get { return Fields[3]; }
        }


        /// <summary>
        /// Id of the quest this object is associated with.
        /// </summary>
        public uint QuestId
        {
            get { return Fields[4]; }
        }

        public bool Large
        {
            get { return Fields[5] != 0; }
        }

        public uint FloatingTooltip
        {
            get { return Fields[6]; }
        }

    	protected internal override void InitEntry()
		{
			LinkedTrapId = Fields[2];
		}
    }
}