using System;
using System.IO;
using System.Xml;
using NLog;
using WCell.Core.Initialization;
using WCell.RealmServer;
using WCell.RealmServer.NPCs;

namespace WCell.Addons.Default.Vehicles
{
    public static class VehicleCorrections
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void FixVehicles()
        {
            var count = 0;
            try
            {
                var path = Path.Combine(RealmServerConfiguration.ContentDir, "VehicleData.xml");
                var doc = new XmlDocument();
                doc.Load(path);
                var root = doc.DocumentElement;
                if (root == null)
                    return;

                Log.Info("Loading VehicleData.xml");

                foreach (XmlNode vehicleNode in root.ChildNodes)
                {
                    uint npcId;
                    if (!uint.TryParse(vehicleNode.FirstChild.InnerText, out npcId))
                        continue;

                    var npcEntry = NPCMgr.GetEntry(npcId);
                    if (npcEntry == null)
                        continue;

                    int seat = 0;

                    foreach (XmlNode child in vehicleNode)
                    {
                        if (string.IsNullOrEmpty(child.InnerText))
                            continue;

                        switch (child.Name)
                        {
                            case "VehicleId":
                                {
                                    uint vehicleId;
                                    if (!uint.TryParse(child.InnerText, out vehicleId))
                                    {
                                        break;
                                    }
                                    npcEntry.VehicleId = vehicleId;
                                    if (npcEntry.VehicleEntry == null)
                                        break;
                                }
                                break;
                            case "Seat":
                                {
                                    if (!int.TryParse(child.InnerText, out seat))
                                    {
                                        break;
                                    }
                                }
                                break;
                            case "PassengerNPCId":
                                {
                                    if (npcEntry.VehicleEntry == null)
                                        continue;
                                    uint passengerNPCId;
                                    if (uint.TryParse(child.InnerText, out passengerNPCId))
                                    {
                                        if(npcEntry.VehicleEntry.Seats == null)
                                        {
                                            npcEntry.VehicleEntry.Seats = new VehicleSeatEntry[8];
                                        }

                                        if (seat > npcEntry.VehicleEntry.Seats.Length)
                                            Array.Resize(ref npcEntry.VehicleEntry.Seats, seat);

                                        if (npcEntry.VehicleEntry.Seats[seat] == null)
                                        {
                                            var seatEntry = NPCMgr.GetVehicleSeatEntry((uint)seat);
                                            if (seatEntry == null)
                                                break;
                                            npcEntry.VehicleEntry.Seats[seat] = seatEntry;
                                        }

                                        var npc = NPCMgr.GetEntry(passengerNPCId);
                                        if(npc == null)
                                            break;

                                        npcEntry.VehicleEntry.Seats[seat].PassengerNPCId = passengerNPCId;
                                    }

                                }
                                break;

                        }
                    }
                    count++;
                }
            }
            catch (Exception ex)
            {
                Log.WarnException("Error loading addon vehicle data from Content//VehicleData.xml", ex);
            }
            Log.Info("Loaded {0} corrections from VehicleData.xml", count);
        }
    }
}
