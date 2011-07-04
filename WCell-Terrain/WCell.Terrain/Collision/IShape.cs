using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision
{
    ///<summary>
    /// Describes a Shape contained in an AABB
    ///</summary>
    public interface IShape
    {
        /// <summary>
        /// The List of Vectors that lie on the corners of this Shape.
        /// </summary>
        Vector3[] Vertices
        {
            get;
        }

        /// <summary>
        /// The order in which the vertices should be rendered.
        /// </summary>
		int[] Indices
        {
            get;
        }

		/// <summary>
		/// A set of all indices of vertices that *could* get hit by the given ray.
		/// It must not ommit any such vertex.
		/// The returned set is desirable to be small however, to provide a better speed.
		/// </summary>
    	IEnumerable<int> GetPotentialColliders(Ray ray);
    }
}
