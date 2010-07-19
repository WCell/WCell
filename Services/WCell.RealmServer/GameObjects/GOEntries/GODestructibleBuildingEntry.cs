using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GODestructibleBuildingEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Intact: Num hits
        /// </summary>
        public uint IntactNumHits
        {
            get { return Fields[0]; }
        }

        public uint CreditProxyCreature
        {
            get { return Fields[1]; }
        }

        public uint Field103_2
        {
            get { return Fields[2]; }
        }

        public uint IntactEvent
        {
            get { return Fields[3]; }
        }

        public uint Field103_4
        {
            get { return Fields[4]; }
        }

        /// <summary>
        /// Damaged: Num hits
        /// </summary>
        public uint DamagedNumHits
        {
            get { return Fields[5]; }
        }

        public uint Field103_6
        {
            get { return Fields[6]; }
        }
        public uint Field103_7
        {
            get { return Fields[7]; }
        }
        public uint Field103_8
        {
            get { return Fields[8]; }
        }

        public uint DamagedEvent
        {
            get { return Fields[9]; }
        }
        public uint Field103_10
        {
            get { return Fields[10]; }
        }
        public uint Field103_11
        {
            get { return Fields[11]; }
        }
        public uint Field103_12
        {
            get { return Fields[12]; }
        }
        public uint Field103_13
        {
            get { return Fields[13]; }
        }

        public uint DestroyedEvent
        {
            get { return Fields[14]; }
        }

        public uint Field103_15
        {
            get { return Fields[15]; }
        }

        /// <summary>
        /// Rebuilding: Time (in seconds)
        /// </summary>
        public uint RebuildingTime
        {
            get { return Fields[16]; }
        }

        public uint Field103_16
        {
            get { return Fields[17]; }
        }

        public uint DestructibleData
        {
            get { return Fields[18]; }
        }

        /// <summary>
        /// Rebuilding: Event
        /// </summary>
        public uint RebuildingEvent
        {
            get { return Fields[19]; }
        }

        public uint Field103_20
        {
            get { return Fields[20]; }
        }
        public uint Field103_21
        {
            get { return Fields[21]; }
        }

        public uint DamageEvent
        {
            get { return Fields[22]; }
        }

        public uint Field103_23
        {
            get { return Fields[23]; }
        }
	}
}