using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.Xna.Framework;

namespace TerrainAnalysis
{
    public class TileVertex
    {
        private CustomVertex.PositionColored vertex, normal;

        public TileVertex(CustomVertex.PositionColored Vertex, Microsoft.Xna.Framework.Vector3 normal)
        {
            this.vertex = Vertex;
            this.normal = new CustomVertex.PositionColored(normal.X, normal.Y, normal.Z, System.Drawing.Color.White.ToArgb());
        }


        public void SetX(float x)
        {
            this.vertex.X = x;
        }

        public void SetY(float y)
        {
            this.vertex.Y = y;
        }

        public void SetZ(float z)
        {
            this.vertex.Z = z;
        }

        public void SetColour(int c)
        {
        }

        public float NormalX
        {
            set
            {
                this.normal.X = value;
            }
        }

        public float NormalY
        {
            set
            {
                this.normal.Y = value;
            }
        }

        public float NormalZ
        {
            set
            {
                this.normal.Z = value;
            }
        }


        public CustomVertex.PositionColored DataNormal
        {
            get
            {
                return this.normal;
            }
        }

        public CustomVertex.PositionColored DataPoint
        {
            get
            {
                return this.vertex;
            }
        }
    }
}
