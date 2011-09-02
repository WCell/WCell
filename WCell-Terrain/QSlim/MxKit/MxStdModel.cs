using System.Collections.Generic;
using System.Diagnostics;

namespace QSlim.MxKit
{
    internal class MxStdModel : MxBlockModel
    {
        private readonly List<VertexData> vData;
        private readonly List<FaceData> fData;
        private readonly List<List<int>> faceLinks;


        internal MxStdModel(int numVerts, int numFaces)
            : base(numVerts, numFaces)
        {
            vData = new List<VertexData>(numVerts);
            fData = new List<FaceData>(numFaces);
            faceLinks = new List<List<int>>(numVerts);
        }

        internal MxStdModel Clone()
        {
            var model = new MxStdModel(VertCount, FaceCount);
            Clone(model);
            return model;
        }

        internal List<int> Neighbors(int vertId)
        {
            return faceLinks[vertId];
        }

        internal void Neighbors(int vertId, List<int> neighbors)
        {
            faceLinks[vertId] = neighbors;
        }

        #region Neighborhoods
        internal void MarkNeighborhood(int vertId, byte mark)
        {
            Debug.Assert(vertId < VertCount);
            
            var neighbors = faceLinks[vertId];
            foreach(var faceId in neighbors)
            {
                FaceMark(faceId, mark);
            }
        }

        internal void CollectUnMarkedNeighbors(int vertId, List<int> faces)
        {
            Debug.Assert(vertId < VertCount);

            var neighbors = faceLinks[vertId];
            foreach(var faceId in neighbors)
            {
                if (FaceMark(faceId) != 0x0) continue;

                faces.Add(faceId);
                FaceMark(faceId, 0x1);
            }
        }

        internal void MarkNeighborhoodDelta(int vertId, byte delta)
        {
            Debug.Assert(vertId < VertCount);
            
            var neighbors = faceLinks[vertId];
            foreach(var faceId in neighbors)
            {
                var curMark = FaceMark(faceId);
                FaceMark(faceId, (byte)(curMark + delta));
            }
        }

        internal void PartitionMarkedNeighbors(int vertId, ushort pivot, List<int> low, List<int> high)
        {
            Debug.Assert(vertId < VertCount);

            var neighbors = faceLinks[vertId];
            foreach(var faceId in neighbors)
            {
                var mark = FaceMark(faceId);
                if (mark == 0x0) continue;
                
                if (mark < pivot) low.Add(faceId);
                else high.Add(faceId);

                FaceMark(faceId, 0x0);
            }
        }

        internal void MarkCorners(List<int> faces, byte mark)
        {
            foreach (var faceId in faces)
            {
                var face = Face(faceId);
                VertMark(face[0], mark);
                VertMark(face[1], mark);
                VertMark(face[2], mark);
            }
        }

        internal void CollectUnMarkedCorners(List<int> faces, List<int> verts)
        {
            foreach (var faceId in faces)
            {
                var face = Face(faceId);
                for (var i = 0; i < 3; i++ )
                {
                    var vertId = face[i];
                    if (VertMark(vertId) != 0x0) continue;

                    verts.Add(vertId);
                    VertMark(vertId, 0x1);
                }
            }
        }

        internal void CollectEdgeNeighbors(int vertId0, int vertId1, List<int> faces)
        {
            MarkNeighborhood(vertId0, 0x1);
            MarkNeighborhood(vertId1, 0x0);
            CollectUnMarkedNeighbors(vertId0, faces);
        }

        internal void CollectVertexStar(int vertId, List<int> verts)
        {
            var neighbors = faceLinks[vertId];
            MarkCorners(neighbors, 0x0);
            VertMark(vertId, 0x1);
            CollectUnMarkedCorners(neighbors, verts);
        }

        internal void CollectNeighborhood(int vertId, int depth, List<int> faces)
        {
            faces.Clear();
            var neighbors = faceLinks[vertId];
            
            faces.AddRange(neighbors);

            while (depth > 0)
            {
                // Unmark the neighbors of all vertices in the neighborhood
                foreach(var faceId in neighbors)
                {
                    var face = Face(faceId);
                    MarkNeighborhood(face[0], 0x0);
                    MarkNeighborhood(face[1], 0x0);
                    MarkNeighborhood(face[2], 0x0);
                }

                // Mark the faces already accumulated
                foreach(var faceId in faces)
                {
                    FaceMark(faceId, 0x1);
                }

                // Collect all unmarked faces
                var limit = faces.Count;
                for (var i = 0; i < limit; i++)
                {
                    var faceId = faces[i];
                    var face = Face(faceId);
                    CollectUnMarkedNeighbors(face[0], faces);
                    CollectUnMarkedNeighbors(face[1], faces);
                    CollectUnMarkedNeighbors(face[2], faces);
                }
            }
        }
        #endregion

        #region TagAccessors
        internal bool VertexIsValid(int vertId)
        {
            var val = VertCheckTag(vertId, MxFlags.Valid);
            return (val != MxFlags.None);
        }

        internal void VertexMarkValid(int vertId)
        {
            VertSetTag(vertId, MxFlags.Valid);
        }

        internal void VertexMarkInvalid(int vertId)
        {
            VertUnSetTag(vertId, MxFlags.Valid);
        }

        internal bool VertexIsProxy(int vertId)
        {
            var val = VertCheckTag(vertId, MxFlags.Proxy);
            return val != MxFlags.None;
        }

        internal void VertexMarkProxy(int vertId)
        {
            VertSetTag(vertId, MxFlags.Proxy);
        }

        internal void VertexMarkNonProxy(int vertId)
        {
            VertUnSetTag(vertId, MxFlags.Proxy);
        }

        internal bool FaceIsValid(int faceId)
        {
            var val = FaceCheckTag(faceId, MxFlags.Valid);
            return (val != MxFlags.None);
        }

        internal void FaceMarkValid(int faceId)
        {
            FaceSetTag(faceId, MxFlags.Valid);
        }

        internal void FaceMarkInvalid(int faceId)
        {
            FaceUnSetTag(faceId, MxFlags.Valid);
        }

        internal MxFlags CheckVertexUserTag(int index, MxFlags tag)
        {
            return (vData[index].UserTag & tag);
        }

        internal void VertSetUserTag(int index, MxFlags tag)
        {
            var vdata = vData[index];
            vdata.UserTag |= tag;
            vData[index] = vdata;
        }

        internal void VertUnSetUserTag(int index, MxFlags tag)
        {
            var vdata = vData[index];
            vdata.UserTag &= (~tag);
            vData[index] = vdata;
        }

        internal byte VertUserMark(int index)
        {
            return vData[index].UserMark;
        }

        internal void VertUserMark(int index, byte mark)
        {
            var vdata = vData[index];
            vdata.UserMark = mark;
            vData[index] = vdata;
        }

        internal MxFlags FaceCheckUserTag(int index, MxFlags tag)
        {
            return (fData[index].UserTag & tag);
        }

        internal void FaceSetUserTag(int index, MxFlags tag)
        {
            var fdata = fData[index];
            fdata.UserTag |= tag;
            fData[index] = fdata;
        }

        internal void FaceUnSetUserTag(int index, MxFlags tag)
        {
            var fdata = fData[index];
            fdata.UserTag &= (~tag);
            fData[index] = fdata;
        }

        internal byte FaceUserMark(int index)
        {
            return fData[index].UserMark;
        }

        internal void FaceUserMark(int index, byte mark)
        {
            var fdata = fData[index];
            fdata.UserMark = mark;
            fData[index] = fdata;
        }

        protected MxFlags VertCheckTag(int index, MxFlags tag)
        {
            return (vData[index].Tag & tag);
        }

        protected void VertSetTag(int index, MxFlags tag)
        {
            var vdata = vData[index];
            vdata.Tag |= tag;
            vData[index] = vdata;
        }

        protected void VertUnSetTag(int index, MxFlags tag)
        {
            var vdata = vData[index];
            vdata.Tag &= (~tag);
            vData[index] = vdata;
        }

        protected byte VertMark(int index)
        {
            return vData[index].Mark;
        }

        protected void VertMark(int index, byte mark)
        {
            var vdata = vData[index];
            vdata.Mark = mark;
            vData[index] = vdata;
        }

        protected MxFlags FaceCheckTag(int index, MxFlags tag)
        {
            return (fData[index].Tag & tag);
        }

        protected void FaceSetTag(int index, MxFlags tag)
        {
            var fdata = fData[index];
            fdata.Tag |= tag;
            fData[index] = fdata;
        }

        protected void FaceUnSetTag(int index, MxFlags tag)
        {
            var fdata = fData[index];
            fdata.Tag &= (~tag);
            fData[index] = fdata;
        }

        internal byte FaceMark(int index)
        {
            return fData[index].Mark;
        }

        internal void FaceMark(int index, byte mark)
        {
            var fdata = fData[index];
            fdata.Mark = mark;
            fData[index] = fdata;
        }
        #endregion

        internal void ComputeVertexNormal(int vertId, ref double[] nml)
        {
            for (var i = 0; i < 3; i++)
            {
                nml[i] = 0.0;
            }
            var star = faceLinks[vertId];

            foreach (var faceId in star)
            {
                var faceNml = new double[3];

                ComputeFaceNormal(faceId, ref faceNml, false);
                MxVectorOps.AddInto3(ref nml, faceNml);
            }

            if (star.Count > 1) MxVectorOps.Unitize3(ref nml);
        }

        internal void SynthesizeNormals(int start)
        {
            var nml = new double[3];
            switch(NormalBinding)
            {
                case(MxBinding.PerFace):
                    {
                        for (var faceId = 0; faceId < FaceCount; faceId++)
                        {
                            ComputeFaceNormal(faceId, ref nml);
                            AddNormal(nml[0], nml[1], nml[2]);
                        }
                        break;
                    }
                case(MxBinding.PerVertex):
                    {
                        for (var vertId = 0; vertId < VertCount; vertId++)
                        {
                            ComputeVertexNormal(vertId, ref nml);
                            AddNormal(nml[0], nml[1], nml[2]);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        internal void RemapVertex(int vertIdFrom, int vertIdTo)
        {
            Debug.Assert(vertIdFrom < VertCount);
            Debug.Assert(vertIdTo < VertCount);
            Debug.Assert(VertexIsValid(vertIdFrom));
            Debug.Assert(VertexIsValid(vertIdTo));

            var neighborsFrom = faceLinks[vertIdFrom];
            foreach (var faceId in neighborsFrom)
            {
                var face = Face(faceId);
                face.RemapVertex(vertIdFrom, vertIdTo);
            }

            MarkNeighborhood(vertIdFrom, 0x0);
            MarkNeighborhood(vertIdTo, 0x1);
            
            CollectUnMarkedNeighbors(vertIdFrom, faceLinks[vertIdTo]);
            faceLinks[vertIdFrom].Clear();
        }

        internal int SplitEdge(int vertA, int vertB)
        {
            var a = Vertex(vertA);
            var b = Vertex(vertB);

            var midX = (a[0] - b[0])*0.5;
            var midY = (a[1] - b[1])*0.5;
            var midZ = (a[2] - b[2])*0.5;

            return SplitEdge(vertA, vertB, midX, midY, midZ);
        }

        internal int SplitEdge(int vertA, int vertB, double midX, double midY, double midZ)
        {
            Debug.Assert(vertA < VertCount);
            Debug.Assert(vertB < VertCount);
            Debug.Assert(VertexIsValid(vertA));
            Debug.Assert(VertexIsValid(vertB));
            Debug.Assert(vertA != vertB);

            var faces = new List<int>();
            CollectEdgeNeighbors(vertA, vertB, faces);
            Debug.Assert(faces.Count > 0);

            var newVertId = AddVertex(midX, midY, midZ);
            foreach (var faceId in faces)
            {
                var face = Face(faceId);
                var vertC = face.OppositeVertex(vertA, vertB);
                Debug.Assert(vertA != vertC);
                Debug.Assert(vertB != vertC);
                Debug.Assert(VertexIsValid(vertC));

                // Remap vertB to newVert
                face.RemapVertex(vertB, newVertId);
                faceLinks[newVertId].Add(faceId);

                // Remove faceId from the neighbors of vertB
                faceLinks[vertB].Remove(faceId);

                // Ensure correct winding
                if (face.IsInOrder(newVertId, vertC))
                {
                    AddFace(newVertId, vertB, vertC);
                }
                else
                {
                    AddFace(newVertId, vertC, vertB);
                }
            }

            return newVertId;
        }

        internal void FlipEdge(int vertA, int vertB)
        {
            var faces = new List<int>();
            CollectEdgeNeighbors(vertA, vertB, faces);
            if (faces.Count != 2) return;

            var faceId0 = faces[0];
            var face0 = Face(faceId0);
            var vertC = face0.OppositeVertex(vertA, vertB);
            var faceId1 = faces[1];
            var face1 = Face(faceId1);
            var vertD = face1.OppositeVertex(vertA, vertB);

            faceLinks[vertA].Remove(faceId1);
            faceLinks[vertB].Remove(faceId0);
            faceLinks[vertC].Add(faceId1);
            faceLinks[vertD].Add(faceId0);

            face0.RemapVertex(vertB, vertD);
            face1.RemapVertex(vertA, vertC);
        }

        internal void SplitFace4(int faceId, List<int> newVerts)
        {
            var face = Face(faceId);
            var vertId0 = face[0];
            var vertId1 = face[1];
            var vertId2 = face[2];

            var pivot = SplitEdge(vertId0, vertId1);
            var newId1 = SplitEdge(vertId1, vertId2);
            var newId2 = SplitEdge(vertId0, vertId2);

            if (newVerts != null)
            {
                newVerts.Clear();
                newVerts.Add(pivot);
                newVerts.Add(newId1);
                newVerts.Add(newId2);
            }

            FlipEdge(pivot, vertId2);
        }

        internal void CleanUpForOutput()
        {
            for(var vertId = 0; vertId < VertCount; vertId++)
            {
                if (!VertexIsValid(vertId)) continue; 
                if (faceLinks[vertId].Count > 0) continue;

                VertexMarkInvalid(vertId);
            }

            CompactVertices();
        }

        internal void CompactVertices()
        {
            int oldId, newId = 0;

            for (oldId = 0; oldId < VertCount; oldId++)
            {
                if (!VertexIsValid(oldId)) continue;

                // Vertex(oldId) is valid
                if (newId == oldId)
                {
                    newId++;
                    continue;
                }
                
                Vertex(newId, Vertex(oldId));
                if (NormalBinding == MxBinding.PerVertex) Normal(newId, Normal(oldId));
                if (ColorBinding == MxBinding.PerVertex) Color(newId, Color(oldId));

                var temp = faceLinks[newId];
                faceLinks[newId] = faceLinks[oldId];
                faceLinks[oldId] = temp;

                VertexMarkValid(newId);

                foreach (var faceId in faceLinks[newId])
                {
                    var face = Face(faceId);
                    face.RemapVertex(oldId, newId);
                }
                newId++;
            }

            for (oldId = newId; newId < VertCount; )
            {
                // VertCount is reduced with each call to RemoveVertex
                RemoveVertex(oldId);
            }
        }

        internal void UnLinkFace(int faceId)
        {
            var face = Face(faceId);
            FaceMarkInvalid(faceId);

            faceLinks[face[0]].Remove(faceId);
            faceLinks[face[1]].Remove(faceId);
            faceLinks[face[2]].Remove(faceId);

            Debug.Assert(!faceLinks[face[0]].Contains(faceId));
            Debug.Assert(!faceLinks[face[1]].Contains(faceId));
            Debug.Assert(!faceLinks[face[2]].Contains(faceId));
        }

        internal void RemoveDegeneracy(List<int> faces)
        {
            foreach (var faceId in faces)
            {
                var face = Face(faceId);
                Debug.Assert(FaceIsValid(faceId));

                if (face[0] == face[1] || face[1] == face[2] || face[2] == face[0])
                {
                    UnLinkFace(faceId);
                }
            }
        }

        internal void ApplyContraction(MxPairContraction conx)
        {
            var vertId1 = conx.VertId1;
            var vertId2 = conx.VertId2;

            // Move vert1 to new position
            var pos = Vertex(vertId1).Pos;
            MxVectorOps.AddInto3(ref pos, conx.DeltaV1);
            Vertex(vertId1).Pos = pos;

            // Remove dead faces
            foreach (var faceId in conx.DeadFaces)
            {
                UnLinkFace(faceId);
            }

            // Modify changed faces
            for (var i = conx.DeltaPivot; i < conx.DeltaFaces.Count; i++)
            {
                var faceId = conx.DeltaFaces[i];
                var face = Face(faceId);
                face.RemapVertex(vertId2, vertId1);
                faceLinks[vertId1].Add(faceId);
            }

            if (NormalBinding == MxBinding.PerFace)
            {
                var nml = new double[3];
                foreach (var faceId in conx.DeltaFaces)
                {
                    ComputeFaceNormal(faceId, ref nml);
                    Normal(faceId, new MxNormal(nml));
                }
            }

            // Remove v2
            VertexMarkInvalid(vertId2);
            faceLinks[vertId2].Clear();
        }

        internal void ApplyExpansion(MxPairContraction conx)
        {
            var vertId1 = conx.VertId1;
            var vertId2 = conx.VertId2;

            var newPos1 = new double[3];
            MxVectorOps.Sub3(ref newPos1, Vertex(vertId1).Pos, conx.DeltaV2);
            Vertex(vertId2).Pos = newPos1;

            var newPos2 = Vertex(vertId2).Pos;
            MxVectorOps.SubFrom3(ref newPos2, conx.DeltaV1);
            Vertex(vertId2).Pos = newPos2;

            foreach (var faceId in conx.DeadFaces)
            {
                FaceMarkValid(faceId);

                var face = Face(faceId);
                faceLinks[face[0]].Add(faceId);
                faceLinks[face[1]].Add(faceId);
                faceLinks[face[2]].Add(faceId);
            }

            for (var i = conx.DeltaPivot; i < conx.DeltaFaces.Count; i++)
            {
                var faceId = conx.DeltaFaces[i];
                var face = Face(faceId);
                face.RemapVertex(vertId1, vertId2);
                faceLinks[vertId2].Add(faceId);
                faceLinks[vertId1].Remove(faceId);
            }

            if (NormalBinding == MxBinding.PerFace)
            {
                var nml = new double[3];
                foreach(var faceId in conx.DeltaFaces)
                {
                    ComputeFaceNormal(faceId, ref nml);
                    Normal(faceId, new MxNormal(nml));
                }

                foreach(var faceId in conx.DeadFaces)
                {
                    ComputeFaceNormal(faceId, ref nml);
                    Normal(faceId, new MxNormal(nml));
                }
            }

            VertexMarkValid(vertId2);
        }

        internal void Contract(int vertId1, int vertId2, double[] newVert, MxPairContraction conx)
        {
            ComputeContraction(vertId1, vertId2, conx, null);
            MxVectorOps.Sub3(ref conx.DeltaV1, newVert, Vertex(vertId1).Pos);
            MxVectorOps.Sub3(ref conx.DeltaV2, newVert, Vertex(vertId2).Pos);
            ApplyContraction(conx);
        }

        internal void Contract(int vertId1, int vertId2, int vertId3, double[] newVert, List<int> changed)
        {
            MarkNeighborhood(vertId1, 0x0);
            MarkNeighborhood(vertId2, 0x0);
            MarkNeighborhood(vertId3, 0x0);
            
            changed.Clear();
            CollectUnMarkedNeighbors(vertId1, changed);
            CollectUnMarkedNeighbors(vertId2, changed);
            CollectUnMarkedNeighbors(vertId3, changed);

            Vertex(vertId1).Pos[0] = newVert[0];
            Vertex(vertId1).Pos[1] = newVert[1];
            Vertex(vertId1).Pos[2] = newVert[2];

            RemapVertex(vertId2, vertId1);
            RemapVertex(vertId3, vertId1);

            RemoveDegeneracy(changed);

            if (NormalBinding != MxBinding.PerFace) return;

            var nml = new double[3];
            foreach (var faceId in changed)
            {
                if (!FaceIsValid(faceId)) continue;

                ComputeFaceNormal(faceId, ref nml);
                Normal(faceId, new MxNormal(nml));
            }
        }

        internal void Contract(int vertId1, List<int> rest, double[] newVert, List<int> changed)
        {
            MarkNeighborhood(vertId1, 0x0);
            foreach (var vertId in rest)
            {
                MarkNeighborhood(vertId, 0x0);
            }

            changed.Clear();

            CollectUnMarkedNeighbors(vertId1, changed);
            foreach(var vertId in rest)
            {
                CollectUnMarkedNeighbors(vertId, changed);
            }

            Vertex(vertId1).Pos[0] = newVert[0];
            Vertex(vertId1).Pos[1] = newVert[1];
            Vertex(vertId1).Pos[2] = newVert[2];

            foreach (var vertId in rest)
            {
                RemapVertex(vertId, vertId1);
            }

            RemoveDegeneracy(changed);
        }

        internal void ComputeContraction(int vertId1, int vertId2, MxPairContraction conx, double[] newVert)
        {
            conx.VertId1 = vertId1;
            conx.VertId2 = vertId2;

            if (newVert != null)
            {
                MxVectorOps.Sub3(ref conx.DeltaV1, newVert, Vertex(vertId1).Pos);
                MxVectorOps.Sub3(ref conx.DeltaV2, newVert, Vertex(vertId2).Pos);
            }
            else
            {
                conx.DeltaV1 = new[] { 0.0, 0.0, 0.0 };
                conx.DeltaV2 = new[] { 0.0, 0.0, 0.0 };
            }

            conx.DeltaFaces.Clear();
            conx.DeadFaces.Clear();

            MarkNeighborhood(vertId2, 0x0);
            MarkNeighborhood(vertId1, 0x1);
            MarkNeighborhoodDelta(vertId2, 0x1);

            PartitionMarkedNeighbors(vertId1, 0x2, conx.DeltaFaces, conx.DeadFaces);
            conx.DeltaPivot = conx.DeltaFaces.Count;
            PartitionMarkedNeighbors(vertId2, 0x2, conx.DeltaFaces, conx.DeadFaces);
        }

        internal void ComputeContraction(int faceId, MxFaceContraction conx)
        {
            var face = Face(faceId);
            conx.FaceId = faceId;
            conx.DeltaV1 = new[] {0.0, 0.0, 0.0};
            conx.DeltaV2 = new[] {0.0, 0.0, 0.0};
            conx.DeltaV3 = new[] {0.0, 0.0, 0.0};

            conx.DeltaFaces.Clear();
            conx.DeadFaces.Clear();

            MarkNeighborhood(face[0], 0x0);
            MarkNeighborhood(face[1], 0x0);
            MarkNeighborhood(face[2], 0x0);

            MarkNeighborhood(face[0], 0x1);
            MarkNeighborhoodDelta(face[1], 0x1);
            MarkNeighborhoodDelta(face[2], 0x1);

            FaceMark(faceId, 0x0);

            PartitionMarkedNeighbors(face[0], 0x2, conx.DeltaFaces, conx.DeadFaces);
            PartitionMarkedNeighbors(face[1], 0x2, conx.DeltaFaces, conx.DeadFaces);
            PartitionMarkedNeighbors(face[2], 0x2, conx.DeltaFaces, conx.DeadFaces);
        }

        internal int ResolveProxies(int vertId)
        {
            while(VertexIsProxy(vertId))
            {
                vertId = Vertex(vertId).Info.Proxy.Parent;
            }
            return vertId;
        }

        internal double[] VertexPosition(int vertId)
        {
            return Vertex(ResolveProxies(vertId)).Pos;
        }

        internal int VertexProxyParent(int vertId)
        {
            Debug.Assert(VertexIsProxy(vertId));
            return Vertex(vertId).Info.Proxy.Parent;
        }

        internal void VertexProxyParent(int vertId, int parentId)
        {
            Vertex(vertId).Info.Proxy.Parent = parentId;
        }

        internal int AddProxyVertex(int parent)
        {
            var vertId = AllocVertex(0.0, 0.0, 0.0); // position ignored
            VertexMarkProxy(vertId);
            VertexProxyParent(vertId, parent);

            return vertId;
        }

        protected override int AllocVertex(double x, double y, double z)
        {
            var id = base.AllocVertex(x, y, z);
            vData.Add(new VertexData
            {
                Tag = 0x0,
                UserTag = 0x0
            });
            VertexMarkValid(id);

            var linkId = faceLinks.Count;
            faceLinks.Add(new List<int>(6));
            Debug.Assert(linkId == id);
            Debug.Assert(faceLinks[id].Count == 0);
            return id;
        }

        protected override void FreeVertex(int vertId)
        {
            faceLinks.MxRemove(vertId);
            vData.MxRemove(vertId);
        }

        protected override void FreeFace(int faceId)
        {
            fData.MxRemove(faceId);
        }

        protected override int AllocFace(int vertId0, int vertId1, int vertId2)
        {
            var id = base.AllocFace(vertId0, vertId1, vertId2);
            Debug.Assert(fData.Count == id);
            fData.Add(new FaceData
            {
                Tag = 0x0,
                UserTag = 0x0
            });
            FaceMarkValid(id);

            return id;
        }

        protected override void InitFace(int faceId)
        {
            var face = Face(faceId);
            faceLinks[face[0]].Add(faceId);
            faceLinks[face[1]].Add(faceId);
            faceLinks[face[2]].Add(faceId);
        }

        private struct VertexData
        {
            internal byte Mark;
            internal MxFlags Tag;
            internal byte UserMark;
            internal MxFlags UserTag;
        }

        private struct FaceData
        {
            internal byte Mark;
            internal MxFlags Tag;
            internal byte UserMark;
            internal MxFlags UserTag;
        }
    }
}
