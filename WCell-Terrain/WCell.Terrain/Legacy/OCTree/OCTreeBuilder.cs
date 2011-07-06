using System.Collections.Generic;
using WCell.Terrain.Legacy;

using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Legacy.OCTree
{
    public class OCTreeBuilder
    {
        // Fields
        private readonly float ExpandBoundsFactor;
        private readonly bool MakeSquare;
        private readonly int MaxPolygonsPerNode;
        private readonly int MaxSubdivisions;
        private readonly Vector3[] nodeOffsets;
        private readonly Vector3[] planeNormals;
        private readonly Vector3[] planeOffsets;
        private readonly float WeldDelta;

        // Methods
    	public OCTreeBuilder(int maxPolygonsPerNode = 5000, int maxSubdivisions = 3, bool makeSquare = false, float expandBoundsFactor = 0.05f) : 
			this(maxPolygonsPerNode, maxSubdivisions, makeSquare, 0.0001f, expandBoundsFactor)
    	{
    	}

    	public OCTreeBuilder(int maxPolygonsPerNode, int maxSubdivisions, bool makeSquare, float weldDelta, float expandBoundsFactor)
        {
            var vectorArray = new[] { (Vector3.Up + Vector3.Forward) + Vector3.Left, (Vector3.Up + Vector3.Forward) + Vector3.Right, (Vector3.Up + Vector3.Backward) + Vector3.Left, (Vector3.Up + Vector3.Backward) + Vector3.Right, (Vector3.Down + Vector3.Forward) + Vector3.Left, (Vector3.Down + Vector3.Forward) + Vector3.Right, (Vector3.Down + Vector3.Backward) + Vector3.Left, (Vector3.Down + Vector3.Backward) + Vector3.Right };
            nodeOffsets = vectorArray;
            vectorArray = new[] { Vector3.Forward, Vector3.Backward, Vector3.Left, Vector3.Right, Vector3.Up, Vector3.Down };
            planeOffsets = vectorArray;
            vectorArray = new[] { Vector3.Backward, Vector3.Forward, Vector3.Right, Vector3.Left, Vector3.Down, Vector3.Up };
            planeNormals = vectorArray;
            MaxPolygonsPerNode = maxPolygonsPerNode;
            MaxSubdivisions = maxSubdivisions;
            MakeSquare = makeSquare;
            WeldDelta = weldDelta;
            ExpandBoundsFactor = expandBoundsFactor;
        }

        public OCTree Build(Vector3[] vertexArray, int[] indexArray)
        {
            var list = new List<OCTPoly>();
            for (var i = 0; i < (indexArray.Length / 3); i++)
            {
                var a = vertexArray[indexArray[i * 3]];
                var b = vertexArray[indexArray[(i * 3) + 1]];
                var c = vertexArray[indexArray[(i * 3) + 2]];
                list.Add(new OCTPoly(a, b, c));
            }
            var polyList = list.ToArray();
            var box = CalculateBounds(polyList);
            var center = ((box.Max + box.Min) * 0.5f);
            var extents = box.Max - center;
            if (MakeSquare)
            {
                var x = extents.X;
                if (extents.Y > x)
                {
                    x = extents.Y;
                }
                if (extents.Z > x)
                {
                    x = extents.Z;
                }
                extents.X = extents.Y = extents.Z = x;
            }
            extents += (extents * ExpandBoundsFactor);
            box.Min = center - extents;
            box.Max = center + extents;
            var node = new IntermediateOCTreeNode
            {
                Bounds = box
            };
            RecursiveBuild(node, polyList, center, extents, 0);
            var tree = new OCTree
            {
                Root = node.Build(WeldDelta)
            };
            return tree;
        }

        private static BoundingBox CalculateBounds(OCTPoly[] polyList)
        {
            var vector = (Vector3.One * 3.402823E+38f);
            var vector2 = (Vector3.One * -3.402823E+38f);
            for (var i = 0; i < polyList.Length; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    Vector3.Min(ref vector, ref polyList[i].Verts[j], out vector);
                    Vector3.Max(ref vector2, ref polyList[i].Verts[j], out vector2);
                }
            }
            return new BoundingBox(vector, vector2);
        }

        private void RecursiveBuild(IntermediateOCTreeNode node, OCTPoly[] PolyList, Vector3 center, Vector3 extents, int depth)
        {
            var planeArray = new[] { MathExtensions.PlaneFromPointNormal(center + (planeOffsets[0] * extents), planeNormals[0]), MathExtensions.PlaneFromPointNormal(center + (planeOffsets[1] * extents), planeNormals[1]), MathExtensions.PlaneFromPointNormal(center + (planeOffsets[2] * extents), planeNormals[2]), MathExtensions.PlaneFromPointNormal(center + (planeOffsets[3] * extents), planeNormals[3]), MathExtensions.PlaneFromPointNormal(center + (planeOffsets[4] * extents), planeNormals[4]), MathExtensions.PlaneFromPointNormal(center + (planeOffsets[5] * extents), planeNormals[5]) };
            var polyArray = new OCTPoly[PolyList.Length];
            var index = 0;
            while (index < PolyList.Length)
            {
                polyArray[index] = new OCTPoly(PolyList[index]);
                index++;
            }
            for (var i = 0; i < planeArray.Length; i++)
            {
                var list = new List<OCTPoly>();
                for (var j = 0; j < polyArray.Length; j++)
                {
                    int num4;
                    var newPolys = new OCTPoly[2];
                    Split(polyArray[j], planeArray[i], ref newPolys, out num4);
                    index = 0;
                    while (index < num4)
                    {
                        var plane = new Plane(newPolys[index].Verts[0], newPolys[index].Verts[1], newPolys[index].Verts[2]);
                        if (!float.IsNaN(plane.Normal.X))
                        {
                            list.Add(newPolys[index]);
                        }
                        index++;
                    }
                }
                polyArray = new OCTPoly[list.Count];
                index = 0;
                while (index < list.Count)
                {
                    polyArray[index] = new OCTPoly(list[index]);
                    index++;
                }
            }
            if ((polyArray.Length <= MaxPolygonsPerNode) || (depth >= MaxSubdivisions))
            {
                node.Polys = new OCTPoly[polyArray.Length];
                if (polyArray.Length > 0)
                {
                    for (index = 0; index < polyArray.Length; index++)
                    {
                        node.Polys[index] = new OCTPoly(polyArray[index]);
                    }
                }
            }
            else
            {
                node.Children = new IntermediateOCTreeNode[8];
                for (index = 0; index < 8; index++)
                {
                    node.Children[index] = new IntermediateOCTreeNode();
                    var vector = (extents * 0.5f);
                    var vector2 = center + (nodeOffsets[index] * vector);
                    node.Children[index].Bounds = new BoundingBox(vector2 - vector, vector2 + vector);
                    RecursiveBuild(node.Children[index], PolyList, vector2, vector, depth + 1);
                }
            }
        }

        private void Split(OCTPoly poly, Plane p, ref OCTPoly[] newPolys, out int numPolys)
        {
            numPolys = 0;
            var num = p.DotCoordinate(poly.Verts[0]);
            var num2 = p.DotCoordinate(poly.Verts[1]);
            var num3 = p.DotCoordinate(poly.Verts[2]);
            var vector = MathExtensions.NormalFromPoints(poly.Verts[0], poly.Verts[1], poly.Verts[2]);
            if (((num > 0f) && (num2 > 0f)) && (num3 > 0f))
            {
                newPolys[0] = new OCTPoly(poly.Verts[0], poly.Verts[1], poly.Verts[2]);
                numPolys = 1;
            }
            else if (((num < 0f) && (num2 < 0f)) && (num3 < 0f))
            {
                numPolys = 0;
            }
            else if (((num == 0f) && (num2 == 0f)) && (num3 == 0f))
            {
                if (p.DotNormal(vector) >= 0f)
                {
                    newPolys[0] = new OCTPoly(poly.Verts[0], poly.Verts[1], poly.Verts[2]);
                    numPolys = 1;
                }
                else
                {
                    numPolys = 0;
                }
            }
            else
            {
                var vectorArray = new Vector3[4];
                var num4 = 0;
                for (var i = 0; i < 3; i++)
                {
                    var vector2 = poly.Verts[i];
                    var vector3 = poly.Verts[((i + 1) > 2) ? 0 : (i + 1)];
                    var num6 = p.DotCoordinate(vector2);
                    var num7 = p.DotCoordinate(vector3);
                    if (num6 >= 0f)
                    {
                        vectorArray[num4++] = vector2;
                    }
                    if (((num7 <= 0f) && (num6 > 0f)) || ((num7 >= 0f) && (num6 < 0f)))
                    {
						Vector3 point;
                        if (Intersection.LineSegmentIntersectsPlane(vector2, vector3, p, out point))
                        {
                            vectorArray[num4++] = point;
                        }
                    }
                }
                switch (num4)
                {
                    case 3:
                    case 4:
                        newPolys[0] = new OCTPoly(vectorArray[0], vectorArray[1], vectorArray[2]);
                        numPolys = 1;
                        if (num4 == 4)
                        {
                            newPolys[1] = new OCTPoly(vectorArray[0], vectorArray[2], vectorArray[3]);
                            numPolys = 2;
                        }
                        break;
                }
            }
        }
    }


}
