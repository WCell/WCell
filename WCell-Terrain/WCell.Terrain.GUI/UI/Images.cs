using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Terrain.Serialization;

namespace WCell.Terrain.GUI.UI
{
	public class TileOverview : PictureBox
	{
		public TileOverview()
		{
			SizeMode = PictureBoxSizeMode.AutoSize;
		}

		public void InitImage(MapId mapId, int dx = 5, int margin = 1)
		{
			var map = new bool[TerrainConstants.TilesPerMapSide, TerrainConstants.TilesPerMapSide];
			var mpqFinder = WCellTerrainSettings.GetDefaultMPQFinder();
			for (var tileX = TerrainConstants.TilesPerMapSide - 1; tileX >= 0; --tileX)
			{
				for (var tileY = TerrainConstants.TilesPerMapSide-1; tileY >= 0; --tileY)
				{
					var fname = ADTReader.GetFilename(mapId, tileX, tileY);
					var archive = mpqFinder.GetArchive(fname);
					if (archive != null && archive.GetFileSize(fname) > 0)
					{
						// X is up
						// Y is left
						map[tileY, tileX] = true;
					}
				}
			}
			InitImage(map, dx, margin);
		}

		public void InitImage(bool[,] map, int dx, int margin)
		{
			var realDx = dx + 2 * margin;
			var w = map.GetLength(0);
			var color = Color.Teal;


			var img = new Bitmap(w * realDx, w * realDx);
			var brush = new SolidBrush(color);

			using (var graphics = Graphics.FromImage(img))
			{
				//set the resize quality modes to high quality 
				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

				// draw the outline


				// draw the map
				for (var y = 0; y < w; ++y)
				{
					for (var x = 0; x < w; ++x)
					{
						if (map[x, y])
						{
							graphics.FillRectangle(brush, x*realDx, y*realDx, dx, dx);
						}
					}
				}
			}

			if (Parent != null)
			{
				Invoke(new Action(() => Image = img));	
			}
			else
			{
				Image = img;
			}
		}
	}
}
