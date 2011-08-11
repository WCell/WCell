using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WCell.Constants.World;
using WCell.Terrain.Serialization;
using WCell.Util;
using WCell.Util.Graphics;
using Color = System.Drawing.Color;

namespace WCell.Terrain.GUI.UI
{
	public class MapTreeNode : TreeNode
	{
		public MapId Map { get; set; }

		public MapTreeNode(MapId map, TreeNode[] arr)
			: base(map.ToString(), arr)
		{
			Map = map;
		}
	}

	public class ZoneTreeNode : TreeNode
	{
		public ZoneId Zone { get; set; }
		public readonly List<Point2D> Coordinates = new List<Point2D>();

		public ZoneTreeNode(MapId map, ZoneId zone, List<Point2D> coords) :
			base(zone.ToString(), coords.TransformArray(coordss => new TileTreeNode(map, coordss)))
		{
			Zone = zone;
			Coordinates = coords;

			foreach (TileTreeNode child in Nodes)
			{
				if (!File.Exists(SimpleTileWriter.GetFileName(map, child.Coords.X, child.Coords.Y)))
				{
					child.BackColor = Color.Gray;
				}
			}
		}
	}

	public class TileTreeNode : TreeNode
	{
		public MapId Map { get; set; }
		public Point2D Coords { get; set; }

		public TileTreeNode(MapId map, Point2D coords) : base(string.Format("X:{0} Y:{1}", coords.X, coords.Y))
		{
			Map = map;
			Coords = coords;
		}
	}
}
