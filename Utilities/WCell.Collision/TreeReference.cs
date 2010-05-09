using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Collision
{
    internal class TreeReference<T> where T : IBounded
    {
        public QuadTree<T> Tree
        {
            get;
            set;
        }

        public object LoadLock
        {
            get;
            private set;
        }

        public bool Broken
        {
            get;
            set;
        }

        
        public TreeReference(QuadTree<T> tree)
        {
            Tree = tree;
            LoadLock = new object();
        }

        public TreeReference()
            : this(null)
        {
        }
    }
}
