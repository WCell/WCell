using System;
using System.IO;
using WCell.Addons.Terrain;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.Global;
using WCell.Terrain;
using WCell.Util.Graphics;
using WCell.Core.Terrain;


namespace WCell.Addons.Terrain
{
	using Terrain = WCell.Terrain.Terrain;

    public class FullTerrainProvider : ITerrainProvider
    {
        public ITerrain CreateTerrain(MapId id)
        {
            if (!Terrain.DoesMapExist(id))
            {
                return new EmptyTerrain();
            }

			// TODO: Use configuration to determine which reader to use (Simple or MPQ)
            //return new Terrain.Terrain(id);
			return new EmptyTerrain();
        }
    }
}