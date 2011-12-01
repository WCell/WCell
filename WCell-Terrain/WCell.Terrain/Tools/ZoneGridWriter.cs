using System.Text;
using WCell.Constants.World;
using WCell.Util.Code;
using WCell.Util.Toolshed;
using WCell.Constants;

namespace WCell.Terrain.Tools
{
	public static class ZoneTileSetWriter
	{
		private static CodeFileWriter writer;
		private static ZoneTileSet[] tileSets;

		[Tool]
		public static void WriteZoneTileSets()
		{
			// TODO: Change file and uncomment
			//var file = ToolConfig.WCellConstantsRoot + "World/ZoneBoundaries.cs";
			//WriteZoneTileSets(file);
		}

		public static void WriteZoneTileSets(string outputFileName)
		{
			tileSets = ZoneBoundaryWriter.ExportTileSets();
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

                // Rows along the x-axis
				for (var x = 0; x < TerrainConstants.TilesPerMapSide; x++)
				{
                    // Columns along the y-axis
					for (var y = 0; y < TerrainConstants.TilesPerMapSide; y++)
					{
						var grid = tileSet.ZoneGrids[y, x];
						if (grid is ZoneGrid && ((ZoneGrid)grid).ZoneIds != null)
						{
							var sameId = true;
							var str = new StringBuilder(10000);
							var declStr = string.Format("tiles.ZoneGrids[{0},{1}] = ", y, x);
							str.Append(string.Format("new ZoneGrid(new uint[{0},{0}] ", TerrainConstants.ChunksPerTileSide));
							str.Append("{");
							for (var col = 0; col < TerrainConstants.ChunksPerTileSide; col++)
							{
								//str.Append(writer.BaseWriter.Indent + "\t");
								str.Append(GetLine(((ZoneGrid)grid).ZoneIds, col, TerrainConstants.ChunksPerTileSide, ref sameId));
								if (col < TerrainConstants.ChunksPerTileSide - 1)
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

		static string GetLine(this uint[,] arr, int col, int maxRow, ref bool same)
		{
			var last = (uint)ZoneId.End;
			var str = new StringBuilder("{ ", 1000);
			for (var row = 0; row < maxRow; row++)
			{
				var id = arr[col, row];
				str.Append(id);
				if (last != (uint)ZoneId.End)
				{
					same = same && last == id;
				}
				last = id;
				if (row < maxRow - 1)
				{
					str.Append(", ");
				}
			}
			str.Append(" }");
			return str.ToString();
		}
	}
}