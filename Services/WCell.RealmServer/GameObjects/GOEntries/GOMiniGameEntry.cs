using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOMiniGameEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// ???
		/// </summary>
    	public int GameType
    	{
			get { return Fields[ 0 ]; }
    	}
    }
}