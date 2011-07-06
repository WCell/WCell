using System;
using System.IO;
using WCell.Constants;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Serialization
{
    public static class SimpleTileReader
    {
		public static TerrainTile ReadTile(SimpleTerrain terrain, int tileX, int tileY)
        {
			var filePath = SimpleTileWriter.GetFileName(terrain.MapId, tileX, tileY);
            if (!File.Exists(filePath))
            {
                return null;
            }

        	TerrainTile tile;

            using(var file = File.OpenRead(filePath))
            using(var reader = new BinaryReader(file))
            {
                var fileType = reader.ReadString();
				if (fileType != SimpleTileWriter.FileTypeId)
                {
                    throw new InvalidDataException(string.Format("ADT file not in valid format: {0}", filePath));
                }

            	var version = reader.ReadInt32();
				if (version != SimpleTileWriter.Version)
				{
					// invalid version -> File is outdated
					return null;
				}

				tile = new TerrainTile(tileX, tileY, terrain);

				terrain.m_IsWmoOnly = reader.ReadBoolean();
                tile.TerrainVertices = reader.ReadVector3Array();
            	tile.TerrainIndices = reader.ReadInt32Array();


				// Write liquid information
				var hasLiquids = reader.ReadBoolean();

				if (hasLiquids)
				{
					tile.LiquidChunks = new TerrainLiquidChunk[TerrainConstants.ChunksPerTileSide,TerrainConstants.ChunksPerTileSide];
					for (var xc = 0; xc < TerrainConstants.ChunksPerTileSide; xc++)
					{
						for (var yc = 0; yc < TerrainConstants.ChunksPerTileSide; yc++)
						{
							var type = (LiquidType) reader.ReadInt32();
							if (type != LiquidType.None)
							{
								var chunk = tile.LiquidChunks[xc, yc] = new TerrainLiquidChunk();
								chunk.Type = type;

								// read heightmap
								chunk.OffsetX = reader.ReadByte();
								chunk.OffsetY = reader.ReadByte();
								chunk.Width = reader.ReadByte();
								chunk.Height = reader.ReadByte();

								chunk.Heights = new float[chunk.Width + 1, chunk.Height + 1];
								for (var x = 0; x <= chunk.Width; x++)
								{
									for (var y = 0; y <= chunk.Height; y++)
									{
										chunk.Heights[x, y] = reader.ReadSingle();
									}
								}
							}
						}
					}
				}
            }


            return tile;
        }
    }
}
