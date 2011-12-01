using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QSlim.MxKit;
using WCell.Util.Graphics;

namespace QSlim
{
    public class QSlim
    {
        private List<Vector3> inputVertices;
        private List<int> inputIndices;
        private MxStdModel baseModel;
        private MxQSlim baseSlim;
        private MxEdgeQSlim edgeSlim;
        private MxFaceQSlim faceSlim;
        private bool willUseFaces;
        private bool willJoinOnly;
        private MxPlacement placementPolicy;
        private MxWeighting weightingPolicy;
        private double boundaryWeight;
        private double compactnessRatio;
        private double meshingPenalty;

        public QSlim(List<Vector3> vertices, List<int> indices)
            : this(vertices, indices, false, false, MxPlacement.Optimal, MxWeighting.Area, 10000, 0.0, 1.0)
        {
        }

        public QSlim(List<Vector3> vertices, List<int> indices, bool contractByFaces, bool joinOnly,
                     MxPlacement placePolicy, MxWeighting weightPolicy, double boundaryWeight, double compactRatio,
                     double meshPenalty)
        {
            inputVertices = vertices;
            inputIndices = indices;
            willUseFaces = contractByFaces;
            willJoinOnly = joinOnly;
            placementPolicy = placePolicy;
            weightingPolicy = weightPolicy;
            this.boundaryWeight = boundaryWeight;
            compactnessRatio = compactRatio;
            meshingPenalty = meshPenalty;
            Init();
        }

        public void Simplify(int target)
        {
            if (willUseFaces)
            {
                faceSlim.Decimate(target);
            }
            else
            {
                edgeSlim.Decimate(target);
            }
        }

        public void GenerateOutput(out List<Vector3> vertices, out List<int> indices)
        {
            baseModel.CleanUpForOutput();
            indices = new List<int>(baseModel.FaceCount*3);
            vertices = new List<Vector3>(baseModel.VertCount);

            for (var faceId = 0; faceId < baseModel.FaceCount; faceId++)
            {
                if (!baseModel.FaceIsValid(faceId)) continue;

                var face = baseModel.Face(faceId);
                var vertId0 = face[0];
                var vert0 = baseModel.Vertex(vertId0);
                var vertId1 = face[1];
                var vert1 = baseModel.Vertex(vertId1);
                var vertId2 = face[2];
                var vert2 = baseModel.Vertex(vertId2);

                if (!IsWoundClockwise(vert0, vert1, vert2))
                {
                    var t = vertId1;
                    vertId1 = vertId2;
                    vertId2 = t;
                }

                indices.Add(vertId0);
                indices.Add(vertId1);
                indices.Add(vertId2);
            }

            for (var vertId = 0; vertId < baseModel.VertCount; vertId++)
            {
                var vert = baseModel.Vertex(vertId);
                var wVert = new Vector3((float)vert[0], (float)vert[1], (float)vert[2]);
                vertices.Add(wVert);
            }
        }

        private void Init()
        {
            InitBaseModel();

            if (willUseFaces)
            {
                baseSlim = faceSlim = new MxFaceQSlim(baseModel)
                {
                    PlacementPolicy = placementPolicy,
                    WeightingPolicy = weightingPolicy,
                    BoundaryWeight = boundaryWeight,
                    CompactnessRatio = compactnessRatio,
                    MeshingPenalty = meshingPenalty,
                    WillJoinOnly = willJoinOnly
                };
                faceSlim.Initialize();
            }
            else
            {
                baseSlim = edgeSlim = new MxEdgeQSlim(baseModel)
                {
                    PlacementPolicy = placementPolicy,
                    WeightingPolicy = weightingPolicy,
                    BoundaryWeight = boundaryWeight,
                    CompactnessRatio = compactnessRatio,
                    MeshingPenalty = meshingPenalty,
                    WillJoinOnly = willJoinOnly
                };
                edgeSlim.Initialize();
            }
        }

        private void InitBaseModel()
        {
            baseModel = new MxStdModel(inputVertices.Count, inputIndices.Count/3)
            {
                ColorBinding = MxBinding.UnBound,
                NormalBinding = MxBinding.UnBound
            };

            foreach(var vec in inputVertices)
            {
                baseModel.AddVertex(vec.X, vec.Y, vec.Z);
            }

            for (var i = 0; i < inputIndices.Count; )
            {
                baseModel.AddFace(inputIndices[i++], inputIndices[i++], inputIndices[i++]);
            }
        }

        private static bool IsWoundClockwise(MxVertex a, MxVertex b, MxVertex c)
        {
            var area = ((b[0] - a[0]) * (c[1] - a[1])) - ((b[1] - a[1]) * (c[0] - a[0]));
            return (area < 0.0);
        }
    }
}
