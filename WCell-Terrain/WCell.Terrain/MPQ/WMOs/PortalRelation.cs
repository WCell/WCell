namespace WCell.Terrain.MPQ.WMOs
{
    /// <summary>
    /// MOPR Chunk. Used in WMO root files
    /// </summary>
    public struct PortalRelation
    {
        /// <summary>
        /// Portal index
        /// </summary>
        public ushort PortalIndex;
        /// <summary>
        /// WMO Group Index
        /// </summary>
        public ushort GroupIndex;
        /// <summary>
        /// 1 or -1
        /// </summary>
        public short Side;
    }
}
