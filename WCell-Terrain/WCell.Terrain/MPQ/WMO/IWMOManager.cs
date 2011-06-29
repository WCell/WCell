using System.Collections.Generic;
using WCell.Terrain.MPQ.ADT.Components;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ.WMO
{
    public interface IWMOManager
    {
        List<Vector3> WmoVertices { get; set; }
        List<int> WmoIndices { get; set; }

        List<Vector3> WmoM2Vertices { get; set; }
        List<int> WmoM2Indices { get; set; }

        List<Vector3> WmoLiquidVertices { get; set; }
        List<int> WmoLiquidIndices { get; set; }

        /// <summary>
        /// Adds a WMO to the manager
        /// </summary>
        /// <param name="currentMODF">MODF (placement information for this WMO)</param>
        void AddWMO(MapObjectDefinition currentMODF);
    }
}