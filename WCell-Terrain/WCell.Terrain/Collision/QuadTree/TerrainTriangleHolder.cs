using System;
using System.Collections.Generic;
using System.IO;
using TerrainDisplay.Util;
using WCell.Collision;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision
{
    public class TerrainTriangleHolder : IQuadObject, ICollidable
    {
        public event EventHandler BoundsChanged;


        public Rect Bounds
        {
            get; 
            private set;
        }

        public int NodeId
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int[] Indices
        {
            get; 
            private set;
        }

        public int Index0 
        { 
            get
            {
                return Indices[0];
            } 
        }
        
        public int Index1
        {
            get
            {
                return Indices[1];
            }
        }
        
        public int Index2
        {
            get
            {
                return Indices[2];
            }
        }
        

        public TerrainTriangleHolder(int[] indices, Vector3[] vertices)
        {
            Bounds = Rect.Empty;
            BoundsChanged = null;

            if (indices.IsNullOrEmpty())
                throw new InvalidDataException("Cannot create a TerrainTriangleHolder from a null or empty integer array.");
            if (indices.Length != 3) throw new InvalidDataException("To create a TerrainTriangleHolder you need exactly 3 integers.");
            if (vertices.IsNullOrEmpty())
                throw new InvalidDataException("Cannot create a TerrainTriangleHolder from a null or empty vertex array.");
            if (vertices.Length != 3) throw new InvalidDataException("To create a TerrainTriangleHolder you need exactly 3 vertices.");

            Indices = indices;
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

            Bounds = new Rect(min.X, min.Y, (max.X - min.X), (max.Y - min.Y));
        }

        public bool IntersectWith(Ray ray, out float distance)
        {
            throw new NotImplementedException();
        }
    }
}
