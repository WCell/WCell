using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;
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
        private List<ExtractedWMO> _WMOs;
        private List<ExtractedM2> _WMOM2s;
        private string _baseDirectory;
        private int _mapId;

        private List<Vector3> _wmoVertices;
        private List<int> _wmoIndices;
        private List<Vector3> _wmoM2Vertices;
        private List<int> _wmoM2Indices;
        private List<Vector3> _wmoLiquidVertices;
        private List<int> _wmoLiquidIndices;

        public ExtractedWMOManager(string _baseDirectory, int _mapId)
        {
            this._baseDirectory = _baseDirectory;
            this._mapId = _mapId;
            _WMOs = new List<ExtractedWMO>();
            _WMOM2s = new List<ExtractedM2>();
        }

        public List<Vector3> WmoVertices
        {
            get 
            { 
                if (_wmoVertices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _wmoVertices; 
            }
            set { _wmoVertices = value; }
        }

        public List<int> WmoIndices
        {
            get
            {
                if (_wmoIndices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _wmoIndices;
            }
            set { _wmoIndices = value; }
        }

        public List<Vector3> WmoM2Vertices
        {
            get
            {
                if (_wmoM2Vertices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _wmoM2Vertices;
            }
            set { _wmoM2Vertices = value; }
        }

        public List<int> WmoM2Indices
        {
            get
            {
                if (_wmoM2Indices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _wmoM2Indices;
            }
            set { _wmoM2Indices = value; }
        }

        public List<Vector3> WmoLiquidVertices
        {
            get
            {
                if (_wmoLiquidVertices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _wmoLiquidVertices;
            }
            set { _wmoLiquidVertices = value; }
        }

        public List<int> WmoLiquidIndices
        {
            get
            {
                if (_wmoLiquidIndices == null)
                {
                    GenerateVerticesAndIndices();
                }
                return _wmoLiquidIndices;
            }
            set { _wmoLiquidIndices = value; }
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
            foreach (var group in wmo.Groups)
            {
                for (var j = 0; j < group.WmoVertices.Count; j++)
                {
                    var vec = group.WmoVertices[j];

                    Vector3 rotatedVector;
                    Vector3.Transform(ref vec, ref def.WMOToWorld, out rotatedVector);

                    // Translate
                    Vector3 finalVector;
                    Vector3.Add(ref rotatedVector, ref def.Position, out finalVector);

                    group.WmoVertices[j] = finalVector;

                    if (!group.HasLiquid) continue;

                    var liqOrigin = group.LiquidBaseCoords;

                    for (var xStep = 0; xStep <= group.LiqTileCountX; xStep++)
                    {
                        for (var yStep = 0; yStep <= group.LiqTileCountY; yStep++)
                        {
                            var xPos = liqOrigin.X + xStep * TerrainConstants.UnitSize;
                            var yPos = liqOrigin.Y + yStep * TerrainConstants.UnitSize;
                            var zPosTop = group.LiquidHeights[xStep, yStep];

                            var liqVecTop = new Vector3(xPos, yPos, zPosTop);

                            Vector3 rotatedTop;
                            Vector3.Transform(ref liqVecTop, ref def.WMOToWorld, out rotatedTop);

                            Vector3 vecTop;
                            Vector3.Add(ref rotatedTop, ref def.Position, out vecTop);

                            group.LiquidVertices.Add(vecTop);
                        }
                    }
                }
            }

            foreach (var m2 in m2s)
            {
                for (var j = 0; j < m2.BoundingVertices.Count; j++)
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
            _wmoVertices = new List<Vector3>();
            _wmoM2Vertices = new List<Vector3>();
            _wmoLiquidVertices = new List<Vector3>();

            foreach (var wmo in _WMOs)
            {
                if (wmo == null) continue;
                foreach (var group in wmo.Groups)
                {
                    if (group == null) continue;

                    var offset = _wmoVertices.Count;
                    foreach (var vec in group.WmoVertices)
                    {
                        _wmoVertices.Add(vec);
                    }

                    foreach (var index in group.Tree.Indices)
                    {
                        _wmoIndices.Add(index + offset);
                    }

                    if (!group.HasLiquid) continue;

                    offset = _wmoLiquidVertices.Count;
                    for (var row = 0; row < group.LiqTileCountX; row++)
                    {
                        for (var col = 0; col < group.LiqTileCountY; col++)
                        {
                            if (group.LiquidTileMap[row, col]) continue;

                            var index = ((row + 1) * (group.LiqTileCountY + 1) + col);
                            _wmoLiquidIndices.Add(offset + index);

                            index = (row * (group.LiqTileCountY + 1) + col);
                            _wmoLiquidIndices.Add(offset + index);

                            index = (row * (group.LiqTileCountY + 1) + col + 1);
                            _wmoLiquidIndices.Add(offset + index);

                            index = ((row + 1) * (group.LiqTileCountY + 1) + col + 1);
                            _wmoLiquidIndices.Add(offset + index);

                            index = ((row + 1) * (group.LiqTileCountY + 1) + col);
                            _wmoLiquidIndices.Add(offset + index);

                            index = (row * (group.LiqTileCountY + 1) + col + 1);
                            _wmoLiquidIndices.Add(offset + index);
                        }
                    }
                }
            }

            foreach (var m2 in _WMOM2s)
            {
                if (m2 == null) continue;
                var offset = _wmoM2Vertices.Count;
                foreach (var vec in m2.BoundingVertices)
                {
                    _wmoM2Vertices.Add(vec);
                }

                foreach (var index3 in m2.BoundingTriangles)
                {
                    _wmoM2Indices.Add(index3.Index0 + offset);
                    _wmoM2Indices.Add(index3.Index1 + offset);
                    _wmoM2Indices.Add(index3.Index2 + offset);
                }
            }
        }
    }
}
