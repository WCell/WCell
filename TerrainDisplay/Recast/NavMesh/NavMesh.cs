using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TerrainDisplay.Recast
{
    public class NavMesh
    {
        public const ushort ExternalLinkId = 0x8000;

    	public readonly long Id;
		public readonly Vector3 Origin;
    	public readonly float TileWidth, TileHeight;
    	public readonly int MaxTileCount;

    	public readonly int Width, Height;
		public readonly NavMeshTile[,] Tiles;

		public NavMesh(long id, int w, int h, Vector3 origin, float tilew, float tileh, int maxTiles)
		{
			Id = id;
			Tiles = new NavMeshTile[Width = w, Height = h];
			Origin = origin;
			TileWidth = tilew;
			TileHeight = tileh;
			MaxTileCount = maxTiles;
		}

    	public bool Initialized
    	{
    		get;
			internal set;
    	}

		public override string ToString()
		{
			return string.Format("NavMesh #{0} ({1} x {2})", Id, Width, Height);
		}
    }
}
