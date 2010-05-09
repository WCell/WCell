using System;
using NLog;
using WCell.Constants;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class GOChairEntry : GOEntry
	{
		private static Logger sLog = LogManager.GetCurrentClassLogger();

	    /// <summary>
	    /// The maximum amount of Units that can sitting on the chair at the same time.
	    /// </summary>
	    public uint MaxCount
	    {
            get { return Fields[0]; }
	    }

		/// <summary>
		/// The height of the chair
		/// </summary>
        public ChairHeight Height
        {
            get { return (ChairHeight)Fields[1]; }
        }

	    /// <summary>
	    /// The StandState when sitting in this chair
	    /// </summary>
	    public StandState SitState
	    {
            get { return (StandState)(Height + 4); }
	    }

	    /// <summary>
	    /// True if only the creator of the chair can sit in it.
	    /// "onlyCreatorUse" in the fields
	    /// </summary>
	    public bool PrivateChair
	    {
            get { return Fields[2] != 0; }
	    }

	    public uint TriggeredEvent
	    {
            get { return Fields[3]; }
	    }
	}
}
