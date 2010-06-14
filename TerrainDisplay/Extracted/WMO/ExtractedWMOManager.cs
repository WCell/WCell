using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrainDisplay;
using TerrainDisplay.MPQ.ADT.Components;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.Extracted.M2;
using TerrainDisplay.Extracted.WMO;

namespace TerrainDisplay.Extracted
{
    public class ExtractedWMOManager : IWMOManager
    {
        private List<VertexPositionNormalColored> _renderVertices;
        private List<int> _renderIndices;
        private List<ExtractedWMO> _WMOs;
        private List<ExtractedM2> _WMOM2s;
        private string _baseDirectory;
        private int _mapId;

        public ExtractedWMOManager(string _baseDirectory, int _mapId)
        {
            this._baseDirectory = _baseDirectory;
            this._mapId = _mapId;
            _WMOs = new List<ExtractedWMO>();
            _WMOM2s = new List<ExtractedM2>();
        }

        public List<VertexPositionNormalColored> RenderVertices
        {
            get
            {
                if (_renderVertices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _renderVertices;
            }
            set { _renderVertices = value; }
        }

        public List<int> RenderIndices
        {
            get
            {
                if (_renderIndices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _renderIndices;
            }
            set { _renderIndices = value; }
        }

        public void AddWMO(MapObjectDefinition currentMODF)
        {
            if (currentMODF is ExtractedWMODefinition)
            {
                Add((ExtractedWMODefinition)currentMODF);
            }
        }

        private void Add(ExtractedWMODefinition def)
        {
            var wmo = ExtractedWMOParser.Process(_baseDirectory, _mapId, def.FilePath);

            var doodadSetId = def.DoodadSetId;
            var doodadSet = new List<ExtractedWMOM2Definition>();
            doodadSet.AddRange(wmo.WMOM2Defs[0].Values);
            if (doodadSetId > 0)
            {
                doodadSet.AddRange(wmo.WMOM2Defs[doodadSetId].Values);
            }

            var m2s = new List<ExtractedM2>(doodadSet.Count);
            foreach (var m2Def in doodadSet)
            {
                var m2 = ExtractedM2Parser.Process(_baseDirectory, _mapId, m2Def.FilePath);
                TransformM2(m2, m2Def);

                m2s.Add(m2);
            }
            
            Transform(wmo, m2s, def);

            _WMOs.Add(wmo);
            _WMOM2s.AddRange(m2s);
        }

        private static void Transform(ExtractedWMO wmo, List<ExtractedM2> m2s, ExtractedWMODefinition def)
        {
            for (var i = 0; i < wmo.Groups.Count; i++)
            {
                var group = wmo.Groups[i];
                if (group == null) continue;

                for (int j = 0; j < group.Vertices.Count; j++)
                {
                    var vec = group.Vertices[j];

                    Vector3 rotatedVector;
                    Vector3.Transform(ref vec, ref def.WMOToWorld, out rotatedVector);

                    // Translate
                    Vector3 finalVector;
                    Vector3.Add(ref rotatedVector, ref def.Position, out finalVector);

                    group.Vertices[j] = finalVector;
                }
            }

            for (var i = 0; i < m2s.Count; i++)
            {
                var m2 = m2s[i];

                for (int j = 0; j < m2.BoundingVertices.Count; j++)
                {
                    var vec = m2.BoundingVertices[j];
                    
                    Vector3 rotatedVector;
                    Vector3.Transform(ref vec, ref def.WMOToWorld, out rotatedVector);

                    // Translate
                    Vector3 finalVector;
                    Vector3.Add(ref rotatedVector, ref def.Position, out finalVector);

                    m2.BoundingVertices[j] = finalVector;
                }
            }
        }

        private static void TransformM2(ExtractedM2 m2, ExtractedWMOM2Definition m2Def)
        {
            for (var i = 0; i < m2.BoundingVertices.Count; i++)
            {
                var vec = m2.BoundingVertices[i];
                Vector3 rotatedVec;
                Vector3.Transform(ref vec, ref m2Def.ModeltoWMO, out rotatedVec);

                Vector3 finalVec;
                Vector3.Add(ref rotatedVec, ref m2Def.Position, out finalVec);

                m2.BoundingVertices[i] = finalVec;
            }
        }

        private void GenerateVerticesAndIndices()
        {
            _renderVertices = new List<VertexPositionNormalColored>();
            _renderIndices = new List<int>();

            foreach (var wmo in _WMOs)
            {
                if (wmo == null) continue;
                foreach (var group in wmo.Groups)
                {
                    if (group == null) continue;

                    var offset = _renderVertices.Count;
                    foreach (var vec in group.Vertices)
                    {
                        _renderVertices.Add(new VertexPositionNormalColored(vec, Color.Yellow, Vector3.Up));
                    }

                    foreach (var index in group.Tree.Indices)
                    {
                        _renderIndices.Add(index + offset);
                    }
                }
            }

            foreach (var m2 in _WMOM2s)
            {
                if (m2 == null) continue;
                var offset = _renderVertices.Count;
                foreach (var vec in m2.BoundingVertices)
                {
                    _renderVertices.Add(new VertexPositionNormalColored(vec, Color.HotPink, Vector3.Up));
                }

                foreach (var index3 in m2.BoundingTriangles)
                {
                    _renderIndices.Add(index3.Index0 + offset);
                    _renderIndices.Add(index3.Index1 + offset);
                    _renderIndices.Add(index3.Index2 + offset);
                }
            }
        }
    }
}
