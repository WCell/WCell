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
        private const float intraTileLineWidth = 1.5f;
        private const float interTileLineWidth = 2.5f;
        private NavMesh _mesh;

        private Color MeshPolyColor { get { return Color.Black; } }

        public void SetNavMesh(NavMesh mesh)
        {
            _mesh = mesh;
        }

        public void GetMeshTileVerticesAndIndices(out VertexPositionNormalColored[] vertices, out int[] indices)
        {
            vertices = new VertexPositionNormalColored[0];
            indices = new int[0];

            if (_mesh == null) return;

            var verts = new List<VertexPositionNormalColored>();
            var idxs = new List<int>();
            
            DrawTileOutlines(true, verts, idxs);
            DrawTileOutlines(false, verts, idxs);

            vertices = verts.ToArray();
            indices = idxs.ToArray();
        }

        public void GetMeshVerticesAndIndices(out VertexPositionNormalColored[] vertices, out int[] indices)
        {
            vertices = new VertexPositionNormalColored[0];
            indices = new int[0];

            if (_mesh == null) return;

            var verts = new List<VertexPositionNormalColored>();
            var idxs = new List<int>();

            var tiles = _mesh.Tiles;
            var debugCount = 0;
            for (var x = 0; x < _mesh.Width; x++)
            {
                for (var y = 0; y < _mesh.Height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile == null) continue;
                    debugCount++;
                    //if (debugCount != 5) continue;
                    

                    var offset = verts.Count();
                    for (var i = 0; i < tile.Vertices.Length; i++)
                    {
                        var vertex = tile.Vertices[i];
                        PositionUtil.TransformRecastCoordsToWoWCoords(ref vertex);
                        verts.Add(new VertexPositionNormalColored(vertex, MeshPolyColor, Vector3.Down));
                    }

                    var detailOffset = verts.Count;
                    for (var i = 0; i < tile.DetailedVertices.Length; i++)
                    {
                        var vertex = tile.DetailedVertices[i];
                        PositionUtil.TransformRecastCoordsToWoWCoords(ref vertex);
                        verts.Add(new VertexPositionNormalColored(vertex, MeshPolyColor, Vector3.Down));
                    }

                    for (int i = 0; i < tile.Polygons.Length; i++)
                    {
                        var polygon = tile.Polygons[i];
                        if (polygon == null) continue;
                        if (polygon.Type == NavMeshPolyTypes.OffMeshConnection) continue;

                        var detail = tile.DetailPolygons[i];
                        for (var j = 0; j < detail.TriCount; j++)
                        {
                            var detailTri = tile.DetailedTriangles[detail.TriBase + j];
                            for (var k = 0; k < 3; k++)
                            {
                                var curVertIdx = detailTri[k];
                                if (curVertIdx < polygon.VertCount)
                                {
                                    var polyVert = polygon.Vertices[curVertIdx];
                                    idxs.Add(offset + polyVert);
                                }
                                else
                                {
                                    var detailIdx = (detail.VertBase + curVertIdx - polygon.VertCount);
                                    idxs.Add(detailOffset + detailIdx);
                                }
                            }
                        }
                        //TriangulatePolygon(polygon, offset, idxs);
                    }
                }
            }

            vertices = verts.ToArray();
            indices = idxs.ToArray();
        }

        private void DrawTileOutlines(bool drawInnerBorders, List<VertexPositionNormalColored> verts, List<int> idxs)
        {
            var tiles = _mesh.Tiles;
            var thr = 0.01f * 0.01f;

            for (var x = 0; x < _mesh.Width; x++)
            {
                for (var y = 0; y < _mesh.Height; y++)
                {
                    var tile = tiles[x, y];
                    if (tile == null) continue;

                    for (var i = 0; i < tile.Polygons.Length; i++)
                    {
                        var polygon = tile.Polygons[i];
                        if (polygon.Type == NavMeshPolyTypes.OffMeshConnection) continue;
                        for (var j = 0; j < polygon.VertCount; j++)
                        {
                            var color = Color.LimeGreen;
                            float scalar;

                            if (drawInnerBorders)
                            {
                                scalar = intraTileLineWidth;
                                if (polygon.Neighbors[j] == 0) continue;


                                if ((polygon.Neighbors[j] & NavMesh.ExternalLinkId) != 0)
                                {
                                    var connected = false;
                                    for (var k = polygon.FirstLink;
                                         k != NavMeshPolyLink.NullLink;
                                         k = tile.Links[k].Next)
                                    {
                                        if (tile.Links[k].Edge == j)
                                        {
                                            connected = true;
                                            break;
                                        }
                                    }

                                    color = (connected) ? Color.White : Color.Black;
                                }
                                else
                                {
                                    color = new Color(0, 48, 64, 0);
                                }
                            }
                            else
                            {
                                if (polygon.Neighbors[j] != 0) continue;
                                scalar = interTileLineWidth;
                            }

                            var v0 = tile.Vertices[polygon.Vertices[j]];
                            PositionUtil.TransformRecastCoordsToWoWCoords(ref v0);
                            var v1 = tile.Vertices[polygon.Vertices[(j + 1)%polygon.VertCount]];
                            PositionUtil.TransformRecastCoordsToWoWCoords(ref v1);

                            DrawLine(v0, v1, scalar, color, verts, idxs);

                            // Draw detail mesh edges which align with the actual poly edge.
                            // This is really slow.
                            /*var polyDetail = tile.DetailPolygons[i];
                            for (var k = 0; k < polyDetail.TriCount; ++k)
                            {
                                var detailedTriangle = tile.DetailedTriangles[(polyDetail.TriBase + k)];
                                var triVec = new Vector3[3];
                                for (var m = 0; m < 3; m++)
                                {
                                    if (detailedTriangle[m] < polygon.VertCount)
                                    {
                                        triVec[m] = tile.Vertices[polygon.Vertices[detailedTriangle[m]]];
                                    }
                                    else
                                    {
                                        triVec[m] =
                                            tile.DetailedVertices[
                                                polyDetail.VertBase + detailedTriangle[m] - polygon.VertCount];
                                    }
                                }

                                for (int m = 0, n = 2; m < 3; n = m++)
                                {
                                    if (((detailedTriangle[3] >> (n*2)) & 0x3) == 0)
                                        continue; // Skip inner detail edges.
                                    if (distancePtLine2d(ref triVec[n], ref v0, ref v1) < thr &&
                                        distancePtLine2d(ref triVec[m], ref v0, ref v1) < thr)
                                    {
                                        DrawLine(triVec[n], triVec[m], scalar, color, verts, idxs);
                                    }
                                }
                            }*/
                        }
                    }
                }
            }
        }

        private void DrawLine(Vector3 startPos, Vector3 endPos, float lineWidth, Color color, List<VertexPositionNormalColored> verts, List<int> idxs)
        {
            Vector3 lineDir;
            Vector3.Subtract(ref endPos, ref startPos, out lineDir);
            lineDir.Normalize();

            var rotMat = Matrix.CreateRotationZ(MathHelper.ToRadians(90));

            Vector3 width;
            Vector3.TransformNormal(ref lineDir, ref rotMat, out width);
            width *= (lineWidth/2);

            var botRight = (startPos + width);
            var botLeft = (startPos - width);
            var topRight = (endPos + width);
            var topLeft = (endPos - width);

            var offset = verts.Count();
            verts.Add(new VertexPositionNormalColored(botRight, color, Vector3.Up));
            verts.Add(new VertexPositionNormalColored(botLeft, color, Vector3.Up));
            verts.Add(new VertexPositionNormalColored(topRight, color, Vector3.Up));
            verts.Add(new VertexPositionNormalColored(topLeft, color, Vector3.Up));

            idxs.Add(offset + 0);
            idxs.Add(offset + 2);
            idxs.Add(offset + 3);

            idxs.Add(offset + 0);
            idxs.Add(offset + 3);
            idxs.Add(offset + 1);
        }

        private static void TriangulatePolygon(NavMeshPolygon polygon, int offset, List<int> idxs)
        {
            var numTris = polygon.VertCount - 2;

            for (var t = 0; t < numTris; t++)
            {
                idxs.Add(offset + polygon.Vertices[0]);
                idxs.Add(offset + polygon.Vertices[t + 1]);
                idxs.Add(offset + polygon.Vertices[t + 2]);
            }
        }

        static float distancePtLine2d(ref Vector3 pt, ref Vector3 p, ref Vector3 q)
        {
            float pqx = q.X - p.X;
            float pqz = q.Z - p.Z;
            float dx = pt.X - p.X;
            float dz = pt.Z - p.Z;
            float d = pqx*pqx + pqz*pqz;
            float t = pqx*dx + pqz*dz;
            if (d != 0) t /= d;
            dx = p.X + t*pqx - pt.X;
            dz = p.Z + t*pqz - pt.Z;
            return dx*dx + dz*dz;
        }
    }
}
