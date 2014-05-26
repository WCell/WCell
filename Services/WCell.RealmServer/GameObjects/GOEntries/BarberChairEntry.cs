using WCell.Util.Logging;

namespace WCell.RealmServer.GameObjects.GOEntries
{
	public class BarberChairEntry : GOEntry //TODO: Does this class actually help achieve anything
	{
		private static readonly Logger sLog = LogManager.GetCurrentClassLogger();


		/// <summary>
		/// MapId from Maps.dbc
		/// </summary>
    	public int MapId
    	{
			get { return Fields[ 0 ]; }
			set { Fields[ 0 ] = value; }
    	}

		/// <summary>
		/// Whether or not the dungeon is Heroic (?)
		/// </summary>
    	public bool Difficulty
    	{
			get
			{
				if( Fields[ 1 ] < 2 )
					return ( Fields[ 1 ] == 1 );
				else
				{
					sLog.Error( "GODungeonDifficultyEntry: Invalid value found for Difficulty: {0}, defaulting to false.",
						Fields[ 1 ] );
					return false;
				}
			}
			set { Fields[ 1 ] = ( value ? 1 : 0 ); }
    	}
	}
}