using System;
using System.Collections.Generic;
using System.IO;
using TerrainDisplay.Util;
using WCell.Collision;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision
{
    public class TriIndex : IQuadObject
    {
        private Rect _bounds;
        private int[] _indices;
        public event EventHandler BoundsChanged;


        public Rect Bounds { get { return _bounds; } }
        public int[] Indices { get { return _indices; } }
        public int Index0 { get { return Indices[0]; } }
        public int Index1 { get { return Indices[1]; } }
        public int Index2 { get { return Indices[2]; } }
        

        public TriIndex(int[] indices, Vector3[] vertices)
        {
            _bounds = Rect.Empty;
            BoundsChanged = null;

            if (indices.IsNullOrEmpty())
                throw new InvalidDataException("Cannot create a TriIndex from a null or empty integer array.");
            if (indices.Length != 3) throw new InvalidDataException("To create a TriIndex you need exactly 3 integers.");
            if (vertices.IsNullOrEmpty())
                throw new InvalidDataException("Cannot create a TriIndex from a null or empty vertex array.");
            if (vertices.Length != 3) throw new InvalidDataException("To create a TriIndex you need exactly 3 vertices.");

            _indices = indices;
            CalculateBounds(vertices);
        }

        private void CalculateBounds(IEnumerable<Vector3> vertices)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            foreach (var vertex in vertices)
            {
                min = Vector3.Min(min, vertex);
                max = Vector3.Max(max, vertex);
            }

            _bounds = new Rect(min.X, min.Y, (max.X - min.X), (max.Y - min.Y));
        }
    }
}
