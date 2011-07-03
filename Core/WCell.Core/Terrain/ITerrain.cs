using WCell.Core.Paths;
using WCell.Util.Graphics;

namespace WCell.Core.Terrain
{
	/// <summary>
	/// Represents the Terrain of a certain Map
	/// </summary>
	public interface ITerrain
	{
	    /// <summary>
	    /// Returns true if there is a clear line of sight between startPos and endPos.
	    /// </summary>
	    bool HasLOS(Vector3 startPos, Vector3 endPos);

		/// <summary>
		/// Asynchronously queries the shortest path between the given two points and calls the callback upon return
		/// </summary>
		void QueryDirectPathAsync(PathQuery query);

        /// <summary>
        /// Returns the height of the terrain right underneath the given worldPos
        /// </summary>
        float QueryHeightUnderneath(Vector3 worldPos);
	}
}