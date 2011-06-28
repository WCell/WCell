using WCell.Util.Graphics;

namespace TerrainDisplay.MPQ.WMO.Components
{
    /// <summary>
    /// MOGI Chunk. Used in WMO Root files
    /// </summary>
    public class GroupInformation
    {
        /// <summary>
        /// Seems to be a limited version of the full group flags
        /// </summary>
        public WMOGroupFlags Flags;
        public BoundingBox BoundingBox;
        // -1 for no name?
        public int NameIndex;
    }
}
