using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Tools.Code;
using WCell.Constants.World;
using WCell.Util.Graphics;
using WCell.Util.Toolshed;

namespace WCell.Tools.Maps
{
	public static class RegionBoundaryWriter
	{
		private static CodeFileWriter writer;

		[Tool]
		public static void WriteRegionBoundaries()
		{
			WriteRegionBoundaries(ToolConfig.WCellConstantsRoot + "World/RegionBoundaries2.cs");
		}

		public static void WriteRegionBoundaries(string outputFileName)
		{
			using (writer = new CodeFileWriter(outputFileName, "WCell.Constants.World", "RegionBoundaries", "static class", "",
				"WCell.Util.Graphics"))
			{
				writer.WriteMethod("public", "static BoundingBox[]", "GetRegionBoundaries", "", WriteMethod);
			}
		}

		private static void WriteMethod()
		{
			var arr = RegionBoundaryUtil.ExportBoundaries();
			writer.WriteLine("var boxes = new BoundingBox[(int)MapId.End];");
			for (var i = 0; i < arr.Length; i++)
			{
				var bounds = arr[i];
				var map = (MapId)i;
				if (bounds == default(BoundingBox))
				{
					continue;
				}

				writer.WriteLine(
					"boxes[(int)MapId.{0}] = new BoundingBox(new Vector3({1}f, {2}f, {3}f), new Vector3({4}f, {5}f, {6}f));", 
					map,
					bounds.Min.X, bounds.Min.Y, bounds.Min.Z,
					bounds.Max.X, bounds.Max.Y, bounds.Max.Z);
			}
			writer.WriteLine("return boxes;");
		}
	}
}
