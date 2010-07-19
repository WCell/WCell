using NLog;
using WCell.RealmServer.Misc;
using WCell.Util;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOCameraEntry : GOEntry
    {
        /// <summary>
        /// LockId from Lock.dbc
        /// </summary>
        public uint LockId
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// CinematicCameraId from CinematicCamera.dbc
        /// </summary>
        public uint CinematicCameraId
        {
            get { return Fields[1]; }
        }

        /// <summary>
        /// The Id of an Event associated with this camera (?)
        /// </summary>
        public uint EventId
        {
            get { return Fields[2]; }
        }

        /// <summary>
        /// The Id of a text object associated with this camera (?)
        /// </summary>
        public uint OpenTextId
        {
            get { return Fields[3]; }
        }

        protected internal override void InitEntry()
        {
            Lock = LockEntry.Entries.Get(LockId);
        }
    }
}