using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Geometry;
using Terra.Maps;
using Terra.Memory;
using WCell.Util.Graphics;
using Plane = Terra.Geometry.Plane;

namespace Terra.Greedy
{
    public class GreedySubdivision<T> : Subdivision
    {
        public ImportMask<float> Mask;
        private Heap heap;
        private uint count;
        protected Map<T> H;
        public DataPoint[,] IsUsed;


        public uint PointCount
        {
            get { return count; }
        }

        public Map<T> Data
        {
            get { return H; }
        }

        public GreedySubdivision(Map<T> map)
        {
            H = map;
            heap = new Heap(128);

            var w = H.Width;
            var h = H.Height;

            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    IsUsed[x, y] = DataPoint.Unused;
                }
            }

            InitMesh(new Vector2(0.0f, 0.0f), 
                     new Vector2(0.0f, h - 1.0f),
                     new Vector2((w - 1.0f), (h - 1.0f)),
                     new Vector2((w - 1.0f), 0.0f));

            IsUsed[0, 0] = DataPoint.Used;
            IsUsed[0, (h - 1)] = DataPoint.Used;
            IsUsed[(w - 1), (h - 1)] = DataPoint.Used;
            IsUsed[(w - 1), 0] = DataPoint.Used;

            count = 4;
        }

        public Edge Select(int subX, int subY)
        {
            return Select(subX, subY, null);
        }

        public Edge Select(int subX, int subY, Triangle tri)
        {
        }

        public void ScanTriangle(TrackedTriangle<T> tri)
        {
            Plane zPlane;
            ComputePlane(out zPlane, tri, H);


        }

        public int GreedyInsert()
        {
        }

        public float MaxError()
        {
        }

        public float RMSError()
        {
        }

        public float Eval(int x, int y)
        {
        }

        protected override Triangle AllocFace(Edge edge)
        {
        }

        protected void ComputePlane(out Plane plane, Triangle tri, Map<T> map)
        {
            var point1 = tri.Point1;
            var point2 = tri.Point2;
            var point3 = tri.Point3;

            var vec1 = new Vector3(point1, map[point1.X, point1.Y]);
            var vec2 = new Vector3(point2, map[point2.X, point2.Y]);
            var vec3 = new Vector3(point3, map[point3.X, point3.Y]);

            plane.Init(ref vec1, ref vec2, ref vec3);
        }

        protected void ScanTriangleLine(Plane plane, int y, float x1, float x2, Candidate candidate)
        {
            var startX = (int) Math.Ceiling(Math.Min(x1, x2));
            var endX = (int) Math.Floor(Math.Max(x1, x2));
            if (startX > endX) return;

            var z0 = plane[startX, y];
            var dz = plane.A;
            float z;
            float diff;

            for (var x = startX; x <= endX; x++)
            {
                if (IsUsed[x, y] != DataPoint.Unused)
                {
                    z0 += dz;
                    continue;
                }

                z = H[x, y];
                diff = (z - z0);
                diff = (diff > 0) ? diff : -diff;

                candidate.Consider(x, y, Mask.Apply(x, y, diff));

                z0 += dz;
            }
        }
    }
}
