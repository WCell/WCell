using System;
using System.IO;
using WCell.Util.Graphics;

namespace TerrainDisplay.MPQ.WMO.Components
{
    /// <summary>
    /// MLIQ Info
    /// </summary>
    public class LiquidInfo
    {
        /// <summary>
        /// number of X vertices (xverts)
        /// </summary>
        public int XVertexCount;
        /// <summary>
        /// number of Y vertices (yverts)
        /// </summary>
        public int YVertexCount;
        /// <summary>
        /// number of X tiles (xtiles = xverts-1)
        /// </summary>
        public int XTileCount;
        /// <summary>
        /// number of Y tiles (ytiles = yverts-1)
        /// </summary>
        public int YTileCount;

        public Vector3 BaseCoordinates;
        public ushort MaterialId;

        public int VertexCount
        {
            get { return XVertexCount*YVertexCount; }
        }
        public int TileCount
        {
            get { return XTileCount*YTileCount; }
        }

        /// <summary>
        /// Grid of the Upper height bounds for the liquid vertices
        /// </summary>
        public float[,] HeightMapMax;

        /// <summary>
        /// Grid of the lower height bounds for the liquid vertices
        /// </summary>
        public float[,] HeightMapMin;

        /// <summary>
        /// Grid of flags for the liquid tiles
        /// They are often masked with 0xF
        /// They seem to determine the liquid type?
        /// </summary>
        public byte[,] LiquidTileFlags;

        public BoundingBox Bounds;

        /// <summary>
        /// The calculated LiquidType (for the DBC lookup) for the whole WMO
        /// Not parsed initially
        /// </summary>
        public uint LiquidType;



        internal void Dump(StreamWriter file)
        {
            for (var y = 0; y < YTileCount; y++)
            {
                for (var x = 0; x < XTileCount; x++)
                {
                    if ((LiquidTileFlags[x, y] & 0x0F) == 0x0F) continue;
                    file.WriteLine(
                        String.Format("{0:00.00e00} | {1:00.00} | {2:00.00e00}", 
                                    HeightMapMin[x, y],
                                    BaseCoordinates.Z,
                                    HeightMapMax[x, y]));
                }
            }
        }
    }
}
