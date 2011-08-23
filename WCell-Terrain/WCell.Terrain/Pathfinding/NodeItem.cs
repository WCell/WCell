using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Pathfinding
{
    public struct NodeItem
    {
        public Vector3 Node;
        public SearchItem Item;

        public NodeItem(Vector3 node, SearchItem item)
        {
            Node = node;
            Item = item;
        }
    }
}
