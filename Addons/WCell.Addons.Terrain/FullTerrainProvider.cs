using System;
using System.IO;
using WCell.Addons.Collision;
using WCell.Constants.World;
using WCell.Core.Paths;
using WCell.Core.Terrain.Paths;
using WCell.RealmServer.Global;
using WCell.Terrain;
using WCell.Util.Graphics;
using WCell.Core.Terrain;
using Path = WCell.Core.Paths.Path;

namespace WCell.Addons.Collision
{
    public class FullTerrainProvider : ITerrainProvider
    {
        public ITerrain CreateTerrain(MapId id)
        {
            if (!Terrain.Terrain.DoesMapExist(id))
            {
                return new EmptyTerrain();
            }

			// TODO: Use configuration to determine which reader to use (Simple or MPQ)
            //return new Terrain.Terrain(id);
			return new EmptyTerrain();
        }
    }
}