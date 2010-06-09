using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MPQNav;
using MPQNav.MPQ;
using MPQNav.MPQ.ADT;
using MPQNav.MPQ.M2;
using MPQNav.MPQ.WDT;
using MPQNav.MPQ.WMO;

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

        public void LoadTile(int tileX, int tileY)
        {
            var loaded = _adtManager.LoadTile(tileX, tileY);
            if (!loaded)
            {
                // Do something drastic

            }
        }

        public ExtractedTerrain(string dataPath, int mapId)
        {
            _mapId = mapId;
            _baseDirectory = dataPath;

            _adtManager = new ExtractedADTManager(this, _baseDirectory, _mapId);
            _m2Manager = new ExtractedM2Manager(_baseDirectory, _mapId);
            _wmoManager = new ExtractedWMOManager(_baseDirectory, _mapId);
            
        }

        public Vector3[] GetRecastTriangleMesh()
        {
            var vecList = new List<Vector3>();
            List<VertexPositionNormalColored> vertices;

            // Get the ADT triangles
            foreach (var tile in _adtManager.MapTiles)
            {
                vertices = tile.Vertices;
                foreach (var index in tile.Indices)
                {
                    var vec = vertices[index].Position;
                    PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                    vecList.Add(vec);
                }

                vertices = tile.LiquidVertices;
                foreach (var index in tile.LiquidIndices)
                {
                    var vec = vertices[index].Position;
                    PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                    vecList.Add(vec);
                }
            }

            // Get the WMO triangles
            vertices = _wmoManager.RenderVertices;
            foreach (var index in _wmoManager.RenderIndices)
            {
                var vec = vertices[index].Position;
                PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                vecList.Add(vec);
            }

            // Get the M2 triangles
            vertices = _m2Manager.RenderVertices;
            foreach (var index in _m2Manager.RenderIndices)
            {
                var vec = vertices[index].Position;
                PositionUtil.TransformWoWCoordsToXNACoords(ref vec);
                vecList.Add(vec);
            }

            return vecList.ToArray();
        }
    }
}
