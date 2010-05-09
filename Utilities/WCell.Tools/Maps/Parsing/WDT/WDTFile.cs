using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.MPQTool;
using WCell.Tools.Maps.Parsing;

namespace WCell.Tools.Maps
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

    public class WDTFile
    {
    	public MpqManager Manager;
    	public DBCMapEntry Entry;

        public int Version;
        public string Name;
        public string Path;
        public readonly WDTHeader Header = new WDTHeader();

        ///<summary>
        /// A profile of the tiles in this map
        ///</summary>
		public readonly bool[,] TileProfile = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
        public readonly List<string> ObjectFiles = new List<string>();
        public readonly List<MapObjectDefinition> ObjectDefinitions = new List<MapObjectDefinition>();
    }
}
