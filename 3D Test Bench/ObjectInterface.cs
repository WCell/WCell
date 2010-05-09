using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Collections;

namespace _3D_Test_Bench
{
    /// <summary>
    /// Provides an interface to render plain objects. Scope should be limited to points, wireframe and solid shaded triangles
    /// </summary>
    public interface PlainObjectInterface
    {
        Vector3 ObjectCenter { get; }
        Vector3[] ObjectPoints { get; }
        Int16[] VertexIndices { get; }

        System.Drawing.Color ObjectColour { get; }

        PlainObjectInterface ObjectParent { get; }

        PrimitiveType RenderingType { get; }
    }

    public class WireFrameObject : PlainObjectInterface
    {
        public override string ToString()
        {
            return "Wire Frame Object - " + this.sName;
        }

        PrimitiveType PlainObjectInterface.RenderingType
        {
            get
            {
                return PrimitiveType.LineList;
            }
        }

        private ArrayList objectPoints = new ArrayList(), vertexIndices = new ArrayList();
        private readonly PlainObjectInterface parent;
        private System.Drawing.Color objectColour;
        private readonly Vector3 position;
        private readonly string sName;

        public WireFrameObject(PlainObjectInterface Parent, System.Drawing.Color Colour, Vector3 RelativePosition, string shortName)
        {
            this.parent = Parent;
            this.objectColour = Colour;
            this.position = RelativePosition;
            sName = shortName;
        }

        /// <summary>
        /// Adds a single vertex to the object
        /// </summary>
        /// <param name="vertex">3D vector defining the relative location of the point</param>
        public void AddPointData(Vector3 vertex)
        {
            this.objectPoints.Add(vertex);
        }

        /// <summary>
        /// Adds an array of data points to the object. All are relative to the object center
        /// </summary>
        /// <param name="vertices">Array of 3D vectors</param>
        public void AddPointData(Vector3[] vertices)
        {
            foreach (Vector3 vert in vertices)
                this.objectPoints.Add(vert);
        }

        /// <summary>
        /// Adds a range of points from a vector array using a start index and length to add. All points are relative to the object center
        /// </summary>
        /// <param name="vertices">Array of 3D vectors</param>
        /// <param name="start">Start index</param>
        /// <param name="size">Number of elements from the array to add</param>
        public void AddPointData(Vector3[] vertices, int start, int size)
        {
            for (int i = start; i < start + size && i<vertices.Count() && i>=0; i++)
            {
                this.objectPoints.Add(vertices[i]);
            }
        }

        /// <summary>
        /// Adds two vertex indices to define a line in the wireframe.
        /// </summary>
        /// <param name="startIndex">Start index</param>
        /// <param name="endIndex">End index</param>
        public void AddLine(int startIndex, int endIndex)
        {
            this.vertexIndices.Add(startIndex);
            this.vertexIndices.Add(endIndex);
        }

        Vector3 PlainObjectInterface.ObjectCenter
        {
            get
            {
                return this.position + (this.parent!=null ? this.parent.ObjectCenter : new Vector3(0,0,0));
            }
        }

        System.Drawing.Color PlainObjectInterface.ObjectColour
        {
            get
            {
                return this.objectColour;
            }
        }

        Int16[] PlainObjectInterface.VertexIndices
        {
            get
            {
                Int16[] tempArray = new Int16[this.vertexIndices.Count];
                int count = 0;
                foreach (Int16 tempInt in this.vertexIndices)
                    tempArray[count++] = tempInt;
                return tempArray;
            }
        }

        PlainObjectInterface PlainObjectInterface.ObjectParent
        {
            get
            {
                return this.parent;
            }
        }

        Vector3[] PlainObjectInterface.ObjectPoints
        {
            get
            {
                Vector3[] tempArray = new Vector3[this.objectPoints.Count];
                int count = 0;
                foreach (Vector3 tempInt in this.vertexIndices)
                    tempArray[count++] = tempInt;
                return tempArray;
            }
        }
    }

    public class DotPointObject : PlainObjectInterface
    {
        public override string ToString()
        {
            return "Dot Point Object";
        }

        PrimitiveType PlainObjectInterface.RenderingType
        {
            get
            {
                return PrimitiveType.PointList;
            }
        }

        private readonly PlainObjectInterface parent;
        private System.Drawing.Color dotColour;
        private readonly Vector3 position;

        public DotPointObject(PlainObjectInterface Parent, System.Drawing.Color Colour, Vector3 RelativePosition)
        {
            this.parent = Parent;
            this.dotColour = Colour;
            this.position = RelativePosition;
        }

        /// <summary>
        /// Grabs the absolute position by recursively super-imposing the each object centre up the tree
        /// </summary>
        Vector3 PlainObjectInterface.ObjectCenter
        {
            get
            {
                return this.position + (this.parent!=null ? this.parent.ObjectCenter : new Vector3(0,0,0));
            }
        }

        /// <summary>
        /// Grabs the single point of this object. Is always 0,0,0 since it will exist at its origin
        /// </summary>
        Vector3[] PlainObjectInterface.ObjectPoints
        {
            get
            {
                return new Vector3[] { new Vector3(0, 0, 0) };
            }
        }

        /// <summary>
        /// Grabs an array of 16-bit integers which index the object vertices. The indices define the lines that form the wireframe object. This is always zero for a point
        /// </summary>
        Int16[] PlainObjectInterface.VertexIndices
        {
            get
            {
                return new Int16[] { 0 };
            }
        }

        /// <summary>
        /// Returns the colour the point will be rendered as
        /// </summary>
        System.Drawing.Color PlainObjectInterface.ObjectColour
        {
            get
            {
                return this.dotColour;
            }
        }

        /// <summary>
        /// Grabs the parent of this object
        /// </summary>
        PlainObjectInterface PlainObjectInterface.ObjectParent
        {
            get
            {
                return this.parent;
            }
        }
    }
}
