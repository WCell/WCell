using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TerrainDisplay
{
    /// <summary>
    /// Struct outlining our custom vertex
    /// </summary>
    public struct VertexPositionNormalColored
    {
        /// <summary>
        /// Vector3 Position for this vertex
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Color of this vertex
        /// </summary>
        public Color Color;
        /// <summary>
        /// Normal vector for this Vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Constructor for a VertexPositionNormalColored
        /// </summary>
        /// <param name="position">Vector3 Position of the vertex</param>
        /// <param name="color">Color of the vertex</param>
        /// <param name="normal">Normal vector of the vertex</param>
        public VertexPositionNormalColored(Vector3 position, Color color, Vector3 normal)
        {
            Position = position;
            Color = color;
            Normal = normal;
        }
        /// <summary>
        /// Memory size for a VertexPositionNormalColored
        /// </summary>
        public static int SizeInBytes = 7 * 4;
        /// <summary>
        /// VertexElement array (used for rendering)
        /// </summary>
        public static readonly VertexElement[] VertexElements = new[] {
                                                                          new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                                                                          new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Color, VertexElementMethod.Default, VertexElementUsage.Color, 0 ),
                                                                          new VertexElement( 0, sizeof(float) * 4, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
                                                                      };
    }
}