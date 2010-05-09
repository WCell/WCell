using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOCapturePointEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// The activation radius (?)
		/// </summary>
		public uint Radius
		{
    		get { return Fields[ 0 ]; }
    	}

		/// <summary>
		/// Unknown, possibly a server-side dummy spell-effect. Not a SpellId from Spells.dbc
		/// </summary>
		public uint SpellId
		{
    		get { return Fields[ 1 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint WorldState1
		{
    		get { return Fields[ 2 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint WorldState2
		{
    		get { return Fields[ 3 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint WinEventId1
		{
    		get { return Fields[ 4 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint WinEventId2
		{
    		get { return Fields[ 5 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint ContestedEventId1
		{
    		get { return Fields[ 6 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint ContestedEventId2
		{
    		get { return Fields[ 7 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint ProgressEventId1
		{
    		get { return Fields[ 8 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint ProgressEventId2
		{
    		get { return Fields[ 9 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint NeutralEventId1
		{
    		get { return Fields[ 10 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint NeutralEventId2
		{
    		get { return Fields[ 11 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint NeutralPercent
		{
    		get { return Fields[ 12 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint WorldState3
		{
    		get { return Fields[ 13 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint MinSuperiority
		{
    		get { return Fields[ 14 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint MaxSuperiority
		{
    		get { return Fields[ 15 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint MinTime
		{
    		get { return Fields[ 16 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public uint MaxTime
		{
    		get { return Fields[ 17 ]; }
    	}

		/// <summary>
		/// ???
		/// </summary>
		public bool Large
		{
            get { return Fields[18] != 0; }
    	}

		/// <summary>
		/// ??? Is this a bool?
		/// </summary>
		public bool Highlight
		{
            get { return Fields[19] != 0; }
		}

        public uint StartingValue
        {
            get { return Fields[20]; }
        }

        public bool Unidirectional
        {
            get { return Fields[21] != 0; }
        }
    }
}
