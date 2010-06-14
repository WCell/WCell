using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using TerrainDisplay;
using TerrainDisplay.MPQ;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WDT;
using TerrainDisplay.MPQ.WMO;

namespace TerrainDisplay.Extracted
{
    public class ExtractedTerrain : ITerrainManager
    {
        private int _mapId;
        private string _baseDirectory;
        private IADTManager _adtManager;
        private IWMOManager _wmoManager;
        private IM2Manager _m2Manager;

        private WDTFile _wdt;

        public IADTManager ADTManager
        {
            get { return _adtManager; }
        }

        public IWMOManager WMOManager
        {
            get { return _wmoManager; }
        }

        public IM2Manager M2Manager
        {
            get { return _m2Manager; }
        }

        public WDTFile WDT
        {
            get { return _wdt; }
        }

        public void LoadTile(TileIdentifier tileId)
        {
            var tileX = tileId.TileX;
            var tileY = tileId.TileY;
            var loaded = _adtManager.LoadTile(tileX, tileY);
            if (!loaded)
            {
                // Do something drastic

            }
        }

        public ExtractedTerrain(string dataPath, TileIdentifier tileId)
        {
            _mapId = tileId.MapId;
            _baseDirectory = dataPath;

            _adtManager = new ExtractedADTManager(this, _baseDirectory, _mapId);
            _m2Manager = new ExtractedM2Manager(_baseDirectory, _mapId);
            _wmoManager = new ExtractedWMOManager(_baseDirectory, _mapId);
            
        }

        public void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices)
        {
            var vecList = new List<Vector3>();
            var indexList = new List<int>();
            List<VertexPositionNormalColored> renderVertices;

            int offset;
            // Get the ADT triangles
            foreach (var tile in _adtManager.MapTiles)
            {
                offset = vecList.Count;
                renderVertices = tile.Vertices;
                if (renderVertices != null)
                {
                    foreach (var vertex in renderVertices)
                    {
                        var vec = vertex.Position;
                        PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                        vecList.Add(vec);
                    }
                    foreach (var index in tile.Indices)
                    {
                        indexList.Add(index + offset);
                    }
                }

                offset = vecList.Count;
                renderVertices = tile.Vertices;
                if (renderVertices == null) continue;
                foreach (var vertex in renderVertices)
                {
                    var vec = vertex.Position;
                    PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                    vecList.Add(vec);
                }
                foreach (var index in tile.Indices)
                {
                    indexList.Add(index + offset);
                }
            }

            // Get the WMO triangles
            offset = vecList.Count;
            renderVertices = _wmoManager.RenderVertices;
            var renderIndices = _wmoManager.RenderIndices;

            if (renderVertices != null)
            {
                foreach (var vertex in renderVertices)
                {
                    var vec = vertex.Position;
                    PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                    vecList.Add(vec);
                }
            }
            if (renderIndices != null)
            {
                foreach (var index in renderIndices)
                {
                    indexList.Add(index + offset);
                }
            }

            // Get the M2 triangles
            offset = vecList.Count;
            renderVertices = _m2Manager.RenderVertices;
            renderIndices = _m2Manager.RenderIndices;
            if (renderVertices != null)
            {
                foreach (var vertex in renderVertices)
                {
                    var vec = vertex.Position;
                    PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                    vecList.Add(vec);
                }
            }
            if (renderIndices != null)
            {
                foreach (var index in renderIndices)
                {
                    indexList.Add(index + offset);
                }
            }

            vertices = vecList.ToArray();
            indices = indexList.ToArray();
        }
    }
}
