using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using MPQNav.MPQ.M2;
using MPQNav.MPQ.ADT;
using MPQNav.MPQ.WDT;
using MPQNav.MPQ.WMO;

namespace MPQNav.MPQ
{
    public class MpqTerrainManager : ITerrainManager
    {
        private bool _loaded;
        private string _internalMapName;
        private int _mapId;
        private string _baseDirectory;

        private readonly IADTManager _adtManager;
        public IADTManager ADTManager
        {
            get { return _adtManager; }
        }

        private readonly WMOManager _wmoManager;
        public IWMOManager WMOManager
        {
            get { return _wmoManager; }
        }

        private readonly M2Manager _m2Manager;
        public IM2Manager M2Manager
        {
            get { return _m2Manager; }
        }

        private readonly WDTFile _wdtFile;
        public WDTFile WDT
        {
            get { return _wdtFile; }
        }

        //public List<VertexPositionNormalColored> Vertices = new List<VertexPositionNormalColored>(100000);
        //public List<int> Indices = new List<int>(100000);
        
        public MpqTerrainManager(string baseFileDirectory, ContinentType internalMapName, int mapId)
        {
            _mapId = mapId;
            _internalMapName = internalMapName.ToString();

            if (Directory.Exists(baseFileDirectory))
            {
                _loaded = true;
                _baseDirectory = baseFileDirectory;
            }
            else
            {
                MessageBox.Show("Invalid data directory entered. Please exit and update your app.CONFIG file",
                                "Invalid Data Directory");
            }

            _wdtFile = WDTParser.Process(baseFileDirectory, internalMapName);
            _adtManager = new ADTManager(baseFileDirectory, internalMapName, this);
            _wmoManager = new WMOManager(baseFileDirectory);
            _m2Manager = new M2Manager(baseFileDirectory);
        }

        public void LoadTile(int tileX, int tileY)
        {
            var loaded = _adtManager.LoadTile(tileX, tileY);
            _loaded = loaded;
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

        //public void LoadZone(int zoneId)
        //{
        //    // Rows
        //    for (var x = 0; x < 64; x++)
        //    {
        //        // Columns
        //        for (var y = 0; y < 64; y++)
        //        {
        //            var tileExists = _wdtFile.TileProfile[y, x];
        //            if (!tileExists) continue;

        //            var _adtPath = "WORLD\\MAPS";
        //            var _basePath = Path.Combine(_baseDirectory, _adtPath);
        //            var _continent = "Azeroth";
        //            var continentPath = Path.Combine(_basePath, _continent);

        //            if (!Directory.Exists(continentPath))
        //            {
        //                throw new Exception("Continent data missing");
        //            }

        //            var filePath = string.Format("{0}\\{1}_{2:00}_{3:00}.adt", continentPath, _continent, x, y);

        //            if (!File.Exists(filePath))
        //            {
        //                throw new Exception("ADT Doesn't exist: " + filePath);
        //            }

        //            var adt = ADTParser.Process(filePath, this);

        //            for (var j = 0; j < 16; j++)
        //            {
        //                for (var i = 0; i < 16; i++)
        //                {
        //                    var mapChunk = adt.MapChunks[i, j];
        //                    if (mapChunk == null) continue;
        //                    if (mapChunk.Header.AreaId != zoneId) continue;
        //                }
        //            }

        //            _adtManager.MapTiles.Clear();
        //        }
        //    }
        //}
    }
}
