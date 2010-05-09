using NLog;
using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOAuraGeneratorEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// ???
		/// </summary>
    	public bool StartOpen
    	{
    		get
			{
				if( Fields[ 0 ] < 2 )
					return ( Fields[ 0 ] == 1 );
				else
				{
					sLog.Error( "GOAuraGeneratorEntry: Invalid value found for StartOpen: {0}, defaulting to false.", 
						Fields[ 0 ] );
					return false;
				}
			}
			set { Fields[ 0 ] = ( value ? 1u : 0u ); }
    	}

		/// <summary>
		/// Area of effect (?)
		/// </summary>
    	public uint Radius
    	{
    		get { return Fields[ 1 ]; }
			set { Fields[ 1 ] = value;}
    	}

        /// <summary>
        /// SpellId from Spells.dbc
        /// </summary>
        public SpellId AuraId1
        {
            get { return (SpellId)Fields[2]; }
        }

		/// <summary>
		/// ???
		/// </summary>
    	public uint ConditionId1
    	{
    		get { return Fields[ 3 ]; }
			set { Fields[ 3 ] = value;}
    	}

        /// <summary>
        /// SpellId from Spells.dbc
        /// </summary>
        public SpellId AuraId2
        {
            get { return (SpellId) Fields[4]; }
        }

		/// <summary>
		/// ???
		/// </summary>
    	public uint ConditionId2
    	{
			get { return Fields[ 5 ]; }
			set { Fields[ 5 ] = value; }
    	}

		/// <summary>
		/// ???
		/// </summary>
    	public bool ServerOnly
    	{
    		get
			{
				if( Fields[ 6 ] < 2 )
					return ( Fields[ 6 ] == 1 );
				else
				{
					sLog.Error( "GOButtonEntry: Invalid value found for StartOpen: {0}, defaulting to false.", 
						Fields[ 6 ] );
					return false;
				}
			}
			set { Fields[ 6 ] = ( value ? 1u : 0u ); }
    	}
	}
}
