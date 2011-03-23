using WCell.Util.Logging;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOAuctionHouseEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// The AuctionHouseId from AuctionHouse.dbc
		/// </summary>
    	public int AuctionHouseId
    	{
			get { return Fields[ 0 ]; }
			set { Fields[ 0 ] = value; }
    	}
    }
}