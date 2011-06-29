using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerrainDisplay.Collision;
using TerrainDisplay.Util;
using WCell.Constants.World;
using WCell.Util.Graphics;
using TerrainDisplay;
using WCell.Terrain.MPQ;
using WCell.Terrain.MPQ.ADT;
using WCell.Terrain.MPQ.M2;
using WCell.Terrain.MPQ.WDT;
using WCell.Terrain.MPQ.WMO;
using TerrainDisplay.Recast;

namespace TerrainDisplay.Extracted
{
    public class ExtractedTerrainManager : ITerrainManager
    {
        private MapId _mapId;
        private string _baseDirectory;
        private WDT _wdt;


        private readonly IADTManager _adtManager;
        public IADTManager ADTManager
        {
            get { return _adtManager; }
        }

        private readonly IWMOManager _wmoManager;
        public IWMOManager WMOManager
        {
            get { return _wmoManager; }
        }

        private readonly IM2Manager _m2Manager;
        public IM2Manager M2Manager
        {
            get { return _m2Manager; }
        }

        private readonly NavMeshManager _meshManager;
        public NavMeshManager MeshManager
        {
            get { return _meshManager; }
        }

        public WDT WDT
        {
            get { return _wdt; }
        }

        public bool LoadTile(TileIdentifier tileId)
        {
            var loaded = _adtManager.LoadTile(tileId);
            if (!loaded)
            {
                // Do something drastic

            }
            return loaded;
        }

        public ExtractedTerrainManager(string dataPath, TileIdentifier tileId)
        {
            _mapId = tileId.MapId;
            _baseDirectory = dataPath;

            _adtManager = new ExtractedADTManager(this, _baseDirectory, _mapId);
            _m2Manager = new ExtractedM2Manager(_baseDirectory, _mapId);
            _wmoManager = new ExtractedWMOManager(_baseDirectory, _mapId);
            _meshManager = new NavMeshManager(this);
        }

        public void GetRecastTriangleMesh(out Vector3[] vertices, out int[] indices)
        {
            var vecList = new List<Vector3>();
            var indexList = new List<int>();
            List<Vector3> renderVertices;

            int offset;
            // Get the ADT triangles
            foreach (var tile in _adtManager.MapTiles)
            {
                offset = vecList.Count;
                renderVertices = tile.TerrainVertices;
                if (renderVertices != null)
                {
                    for (int i = 0; i < renderVertices.Count; i++)
                    {
                        var vertex = renderVertices[i];
                        vecList.Add(vertex);
                    }
                    foreach (var index in tile.Indices)
                    {
                        indexList.Add(index + offset);
                    }
                }

                offset = vecList.Count;
                renderVertices = tile.TerrainVertices;
                if (renderVertices == null) continue;
                for (int i = 0; i < renderVertices.Count; i++)
                {
                    var vertex = renderVertices[i];
                    vecList.Add(vertex);
                }
                foreach (var index in tile.Indices)
                {
                    indexList.Add(index + offset);
                }
            }

            // Get the WMO triangles
            offset = vecList.Count;
            renderVertices = _wmoManager.WmoVertices;
            var renderIndices = _wmoManager.WmoIndices;

            if (renderVertices != null)
            {
                for (int i = 0; i < renderVertices.Count; i++)
                {
                    var vertex = renderVertices[i];
                    vecList.Add(vertex);
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
                for (int i = 0; i < renderVertices.Count; i++)
                {
                    var vertex = renderVertices[i];
                    vecList.Add(vertex);
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
