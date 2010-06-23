using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TerrainDisplay.Recast
{
    public class NavMeshDetailTriIndex
    {
        private readonly byte[] indices = new [] {
            byte.MinValue,
            byte.MinValue,
            byte.MinValue
        };

        public byte Index0 
        { 
            get { return indices[0]; } 
            set { indices[0] = value; }
        }

        public byte Index1
        {
            get { return indices[1]; }
            set { indices[1] = value; }
        }

        public byte Index2
        {
            get { return indices[2]; }
            set { indices[2] = value; }
        }

        public byte this[int index]
        {
            get { return indices[index]; }
            set { indices[index] = value; }
        }
    }
}
