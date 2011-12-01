using System;
using System.Collections.Generic;
using System.Diagnostics;
using WCell.Util.Graphics;

namespace QSlim.MxKit
{
    internal class MxBlockModel
    {
        internal static int MX_NORMAL_MASK = 0x3;
        internal static int MX_COLOR_MASK = (0x3 << 2);
        internal static int MX_TEXTURE_MASK = (0x3 << 4);
        internal static int MX_ALL_MASK = (MX_NORMAL_MASK | MX_COLOR_MASK | MX_TEXTURE_MASK);

        private List<MxVertex> Vertices;
        private List<MxFace> Faces;
        private List<MxNormal> Normals;
        private List<Color> Colors;

        internal int BindingMask;

        internal MxBlockModel(int numVerts, int numFaces)
        {
            Vertices = new List<MxVertex>(numVerts);
            Faces = new List<MxFace>(numFaces);

            Colors = null;
            Normals = null;
            ColorBinding = MxBinding.UnBound;
            NormalBinding = MxBinding.UnBound;
            BindingMask = MX_ALL_MASK;
        }

        internal MxBlockModel Clone(MxBlockModel m)
        {
            if (m == null)
            {
                m = new MxBlockModel(VertCount, FaceCount);
            }

            foreach (var vertex in Vertices)
            {
                m.AddVertex(vertex);
            }
            foreach (var face in Faces)
            {
                m.AddFace(face);
            }

            m.NormalBinding = NormalBinding;
            if (NormalBinding != MxBinding.UnBound)
            {
                m.Normals = new List<MxNormal>(Normals);
            }

            m.ColorBinding = ColorBinding;
            if (ColorBinding != MxBinding.UnBound)
            {
                m.Colors = new List<Color>(Colors);
            }

            return m;
        }

        internal int VertCount { get { return Vertices.Count; } }
        internal int FaceCount { get { return Faces.Count; } }
        internal int ColorCount { get { return (Colors == null ? 0 : Colors.Count); } }
        internal int NormalCount { get { return (Normals == null ? 0 : Normals.Count); } }

        internal int AddVertex(MxVertex vert)
        {
            return AddVertex(vert[0], vert[1], vert[2]);
        }

        internal int AddVertex(double x, double y, double z)
        {
            var id = AllocVertex(x, y, z);
            InitVertex(id);
            return id;
        }

        internal int AddFace(MxFace face)
        {
            return AddFace(face[0], face[1], face[2]);
        }

        internal int AddFace(int vertId0, int vertId1, int vertId2, bool willLink = true)
        {
            var id = AllocFace(vertId0, vertId1, vertId2);
            if (willLink) InitFace(id);
            return id;
        }

        internal int AddColor(Color color)
        {
            Debug.Assert(Colors != null);
            var id = Colors.Count;
            Colors.Add(color);
            return id;
        }

        internal int AddNormal(double x, double y, double z)
        {
            Debug.Assert(Normals != null);
            var id = Normals.Count;
            Normals.Add(new MxNormal(x, y, z));
            return id;
        }

        internal void RemoveVertex(int vertId)
        {
            Debug.Assert(vertId < VertCount);
            FreeVertex(vertId);
            Vertices.MxRemove(vertId);
            if (NormalBinding == MxBinding.PerVertex) Normals.RemoveAt(vertId);
            if (ColorBinding == MxBinding.PerVertex) Colors.RemoveAt(vertId);
        }

        internal void RemoveFace(int faceId)
        {
            Debug.Assert(faceId < FaceCount);
            FreeFace(faceId);
            Faces.MxRemove(faceId);
            if (NormalBinding == MxBinding.PerFace) Normals.RemoveAt(faceId);
            if (ColorBinding == MxBinding.PerFace) Normals.RemoveAt(faceId);
        }

        internal MxVertex Vertex(int vertId)
        {
            return Vertices[vertId];
        }

        internal void Vertex(int vertId, MxVertex vertex)
        {
            Vertices[vertId] = vertex;
        }

        internal MxFace Face(int faceId)
        {
            return Faces[faceId];
        }

        internal void Face(int faceId, MxFace face)
        {
            Faces[faceId] = face;
        }

        internal MxVertex Corner(int faceId, int corner)
        {
            return Vertex(Face(faceId)[corner]);
        }

        internal MxNormal Normal(int nId)
        {
            Debug.Assert(Normals != null);
            return Normals[nId];
        }

        internal void Normal(int nId, MxNormal normal)
        {
            Normals[nId] = normal;
        }

        internal Color Color(int cId)
        {
            Debug.Assert(Colors != null);
            return Colors[cId];
        }

        internal void Color(int cId, Color color)
        {
            Colors[cId] = color;
        }

        internal MxBinding ColorBinding { get; set; }

        internal MxBinding NormalBinding { get; set; }

        internal void ComputeFaceNormal(int faceId, ref double[] nml, bool willUnitize = true)
        {
            var v0 = Vertex(Face(faceId)[0]);
            var v1 = Vertex(Face(faceId)[1]);
            var v2 = Vertex(Face(faceId)[2]);

            var a = new[] {v1[0] - v0[0], v1[1] - v0[1], v1[2] - v0[2]};
            var b = new[] {v2[0] - v0[0], v2[1] - v0[1], v2[2] - v0[2]};

            MxVectorOps.Cross3(ref nml, a, b);
            if (willUnitize) MxVectorOps.Unitize3(ref nml);
        }

        internal void ComputeFacePlane(int faceId, ref double[] plane, bool willUnitize = true)
        {
            ComputeFaceNormal(faceId, ref plane, willUnitize);
            plane[3] = -MxVectorOps.Dot3(plane, Corner(faceId, 0).Pos);
        }

        internal double ComputeFaceArea(int faceId)
        {
            var normal = new double[3];
            ComputeFaceNormal(faceId, ref normal, false);
            return 0.5*MxVectorOps.Length3(normal);
        }

        internal double ComputeFacePerimeter(int faceId, bool[] flags)
        {
            var perim = 0.0;
            var face = Face(faceId);

            for (var i = 0; i < 3; i++)
            {
                if (flags != null)
                {
                   if (!flags[i]) continue;
                }

                var vi = Vertex(face[i]);
                var vj = Vertex(face[(i + 1)%3]);
                var diff = new double[3];

                MxVectorOps.Sub3(ref diff, vi.Pos, vj.Pos);
                perim += MxVectorOps.Length3(diff);
            }

            return perim;
        }

        internal double ComputeCornerAngle(int faceId, int corner)
        {
            var cornerPrev = (corner == 0) ? 2 : (corner - 1);
            var cornerNext = (corner == 2) ? 0 : (corner + 1);

            var ePrev = new double[3];
            MxVectorOps.Sub3(ref ePrev, Corner(faceId, cornerPrev).Pos, Corner(faceId, corner).Pos);
            MxVectorOps.Unitize3(ref ePrev);

            var eNext = new double[3];
            MxVectorOps.Sub3(ref eNext, Corner(faceId, cornerNext).Pos, Corner(faceId, corner).Pos);
            MxVectorOps.Unitize3(ref eNext);

            return Math.Acos(MxVectorOps.Dot3(ePrev, eNext));
        }

        protected virtual int AllocVertex(double x, double y, double z)
        {
            var nextId = Vertices.Count;
            Vertices.Add(new MxVertex(x, y, z));
            return nextId;
        }

        protected virtual void InitVertex(int vertId)
        {
        }

        protected virtual void FreeVertex(int vertId)
        {
        }

        protected virtual int AllocFace(int vertId0, int vertId1, int vertId2)
        {
            var nextId = Faces.Count;
            Faces.Add(new MxFace(vertId0, vertId1, vertId2));
            return nextId;
        }

        protected virtual void InitFace(int faceId)
        {
        }

        protected virtual void FreeFace(int faceId)
        {
        }
    }
}
