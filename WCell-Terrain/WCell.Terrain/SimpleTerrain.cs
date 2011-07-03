using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Terrain.Serialization;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	/// <summary>
	/// Represents a Terrain but only contains necessary information
	/// </summary>
	public class SimpleTerrain : Terrain
	{
		internal bool m_IsWmoOnly;

		public SimpleTerrain(MapId mapId) : base(mapId)
		{
		}

		public override bool IsWMOOnly
		{
			get { return m_IsWmoOnly; }
		}

		protected override TerrainTile LoadTile(Point2D tileCoord)
		{
			return SimpleADTReader.ReadTile(this, tileCoord);
		}

		/// <summary>
		/// Creates a dummy Terrain and loads the given tile into it
		/// </summary>
		public static TerrainTile LoadTile(MapId map, Point2D coords)
		{
			var terrain = new SimpleTerrain(map);
			terrain.TileProfile[coords.X, coords.Y] = true;
			return terrain.LoadTile(coords);
		}
	}
}
