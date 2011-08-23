using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Paths;
using WCell.Util.Graphics;

namespace WCell.Terrain.Pathfinding
{
    public static class PathExtensions
    {
        public static void Add(this Path path, List<NodeItem> pairs)
        {
            var pathNodes = new List<Vector3>(pairs.Count);
            foreach (var pair in pairs)
            {
                pathNodes.Add(pair.Node);
            }
            path.Add(pathNodes);
        }
    }
}
