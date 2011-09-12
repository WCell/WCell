using System;
using System.IO;
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
            	if (npcEntry == null) continue;

            	npcEntry.VehicleId = data.VehicleId;
            	if (data.PassengerNPCId != 0)
            	{
            		if (data.Seat > npcEntry.VehicleEntry.Seats.Length)
            			Array.Resize(ref npcEntry.VehicleEntry.Seats, (int) data.Seat);

            		if (npcEntry.VehicleEntry.Seats[data.Seat] != null)
            		{
            			var npc = NPCMgr.GetEntry(data.PassengerNPCId);
            			if (npc != null)
            			{
            				npcEntry.VehicleEntry.Seats[data.Seat].PassengerNPCId = data.PassengerNPCId;
            			}
            		}
            	}

            	if (data.VehicleAimAdjustment != 0)
            	{
            		npcEntry.VehicleAimAdjustment = data.VehicleAimAdjustment;
            	}

            	npcEntry.VehicleEntry.IsMinion = data.IsMinion;
            	count++;
            }

            Log.Info("Loaded {0} corrections from VehicleData.xml", count);
        }

        [Initialization]
        [DependentInitialization(typeof(NPCMgr))]
        public static void FixVehicleSpellData()
        {
            #region Frosthound
            var npcEntry = NPCMgr.GetEntry(NPCId.Frosthound);
			if (npcEntry != null)
			{
				var spell = SpellHandler.Get(SpellId.IceSlick);
				if (spell != null)
					npcEntry.AddSpell(spell);

				spell = SpellHandler.Get(SpellId.CastNet_2);
				if (spell != null)
					npcEntry.AddSpell(spell);
			}

        	#endregion

			#region Wintergrasp Siege Engine
			npcEntry = NPCMgr.GetEntry(NPCId.WintergraspSiegeEngine);
			var ramSpell = SpellHandler.Get(SpellId.Ram_2);
			if (npcEntry != null && ramSpell != null)
			{
				npcEntry.AddSpell(ramSpell);
			}

			npcEntry = NPCMgr.GetEntry(NPCId.WintergraspSiegeEngine_2);
			if (npcEntry != null && ramSpell != null)
			{
				npcEntry.AddSpell(ramSpell);
			}

			npcEntry = NPCMgr.GetEntry(NPCId.WintergraspSiegeTurret);
			var cannonSpell = SpellHandler.Get(SpellId.FireCannon_10); 
			if (npcEntry != null && cannonSpell != null)
			{
				npcEntry.AddSpell(cannonSpell);
			}

			npcEntry = NPCMgr.GetEntry(NPCId.WintergraspSiegeTurret_2);
			if (npcEntry != null && cannonSpell != null)
			{
				npcEntry.AddSpell(cannonSpell);
			}

        	#endregion
		}
    }
}

