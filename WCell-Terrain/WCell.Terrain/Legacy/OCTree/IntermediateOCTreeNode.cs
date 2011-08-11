using System.Collections.Generic;

using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.Terrain.Legacy.OCTree
{
    public class IntermediateOCTreeNode
    {
        // Fields
        public BoundingBox Bounds;
        public IntermediateOCTreeNode[] Children;
        public OCTPoly[] Polys;

        // Methods
        public OCTreeNode Build(float WeldDelta)
        {
            int num;
            var node = new OCTreeNode
            {
                Bounds = Bounds,
                Center = ((Bounds.Min + Bounds.Max) * 0.5f)
            };

            if ((Polys != null) && (Polys.Length > 0))
            {
                var list = new List<Vector3>();
                var list2 = new List<int>();
                for (num = 0; num < Polys.Length; num++)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var vector = Polys[num].Verts[i];
                        var item = -1;
                        for (var j = 0; j < list.Count; j++)
                        {
                            if (list[j].NearlyEqual(vector, WeldDelta))
                            {
                                item = j;
                                goto Label_00DD;
                            }
                        }
                    Label_00DD:
                        if (item != -1)
                        {
                            list2.Add(item);
                        }
                        else
                        {
                            list.Add(Polys[num].Verts[i]);
                            list2.Add(list.Count - 1);
                        }
                    }
                }
                var list3 = new List<int>();
                for (num = 0; num < (list2.Count / 3); num++)
                {
                    if (((Vector3.DistanceSquared(list[list2[num * 3]], list[list2[(num * 3) + 1]]) > MathExtensions.Epsilon) &&
                         (Vector3.DistanceSquared(list[list2[(num * 3) + 1]], list[list2[(num * 3) + 2]]) >
                          MathExtensions.Epsilon)) &&
                        (Vector3.DistanceSquared(list[list2[(num * 3) + 2]], list[list2[num * 3]]) > MathExtensions.Epsilon))
                    {
                        var plane = new Plane(list[list2[num * 3]], list[list2[(num * 3) + 1]], list[list2[(num * 3) + 2]]);
                        if (!float.IsNaN(plane.Normal.X))
                        {
                            list3.Add(list2[num * 3]);
                            list3.Add(list2[(num * 3) + 1]);
                            list3.Add(list2[(num * 3) + 2]);
                        }
                    }
                }
                node.Indices = list3.ToArray();
                node.Vertices = list.ToArray();
                node.Planes = new Plane[node.Indices.Length / 3];
                for (num = 0; num < (node.Indices.Length / 3); num++)
                {
                    node.Planes[num] = new Plane(node.Vertices[node.Indices[num * 3]],
                                                 node.Vertices[node.Indices[(num * 3) + 2]],
                                                 node.Vertices[node.Indices[(num * 3) + 1]]);
                }
            }
            if (Children != null)
            {
                node.Children = new OCTreeNode[Children.Length];
                for (num = 0; num < Children.Length; num++)
                {
                    node.Children[num] = Children[num].Build(WeldDelta);
                }
            }
            return node;
        }
    }
}
