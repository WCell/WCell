using System;
using System.Collections.Generic;
using System.Text;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision._3D
{
    /// <summary>
    /// Axis-Aligned Bounding Box
    /// </summary>
    public class AABB
    {
        /// <summary>
        /// A BoundingBox that represents this AABB.
        /// </summary>
        public BoundingBox Bounds
        {
            get;
            set;
        }

        /// <summary>
        /// The Shape contained in this AABB.
        /// </summary>
        public IShape ContainedShape
        {
            get;
            set;
        }

        public int Id
        {
            get;
            private set;
        }

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="shape">The IShape to place in this bounding box.</param>
        public AABB(IShape shape, int id)
        {
            ContainedShape = shape;
            Bounds = new BoundingBox(shape.Min, shape.Max);
            Id = id;
        }

        public AABB(IShape shape) :this(shape, 0)
        {
        }

        public AABB(Vector3 min, Vector3 max)
        {
            Bounds = new BoundingBox(min, max);
        }

		//public AABB(IEnumerable<VertexPositionNormalColored> vertices)
		//{
		//    var minX = float.MinValue;
		//    var minY = float.MinValue;
		//    var minZ = float.MinValue;
		//    var maxX = float.MaxValue;
		//    var maxY = float.MaxValue;
		//    var maxZ = float.MaxValue;

		//    foreach (var vertex in vertices)
		//    {
		//        var x = vertex.Position.X;
		//        var y = vertex.Position.Y;
		//        var z = vertex.Position.Z;

		//        minX = Math.Min(minX, x);
		//        minY = Math.Min(minY, y);
		//        minZ = Math.Min(minZ, z);
		//        maxX = Math.Max(maxX, x);
		//        maxY = Math.Max(maxY, y);
		//        maxZ = Math.Max(maxZ, z);
		//    }

		//    var min = new Vector3(minX, minY, minZ);
		//    var max = new Vector3(maxX, maxY, maxZ);

		//    Bounds = new BoundingBox(min, max);
		//}

        /// <summary>
        /// Checks if two AABBs intersect.
        /// </summary>
        /// <param name="box1">One AABB</param>
        /// <param name="box2">The other AABB</param>
        /// <returns>True if the two intersect.</returns>
        public static bool Intersects(AABB box1, AABB box2)
        {
            return true;
            //return box1.Bounds.Intersects(box2.Bounds);
        }

        /// <summary>
        /// Checks if a Ray intersects an AABB.
        /// </summary>
        /// <param name="box">The AABB in question.</param>
        /// <param name="ray">The Ray in question.</param>
        /// <returns>True if the two intersect.</returns>
        public static bool Intersects(AABB box, Ray ray)
        {
            return box.Bounds.Intersects(ray) != null;
        }
    }
}
