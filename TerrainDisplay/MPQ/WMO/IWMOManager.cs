using System.Collections.Generic;
using TerrainDisplay.MPQ.ADT.Components;

namespace TerrainDisplay.MPQ.WMO
{
    public interface IWMOManager
    {
        List<VertexPositionNormalColored> RenderVertices
        {
            get; 
            set;
        }

        List<int> RenderIndices
        {
            get; 
            set;
        }

        /// <summary>
        /// Adds a WMO to the manager
        /// </summary>
        /// <param name="currentMODF">MODF (placement information for this WMO)</param>
        void AddWMO(MapObjectDefinition currentMODF);
    }
}