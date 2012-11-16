using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.MPQ;
using WCell.MPQTool;
using WCell.Terrain.MPQ.ADTs;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.Strings;

namespace WCell.Terrain.Serialization
{
	/// <summary>
	/// Stores ADTs in a faster accessible format.
	/// Question: Why not just decompress and store them as-is?
	/// </summary>
	public static class SimpleTileWriter
	{
		/// <summary>
		/// Version of our custom tile file format:
		/// 
		/// 1: Never quite used
		/// 2: Triangle Mesh
		/// 3: Liquids added
		/// </summary>
		public const int Version = 3;
		public const int HeightfieldVersion = 1;

		public const string FileTypeId = "sadt";
		public const string FileExtension = "." + FileTypeId;

		public const string HeightfieldFileTypeId = "hfadt";
		public const string HeightfieldFileExtension = "." + HeightfieldFileTypeId;

		public static string GetMapDirectory(MapId map)
		{
			return Path.Combine(WCellTerrainSettings.SimpleMapDir, ((int)map).ToString());
		}

		public static string GetFileName(MapId map, int tileX, int tileY)
		{
			var path = GetMapDirectory(map);
			return Path.Combine(path, TerrainConstants.GetTileName(tileX, tileY) + FileExtension);
		}

		public static string GetHeightfieldFileName(MapId map, int tileX, int tileY)
		{
			var path = GetMapDirectory(map);
			return Path.Combine(path, TerrainConstants.GetTileName(tileX, tileY) + HeightfieldFileExtension);
		}

		public static bool GetTileCoordsFromFileName(string file, out int x, out int y)
		{
			var ss = new StringStream(file);
			x = ss.NextInt(-1, "_");
			y = ss.NextInt(-1, ".");
			return x >= 0 && y >= 0;
		}

		/// <summary>
		/// Writes only a simplified (but slightly bigger) version of the heightfield of the ADT
		/// </summary>
		/// <param name="adt"></param>
		public static void WriteHeightField(ADT adt)
		{
			var path = GetMapDirectory(adt.Terrain.MapId);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			using (var file = File.Create(GetFileName(adt.Terrain.MapId, adt.TileX, adt.TileY)))
			{
				using (var writer = new BinaryWriter(file))
				{
					writer.Write(HeightfieldFileTypeId);
					writer.Write(Version);


					for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
					{
						for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
						{
							var chunk = adt.Chunks[x, y];
							var heights = chunk.Heights.GetLowResMapMatrix();
							var holes = (chunk.HolesMask > 0) ? chunk.HolesMap : ADT.EmptyHolesArray;

							// Add the height map values, inserting them into their correct positions
							for (var unitX = 0; unitX <= TerrainConstants.UnitsPerChunkSide; unitX++)
							{
								for (var unitY = 0; unitY <= TerrainConstants.UnitsPerChunkSide; unitY++)
								{
									var tileX = (x*TerrainConstants.UnitsPerChunkSide) + unitX;
									var tileY = (y*TerrainConstants.UnitsPerChunkSide) + unitY;

									var vertIndex = tileVertLocations[tileX, tileY];
									if (vertIndex == -1)
									{
										var xPos = TerrainConstants.CenterPoint
										           - (adt.TileX*TerrainConstants.TileSize)
										           - (tileX*TerrainConstants.UnitSize);
										var yPos = TerrainConstants.CenterPoint
												   - (adt.TileY * TerrainConstants.TileSize)
										           - (tileY*TerrainConstants.UnitSize);
										var zPos = (heights[unitX, unitY] + chunk.MedianHeight);
										tileVertLocations[tileX, tileY] = tileVerts.Count;
										tileVerts.Add(new Vector3(xPos, yPos, zPos));
									}
								}
							}
						}
					}
				}
			}

		}

		/// <summary>
		/// Writes all height maps as trimeshes and liquid information to the default MapDir
		/// </summary>
		//public static void WriteADTs(WDT wdt)
		public static void WriteADT(ADT adt)
		{
			// Map data should only be stored per map
			var path = GetMapDirectory(adt.Terrain.MapId);
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			using (var file = File.Create(GetFileName(adt.Terrain.MapId, adt.TileX, adt.TileY)))
			{
				using (var writer = new BinaryWriter(file))
				{
					writer.Write(FileTypeId);
					writer.Write(Version);

					writer.Write(adt.IsWMOOnly);

					// Write heightfield trimesh
					writer.Write(adt.TerrainVertices);
					writer.Write(adt.TerrainIndices);


					// Write liquid information
					var hasLiquids = false;
					for (var xc = 0; xc < TerrainConstants.ChunksPerTileSide; xc++)
					{
						for (var yc = 0; yc < TerrainConstants.ChunksPerTileSide; yc++)
						{
							var chunk = adt.Chunks[xc, yc];
                            if (!chunk.HasLiquid) continue;

						    hasLiquids = true;
						    break;
						}
					}

					writer.Write(hasLiquids);
					if (hasLiquids)
					{
						for (var xc = 0; xc < TerrainConstants.ChunksPerTileSide; xc++)
						{
							for (var yc = 0; yc < TerrainConstants.ChunksPerTileSide; yc++)
							{
								var chunk = adt.Chunks[xc, yc];

								writer.Write(chunk.HasLiquid ? (int)chunk.LiquidType : (int)LiquidType.None);
								if (chunk.HasLiquid)
								{
									// write boundaries
									var header = chunk.WaterInfo.Header;
									writer.Write(header.XOffset);
									writer.Write(header.YOffset);
									var w = (byte)Math.Min(header.Width, TerrainConstants.UnitsPerChunkSide - header.XOffset);
									var h = (byte)Math.Min(header.Height, TerrainConstants.UnitsPerChunkSide - header.YOffset);
									writer.Write(w);
									writer.Write(h);

									var heights = chunk.LiquidHeights;
									for (var x = header.XOffset; x <= w + header.XOffset; x++)
									{
										for (var y = header.YOffset; y <= h + header.YOffset; y++)
										{
											//var height = chunk.LiquidMap[x, y] ? heights[x, y] : float.MinValue;
											var height = heights[x, y];
											writer.Write(height);
										}
									}
								}
							}
						}
					}
				}
			}

		}

	}
}
