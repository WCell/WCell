using WCell.Util.Graphics;

namespace WCell.Addons.Terrain
{
    public class WMOGroup
    {
        // Model Coords (west, up, north)
        public BoundingBox Bounds;
        public Vector3[] Vertices;
        public BSPTree Tree;

        /// <summary>
        /// Returns the earliest time this BuildingGroup Intersects the given ray.
        /// Notice that the Ray <i>must</i> be transformed to model space prior to calling this function.
        /// </summary>
        public float? IntersectsWith(ref Ray ray, ref float tMax)
        {
            // Does the ray intersect the bounds of the this group?
            var result = Bounds.Intersects(ray);
            if (result == null) return null;
            if (result > tMax) return null;

            // Is there a BSP-Tree containing poly info for this node?
            if (Tree == null) return null;

            // Get the first intersection with the Polys in this BSP-Tree
            result = Tree.IntersectsWith(ref ray, ref tMax, Vertices);
            if (result == null) return null;
            
            return (result < tMax) ? result : null;
        }

        public float? IntersectsWith(ref Ray ray, ref float tMax, out Vector3 intersection)
        {
            // Does the ray pass through the Group's Bounds?
            var result = Bounds.Intersects(ray);
            if (result == null)
            {
                intersection = Vector3.Zero;
                return null;
            }
            if (result > tMax)
            {
                intersection = Vector3.Zero;
                return null;
            }

            // Does this Group have any triangles in it?
            if (Tree == null)
            {
                intersection = Vector3.Zero;
                return null;
            }

            // Get the first point of intersection from the BSP-Tree containing the poly information
            result = Tree.FirstPointOfIntersection(ref ray, ref tMax, Vertices, out intersection);
            if (result == null) return null;

            return result < tMax ? result : null;
        }
    }
}