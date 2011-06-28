using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.World;
using WCell.Core.Terrain;

namespace WCell.Core.TerrainAnalysis
{
	public interface ITerrainProvider
	{
		ITerrain CreateTerrain(MapId id);
	}
}