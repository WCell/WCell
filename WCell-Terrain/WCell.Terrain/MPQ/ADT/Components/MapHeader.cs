namespace WCell.Terrain.MPQ.ADT.Components
{
    /// <summary>
    /// Contains offsets (relative to Base) for some other chunks that appear in the file. Since the file follows a well-defined structure, this is redundant information.
    /// </summary>
    public class MapHeader
    {
        public uint Base;

        public uint offsInfo;
        public uint offsTex;
        public uint offsModels;
        public uint offsModelIds;
        public uint offsMapObjects;
        public uint offsMapObjectIds;
        public uint offsDoodadDefinitions;
        public uint offsObjectDefinitions;
        public uint offsFlightBoundary; // tbc, wotlk	
        public uint offsMH2O;		// new in WotLK
    }
}