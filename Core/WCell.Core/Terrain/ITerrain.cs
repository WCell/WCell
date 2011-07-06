using WCell.Constants.World;
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
		/// Indicates whether the given tile is available at all.
		/// When false is returned, this terrain will probably not provide any collision detection or pathfinding features.
		/// </summary>
		bool IsAvailable(int tileX, int tileY);

	    /// <summary>
	    /// Returns true if there is a clear line of sight between startPos and endPos.
	    /// Also returns true, if it could not reliably determine LoS.
	    /// </summary>
	    bool HasLOS(Vector3 startPos, Vector3 endPos);

		/// <summary>
		/// Asynchronously queries the shortest path between the given two points and calls the callback upon return.
		/// Might be executed synchronously.
		/// </summary>
		void FindPath(PathQuery query);

        /// <summary>
        /// Returns the height of the terrain right underneath the given worldPos.
        /// Returns float.NaN if it could not reliably determine the height.
        /// </summary>
        float GetGroundHeightUnderneath(Vector3 worldPos);

		/// <summary>
		/// Forces loading of tile. This must be used as it is a long process and will use a lot of resources.
		/// </summary>
		bool ForceLoadTile(int x, int y);
	}
}