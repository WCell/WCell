using WCell.Util.Logging;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Transports;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOTransportEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


    	/// <summary>
    	/// (ms)
    	/// </summary>
    	public int WhenToPause
    	{
    		get { return Fields[ 0 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
    	public int StartOpen
    	{
    		get { return Fields[ 1 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
    	public int AutoClose
    	{
			get { return Fields[ 2 ]; }
    	}

        public int Pause1EventId
        {
            get { return Fields[3]; }
        }

        public int Pause2EventId
        {
            get { return Fields[4]; }
        }

		public override bool IsTransport
		{
			get
			{
				return true;
			}
		}
    }
}