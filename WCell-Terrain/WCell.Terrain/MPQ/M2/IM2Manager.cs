using System.Collections.Generic;
using WCell.Terrain.MPQ.ADT.Components;
using WCell.Util.Graphics;

namespace WCell.Terrain.MPQ.M2
{
    public interface IM2Manager
    {
        List<Vector3> RenderVertices { get; set; }

        List<int> RenderIndices { get; set; }

        /// <summary>
        /// Add a M2 to this manager.
        /// </summary>
        /// <param name="doodadDefinition">MDDF (placement information) for this M2</param>
        void Add(MapDoodadDefinition doodadDefinition);
    }
}