using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOGuardPostEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// The Id of the creature associated with this guard post.
        /// </summary>
        public int CreatureId
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// The number of creatures with Id = CreatureId in this guard post.
        /// </summary>
        public int Charges
        {
            get { return Fields[1]; }
        }
	}
}