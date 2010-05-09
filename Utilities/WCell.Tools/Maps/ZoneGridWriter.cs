using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WCell.Tools.Code;
using WCell.Constants.World;
using WCell.Util.Toolshed;
using WCell.Constants;

namespace WCell.Tools.Maps
{
	public static class ZoneTileSetWriter
	{
		private static CodeFileWriter writer;
		private static ZoneTileSet[] tileSets;

		[Tool]
		public static void WriteZoneTileSets()
		{
			var file = ToolConfig.WCellConstantsRoot + "World/ZoneBoundaries.cs";
			WriteZoneTileSets(file);
		}

		public static void WriteZoneTileSets(string outputFileName)
		{
			tileSets = ZoneTileSetReader.ExportTileSets();
			using (writer = new CodeFileWriter(outputFileName, "WCell.Constants.World", "ZoneBoundaries", "static class", "",
				"WCell.Util.Graphics"))
			{
				writer.WriteMethod("public", "static ZoneTileSet[]", "GetZoneTileSets", "", WriteMethod);
			}
		}

		private static void WriteMethod()
		{
			writer.WriteLine("ZoneTileSet tiles;");
			writer.WriteLine("var sets = new ZoneTileSet[(int)MapId.End];");
			for (var i = 0; i < tileSets.Length; i++)
			{
				var tileSet = tileSets[i];
				var map = (MapId)i;
				if (tileSet == null)
				{
					continue;
				}

				writer.WriteLine("sets[(int)MapId.{0}] = tiles = new ZoneTileSet();", map);

				for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
				{
					for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
					{
						var grid = tileSet.ZoneGrids[x, y];
						if (grid is ZoneGrid && ((ZoneGrid)grid).ZoneIds != null)
						{
							var sameId = true;
							var str = new StringBuilder(10000);
							var declStr = string.Format("tiles.ZoneGrids[{0},{1}] = ", x, y);
							str.Append(string.Format("new ZoneGrid(new uint[{0},{0}] ", TerrainConstants.ChunksPerTileSide));
							str.Append("{");
							for (var row = 0; row < TerrainConstants.ChunksPerTileSide; row++)
							{
								//str.Append(writer.BaseWriter.Indent + "\t");
								str.Append(GetLine(((ZoneGrid)grid).ZoneIds, row, TerrainConstants.ChunksPerTileSide, ref sameId));
								if (row < TerrainConstants.ChunksPerTileSide - 1)
								{
									str.Append(",");
								}
								//str.AppendLine();
							}
							str.Append("});");
							if (sameId)
							{
								var zoneId = ((ZoneGrid)grid).ZoneIds[0, 0];
								str = new StringBuilder(string.Format("new SimpleZoneGrid({0});", zoneId));
							}

							writer.WriteLine(declStr + str);
						}
					}
				}
			}
			writer.WriteLine("return sets;");
		}

		static string GetLine(this uint[,] arr, int x, int maxY, ref bool same)
		{
			var last = (uint)ZoneId.End;
			var str = new StringBuilder("{ ", 1000);
			for (var y = 0; y < maxY; y++)
			{
				var id = arr[x, y];
				str.Append(id);
				if (last != (uint)ZoneId.End)
				{
					same = same && last == id;
				}
				last = id;
				if (y < maxY - 1)
				{
					str.Append(", ");
				}
			}
			str.Append(" }");
			return str.ToString();
		}
	}
}
