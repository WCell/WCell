using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.Collision.QuadTree;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ
{
	/// <summary>
	/// Collection of heightmap-, liquid-, WMO- and M2- data of a single map tile
	/// </summary>
    public class ADT : TerrainTile, IQuadObject
    {
    	public MapId Map { get; set; }
    	private const float MAX_FLAT_LAND_DELTA = 0.005f;
        private const float MAX_FLAT_WATER_DELTA = 0.001f;

        #region Parsing

        /// <summary>
        /// Version of the ADT
        /// </summary>
        /// <example></example>
        public int Version;

        public readonly MapHeader Header = new MapHeader();

        public MapChunkInfo[] MapChunkInfo;

        /// <summary>
        /// List of MDDF Chunks which are placement information for M2s
        /// </summary>
        public readonly List<MapDoodadDefinition> DoodadDefinitions = new List<MapDoodadDefinition>();
        /// <summary>
        /// List of MODF Chunks which are placement information for WMOs
        /// </summary>
        public readonly List<MapObjectDefinition> ObjectDefinitions = new List<MapObjectDefinition>();

        public readonly List<string> ModelFiles = new List<string>();
        public readonly List<int> ModelNameOffsets = new List<int>();
        public readonly List<string> ObjectFiles = new List<string>();
        public readonly List<int> ObjectFileOffsets = new List<int>();

        #endregion


        private int _nodeId = -1;
        public int NodeId
        {
            get { return _nodeId; }
            set { _nodeId = value; }
        }

        public QuadTree<ADTChunk> QuadTree;
        public bool IsWMOOnly;


		/// <summary>
		/// Array of MH20 chunks which give the ADT FLUID vertex information for this ADT
		/// </summary>
		public readonly MH2O[,] LiquidInfo = new MH2O[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];

        #region Constructors
		public ADT(Point2D coordinates, Terrain terrain) :
			base(coordinates, terrain)
        {
        }

    	#endregion

		public ADTChunk GetADTChunk(int x, int y)
		{
			return (ADTChunk) Chunks[x, y];
		}

        public void BuildQuadTree()
        {
            var basePoint = Bounds.BottomRight;
            QuadTree = new QuadTree<ADTChunk>(basePoint, TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunkSize);
            foreach (ADTChunk chunk in Chunks)
            {
                var topLeftX = basePoint.X - chunk.X * TerrainConstants.ChunkSize;
                var topLeftY = basePoint.Y - chunk.Y * TerrainConstants.ChunkSize;
                var botRightX = topLeftX - TerrainConstants.ChunkSize;
                var botRightY = topLeftY - TerrainConstants.ChunkSize;
                
                chunk.Bounds = new Rect(new Point(topLeftX - 1f, topLeftY - 1f),
                                        new Point(botRightX + 1f, botRightY + 1f));
                if (!QuadTree.Insert(chunk))
                {
                    Console.WriteLine((string) "Failed to insert ADTChunk into the QuadTree: {0}", (object) chunk);
                }

                chunk.Bounds = new Rect(new Point(topLeftX, topLeftY),
                                        new Point(botRightX, botRightY));
            }
		}


		#region Vertex & Index Generation
		#endregion
    }
}
