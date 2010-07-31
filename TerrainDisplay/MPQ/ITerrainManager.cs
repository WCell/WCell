using System.Collections.Generic;
using WCell.Util.Graphics;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WDT;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.Recast;

namespace TerrainDisplay.MPQ
{
    public interface ITerrainManager
    {
        IADTManager ADTManager { get; }
        IWMOManager WMOManager { get; }
        IM2Manager M2Manager { get; }
        NavMeshManager MeshManager { get; }
        WDTFile WDT { get; }

        void LoadTile(TileIdentifier tileId);

		/// <summary>
		/// Gets the vertice and index lists in a way that is interpretable by Recast
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indices"></param>
        void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices);
    }
}