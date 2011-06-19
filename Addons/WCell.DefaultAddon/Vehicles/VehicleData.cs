using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WCell.Constants.NPCs;
using WCell.Util;

namespace WCell.Addons.Default.Vehicles
{
    public class VehicleData
    {
        public uint NPCId;
        public uint VehicleId;
        public float VehicleAimAdjustment;
        public bool IsMinion;
        public uint Seat;
        public uint PassengerNPCId;
    }

    [XmlRoot("VehicleData")]
    public class VehicleDataCollection : XmlFile<VehicleDataCollection>
    {
        [XmlElement("Vehicle")]
        public List<VehicleData> VehicleDataList;
    }
}
