using WCell.Util.Logging;
using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOSummoningRitualEntry : GOEntry
    {
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


        /// <summary>
        /// Amount of casters?
        /// </summary>
        public int CasterCount
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// SpellId
        /// </summary>
        public SpellId SpellId
        {
            get { return (SpellId)Fields[1]; }
        }

        /// <summary>
        /// SpellId
        /// </summary>
        public SpellId AnimSpellId
        {
            get { return (SpellId)Fields[2]; }
        }

        /// <summary>
        /// ???
        /// </summary>
        public bool RitualPersistent
        {
            get { return Fields[3] != 0; }
        }

        /// <summary>
        /// SpellId
        /// </summary>
        public SpellId CasterTargetSpellId
        {
            get { return (SpellId)Fields[4]; }
        }

		/// <summary>
		/// ??? Not sure if this is actually a bool
		/// </summary>
    	public bool CasterTargetSpellTargets
    	{
			get
			{
				if( Fields[ 5 ] < 2 )
					return ( Fields[ 5 ] == 1 );
				else
				{
					sLog.Error( "GOSummoningRitualEntry: Invalid value found for CasterTargetSpellTargets: {0}, defaulting to false.",
						Fields[ 5 ] );
					return false;
				}
			}
			set { Fields[ 5 ] = ( value ? 1 : 0 ); }
    	}

        /// <summary>
        /// Whether or not the Casters of this SummoningRitual are in the same Group
        /// </summary>
        public bool CastersGrouped
        {
            get { return Fields[6] > 0; }
        }

        public bool RitualNoTargetCheck
        {
            get { return Fields[7] != 0; }
        }
	}
}