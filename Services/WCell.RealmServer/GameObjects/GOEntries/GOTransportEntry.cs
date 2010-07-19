using NLog;
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
    	public uint WhenToPause
    	{
    		get { return Fields[ 0 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
    	public uint StartOpen
    	{
    		get { return Fields[ 1 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
    	public uint AutoClose
    	{
			get { return Fields[ 2 ]; }
    	}

        public uint Pause1EventId
        {
            get { return Fields[3]; }
        }

        public uint Pause2EventId
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