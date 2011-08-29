using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QSlim.MxKit
{
    internal class MxQSlim : MxStdSlim
    {
        protected MxBlock<MxQuadric3> quadrics;
        internal MxMatrix4 ObjectTransform;
        
        internal MxQSlim(MxStdModel model) 
            : base(model)
        {
            quadrics = new MxBlock<MxQuadric3>(model.VertCount);
            ObjectTransform = null;
        }

        internal MxQuadric3 VertexQuadric(int vertId)
        {
            return quadrics[vertId];
        }

        internal override void Initialize()
        {
            CollectQuadrics();
            if (BoundaryWeight > 0.0) ConstrainBoundaries();
            if (ObjectTransform != null) TransformQuadrics(ObjectTransform);
            IsInitialized = true;
        }

        protected void DiscontinuityConstraint(int vertI, int vertJ, List<int> faceList)
        {
            foreach (var faceId in faceList)
            {
                var orig = new MxVector3(model.Vertex(vertI));
                var dest = new MxVector3(model.Vertex(vertJ));
                var edge = dest - orig;

                var nml = new[] {0.0, 0.0, 0.0};
                model.ComputeFaceNormal(faceId, ref nml);
                var n = new MxVector3(nml);

                var n2 = edge ^ n;
                MxVector3.Unitize(ref n2);

                var quad = new MxQuadric3(n2, -(n2*orig));
                quad *= BoundaryWeight;

                if (WeightingPolicy == MxWeighting.Area || WeightingPolicy == MxWeighting.AreaAverage)
                {
                    quad.Area = MxVector3.Norm(edge);
                    quad *= quad.Area;
                }

                quadrics[vertI] += quad;
                quadrics[vertJ] += quad;
            }
        }

        protected void CollectQuadrics()
        {
            foreach (var quad in quadrics)
            {
                quad.Clear();
            }

            for (var i = 0; i < model.FaceCount; i++)
            {
                var face = model.Face(i);
                var vert0 = new MxVector3(model.Vertex(face[0]));
                var vert1 = new MxVector3(model.Vertex(face[1]));
                var vert2 = new MxVector3(model.Vertex(face[2]));

                var plane = (WeightingPolicy == MxWeighting.RawNormals)
                                ? MxGeom3d.TriangleRawPlane(vert0, vert1, vert2)
                                : MxGeom3d.TrianglePlane(vert0, vert1, vert2);

                var quad = new MxQuadric3(plane[0], plane[1], plane[2], plane[3], model.ComputeFaceArea(i));

                switch(WeightingPolicy)
                {
                    case MxWeighting.Angle:
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                var quadJ = new MxQuadric3(quad);
                                quadJ *= model.ComputeCornerAngle(i, j);
                                quadrics[face[j]] += quadJ;
                            }
                            break;
                        }
                    case MxWeighting.Area:
                        {
                            quad *= quad.Area;
                            quadrics[face[0]] += quad;
                            quadrics[face[1]] += quad;
                            quadrics[face[2]] += quad;
                            break;
                        }
                    case MxWeighting.AreaAverage:
                        {
                            quad *= quad.Area;
                            quadrics[face[0]] += quad;
                            quadrics[face[1]] += quad;
                            quadrics[face[2]] += quad;
                            break;
                        }
                    default:
                        {
                            quadrics[face[0]] += quad;
                            quadrics[face[1]] += quad;
                            quadrics[face[2]] += quad;
                            break;
                        }
                }
            }
        }

        protected void TransformQuadrics(MxMatrix4 transform)
        {
            foreach(var quad in quadrics)
            {
                quad.Transform(transform);
            }
        }

        protected void ConstrainBoundaries()
        {
            var star = new List<int>();
            var faceIds = new List<int>();

            for (var i = 0; i < model.VertCount; i++)
            {
                star.Clear();
                model.CollectVertexStar(i, star);

                foreach (var id in star)
                {
                    if (i >= id) continue;

                    faceIds.Clear();
                    Model.CollectEdgeNeighbors(i, id, faceIds);
                    if (faceIds.Count != 1) continue;

                    DiscontinuityConstraint(i, id, faceIds);
                }
            }
        }
    }
}
