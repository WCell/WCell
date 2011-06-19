using System;
using System.IO;
using System.Xml;
using NLog;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.Addons.Default.Vehicles
{
    public static class VehicleCorrections
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void FixVehicleData()
        {
            var count = 0;

            var path = Path.Combine(RealmServerConfiguration.ContentDir, "VehicleData.xml");

            Log.Info("Loading VehicleData.xml");

            var dataCollection = XmlFile<VehicleDataCollection>.Load(path);

            foreach (var data in dataCollection.VehicleDataList)
            {
                var npcEntry = NPCMgr.GetEntry(data.NPCId);
                if (npcEntry != null)
                {
                    npcEntry.VehicleId = data.VehicleId;
                    if (data.PassengerNPCId != 0)
                    {
                        if (npcEntry.VehicleEntry.Seats == null)
                        {
                            npcEntry.VehicleEntry.Seats = new VehicleSeatEntry[8];
                        }

                        if (data.Seat > npcEntry.VehicleEntry.Seats.Length)
                            Array.Resize(ref npcEntry.VehicleEntry.Seats, (int) data.Seat);

                        if (npcEntry.VehicleEntry.Seats[data.Seat] == null)
                        {
                            var seatEntry = NPCMgr.GetVehicleSeatEntry(data.Seat);
                            if (seatEntry != null)
                                npcEntry.VehicleEntry.Seats[data.Seat] = seatEntry;
                        }

                        var npc = NPCMgr.GetEntry(data.PassengerNPCId);
                        if (npc != null && npcEntry.VehicleEntry.Seats[data.Seat] != null)
                        {
                            npcEntry.VehicleEntry.Seats[data.Seat].PassengerNPCId = data.PassengerNPCId;
                        }
                    }

                    if (data.VehicleAimAdjustment != 0)
                    {
                        npcEntry.VehicleAimAdjustment = data.VehicleAimAdjustment;
                    }

                    npcEntry.VehicleEntry.IsMinion = data.IsMinion;
                    count++;
                }
            }

            Log.Info("Loaded {0} corrections from VehicleData.xml", count);
        }

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void FixVehicleSpellData()
        {
            var npcEntry = NPCMgr.GetEntry(NPCId.Frosthound);
            if (npcEntry == null) return;

            var spell = SpellHandler.Get(SpellId.IceSlick);
            if (spell != null)
                npcEntry.AddSpell(spell);

            spell = SpellHandler.Get(SpellId.CastNet_2);
            if (spell != null)
                npcEntry.AddSpell(spell);
        }
    }
}

