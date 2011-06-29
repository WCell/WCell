using System;
using System.Collections.Generic;
using TerrainDisplay.World.DBC;
using WCell.MPQTool;
using WCell.Terrain.MPQ.ADT.Components;

namespace WCell.Terrain.MPQ.WDT
{
    [Flags]
    public enum WDTFlags
    {
        GlobalWMO = 1,
    }
    /// <summary>
    /// MPHD chunk
    /// </summary>
    public class WDTHeader
    {
        public WDTFlags Header1;
        public int Header2;
        public int Header3;
        public int Header4;
        public int Header5;
        public int Header6;
        public int Header7;
        public int Header8;

        public bool IsWMOMap
        {
            get { return (Header1 & WDTFlags.GlobalWMO) != 0; }
        }
    }

    public class WDT
    {
        public MpqManager Manager;
        public MapInfo Entry;

        public int Version;
        public string Name;
        public string Path;
        public readonly WDTHeader Header = new WDTHeader();

        public bool IsWMOOnly;
        ///<summary>
        /// A profile of the tiles in this map
        /// Contains true if Tile [y, x] exists
        ///</summary>
        public readonly bool[,] TileProfile = new bool[64, 64];
        public readonly List<string> WmoFiles = new List<string>();
        public readonly List<MapObjectDefinition> WmoDefinitions = new List<MapObjectDefinition>();
    }
}
