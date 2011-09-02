using System.Collections.Generic;

namespace QSlim.MxKit
{
    internal class MxFaceQSlim : MxQSlim
    {
        private MxBlock<TriInfo> faceInfo; 

        internal MxFaceQSlim(MxStdModel model) : base(model)
        {
            faceInfo = new MxBlock<TriInfo>(model.FaceCount);
        }

        internal void ComputeFaceInfo(int faceId)
        {
            var info = faceInfo[faceId];
            info.FaceId = faceId;

            var i = model.Face(faceId)[0];
            var j = model.Face(faceId)[1];
            var k = model.Face(faceId)[2];

            var Qi = quadrics[i];
            var Qj = quadrics[j];
            var Qk = quadrics[k];

            var Q = Qi + Qj + Qk;

            if (PlacementPolicy == MxPlacement.Optimal &&
                Q.Optimize(ref info.vNew[0], ref info.vNew[1], ref info.vNew[2]))
            {
                info.HeapKey = (float)-Q.Evaluate(info.vNew);
            }
            else
            {
                var v1 = new MxVector3(model.Vertex(i));
                var v2 = new MxVector3(model.Vertex(j));
                var v3 = new MxVector3(model.Vertex(k));
                var e1 = Q.Evaluate(v1);
                var e2 = Q.Evaluate(v2);
                var e3 = Q.Evaluate(v3);

                MxVector3 best;
                double eMin;
                if (e1 <= e2 && e1 <= e3)
                {
                    eMin = e1;
                    best = v1;
                } 
                else if (e2 <= e1 && e2 <= e3)
                {
                    eMin = e2;
                    best = v2;
                }
                else
                {
                    eMin = e3;
                    best = v3;
                }

                info.vNew[0] = best[0];
                info.vNew[1] = best[1];
                info.vNew[2] = best[2];
                info.HeapKey = (float)(-eMin);
            }

            if (WeightingPolicy == MxWeighting.AreaAverage)
            {
                info.HeapKey = (float)(info.HeapKey/Q.Area);
            }

            if (info.IsInHeap) heap.Update(info);
            else heap.Insert(info);
        }

        internal override void Initialize()
        {
            base.Initialize();

            for (var faceId = 0; faceId < model.FaceCount; faceId++)
            {
                ComputeFaceInfo(faceId);
            }
        }

        internal override bool Decimate(int target)
        {
            var i = 0;
            var changed = new List<int>();
            while(ValidFaces > target)
            {
                var info = heap.Extract() as TriInfo;
                if (info == null) return false;

                var faceId = info.FaceId;
                var v1 = model.Face(faceId)[0];
                var v2 = model.Face(faceId)[1];
                var v3 = model.Face(faceId)[2];

                if (!model.FaceIsValid(faceId)) continue;
                
                model.Contract(v1, v2, v3, info.vNew, changed);

                quadrics[v1] += quadrics[v2];
                quadrics[v1] += quadrics[v3];

                ValidVerts -= 2;
                foreach (var changedFace in changed)
                {
                    if (!model.FaceIsValid(changedFace)) 
                        ValidFaces--;
                }

                foreach (var face in changed)
                {
                    if (model.FaceIsValid(face)) ComputeFaceInfo(face);
                    else heap.RemoveItem(faceInfo[face]);
                }
            }

            return true;
        }

        private class TriInfo : MxIHeapable
        {
            internal int FaceId;
            internal double[] vNew = new double[3];
            public bool IsInHeap { get { return HeapPosition != MxHeap.NOT_IN_HEAP; } }
            public int HeapPosition { get; set; }
            public float HeapKey { get; set; }
            public void SetAsNotInHeap()
            {
                HeapPosition = MxHeap.NOT_IN_HEAP;
            }

            public TriInfo()
            {
                SetAsNotInHeap();
                HeapKey = 0.0f;
            }
        }
    }
}