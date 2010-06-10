using System.Collections.Generic;

namespace MPQNav.MPQ.ADT
{
    public abstract class ADTBase
    {
        public List<int> LiquidIndices;
        public List<VertexPositionNormalColored> LiquidVertices;
        public List<int> Indices;
        public List<VertexPositionNormalColored> Vertices;
        public abstract void GenerateLiquidVertexAndIndices();
        public abstract void GenerateHeightVertexAndIndices();

        /// <summary>
        /// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="vertices">The Collection to add the vertices to.</param>
        /// <returns>The number of vertices added.</returns>
        public abstract int GenerateLiquidVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices);

        /// <summary>
        /// Adds the rendering liquid indices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
        /// <param name="indices">The Collection to add the indices to.</param>
        public abstract void GenerateLiquidIndices(int indexY, int indexX, int offset, List<int> indices);

        /// <summary>
        /// Adds the rendering liquid vertices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="vertices">The Collection to add the vertices to.</param>
        /// <returns>The number of vertices added.</returns>
        public abstract int GenerateHeightVertices(int indexY, int indexX, ICollection<VertexPositionNormalColored> vertices);

        /// <summary>
        /// Adds the rendering indices to the provided list for the MapChunk given by:
        /// </summary>
        /// <param name="indexY">The y index of the map chunk.</param>
        /// <param name="indexX">The x index of the map chunk</param>
        /// <param name="offset">The number to add to the indices so as to match the end of the Vertices list.</param>
        /// <param name="indices">The Collection to add the indices to.</param>
        public abstract void GenerateHeightIndices(int indexY, int indexX, int offset, List<int> indices);
    }
}