using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TerrainDisplay.Collision;
using TerrainDisplay.Util;
using TerrainDisplay.World.DBC;
using WCell.Constants.World;
using WCell.MPQTool;
using WCell.Util.Graphics;
using TerrainDisplay.MPQ.ADT;
using TerrainDisplay.MPQ.M2;
using TerrainDisplay.MPQ.WDT;
using TerrainDisplay.MPQ.WMO;
using TerrainDisplay.Recast;

namespace TerrainDisplay.MPQ
{
    public class MpqTerrainManager : ITerrainManager
    {
        public static MpqManager MpqManager;
        public TileIdentifier TileId;

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

        private readonly NavMeshManager _meshManager;
        public NavMeshManager MeshManager
        {
            get { return _meshManager; }
        }

        private readonly SelectedTriangleManager _selectedTriangleManager;
        public SelectedTriangleManager SelectedTriangleManager
        {
            get { return _selectedTriangleManager; }
        }

        private readonly WDT.WDT _wdt;
        public WDT.WDT WDT
        {
            get { return _wdt; }
        }


        public MpqTerrainManager(TileIdentifier tileId)
        {
            var wowRootDir = DBCTool.FindWowDir(TerrainDisplayConfig.WoWPath);
            MpqManager = new MpqManager(wowRootDir);
            TileId = tileId;

            var entries = GetMapEntries();
            MapInfo entry = null;
            foreach (var item in entries)
            {
                if (item.Id != tileId.MapId) continue;

                entry = item;
                break;
            }
            
            WDTParser.MpqManager = MpqManager;
            _wdt = WDTParser.Process(entry);
            
            _adtManager = new ADTManager(this);
            _wmoManager = new WMOManager();
            _m2Manager = new M2Manager();
            _meshManager = new NavMeshManager();
            _selectedTriangleManager = new SelectedTriangleManager(_adtManager);
        }

        public bool LoadTile(TileIdentifier tileId)
        {
            var loaded = _adtManager.LoadTile(tileId);
            return loaded;
        }

        public void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices)
        {
            int vecCount;
            int idxCount;
            CalcArraySizes(out vecCount, out idxCount);
            
            vertices = new Vector3[vecCount];
            indices = new int[idxCount];

            var vecOffset = 0;
            var idxOffset = 0;
            
            // Get the ADT triangles
        	foreach (var tile in _adtManager.MapTiles)
        	{
        	    if (tile == null) continue;

        	    // The heightmap information
        	    idxOffset = CopyIndicesToRecastArray(tile.Indices, indices, idxOffset, vecOffset);
                vecOffset = CopyVectorsToRecastArray(tile.TerrainVertices, vertices, vecOffset);

        	    // The liquid information
                //idxOffset = CopyIndicesToRecastArray(tile.LiquidIndices, indices, idxOffset, vecOffset);
                //vecOffset = CopyVectorsToRecastArray(tile.LiquidVertices, vertices, vecOffset);
        	}

        	// Get the WMO triangles
            idxOffset = CopyIndicesToRecastArray(_wmoManager.WmoIndices, indices, idxOffset, vecOffset);
            vecOffset = CopyVectorsToRecastArray(_wmoManager.WmoVertices, vertices, vecOffset);
            
            // Get the M2 triangles
            idxOffset = CopyIndicesToRecastArray(_m2Manager.RenderIndices, indices, idxOffset, vecOffset);
            vecOffset = CopyVectorsToRecastArray(_m2Manager.RenderVertices, vertices, vecOffset);
        }

        private void CalcArraySizes(out int vecCount, out int idxCount)
        {
            vecCount = CalcVecArraySize(true, false, true, true);
            idxCount = CalcIntArraySize(true, false, true, true);
        }

        private static int CopyIndicesToRecastArray(IList<int> renderIndices, IList<int> indices, int idxOffset, int vecOffset)
        {
            if (renderIndices != null)
            {
                const int second = 2;
                const int third = 1;

                var length = renderIndices.Count / 3;
                for (var i = 0; i < length; i++)
                {
                    var idx = i*3;
                    var index = renderIndices[idx + 0];
                    indices[idxOffset + idx + 0] = (index + vecOffset);

                    // reverse winding for recast
                    index = renderIndices[idx + second];
                    indices[idxOffset + idx + 1] = (index + vecOffset);

                    index = renderIndices[idx + third];
                    indices[idxOffset + idx + 2] = (index + vecOffset);
                }
                idxOffset += renderIndices.Count;
            }
            return idxOffset;
        }

        private static int CopyVectorsToRecastArray(IList<Vector3> renderVertices, IList<Vector3> vertices, int vecOffset)
        {
            if (renderVertices != null)
            {
                var length = renderVertices.Count;
                for (var i = 0; i < length; i++)
                {
                    var vertex = renderVertices[i];
                    PositionUtil.TransformWoWCoordsToRecastCoords(ref vertex);
                    vertices[vecOffset + i] = vertex;
                }
                vecOffset += renderVertices.Count;
            }
            return vecOffset;
        }

        private int CalcIntArraySize(bool includeTerrain, bool includeLiquid, bool includeWMO, bool includeM2)
        {
            var count = 0;
            List<int> renderIndices;
            for (var t = 0; t < _adtManager.MapTiles.Count; t++)
            {
                var tile = _adtManager.MapTiles[t];
                if (includeTerrain)
                {
                    renderIndices = tile.Indices;
                    if (renderIndices != null)
                    {
                        count += renderIndices.Count;
                    }
                }

                if (includeLiquid)
                {
                    renderIndices = tile.LiquidIndices;
                    if (renderIndices == null) continue;
                    count += renderIndices.Count;
                }
            }

            // Get the WMO triangles
            if (includeWMO)
            {

                renderIndices = _wmoManager.WmoIndices;
                if (renderIndices != null)
                {
                    count += renderIndices.Count;
                }
            }

            // Get the M2 triangles
            if (includeM2)
            {
                renderIndices = _m2Manager.RenderIndices;
                if (renderIndices != null)
                {
                    count += renderIndices.Count;
                }
            }

            return count;
        }

        private int CalcVecArraySize(bool includeTerrain, bool includeLiquid, bool includeWMO, bool includeM2)
        {
            var count = 0;
            List<Vector3> renderVertices;
            foreach (var tile in _adtManager.MapTiles)
            {
                if (tile == null) continue;

                if (includeTerrain)
                {
                    renderVertices = tile.TerrainVertices;
                    if (renderVertices != null)
                    {
                        count += renderVertices.Count;
                    }
                }

                if (includeLiquid)
                {
                    renderVertices = tile.LiquidVertices;
                    if (renderVertices == null) continue;
                    count += renderVertices.Count;
                }
            }

            // Get the WMO triangles
            if (includeWMO)
            {
                renderVertices = _wmoManager.WmoVertices;
                if (renderVertices != null)
                {
                    count += renderVertices.Count;
                }
            }

            // Get the M2 triangles
            if (includeM2)
            {
                renderVertices = _m2Manager.RenderVertices;
                if (renderVertices != null)
                {
                    count += renderVertices.Count;
                }
            }

            return count;
        }

        private static List<MapInfo> GetMapEntries()
        {
            var dbcPath = Path.Combine(TerrainDisplayConfig.DBCDir, TerrainDisplayConfig.MapDBCName);
            var dbcMapReader = new MappedDBCReader<MapInfo, DBCMapEntryConverter>(dbcPath);
            return dbcMapReader.Entries.Values.ToList();
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
