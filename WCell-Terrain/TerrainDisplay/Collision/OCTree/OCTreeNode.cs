using System.Collections.Generic;
using TerrainDisplay.Util;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision.OCTree
{
    public class OCTreeNode
    {
        // Fields
        public BoundingBox Bounds;
        public Vector3 Center;
        public OCTreeNode[] Children;
        public int[] Indices;
        public Plane[] Planes;
        public Vector3[] Vertices;

        // Methods
        public bool GetIntersectingNodes(ref BoundingBox bbox, ref List<OCTreeNode> nodes)
        {
            if (bbox.Intersects(Bounds))
            {
                if (Indices != null)
                {
                    nodes.Add(this);
                }
                if (Children != null)
                {
                    for (var i = 0; i < Children.Length; i++)
                    {
                        Children[i].GetIntersectingNodes(ref bbox, ref nodes);
                    }
                }
            }
            return (nodes.Count > 0);
        }

        public bool GetIntersectingNodes(ref BoundingSphere sphere, ref List<OCTreeNode> nodes)
        {
            if (sphere.Intersects(Bounds))
            {
                if (Indices != null)
                {
                    nodes.Add(this);
                }
                if (Children != null)
                {
                    for (var i = 0; i < Children.Length; i++)
                    {
                        Children[i].GetIntersectingNodes(ref sphere, ref nodes);
                    }
                }
            }
            return (nodes.Count > 0);
        }

        public bool GetIntersectingPolygon(ref Ray ray, ref OCTreeIntersection closest)
        {
            if (Indices != null)
            {
                var nullable = ray.Intersects(Bounds);
                if (nullable.HasValue)
                {
                    var num = (closest.IntersectType == OCTreeIntersectionType.None) ? float.MaxValue : closest.DistanceSquared;
                    if (nullable.Value < num)
                    {
                        for (var i = 0; i < (Indices.Length / 3); i++)
                        {
                            float num2;
                            float num3;
                            float num4;
                            if (Intersection.RayTriangleIntersect(ray.Position, ray.Direction, Vertices[Indices[i * 3]], Vertices[Indices[(i * 3) + 1]], Vertices[Indices[(i * 3) + 2]], out num2, out num3, out num4))
                            {
                                var vector = ray.Position + ((ray.Direction * num2));
                                var num6 = Vector3.DistanceSquared(vector, ray.Position);
                                if (num6 < num)
                                {
                                    num = num6;
                                    closest.IntersectionNormal = MathExtensions.NormalFromPoints(Vertices[Indices[i * 3]], Vertices[Indices[(i * 3) + 1]], Vertices[Indices[(i * 3) + 2]]);
                                    closest.IntersectionPoint = vector;
                                    closest.Node = this;
                                    closest.PolygonIndex = i;
                                    closest.DistanceSquared = num6;
                                    closest.IntersectType = OCTreeIntersectionType.Inside;
                                }
                            }
                        }
                    }
                }
            }
            else if (Children != null)
            {
                for (var j = 0; j < Children.Length; j++)
                {
                    Children[j].GetIntersectingPolygon(ref ray, ref closest);
                }
            }
            return (closest.IntersectType != OCTreeIntersectionType.None);
        }

        public bool GetIntersectingPolygons(ref BoundingSphere sphere, ref Vector3 velocityNormal, ref List<OCTreeIntersection> nodes)
        {
            if (Bounds.Contains(sphere) != ContainmentType.Disjoint)
            {
                if (Indices != null)
                {
                    for (var i = 0; i < (Indices.Length / 3); i++)
                    {
                        OCTreeIntersection intersection;
                        if (GetIntersection(ref sphere, ref velocityNormal, i, out intersection))
                        {
                            nodes.Add(intersection);
                        }
                    }
                }
                if (Children != null)
                {
                    for (var j = 0; j < Children.Length; j++)
                    {
                        Children[j].GetIntersectingPolygons(ref sphere, ref velocityNormal, ref nodes);
                    }
                }
            }
            return (nodes.Count > 0);
        }

        public bool GetIntersection(ref BoundingSphere sphere, ref Vector3 velocityNormal, int polyIndex, out OCTreeIntersection intersection)
        {
            var flag = false;
            intersection = new OCTreeIntersection();
            var plane = Planes[polyIndex];
            if (plane.DotNormal(velocityNormal) < 0f)
            {
                if (plane.DotCoordinate(sphere.Center) < 0f)
                {
                    return false;
                }
                if (plane.Intersects(sphere) != PlaneIntersectionType.Intersecting)
                {
                    return false;
                }
                var a = Vertices[Indices[polyIndex * 3]];
                var b = Vertices[Indices[(polyIndex * 3) + 1]];
                var c = Vertices[Indices[(polyIndex * 3) + 2]];
                var p = Intersection.ClosestPointOnPlane(sphere.Center, plane);
                if (Intersection.PointInTriangle(p, a, b, c))
                {
                    intersection.IntersectionPoint = p;
                    intersection.IntersectionNormal = plane.Normal;
                    intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(p, sphere.Center);
                    intersection.Node = this;
                    intersection.PolygonIndex = polyIndex;
                    intersection.IntersectType = OCTreeIntersectionType.Inside;
                    flag = true;
                }
                else
                {
                    float num;
                    Vector3 vector5;
                    if (sphere.Contains(a) != ContainmentType.Disjoint)
                    {
                        intersection.IntersectionPoint = a;
                        intersection.IntersectionNormal = Vector3.Normalize(sphere.Center - a);
                        intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(a, sphere.Center);
                        intersection.Node = this;
                        intersection.PolygonIndex = polyIndex;
                        intersection.IntersectType = OCTreeIntersectionType.Point;
                        return true;
                    }
                    if (sphere.Contains(b) != ContainmentType.Disjoint)
                    {
                        intersection.IntersectionPoint = b;
                        intersection.IntersectionNormal = Vector3.Normalize(sphere.Center - b);
                        intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(b, sphere.Center);
                        intersection.Node = this;
                        intersection.PolygonIndex = polyIndex;
                        intersection.IntersectType = OCTreeIntersectionType.Point;
                        return true;
                    }
                    if (sphere.Contains(c) != ContainmentType.Disjoint)
                    {
                        intersection.IntersectionPoint = c;
                        intersection.IntersectionNormal = Vector3.Normalize(sphere.Center - c);
                        intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(c, sphere.Center);
                        intersection.Node = this;
                        intersection.PolygonIndex = polyIndex;
                        intersection.IntersectType = OCTreeIntersectionType.Point;
                        return true;
                    }
                    Intersection.ClosestPointOnSegment(sphere.Center, a, b, out num, out vector5);
                    if (sphere.Contains(vector5) != ContainmentType.Disjoint)
                    {
                        intersection.IntersectionPoint = vector5;
                        intersection.IntersectionNormal = Vector3.Normalize(sphere.Center - vector5);
                        intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(vector5, sphere.Center);
                        intersection.IntersectType = OCTreeIntersectionType.Edge;
                        intersection.Node = this;
                        intersection.PolygonIndex = polyIndex;
                        return true;
                    }
                    Intersection.ClosestPointOnSegment(sphere.Center, b, c, out num, out vector5);
                    if (sphere.Contains(vector5) != ContainmentType.Disjoint)
                    {
                        intersection.IntersectionPoint = vector5;
                        intersection.IntersectionNormal = Vector3.Normalize(sphere.Center - vector5);
                        intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(vector5, sphere.Center);
                        intersection.IntersectType = OCTreeIntersectionType.Edge;
                        intersection.Node = this;
                        intersection.PolygonIndex = polyIndex;
                        flag = true;
                    }
                    else
                    {
                        Intersection.ClosestPointOnSegment(sphere.Center, c, a, out num, out vector5);
                        if (sphere.Contains(vector5) != ContainmentType.Disjoint)
                        {
                            intersection.IntersectionPoint = vector5;
                            intersection.IntersectionNormal = Vector3.Normalize(sphere.Center - vector5);
                            intersection.IntersectionDepth = sphere.Radius - Vector3.Distance(vector5, sphere.Center);
                            intersection.IntersectType = OCTreeIntersectionType.Edge;
                            intersection.Node = this;
                            intersection.PolygonIndex = polyIndex;
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        public bool UpdateIntersection(ref OCTreeIntersection intersection, ref BoundingSphere sphere, ref Vector3 velocityNormal)
        {
            OCTreeIntersection intersection2;
            if (GetIntersection(ref sphere, ref velocityNormal, intersection.PolygonIndex, out intersection2))
            {
                intersection = intersection2;
            }
            else
            {
                intersection.IntersectType = OCTreeIntersectionType.None;
            }
            return (intersection.IntersectType != OCTreeIntersectionType.None);
        }
    }
}