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
        /// Returns the height of the nearest point beneath the given position.
        /// </summary>
        float QueryWorldHeight(Vector3 worldPos);

		/// <summary>
		/// Sends a query to retrieve the shortest path and calls the callback upon return.
		/// </summary>
		void QueryDirectPath(PathQuery query);

        /// <summary>
        /// Returns the interpolated height of the terrain based on stored height-map data.
        /// </summary>
        float QueryTerrainHeight(Vector3 worldPos);

		float GetEvironmentHeight(float x, float y);

        /// <summary>
        /// Returns true if the line between startPos and endPos does not intersect the terrain.
        /// </summary>
        bool HasTerrainLOS(Vector3 startPos, Vector3 endPos);

        /// <summary>
        /// Returns null if no buildings exist between startPos and endPos, else the distance to the first building.
        /// </summary>
	    float? QueryWMOCollision(Vector3 startPos, Vector3 endPos);

        /// <summary>
        /// Returns true if the line between startPos and endPos does not intersect any building parts.
        /// </summary>
	    bool HasWMOLOS(Vector3 startPos, Vector3 endPos);

        /// <summary>
        /// Returns the height of the WMO piece directly beneath a position or null if no piece exists.
        /// </summary>
	    float? QueryWMOHeight(Vector3 worldPos);

        /// <summary>
        /// Returns null if no models exist between startPos and endPos, else the distance to the first model.
        /// </summary>
        float? QueryModelCollision(Vector3 startPos, Vector3 endPos);

        /// <summary>
        /// Returns true if the line between startPos and endPos does not intersect any model parts.
        /// </summary>
        bool HasModelLOS(Vector3 startPos, Vector3 endPos);

        /// <summary>
        /// Returns the height of the Model piece directly beneath a position or null if no piece exists.
        /// </summary>
        float? QueryModelHeight(Vector3 worldPos);
	}
}