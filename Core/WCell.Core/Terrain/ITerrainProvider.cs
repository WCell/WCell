using WCell.Constants.World;

namespace WCell.Core.Terrain
{
	public interface ITerrainProvider
	{
		ITerrain CreateTerrain(MapId id);
	}
}