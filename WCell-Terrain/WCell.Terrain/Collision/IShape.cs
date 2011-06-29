using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace TerrainDisplay.Collision._3D
{
    ///<summary>
    /// Describes a Shape contained in an AABB
    ///</summary>
    public interface IShape
    {
        /// <summary>
        /// The List of Vectors that lie on the corners of this Shape.
        /// </summary>
        List<Vector3> Vertices
        {
            get;
        }

        /// <summary>
        /// The order in which the vertices should be rendered.
        /// </summary>
        List<int> Indices
        {
            get;
        }

        /// <summary>
        /// The normalized Normal vector to the surface of this shape
        /// </summary>
        Vector3 UnitNormal
        {
            get;
        }

        /// <summary>
        /// The Vector that contains the minimum values of the vertices.
        /// </summary>
        Vector3 Min
        {
            get;
        }

        /// <summary>
        /// The Vector that contains the maximum values of the vertices.
        /// </summary>
        Vector3 Max
        {
            get;
        }

        /// <summary>
        /// Does a Ray intersect with this Shape?
        /// </summary>
        /// <param name="ray">The Ray in question.</param>
        /// <returns>True if they intersect.</returns>
        float? IntersectsWith(Ray ray);
    }
}
