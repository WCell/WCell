using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxStdSlim
    {
        protected MxStdModel model;
        protected MxHeap heap;

        internal int ValidVerts;
        internal int ValidFaces;
        internal bool IsInitialized;

        internal MxPlacement PlacementPolicy;
        internal MxWeighting WeightingPolicy;
        internal bool WillJoinOnly;

        internal double BoundaryWeight;
        internal double CompactnessRatio;
        internal double MeshingPenalty;
        internal double LocalValidityThreshold;
        internal int VertexDegreeLimit;


        internal MxStdSlim(MxStdModel model)
        {
            heap = new MxHeap(64);
            this.model = model;
            PlacementPolicy = MxPlacement.Optimal;
            WeightingPolicy = MxWeighting.Area;
            BoundaryWeight = 1000.0;
            CompactnessRatio = 0.0;
            MeshingPenalty = 1.0;
            LocalValidityThreshold = 0.0;
            VertexDegreeLimit = 24;
            WillJoinOnly = false;

            ValidFaces = 0;
            ValidVerts = 0;
            IsInitialized = false;

            for (var faceId = 0; faceId < model.FaceCount; faceId++)
            {
                if (!model.FaceIsValid(faceId)) continue;
                ValidFaces++;
            }

            for (var vertId = 0; vertId < model.VertCount; vertId++)
            {
                if (!model.VertexIsValid(vertId)) continue;
                ValidVerts++;
            }
        }


        internal virtual void Initialize()
        {
        }

        internal virtual bool Decimate(int dec)
        {
            return false;
        }

        internal MxStdModel Model
        {
            get { return model; }
        }
    }
}
