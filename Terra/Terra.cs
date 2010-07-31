using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terra.Geometry;
using Terra.Greedy;
using Terra.Maps;
using WCell.Util.Graphics;

namespace Terra
{
    public class Terra
    {
        private readonly Map heightMap;
        private readonly FloatMask mask;
        private readonly GreedySubdivision mesh;
        
        private float ErrorThreshold;
        private int PointCountLimit = -1;
        private float HeightScale = 1.0f;


        public Terra(float[,] heightvalues)
            : this(0.0f, heightvalues)
        {
        }

        public Terra(float errorThreshold, float[,] heightValues)
            : this(errorThreshold, -1, heightValues)
        {
        }

        public Terra(float errorThreshold, int pointCountLimit, float[,] heightValues) 
              : this(errorThreshold, pointCountLimit, 1.0f, heightValues, null)
        {    
        }

        public Terra(float errorThreshold, int pointCountLimit, float heightScale, float[,] heightValues, float[,] importanceMask)
        {
            ErrorThreshold = errorThreshold;
            HeightScale = heightScale;
            heightMap = Map.LoadFromArray(heightValues);
            PointCountLimit = (pointCountLimit > -1)? pointCountLimit : (heightMap.Width*heightMap.Height);
            mask = importanceMask.IsNullOrEmpty()
                       ? new DefaultMask(heightMap.Width, heightMap.Height)
                       : FloatMask.LoadFromArray(importanceMask);
            mesh = new GreedySubdivision(heightMap, mask);
        }

        /// <summary>
        /// Inserts a set of fixed points into the triangulation as given in the provided 'script'
        /// Call this before calling Triangulate()
        /// </summary>
        /// <param name="indices">An insertion 'script' given as a square array of size [numPoints, 2] 
        /// with the entries given as {w, h}.</param>
        public void ScriptedPreInsertion(List<Index2> indices)
        {
            for (var i = 0; i < mesh.Data.Width; i++)
            {
                if (mesh.PointStates[i, mesh.Data.Width - 1] != PointState.Unused) continue;
                mesh.Select(i, mesh.Data.Width - 1);
                mesh.PointStates[i, mesh.Data.Width - 1] = PointState.Used;
            }

            if (indices == null) return;

            foreach (var index in indices)
            {
                if (mesh.PointStates[index.X, index.Y] != PointState.Unused) continue;

                mesh.Select(index.X, index.Y);
                mesh.PointStates[index.X, index.Y] = PointState.Used;
            }
        }

        /// <summary>
        /// Start the re-triangulation process
        /// </summary>
        public void Triangulate()
        {
            while (!GoalMet())
            {
                if (mesh.GreedyInsert()) continue;
                break;
            }
        }

        public void GenerateOutput(List<Index2> outputCull, out List<Vector3> vertices, out List<int> indices)
        {
            var newVertices = new List<Vector3>((int)mesh.PointCount);
            var newIndices = new List<int>((int)mesh.PointCount*3);
            var vertexArrayPositions = new int[heightMap.Width, heightMap.Height];
            vertexArrayPositions.Fill(-1);

            mesh.OverFaces((tri, obj) =>
            {
                var triVertices = new[]
                {
                    tri.Point1,
                    tri.Point2,
                    tri.Point3
                };

                foreach (var holeMarker in outputCull)
                {
                    var count = 0;
                    foreach (var vertex in triVertices)
                    {
                        var idx = new Index2
                        {
                            X = (int)vertex.X,
                            Y = (int)vertex.Y
                        };

                        // The triangle abuts one of the hole markers
                        if (holeMarker == idx) return;
                        
                        // The triangle is within the hole markers neighborhood
                        if (Index2.Abs(idx - holeMarker) <= Index2.One) count++;
                    }
                    if (count > 2) return;
                }

                if (GeometryHelpers.IsCounterClockwise(triVertices[0], triVertices[1], triVertices[2]))
                {
                    var temp = triVertices[1];
                    triVertices[1] = triVertices[2];
                    triVertices[2] = temp;
                }

                foreach (var vertex in triVertices)
                {
                    if (vertexArrayPositions[(int) vertex.X, (int) vertex.Y] == -1)
                    {
                        vertexArrayPositions[(int) vertex.X, (int) vertex.Y] = newVertices.Count;
                        newVertices.Add(new Vector3(vertex, heightMap.Eval(vertex.X, vertex.Y)));
                    }

                    newIndices.Add(vertexArrayPositions[(int) vertex.X, (int) vertex.Y]);
                }

            });

            vertices = newVertices;
            indices = newIndices;
        }

        private bool GoalMet()
        {
            return ((mesh.MaxError() < ErrorThreshold) || (mesh.PointCount > PointCountLimit));
        }

        
    }
}
