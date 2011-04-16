using System.Collections.Generic;
using WCell.Tools.Maps.Parsing.M2s.Components;
using WCell.Tools.Maps.Parsing.WMO.Components;
using WCell.Tools.Maps.Structures;
using WCell.Util.Graphics;

namespace WCell.Tools.Maps.Parsing.M2s
{
    public class M2Model
    {
        public ModelHeader Header;

        public string Name;

        /// <summary>
        /// A list of timestamps that act as 
        /// upper limits for global sequence ranges.
        /// </summary>
        public uint[] GlobalSequenceTimestamps;

        /// <summary>
        /// Models, too, use a Z-up coordinate systems, 
        /// so in order to convert to Y-up, the X, Y, Z 
        /// values become (X, -Z, Y). 
        /// </summary>
        public ModelVertices[] Vertices;

        public Vector3[] BoundingVertices;
        public Index3[] BoundingTriangles;
        public Vector3[] BoundingNormals;

        public string FilePath;

        internal BoundingBox CreateWorldBounds(DoodadDefinition dDef, out Matrix modelToWorld)
        {
            modelToWorld = Matrix.Identity;
            if (BoundingVertices == null) return BoundingBox.INVALID;
            if (BoundingVertices.Length == 0) return BoundingBox.INVALID;

            Matrix scaleMatrix;
            Matrix.CreateScale(dDef.Scale, out scaleMatrix);

            Matrix rotMatrix;
            Matrix.CreateFromQuaternion(ref dDef.Rotation, out rotMatrix);

            Matrix.Multiply(ref scaleMatrix, ref rotMatrix, out modelToWorld);

            var tempVertices = new List<Vector3>(BoundingVertices.Length);
            foreach (var vertex in BoundingVertices)
            {
                var tempVertex = Vector3.Transform(vertex, modelToWorld);
                tempVertices.Add(tempVertex + dDef.Position);
            }

            return new BoundingBox(tempVertices.ToArray());
        }
    }
}