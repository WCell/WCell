using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPQNav;
using MPQNav.MPQ.ADT.Components;
using MPQNav.MPQ.WMO;

namespace TerrainDisplay.Extracted
{
    public class ExtractedWMOManager : IWMOManager
    {
        private List<VertexPositionNormalColored> _renderVertices;

        private List<int> _renderIndices;

        public List<VertexPositionNormalColored> RenderVertices
        {
            get { return _renderVertices; }
            set { _renderVertices = value; }
        }

        public List<int> RenderIndices
        {
            get { return _renderIndices; }
            set { _renderIndices = value; }
        }

        public void AddWMO(MapObjectDefinition currentMODF)
        {
            throw new NotImplementedException();
        }
    }
}
