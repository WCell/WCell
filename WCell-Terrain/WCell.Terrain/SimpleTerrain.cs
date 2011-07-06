using System;
using System.Collections.Generic;
using System.IO;
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
		public delegate TerrainTile TerrainLoader(SimpleTerrain terrain, int x, int y);

		/// <summary>
		/// Creates a dummy Terrain and loads the given tile into it
		/// </summary>
		public static TerrainTile LoadTile(MapId map, int x, int y)
		{
			var terrain = new SimpleTerrain(map);
			terrain.TileProfile[x, y] = true;
			var tile = terrain.LoadTile(x, y);
			terrain.Tiles[x, y] = tile;
			return tile;
		}


		internal bool m_IsWmoOnly;

		public SimpleTerrain(MapId mapId, bool loadOnDemand = true)
			: this(mapId, SimpleTileReader.ReadTile, loadOnDemand)
		{
		}

		public SimpleTerrain(MapId mapId, TerrainLoader loader, bool loadOnDemand = true)
			: base(mapId, loadOnDemand)
		{
			Loader = loader;
		}

		public TerrainLoader Loader
		{
			get;
			set;
		}

		public override bool IsWMOOnly
		{
			get { return m_IsWmoOnly; }
		}

		public override void FillTileProfile()
		{
			var dir = new DirectoryInfo(SimpleTileWriter.GetMapDirectory(MapId));
			if (!dir.Exists) return;

			foreach (var file in dir.EnumerateFiles())
			{
				int tileX, tileY;
				if (SimpleTileWriter.GetTileCoordsFromFileName(file.Name, out tileX, out tileY))
				{
					TileProfile[tileX, tileY] = true;
				}
			}
		}

		protected override TerrainTile LoadTile(int x, int y)
		{
			return Loader(this, x, y);
		}
	}
}
