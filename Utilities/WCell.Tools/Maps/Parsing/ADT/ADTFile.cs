using System.Collections.Generic;

namespace WCell.Tools.Maps
{
    public class ADTFile
    {
        public string Name;
        public string Path;
        /// <summary>
        /// Version of the ADT
        /// </summary>
        public int Version;

        public readonly MapHeader Header = new MapHeader();

        public MapChunkInfo[] MapChunkInfo;
        /// <summary>
        /// Array of MCNK chunks which give the ADT vertex information for this ADT
        /// </summary>
        public readonly MCNK[,] MapChunks = new MCNK[16, 16];

        public readonly MCVT[,] HeightMaps = new MCVT[16, 16];

        public readonly MH2O[,] LiquidMaps = new MH2O[16, 16];

        public readonly List<string> ObjectFiles = new List<string>();
        /// <summary>
        /// List of MODF Chunks which are placement information for WMOs
        /// </summary>
        public readonly List<MapObjectDefinition> ObjectDefinitions = new List<MapObjectDefinition>();
        /// <summary>
        /// List of MDDF Chunks which are placement information for M2s
        /// </summary>
        public readonly List<MapDoodadDefinition> DoodadDefinitions = new List<MapDoodadDefinition>(); // NEW
        public readonly List<string> ModelFiles = new List<string>();
        public readonly List<int> ModelNameOffsets = new List<int>();
    }
}
