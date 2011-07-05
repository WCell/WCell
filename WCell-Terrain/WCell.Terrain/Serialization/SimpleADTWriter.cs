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
	public static class SimpleADTWriter
	{
		/// <summary>
		/// Version of our custom tile file format
		/// </summary>
		public const int Version = 2;

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

					// TODO: Write water
				}
			}

		}

	}
}
