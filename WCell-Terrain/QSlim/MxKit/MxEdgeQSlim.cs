using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxEdgeQSlim : MxQSlim
    {
        private MxBlock<List<MxQSlimEdge>> edgeLinks;
        private List<int> starVertIds, starvertId2;
        private MxPairContraction conxTemp;
        internal event ContractionEventHandler Contracted;
        internal delegate void ContractionEventHandler(MxPairContraction conx, float arg);

        internal MxEdgeQSlim(MxStdModel model) 
            : base(model)
        {
            edgeLinks = new MxBlock<List<MxQSlimEdge>>(model.VertCount);
            Contracted += ContractionCallback;
        }

        internal override void Initialize()
        {
            base.Initialize();
            CollectEdges();
        }

        internal void Initialize(MxEdge[] edges, int count)
        {
            base.Initialize();
            for(var i = 0; i < count; i++)
            {
                CreateEdge(edges[i].V1, edges[i].V2);
            }
        }

        internal int EdgeCount { get { return heap.Size; } }
        
        internal MxQSlimEdge Edge(int i)
        {
            return heap[i] as MxQSlimEdge;
        }

        internal void Edge(int i, MxQSlimEdge edge)
        {
            heap[i] = edge;
        }

        internal double CheckLocalCompactness(int vertId, double[] vNew)
        {
            var n1 = model.Neighbors(vertId);
            var cMin = 1.0;

            foreach (var faceId in n1)
            {
                if (!model.FaceIsValid(faceId)) continue;

                var face = model.Face(faceId);
                MxVector3[] fAfter = new MxVector3[3];
                for (var i = 0; i < 3; i++)
                {
                    fAfter[i] = (face[i] == vertId)
                                    ? new MxVector3(vNew)
                                    : new MxVector3(model.Vertex(face[i]));
                }

                var c = MxGeom3d.TriangleCompactness(fAfter[0], fAfter[1], fAfter[2]);
                if (c < cMin) cMin = c;
            }

            return cMin;
        }

        internal double CheckLocalInversion(int vertId, double[] vNew)
        {
            var nMin = 1.0;
            var n1 = model.Neighbors(vertId);

            foreach (var faceId in n1)
            {
                if (!model.FaceIsValid(faceId)) continue;

                var face = model.Face(faceId);
                var nml = new double[3];
                model.ComputeFaceNormal(faceId, ref nml);
                var nBefore = new MxVector3(nml);

                var fAfter = new MxVector3[3];
                for (var i = 0; i < 3; i++)
                {
                    fAfter[i] = (face[i] == vertId)
                                    ? new MxVector3(vNew)
                                    : new MxVector3(model.Vertex(face[i]));
                }

                var newNml = MxGeom3d.TriangleNormal(fAfter[0], fAfter[1], fAfter[2]);
                var delta = nBefore*newNml;
                if (delta < nMin) nMin = delta;
            }

            return nMin;
        }

        internal int CheckLocalValidity(int vertId, double[] vNew)
        {
            var n1 = model.Neighbors(vertId);
            var nFailed = 0;

            foreach (var faceId in n1)
            {
                if (!model.FaceIsValid(faceId)) continue;

                var face = model.Face(faceId);
                var k = face.FindVertex(vertId);
                var x = face[(k + 1)%3];
                var y = face[(k + 2)%3];
                var vPos = model.Vertex(vertId).Pos;
                var xPos = model.Vertex(x).Pos;
                var yPos = model.Vertex(y).Pos;

                var dYX = new double[3];
                MxVectorOps.Sub3(ref dYX, xPos, yPos);

                var dVX = new double[3];
                MxVectorOps.Sub3(ref dVX, vPos, xPos);

                var dVNew = new double[3];
                MxVectorOps.Sub3(ref dVNew, vNew, xPos);

                var fN = new double[3];
                MxVectorOps.Cross3(ref fN, dYX, dVX);

                var nml = new double[3];
                MxVectorOps.Cross3(ref nml, dYX, dVX);
                MxVectorOps.Unitize3(ref nml);

                if(MxVectorOps.Dot3(dVNew, nml) < LocalValidityThreshold*MxVectorOps.Dot3(dVX, nml))
                {
                    nFailed++;
                }
            }

            return nFailed;
        }

        internal int CheckLocalDegree(int vertId1, int vertId2)
        {
            var n1 = model.Neighbors(vertId1);
            var n2 = model.Neighbors(vertId2);

            var degree = 0;
            foreach (var faceId in n1)
            {
                if (!model.FaceIsValid(faceId)) continue;
                degree++;
            }

            foreach (var faceId in n2)
            {
                if (!model.FaceIsValid(faceId)) continue;
                degree++;
            }

            if (degree > VertexDegreeLimit) return (degree - VertexDegreeLimit);
            return 0;
        }

        internal void ApplyMeshPenalties(MxQSlimEdge info)
        {
            var n1 = model.Neighbors(info.V1);
            var n2 = model.Neighbors(info.V2);

            foreach (var faceId in n2) model.FaceMark(faceId, 0x00);
            foreach (var faceId in n1) model.FaceMark(faceId, 0x01);
            foreach (var faceId in n2) model.FaceMark(faceId, (byte)(model.FaceMark(faceId) + 0x1));

            var baseError = info.HeapKey;
            var bias = 0.0;

            var maxDeg = Math.Max(n1.Count, n2.Count);
            if (maxDeg > VertexDegreeLimit) bias += (maxDeg - VertexDegreeLimit)*MeshingPenalty*0.001;

            var nFailed = CheckLocalValidity(info.V1, info.vNew);
            nFailed += CheckLocalValidity(info.V2, info.vNew);
            if (nFailed > 0) bias += nFailed*MeshingPenalty;

            if (CompactnessRatio > 0.0)
            {
                var c1Min = CheckLocalCompactness(info.V1, info.vNew);
                var c2Min = CheckLocalCompactness(info.V2, info.vNew);
                var cMin = Math.Min(c1Min, c2Min);

                if (cMin < CompactnessRatio) bias += (1 - cMin);
            }

            info.HeapKey = (float)(baseError - bias);
        }

        internal void ComputeTargetPlacement(MxQSlimEdge info)
        {
            var i = info.V1;
            var j = info.V2;

            var Qi = quadrics[i];
            var Qj = quadrics[j];
            var Q = Qi + Qj;

            var eMin = 0.0;
            if (PlacementPolicy == MxPlacement.Optimal 
                && Q.Optimize(ref info.vNew[0], ref info.vNew[1], ref info.vNew[2]))
            {
                eMin = Q.Evaluate(info.vNew);
            }
            else
            {
                var vi = new MxVector3(model.Vertex(i));
                var vj = new MxVector3(model.Vertex(j));
                var best = new MxVector3(0.0, 0.0, 0.0);

                if (PlacementPolicy == MxPlacement.Line && Q.Optimize(vi, vj, ref best))
                {
                    eMin = Q.Evaluate(best);
                }
                else
                {
                    var ei = Q.Evaluate(vi);
                    var ej = Q.Evaluate(vj);

                    if (ei < ej)
                    {
                        eMin = ei;
                        best = vi;
                    }
                    else
                    {
                        eMin = ej;
                        best = vj;
                    }

                    if (PlacementPolicy == MxPlacement.EndOrMid)
                    {
                        var mid = (vi + vj)*0.5;
                        var eMid = Q.Evaluate(mid);
                        if (eMid < eMin)
                        {
                            eMin = eMid;
                            best = mid;
                        }
                    }
                }

                info.vNew[0] = best[0];
                info.vNew[1] = best[1];
                info.vNew[2] = best[2];
            }

            if (WeightingPolicy == MxWeighting.AreaAverage)
            {
                eMin /= Q.Area;
            }

            info.HeapKey = (float)(-eMin);
        }

        internal void FinalizeEdgeUpdate(MxQSlimEdge info)
        {
            if (MeshingPenalty > 1.0) ApplyMeshPenalties(info);
            if (info.IsInHeap) heap.Update(info);
            else heap.Insert(info);
        }

        internal void ComputeEdgeInfo(MxQSlimEdge info)
        {
            ComputeTargetPlacement(info);
            FinalizeEdgeUpdate(info);
        }

        internal void CreateEdge(int vertI, int vertJ)
        {
            var info = new MxQSlimEdge
            {
                V1 = vertI,
                V2 = vertJ
            };

            ComputeEdgeInfo(info);

            edgeLinks[vertI].Add(info);
            edgeLinks[vertJ].Add(info);
        }

        internal void CollectEdges()
        {
            var starList = new List<int>();

            for (var i = 0; i < model.VertCount; i++)
            {
                starList.Clear();
                model.CollectVertexStar(i, starList);

                foreach (var t in starList)
                {
                    if (i >= t) continue;
                    CreateEdge(i, t);
                }
            }
        }

        internal void UpdatePreContract(MxPairContraction conx)
        {
            var v1 = conx.VertId1;
            var v2 = conx.VertId2;

            var star = new List<int>();

            foreach(var edge in edgeLinks[v1])
            {
                star.Add(edge.OppositeVertex(v1));
            }

            foreach (var edge in edgeLinks[v2])
            {
                var vertU = (edge.V1 == v2) ? edge.V2 : edge.V1;
                Debug.Assert(edge.V1 == v2 || edge.V2 == v2);
                Debug.Assert(vertU != v2);

                if (vertU == v1 || star.Contains(vertU))
                {
                    edgeLinks[vertU].Remove(edge);
                    heap.RemoveItem(edge);
                }
                else
                {
                    edge.V1 = v1;
                    edge.V2 = vertU;
                    edgeLinks[v1].Add(edge);
                }
            }

            edgeLinks[v2].Clear();
        }

        internal void UpdatePostContract(MxPairContraction conx)
        {
        }

        internal void UpdatePreExpand(MxPairContraction conx)
        {
        }

        internal void UpdatePostExpand(MxPairContraction conx)
        {
            var v1 = conx.VertId1;
            var v2 = conx.VertId2;

            var star = new List<int>();
            var star2 = new List<int>();
            edgeLinks[conx.VertId2].Clear();
            model.CollectVertexStar(conx.VertId1, star);
            model.CollectVertexStar(conx.VertId2, star2);

            var i = 0;
            while (i < edgeLinks[v1].Count)
            {
                var edge = edgeLinks[v1][i];
                var vertU = (edge.V1 == v1) ? edge.V2 : edge.V1;
                Debug.Assert(edge.V1 == v1 || edge.V2 == v1);
                Debug.Assert(vertU != v1 && vertU != v2);

                var v1Linked = star.Contains(vertU);
                var v2Linked = star2.Contains(vertU);

                if (v1Linked)
                {
                    if (v2Linked) CreateEdge(v2, vertU);
                    i++;
                }
                else
                {
                    edge.V1 = v2;
                    edge.V2 = vertU;
                    edgeLinks[v2].Add(edge);
                    edgeLinks[v1].RemoveAt(i);
                }

                ComputeEdgeInfo(edge);
            }

            if (star.Contains(v2)) CreateEdge(v1, v2);
        }

        internal void ApplyContraction(MxPairContraction conx)
        {
            ValidVerts--;
            ValidFaces -= conx.DeadFaces.Count;
            quadrics[conx.VertId1] += quadrics[conx.VertId2];

            UpdatePreContract(conx);
            model.ApplyContraction(conx);
            UpdatePostContract(conx);

            foreach (var edge in edgeLinks[conx.VertId1])
            {
                ComputeEdgeInfo(edge);
            }
        }

        internal void ApplyExpansion(MxPairContraction conx)
        {
            UpdatePreExpand(conx);
            
            model.ApplyExpansion(conx);
            ValidVerts++;
            ValidFaces += conx.DeadFaces.Count;
            quadrics[conx.VertId1] -= quadrics[conx.VertId2];

            UpdatePostExpand(conx);
        }

        internal override bool Decimate(int target)
        {
            while(ValidFaces > target)
            {
                var info = heap.Extract() as MxQSlimEdge;
                if (info == null) return false;

                var v1 = info.V1;
                var v2 = info.V2;
                

                if (!model.VertexIsValid(v1) || !model.VertexIsValid(v2)) continue;

                var inversion = CheckLocalInversion(v1, info.vNew);
                if (inversion < 0.00) continue;

                inversion = CheckLocalInversion(v2, info.vNew);
                if (inversion < 0.00) continue;

                var conx = new MxPairContraction();
                model.ComputeContraction(v1, v2, conx, info.vNew);

                if (WillJoinOnly && conx.DeadFaces.Count > 0) continue;
                

                var evt = Contracted;
                if (evt != null)
                {
                    evt(conx, -info.HeapKey);
                }

                ApplyContraction(conx);
            }

            return true;
        }

        internal void ContractionCallback(MxPairContraction conx, float f)
        {
        }
    }

    internal class MxQSlimEdge : MxEdge, MxIHeapable
    {
        internal double[] vNew;
        public bool IsInHeap { get { return HeapPosition != MxHeap.NOT_IN_HEAP; } }
        public int HeapPosition { get; set; }
        public float HeapKey { get; set; }
        public void SetAsNotInHeap()
        {
            HeapPosition = MxHeap.NOT_IN_HEAP;
        }

        internal MxQSlimEdge()
        {
            vNew = new double[3];
            SetAsNotInHeap();
            HeapKey = 0.0f;
        }
    }
}
