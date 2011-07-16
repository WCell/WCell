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

		public const string FileTypeId = "sadt";
		public const string FileExtension = "." + FileTypeId;

		public static string GetMapDirectory(MapId map)
		{
			return Path.Combine(WCellTerrainSettings.SimpleMapDir, ((int)map).ToString());
		}

		public static string GetFileName(MapId map, int tileX, int tileY)
		{
			var path = GetMapDirectory(map);
			return Path.Combine(path, TerrainConstants.GetTileName(tileX, tileY) + FileExtension);
		}

		public static bool GetTileCoordsFromFileName(string file, out int x, out int y)
		{
			var ss = new StringStream(file);
			x = ss.NextInt(-1, "_");
			y = ss.NextInt(-1, ".");
			return x >= 0 && y >= 0;
		}

		/// <summary>
		/// Writes all height maps to the default MapDir
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

					writer.Write(adt.TerrainVertices);
					writer.Write(adt.TerrainIndices);


					// Write liquid information
					var hasLiquids = false;
					for (var xc = 0; xc < TerrainConstants.ChunksPerTileSide; xc++)
					{
						for (var yc = 0; yc < TerrainConstants.ChunksPerTileSide; yc++)
						{
							var chunk = adt.Chunks[xc, yc];

							if (chunk.HasLiquid)
							{
								hasLiquids = true;
								break;
							}
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
