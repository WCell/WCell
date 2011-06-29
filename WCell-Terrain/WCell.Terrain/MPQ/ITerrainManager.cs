using System.Collections.Generic;
using TerrainDisplay;
using TerrainDisplay.Collision;
using WCell.Terrain.MPQ.ADT;
using WCell.Terrain.MPQ.M2;
using WCell.Terrain.MPQ.WMO;
using WCell.Util.Graphics;
using WCell.Terrain.MPQ.WDT;
using TerrainDisplay.Recast;

namespace WCell.Terrain.MPQ
{
    public interface ITerrainManager
    {
        IADTManager ADTManager { get; }
        IWMOManager WMOManager { get; }
        IM2Manager M2Manager { get; }
        NavMeshManager MeshManager { get; }
        WDT.WDT WDT { get; }

        bool LoadTile(TileIdentifier tileId);

		/// <summary>
		/// Gets the vertice and index lists in a way that is interpretable by Recast
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indices"></param>
        void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices);
    }
}