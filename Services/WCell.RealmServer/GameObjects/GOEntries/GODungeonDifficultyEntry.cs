using NLog;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GODungeonDifficultyEntry : GOEntry
    {
        /// <summary>
        /// MapId from Maps.dbc
        /// </summary>
        public uint MapId
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