using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MPQNav.MPQ.M2;
using MPQNav.MPQ.ADT;
using MPQNav.MPQ.WDT;
using MPQNav.MPQ.WMO;

namespace MPQNav.MPQ
{
    public interface ITerrainManager
    {
        IADTManager ADTManager { get; }
        IWMOManager WMOManager { get; }
        IM2Manager M2Manager { get; }
        WDTFile WDT { get; }

        void LoadTile(int tileX, int tileY);

		/// <summary>
		/// Gets the vertice and index lists in a way that is interpretable by Recast
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="indices"></param>
        void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices);
    }
}