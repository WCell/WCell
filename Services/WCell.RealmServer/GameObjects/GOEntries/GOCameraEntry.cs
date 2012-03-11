using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOCameraEntry : GOEntry
    {
        /// <summary>
        /// LockId from Lock.dbc
        /// </summary>
        public int LockId
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// CinematicCameraId from CinematicCamera.dbc
        /// </summary>
        public int CinematicCameraId
        {
            get { return Fields[1]; }
        }

        /// <summary>
        /// The Id of an Event associated with this camera (?)
        /// </summary>
        public int EventId
        {
            get { return Fields[2]; }
        }

        /// <summary>
        /// The Id of a text object associated with this camera (?)
        /// </summary>
        public int OpenTextId
        {
            get { return Fields[3]; }
        }

        protected internal override void InitEntry()
        {
            Lock = LockEntry.Entries.Get((uint)LockId);
        }
    }
}