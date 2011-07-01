using System;
using WCell.Terrain.GUI;
using WCell.Constants;

namespace WCell.Terrain.MPQ.ADTs
{
    /// <summary>
    /// MH20 Chunk - Fluid information for the MCNK
    /// </summary>
    public class MH2O
    {
        ///<summary>
        ///</summary>
        public readonly MH2OHeader Header = new MH2OHeader();
        
        /// <summary>
        /// this will contain all heights of fluid
        /// </summary>
        public float[] HeightsArray;

        public bool IsFlat;

		/// <summary>
		/// Ignore this
		/// </summary>
        public byte[] RenderBitMap;

    	private float[,] heights;
        public float[,] Heights
        {
			get
			{
				if (heights == null)
				{
					if ((Header.Used != true) || (HeightsArray == null))
					{
						throw new Exception("This MH2O chunk is not used");
					}

					//var heights = new float[Header.Width + 1, Header.Height + 1];
					heights = new float[TerrainConstants.UnitsPerChunkSide + 1,TerrainConstants.UnitsPerChunkSide + 1];
					for (var r = 0; r <= TerrainConstants.UnitsPerChunkSide; r++)
					{
						for (var c = 0; c <= TerrainConstants.UnitsPerChunkSide; c++)
						{
							// initialize to minimum value
							heights[c, r] = float.MinValue;

							if (((c < Header.XOffset) || (c > (Header.XOffset + Header.Height))) ||
							    ((r < Header.YOffset) || (r > (Header.YOffset + Header.Width))))
							{
								continue;
							}

							heights[c, r] = HeightsArray[c + r*c];
						}
					}
				}
				return heights;
			}
        }

    	private bool[,] enabled;
        public bool[,] GetRenderBitMapMatrix()
        {
            // array will be initialized to false
			if (enabled == null)
			{
				enabled = new bool[TerrainConstants.UnitsPerChunkSide,TerrainConstants.UnitsPerChunkSide];
				for (var r = 0; r < Header.Height; r++)
				{
					for (var c = 7; c >= 0; c--)
					{
						enabled[c, r] = (((RenderBitMap[r] >> c) & 1) == 1);

					}
				}
			}

        	return enabled;
        }
    }

    public class MH2OHeader
    {
        ///<summary>
        /// 1 if this chunk has liquids, 0 if not. Maybe there are more values. If 0, the other fields are 0 too.
        ///</summary>
        public bool Used;

        public MH2OFlags Flags;

        /// <summary>
        /// Seems to be the type of liquid (0 for normal "lake" water, 1 for Lava and 2 for ocean water)
        /// </summary>
        public FluidType Type;

        /// <summary>
        /// The global liquid-height of this chunk.
        /// </summary>
        public float HeightLevel1;

        /// <summary>
        /// Which is always in there twice. Blizzard knows why. 
        /// (Actually these values are not always identical, 
        /// I think they define the highest and lowest points in the heightmap)
        /// </summary>
        public float HeightLevel2;

        ///<summary>
        /// The X offset of the liquid square (0-7)
        ///</summary>
        public byte XOffset;
        /// <summary>
        /// The Y offset of the liquid square (0-7)
        /// </summary>
        public byte YOffset;
        /// <summary>
        /// The width of the liquid square (1-8)
        /// </summary>
        public byte Width;
        /// <summary>
        /// The height of the liquid square (1-8)
        /// </summary>
        public byte Height;
    }
}