using System.Collections.Generic;
using WCell.Util.Graphics;

namespace WCell.Terrain.Legacy.OCTree
{
	/// <summary>
	/// I don't know where this class comes from, but it seems like it was decompiled from somewhere.
	/// Also, it takes forever to build even the simplest octree with only a few thousand triangles.
	/// All in all: Useless?
	/// </summary>
    public class OCTree
    {
        // Fields
        public OCTreeNode Root;

        // Methods
        public bool GetIntersectingNodes(ref BoundingBox bbox, ref List<OCTreeNode> intersection)
        {
            return Root.GetIntersectingNodes(ref bbox, ref intersection);
        }

        public bool GetIntersectingNodes(ref BoundingSphere sphere, ref List<OCTreeNode> intersection)
        {
            return Root.GetIntersectingNodes(ref sphere, ref intersection);
        }

        public bool GetIntersectingNodes(ref BoundingSphere sphere, ref Matrix world, ref List<OCTreeNode> intersection)
        {
            sphere.Center = Vector3.Transform(sphere.Center, Matrix.Invert(world));
            return Root.GetIntersectingNodes(ref sphere, ref intersection);
        }

        public bool GetIntersectingPolygon(ref Ray ray, out OCTreeIntersection intersection)
        {
            intersection = new OCTreeIntersection();
            return Root.GetIntersectingPolygon(ref ray, ref intersection);
        }

        public bool GetIntersectingPolygon(Ray ray, Matrix world, out OCTreeIntersection intersection)
        {
            intersection = new OCTreeIntersection();
            var matrix = Matrix.Invert(world);
            ray.Position = Vector3.Transform(ray.Position, matrix);
            ray.Direction = Vector3.TransformNormal(ray.Direction, matrix);
            ray.Direction.Normalize();
            var intersectingPolygon = Root.GetIntersectingPolygon(ref ray, ref intersection);
            if (intersectingPolygon)
            {
                intersection.IntersectionPoint = Vector3.Transform(intersection.IntersectionPoint, world);
                intersection.IntersectionNormal = Vector3.TransformNormal(intersection.IntersectionNormal, world);
                intersection.IntersectionNormal.Normalize();
                intersection.IntersectType = OCTreeIntersectionType.Inside;
                intersection.DistanceSquared = Vector3.DistanceSquared(ray.Position, intersection.IntersectionPoint);
            }
            return intersectingPolygon;
        }

        public bool GetIntersectingPolygons(ref BoundingSphere sphere, ref Vector3 velocityNormal, ref List<OCTreeIntersection> intersection)
        {
            var flag = Root.GetIntersectingPolygons(ref sphere, ref velocityNormal, ref intersection);
            if (flag)
            {
                intersection.Sort();
            }
            return flag;
        }

        public void MoveSphere(ref BoundingSphere sphere, ref Vector3 sphereVelocity, float Friction)
        {
            var sphereColliders = new List<OCTreeIntersection>();
            MoveSphere(ref sphere, ref sphereVelocity, Friction, ref sphereColliders);
        }

        public void MoveSphere(ref BoundingSphere sphere, ref Vector3 sphereVelocity, float Friction, ref List<OCTreeIntersection> sphereColliders)
        {
            int num4;
            var vector = sphereVelocity;
            var velocityNormal = Vector3.Normalize(vector);
            var sphere2 = new BoundingSphere(sphere.Center + vector, sphere.Radius);
            var list = new List<OCTreeIntersection>();
            sphere2.Radius = sphere.Radius + vector.Length();
            GetIntersectingPolygons(ref sphere2, ref velocityNormal, ref list);
            sphere2.Radius = sphere.Radius;
            for (var i = 0; i < 5; i++)
            {
                var num2 = 0;
                for (var j = 0; j < list.Count; j++)
                {
                    var intersection = list[j];
                    if (intersection.Node.UpdateIntersection(ref intersection, ref sphere2, ref velocityNormal) &&
                        (sphere2.Contains(intersection.IntersectionPoint) != ContainmentType.Disjoint))
                    {
                        num2++;
                        var vector3 =
                            (intersection.IntersectionNormal * (intersection.IntersectionDepth + 0.001f));
                        sphere2.Center += vector3;
                        vector = sphere2.Center - sphere.Center;
                        velocityNormal = Vector3.Normalize(vector);
                        var flag = false;
                        num4 = 0;
                        while (num4 < sphereColliders.Count)
                        {
                            if ((sphereColliders[num4].Node == intersection.Node) &&
                                (sphereColliders[num4].PolygonIndex == intersection.PolygonIndex))
                            {
                                flag = true;
                                break;
                            }
                            num4++;
                        }
                        if (!flag)
                        {
                            sphereColliders.Add(intersection);
                        }
                    }
                }
                if (num2 == 0)
                {
                    break;
                }
            }
            var flag2 = false;
            for (num4 = 0; num4 < sphereColliders.Count; num4++)
            {
                if (Vector3.Dot(sphereColliders[num4].IntersectionNormal, Vector3.Up) > 0.5f)
                {
                    vector -= ((vector * (Vector3.One - Vector3.Up)) * Friction);
                    flag2 = true;
                    break;
                }
            }
            if (!flag2)
            {
                vector -= (((vector * (Vector3.One - Vector3.Up)) * Friction) * 0.5f);
            }
            sphereVelocity = vector;
            sphere = sphere2;
        }
    }
}
