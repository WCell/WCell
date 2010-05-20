using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Util.Graphics;

namespace WCell.Collision
{
    internal class Building : IBounded
    {
        // World Coords (west, north, up)
        public BoundingBox Bounds
        {
            get;
            set;
        }

        public Vector3 Center;
        // Transforms a vector into Building space given that the vector is in Model coords first
        // (west, up, north)
        public Matrix InverseRotation;
        public BuildingGroup[] BuildingGroups;

        /// <summary>
        /// Returns the earliest time at which a ray intersects the polys in this Building.
        /// Notice that the ray should be in world coords.
        /// </summary>
        public float? IntersectsWith(ref Ray ray, ref float tMax)
        {
            // Does the ray intersect the outer bounding box for this group?
            float? result;
            Bounds.Intersects(ref ray, out result);
            if (result == null) return null;

            // Does this building have any building groups to check against?
            if (BuildingGroups == null) return null;

            // Transform the ray from world position/direction to model-space position/direction
            var localRay = GetLocalRay(ref ray);

            // Get the first time this ray intersects any of the building groups
            result = tMax;
            for (var i = 0; i < BuildingGroups.Length; i++)
            {
                var group = BuildingGroups[i];
                var newResult = group.IntersectsWith(ref localRay, ref tMax);
                if (newResult == null) continue;

                result = Math.Min(result.Value, newResult.Value);
            }

            return (result < tMax) ? result : null;
        }

        /// <summary>
        /// Determines the earliest point of intersection between a ray and this Building.
        /// </summary>
        /// <param name="ray">The Ray to test against.</param>
        /// <param name="tMax">The earliest point of intersection (tmax*Ray.Direction + Ray.Position)</param>
        /// <param name="intersection">The vectorized point of first intersection</param>
        /// <returns></returns>
        public float? IntersectsWith(ref Ray ray, ref float tMax, out Vector3 intersection)
        {
            intersection = Vector3.Zero;

            float? result;
            Bounds.Intersects(ref ray, out result);
            if (result == null)
            {
                return null;
            }
            if (BuildingGroups == null)
            {
                return null;
            }

            var localRay = GetLocalRay(ref ray);

            result = tMax;
            for (var i = 0; i < BuildingGroups.Length; i++)
            {
                var newResult = BuildingGroups[i].IntersectsWith(ref localRay, ref tMax, out intersection);
                if (newResult == null) continue;

                result = Math.Min(result.Value, newResult.Value);
            }

            return result < tMax ? result : null;
        }

        /// <summary>
        /// Transforms a Ray in World Coords into Building-space coords
        /// </summary>
        /// <param name="ray">A ray in World Coordinates</param>
        /// <returns>A Ray in Building Coords</returns>
        private Ray GetLocalRay(ref Ray ray)
        {
            var localRayPos = ReCenter(ray.Position);
            localRayPos = TransformToModelCoords(localRayPos);
            localRayPos = Vector3.TransformNormal(localRayPos, InverseRotation);

            var localRayDir = TransformToModelCoords(ray.Direction);
            localRayDir = Vector3.TransformNormal(localRayDir, InverseRotation);
            return new Ray(localRayPos, localRayDir);
        }

        /// <summary>
        /// Re-centers a vector relative to the World center to the Building center
        /// </summary>
        /// <param name="vec">A vector relative to the World center</param>
        /// <returns>A vector relative to the building center</returns>
        private Vector3 ReCenter(Vector3 vec)
        {
            return vec - Center;
        }

        private static Vector3 TransformToModelCoords(Vector3 vec)
        {
            // shift axis to conform with model space
            var newX = vec.Y;
            var newY = vec.Z;
            var newZ = vec.X;

            return new Vector3(newX, newY, newZ);
        }
    }

    internal class BuildingGroup
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

    internal class PackedVector3
    {
        internal ushort X;
        internal ushort Y;
        internal ushort Z;

        internal Vector3 Unpack()
        {
            return new Vector3(HalfUtils.Unpack(X), HalfUtils.Unpack(Y), HalfUtils.Unpack(Z));
        }
    }
}
