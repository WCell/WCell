using WCell.Util.Logging;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GODungeonDifficultyEntry : GOEntry
    {
        /// <summary>
        /// MapId from Maps.dbc
        /// </summary>
        public int MapId
        {
            get { return Fields[0]; }
        }

        /// <summary>
        /// Whether or not the dungeon is Heroic (?)
        /// </summary>
        public bool Difficulty
        {
            get { return Fields[1] != 0; }
        }
    }
}