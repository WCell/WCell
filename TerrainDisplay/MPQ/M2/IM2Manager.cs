using System.Collections.Generic;
using MPQNav.MPQ.ADT.Components;

namespace MPQNav.MPQ.M2
{
    public interface IM2Manager
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
        /// Add a M2 to this manager.
        /// </summary>
        /// <param name="doodadDefinition">MDDF (placement information) for this M2</param>
        void Add(MapDoodadDefinition doodadDefinition);
    }
}