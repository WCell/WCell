using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Terra.Geometry;
using Terra.Maps;
using Terra.Memory;
using WCell.Util.Graphics;
using Plane = Terra.Geometry.Plane;

namespace Terra.Greedy
{
    internal class GreedySubdivision : Subdivision
    {
        public FloatMask Mask;
        private readonly Heap heap;
        protected Map HeightMap;
        public PointState[,] PointStates;


        public uint PointCount { get; private set; }

        public Map Data
        {
            get { return HeightMap; }
        }

        public GreedySubdivision(Map map, FloatMask mask)
        {
            Mask = mask;
            HeightMap = map;
            heap = new Heap(128);

            var w = HeightMap.Width;
            var h = HeightMap.Height;

            PointStates = new PointState[w,h];
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    PointStates[x, y] = PointState.Unused;
                }
            }

            InitMesh(new Vector2(0.0f, 0.0f), 
                     new Vector2(0.0f, h - 1.0f),
                     new Vector2((w - 1.0f), (h - 1.0f)),
                     new Vector2((w - 1.0f), 0.0f));

            PointStates[0, 0] = PointState.Used;
            PointStates[0, (h - 1)] = PointState.Used;
            PointStates[(w - 1), (h - 1)] = PointState.Used;
            PointStates[(w - 1), 0] = PointState.Used;

            PointCount = 4;
        }

        public Edge Select(int subX, int subY)
        {
            return Select(subX, subY, null);
        }

        public Edge Select(int subX, int subY, Triangle tri)
        {
            if (PointStates[subX, subY] == PointState.Used)
            {
                Console.WriteLine("WARNING: Tried to reinsert point: {0}, {1}", subX, subY);
                return null;
            }

            PointStates[subX, subY] = PointState.Used;
            PointCount++;
            return Insert(new Vector2(subX, subY), tri);
        }

        public void ScanTriangle(TrackedTriangle tri)
        {
            var zPlane = new Plane();
            ComputePlane(zPlane, tri, HeightMap);

            var vecArray = new[] {tri.Point1, tri.Point2, tri.Point3};
            Array.Sort(vecArray, new Vector2TriangleComparer());
            
            var x1 = vecArray[0].X;
            var x2 = vecArray[0].X;
            var dx1 = (vecArray[1].X - vecArray[0].X)/(vecArray[1].Y - vecArray[0].Y);
            var dx2 = (vecArray[2].X - vecArray[0].X)/(vecArray[2].Y - vecArray[0].Y);
            var startY = (int) vecArray[0].Y;
            var endY = (int) vecArray[1].Y;
            var candidate = new Candidate();

            for(var y = startY; y < endY; y++)
            {
                ScanTriangleLine(zPlane, y, x1, x2, candidate);
                x1 += dx1;
                x2 += dx2;
            }

            dx1 = (vecArray[2].X - vecArray[1].X)/(vecArray[2].Y - vecArray[1].Y);
            x1 = vecArray[1].X;
            startY = (int) vecArray[1].Y;
            endY = (int) vecArray[2].Y;

            for (var y = startY; y < endY; y++)
            {
                ScanTriangleLine(zPlane, y, x1, x2, candidate);
                x1 += dx1;
                x2 += dx2;
            }


            if (candidate.Importance < 1e-4f)
            {
                if (tri.Token != Heap.NOT_IN_HEAP)
                {
                    heap.Kill(tri.Token);
                }
                tri.SetCandidate(-69, -69, 0.0f);
                return;
            }

            Debug.Assert(PointStates[candidate.X, candidate.Y] == PointState.Unused);
            tri.SetCandidate(candidate.X, candidate.Y, candidate.Importance);
            if (tri.Token == Heap.NOT_IN_HEAP)
            {
                heap.Insert(tri, candidate.Importance);
            }
            else
            {
                heap.Update(tri, candidate.Importance);
            }
        }

        public bool GreedyInsert()
        {
            var node = heap.Extract();
            if (node == null) return false;

            var trackedTri = node.Object as TrackedTriangle;
            if (trackedTri == null)
            {
                Console.Write("Attempted to access a null TrackedTriangle.");
                return true;
            }
            
            int subX, subY;
            trackedTri.GetCandidate(out subX, out subY);

            Select(subX, subY, trackedTri);
            return true;
        }

        public float MaxError()
        {
            var node = heap.Top();
            return (node == null) ? 0.0f : node.Importance;
        }

        public float RMSError()
        {
            var err = 0.0f;
            var width = HeightMap.Width;
            var height = HeightMap.Height;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var diff = Eval(i, j) - HeightMap.Eval(i, j);
                    err += (diff*diff);
                }
            }

            return (float)Math.Sqrt(err/(width*height));
        }

        public float Eval(int x, int y)
        {
            var point = new Vector2(x, y);
            var tri = Locate(point).LFace;

            var zPlane = new Plane();
            ComputePlane(zPlane, tri, HeightMap);

            return zPlane[x, y];
        }

        protected override Triangle AllocFace(Edge edge)
        {
            var tri = new TrackedTriangle(edge);
            heap.Insert(tri, -1.0f);
            return tri;
        }

        protected void ComputePlane(Plane plane, Triangle tri, Map map)
        {
            var point1 = tri.Point1;
            var point2 = tri.Point2;
            var point3 = tri.Point3;

            var vec1 = new Vector3(point1, map[point1.X, point1.Y]);
            var vec2 = new Vector3(point2, map[point2.X, point2.Y]);
            var vec3 = new Vector3(point3, map[point3.X, point3.Y]);

            plane.Init(vec1, vec2, vec3);
        }

        protected void ScanTriangleLine(Plane plane, int y, float x1, float x2, Candidate candidate)
        {
            var startX = (int) Math.Ceiling(Math.Min(x1, x2));
            var endX = (int) Math.Floor(Math.Max(x1, x2));
            if (startX > endX) return;

            var z0 = plane[startX, y];
            var dz = plane.A;

            for (var x = startX; x <= endX; x++)
            {
                if (PointStates[x, y] != PointState.Used)
                {
                    var z = HeightMap[x, y];
                    var diff = Math.Abs(z - z0);
                    candidate.Consider(x, y, diff);
                }

                z0 += dz;
            }
        }
    }
}
