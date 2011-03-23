using WCell.Util.Logging;
using WCell.Constants;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOAreaDamageEntry : GOEntry
    {
        /// <summary>
        /// LockId from Lock.dbc
        /// </summary>
        public int LockId
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// The radius within which the damage is applied (?)
        /// </summary>
        public int Radius
        {
            get { return Fields[1]; }
        }

        /// <summary>
        /// The minimum damage done.
        /// </summary>
        public int MinDamage
        {
            get { return Fields[2]; }
        }

        /// <summary>
        /// The maximum damage done.
        /// </summary>
        public int MaxDamage
        {
            get { return Fields[3]; }
        }

        /// <summary>
        /// The type of damage done.
        /// </summary>
        public DamageSchool DamageSchool
        {
            get { return (DamageSchool)Fields[4]; }
        }

        /// <summary>
        /// The duration of the damaging effect (?)
        /// </summary>
        public int AutoClose
        {
            get { return Fields[5]; }
        }

        /// <summary>
        /// The Id of a text object to be displayed when the AreaDamage starts. (?)
        /// </summary>
        public int OpenTextId
        {
            get { return Fields[6]; }
        }

        /// <summary>
        /// The Id of a text object to be displayed when the AreaDamage ends. (?)
        /// </summary>
        public int CloseTextId
        {
            get { return Fields[7]; }
        }

		protected internal override void InitEntry()
		{
			Lock = LockEntry.Entries.Get((uint)LockId);
		}
    }
}