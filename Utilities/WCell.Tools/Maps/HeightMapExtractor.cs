using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.RealmServer.Global;
using WCell.Tools.Maps.Parsing;
using WCell.Tools.Maps.Parsing.ADT;
using WCell.Util.Graphics;
using WCell.Util.Toolshed;
using WCell.Util;
using NLog;

namespace WCell.Tools.Maps
{
	public static class HeightMapExtractor
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		private const string FileTypeId = "ter";

		public static void Prepare()
		{
			WDTParser.Parsed += ExportHeightMaps;
		}

		/// <summary>
		/// Writes all height maps to the default MapDir
		/// </summary>
		public static void ExportHeightMaps(WDTFile wdt)
		{
			if ((wdt.Header.Header1 & WDTFlags.GlobalWMO) != 0)
			{
				// has no terrain
				return;
			}

			var terrainInfo = ExtractRegionHeightMaps(wdt);
			var count = 0;
			var path = Path.Combine(ToolConfig.MapDir, wdt.Entry.Id.ToString());
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			for (var tileY = 0; tileY < TerrainConstants.TilesPerMapSide; tileY++)
			{
				for (var tileX = 0; tileX < TerrainConstants.TilesPerMapSide; tileX++)
				{
					var adtTerrainInfo = terrainInfo[tileX, tileY];
					if (adtTerrainInfo == null) continue;
					if (adtTerrainInfo.HeightMaps == null) continue;

					using (var file = File.Create(Path.Combine(path, TerrainConstants.GetHeightMapFile(tileX, tileY))))
					{
						WriteADTTerrainInfo(file, adtTerrainInfo);
					}
					count++;
				}
			}
			log.Info("Extracted {0} tiles for {1}.", count, (MapId)wdt.Entry.Id);
		}

		#region Extract Terrain
		public static ADTTerrainInfo[,] ExtractRegionHeightMaps(WDTFile wdt)
		{
			var terrainInfo = new ADTTerrainInfo[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];

			for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
			{
				for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
				{
					if (!wdt.TileProfile[x, y]) continue;
					var info = new ADTTerrainInfo();

					var adtName = TerrainConstants.GetADTFile(wdt.Name, x, y);
					var adt = ADTParser.Process(WDTParser.MpqManager, wdt.Path, adtName);
					if (adt == null) continue;

					info.LiquidProfile = ExtractTileLiquidProfile(adt);
					info.LiquidTypes = ExtractTileLiquidTypes(adt);
					info.HeightMaps = ExtractTileHeightMaps(adt);
					info.CheckFlatness();

					terrainInfo[x, y] = info;
				}
			}

			return terrainInfo;
		}

		public static HeightMap[,] ExtractTileHeightMaps(ADTFile adt)
		{
			var maps = new HeightMap[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];

			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					var chunk = adt.MapChunks[x, y];
					var heights = adt.HeightMaps[x, y].Heights;
					var liq = adt.LiquidMaps[x, y];

					var heightMap = new HeightMap
					{
						MedianHeight = chunk.Y
					};

					// Read in Outer Heights
					for (var row = 0; row < 17; row += 2)
					{
						if (row % 2 != 0) continue;
						for (var col = 0; col < 9; col++)
						{
							var count = ((row / 2) * 9) + ((row / 2) * 8) + col;
							heightMap.OuterHeightDiff[row / 2, col] = heights[count];
						}
					}

					// Read in Inner Heights
					for (var row = 1; row < 16; row++)
					{
						if (row % 2 != 1) continue;
						for (var col = 0; col < 8; col++)
						{
							var count = (((row + 1) / 2) * 9) + ((row / 2) * 8) + col;
							heightMap.InnerHeightDiff[row / 2, col] = heights[count];
						}
					}

					// Read in Liquid Heights
					if (liq != null && (liq.Used && liq.Heights != null))
					{
						for (var row = 0; row < 9; row++)
						{
							for (var col = 0; col < 9; col++)
							{
								if (row < liq.YOffset || row > (liq.YOffset + liq.Height) ||
									col < liq.XOffset || col > (liq.XOffset + liq.Width))
								{
									heightMap.LiquidHeight[row, col] = float.MinValue;
									continue;
								}

								var oldRow = row - liq.YOffset;
								var oldCol = col - liq.XOffset;
								heightMap.LiquidHeight[row, col] = liq.Heights[oldRow, oldCol];
							}
						}
					}

					maps[x, y] = heightMap;
				}
			}

			return maps;
		}
		#endregion

		#region Extract Liquid Tables
		public static bool[,] ExtractTileLiquidProfile(ADTFile adt)
		{
			var profile = new bool[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					if (adt.LiquidMaps[x, y] == null)
					{
						profile[x, y] = false;
						continue;
					}
					profile[x, y] = adt.LiquidMaps[x, y].Used;
				}
			}
			return profile;
		}

		public static FluidType[,] ExtractTileLiquidTypes(ADTFile adt)
		{
			var types = new FluidType[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					if (adt.LiquidMaps[x, y] == null)
					{
						types[x, y] = FluidType.None;
						continue;
					}
					types[x, y] = adt.LiquidMaps[x, y].Type;
				}
			}
			return types;
		}

		public static bool[,] ReadLiquidProfile(BinaryReader reader)
		{
			var profile = new bool[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					profile[x, y] = reader.ReadBoolean();
				}
			}
			return profile;
		}

		public static FluidType[,] ReadLiquidTypes(BinaryReader reader)
		{
			var types = new FluidType[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					types[x, y] = (FluidType)reader.ReadByte();
				}
			}
			return types;
		}
		#endregion

		#region Write
		public static void WriteADTTerrainInfo(FileStream file, ADTTerrainInfo info)
		{
			var writer = new BinaryWriter(file);
			writer.Write(FileTypeId);

			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					writer.Write(info.LiquidProfile[x, y]);
				}
			}

			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					writer.Write((byte)info.LiquidTypes[x, y]);
				}
			}

			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					WriteHeightMap(writer, info.HeightMaps[x, y], info.LiquidProfile[x, y]);
				}
			}
		}

		public static void WriteHeightMap(BinaryWriter writer, HeightMap map, bool writeLiquid)
		{
			writer.Write(map.IsFlat);
			writer.Write(map.MedianHeight);

			if (!map.IsFlat)
			{
				for (var y = 0; y < 9; y++)
				{
					for (var x = 0; x < 9; x++)
					{
						writer.Write(map.OuterHeightDiff[x, y]);
					}
				}

				for (var y = 0; y < 8; y++)
				{
					for (var x = 0; x < 8; x++)
					{
						writer.Write(map.InnerHeightDiff[x, y]);
					}
				}
			}

			if (writeLiquid)
			{
				for (var y = 0; y < 9; y++)
				{
					for (var x = 0; x < 9; x++)
					{
						writer.Write(map.LiquidHeight[x, y]);
					}
				}
			}
		}
		#endregion

		#region Read Self-made maps
		public static ADTTerrainInfo GetInfoForPos(Region map, Vector3 worldPos)
		{
			var tileCoords = GetTileXYForPos(worldPos);

			// Todo create some tabulated way to keep track of what is loaded and what is not
			return ReadTileInfo(map.Id, tileCoords);
		}

		public static ADTTerrainInfo ReadTileInfo(MapId id, TileCoord coord)
		{
			var filePath = Path.Combine(ToolConfig.MapDir, TerrainConstants.GetHeightMapFile(coord.TileX, coord.TileY));
			var file = File.OpenRead(filePath);
			var reader = new BinaryReader(file);

			var key = reader.ReadString();
			if (key != FileTypeId) log.Error("Invalid file format!");

			var info = new ADTTerrainInfo
			{
				LiquidProfile = ReadLiquidProfile(reader),
				LiquidTypes = ReadLiquidTypes(reader)
			};

			info.HeightMaps = ReadHeightMaps(reader, info.LiquidProfile);
			return info;
		}

		public static HeightMap[,] ReadHeightMaps(BinaryReader reader, bool[,] profile)
		{
			var heightMaps = new HeightMap[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
			for (var y = 0; y < TerrainConstants.ChunksPerTileSide; y++)
			{
				for (var x = 0; x < TerrainConstants.ChunksPerTileSide; x++)
				{
					heightMaps[x, y] = ReadHeightMap(reader, profile[x, y]);
				}
			}
			return heightMaps;
		}

		public static HeightMap ReadHeightMap(BinaryReader reader, bool readLiquid)
		{
			var map = new HeightMap
			{
				IsFlat = reader.ReadBoolean(),
				MedianHeight = reader.ReadSingle()
			};

			if (!map.IsFlat)
			{
				for (var y = 0; y < 9; y++)
				{
					for (var x = 0; x < 9; x++)
					{
						map.OuterHeightDiff[x, y] = reader.ReadSingle();
					}
				}

				for (var y = 0; y < 8; y++)
				{
					for (var x = 0; x < 8; x++)
					{
						map.InnerHeightDiff[x, y] = reader.ReadSingle();
					}
				}
			}

			if (readLiquid)
			{
				for (var y = 0; y < 9; y++)
				{
					for (var x = 0; x < 9; x++)
					{
						map.LiquidHeight[x, y] = reader.ReadSingle();
					}
				}
			}

			return map;
		}
		#endregion

		#region Positions
		public static TileCoord GetTileXYForPos(Vector3 worldPos)
		{
			return new TileCoord
			{
				TileX = (int)((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize),
				TileY = (int)((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize)
			};
		}

		public static ChunkCoord GetChunkXYForPos(Vector3 worldPos)
		{
			var tileXFraction = ((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize);
			var tileYFraction = ((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize);

			return new ChunkCoord
			{
				ChunkX = (int)((tileXFraction - ((int)tileXFraction)) * TerrainConstants.ChunkSize),
				ChunkY = (int)((tileYFraction - ((int)tileYFraction)) * TerrainConstants.ChunkSize)
			};
		}

		public static HeightMapCoord GetHeightMapXYForPos(Vector3 worldPos)
		{
			var tileXFraction = ((TerrainConstants.CenterPoint - worldPos.Y) / TerrainConstants.TileSize);
			var tileYFraction = ((TerrainConstants.CenterPoint - worldPos.X) / TerrainConstants.TileSize);

			var chunkXFraction = ((tileXFraction - ((int)tileXFraction)) * TerrainConstants.ChunkSize);
			var chunkYFraction = ((tileYFraction - ((int)tileYFraction)) * TerrainConstants.ChunkSize);

			return new HeightMapCoord
			{
				HeightMapX = (int)((chunkXFraction - ((int)chunkXFraction)) * TerrainConstants.UnitSize),
				HeightMapY = (int)((chunkYFraction - ((int)chunkYFraction)) * TerrainConstants.UnitSize)
			};
		}
		#endregion
	}

	public class ADTTerrainInfo
	{
		public bool[,] LiquidProfile = new bool[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
		public FluidType[,] LiquidTypes = new FluidType[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];
		public HeightMap[,] HeightMaps = new HeightMap[TerrainConstants.ChunksPerTileSide, TerrainConstants.ChunksPerTileSide];

		internal void CheckFlatness()
		{
			foreach (var heightMap in HeightMaps)
			{
				var localMax = float.MinValue;
				var localMin = float.MaxValue;

				foreach (var height in heightMap.OuterHeightDiff)
				{
					localMin = Math.Min(localMin, height);
					localMax = Math.Max(localMax, height);
				}

				foreach (var height in heightMap.InnerHeightDiff)
				{
					localMin = Math.Min(localMin, height);
					localMax = Math.Max(localMax, height);
				}

				if ((localMax - localMin) < 1.0f)
				{
					heightMap.IsFlat = true;
				}
			}
		}
	}

	public class HeightMap
	{
		public bool IsFlat;
		public float MedianHeight;
		public readonly float[,] LiquidHeight = new float[9, 9];
		public readonly float[,] OuterHeightDiff = new float[9, 9];
		public readonly float[,] InnerHeightDiff = new float[8, 8];
	}

	public class TileCoord
	{
		public int TileX;
		public int TileY;
	}

	public class ChunkCoord
	{
		public int ChunkX;
		public int ChunkY;
	}

	public class HeightMapCoord
	{
		public int HeightMapX;
		public int HeightMapY;
	}
}
