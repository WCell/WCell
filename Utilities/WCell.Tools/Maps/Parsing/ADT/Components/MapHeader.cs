using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Tools.Maps
{
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
