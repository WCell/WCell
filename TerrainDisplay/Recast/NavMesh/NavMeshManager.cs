using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrainDisplay.Recast
{
    public class NavMeshManager
    {
        private Color meshPolyColor = new Color(0, 192, 255);
        private NavMesh _mesh;

        public void SetNavMesh(NavMesh mesh)
        {
            _mesh = mesh;
        }

        public void GetMeshVerticesAndIndices(out VertexPositionNormalColored[] vertices, out int[] indices)
        {
            vertices = new VertexPositionNormalColored[0];
            indices = new int[0];

            if (_mesh == null) return;

            var verts = new List<VertexPositionNormalColored>();
            var idxs = new List<int>();

            var tiles = _mesh.Tiles;
            int offset;
            for (var x = 0; x < _mesh.Width; x++)
            {
                for (var y = 0; y < _mesh.Height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile == null) continue;

                    offset = verts.Count();
                    for (var i = 0; i < tile.Vertices.Length; i++)
                    {
                        var vertex = tile.Vertices[i];
                        PositionUtil.TransformRecastCoordsToWoWCoords(ref vertex);
                        verts.Add(new VertexPositionNormalColored(vertex, meshPolyColor, Vector3.Up));
                    }

                    foreach (var polygon in tile.Polygons)
                    {
                        if (polygon == null) continue;

                        TriangulatePolygon(polygon, offset, idxs);
                    }
                }
            }

            vertices = verts.ToArray();
            indices = idxs.ToArray();
        }

        private static void TriangulatePolygon(NavMeshPolygon polygon, int offset, List<int> idxs)
        {
            var numTris = polygon.Vertices.Length - 2;

            for (var t = 0; t < numTris; t++)
            {
                if (polygon.Vertices[t + 2] == 0) break;
                idxs.Add(offset + polygon.Vertices[0]);
                idxs.Add(offset + polygon.Vertices[t + 1]);
                idxs.Add(offset + polygon.Vertices[t + 2]);
            }
        }
    }
}
