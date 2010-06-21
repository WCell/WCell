using Microsoft.Xna.Framework;

namespace TerrainDisplay
{
    public static class PositionUtil
    {
        public static void TransformToXNACoordSystem(ref Vector3 vector)
        {
            vector.X = (vector.X - TerrainConstants.CenterPoint) * -1;
            vector.Z = (vector.Z - TerrainConstants.CenterPoint) * -1;
        }

        /// <summary>
        /// Changes the coodinate system of a vertex from WoW based to XNA based.
        /// </summary>
        /// <param name="vertex">A vertex with WoW coords</param>
        public static void TransformWoWCoordsToXNACoords(ref VertexPositionNormalColored vertex)
        {
            TransformWoWCoordsToXNACoords(ref vertex.Position);
        }

        public static void TransformWoWCoordsToXNACoords(ref Vector3 vertex)
        {
            var temp = vertex.X;
            vertex.X = vertex.Y * -1;
            vertex.Y = vertex.Z;
            vertex.Z = temp * -1;
        }

        public static void TransformWoWCoordsToRecastCoords(ref Vector3 vertex)
        {
            var temp = vertex.X;
            vertex.X = vertex.Y * -1;
            vertex.Y = vertex.Z;
            vertex.Z = temp;
        }

        internal static void TransformRecastCoordsToWoWCoords(ref Vector3 vertex)
        {
            var temp = vertex.Z;
            vertex.Z = vertex.Y;
            vertex.Y = temp*-1;
            //vertex.X = temp;
        }
    }
}