using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Util.Graphics;

namespace WCell.Terrain
{
	/// <summary>
	/// Represents a Terrain but only contains necessary information
	/// </summary>
	public class SimpleTerrain : Terrain
	{
		private bool m_IsWmoOnly;

		public SimpleTerrain(MapId mapId) : base(mapId)
		{
		}

		public override bool IsWMOOnly
		{
			get { return m_IsWmoOnly; }
		}

		protected override TerrainTile LoadTile(Point2D tileCoord)
		{
			// TODO: Load tile
			throw new NotImplementedException();
		}
	}
}
