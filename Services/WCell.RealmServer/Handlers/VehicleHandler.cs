using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs.Vehicles;

namespace WCell.RealmServer.Handlers {
    public static class VehicleHandler {
        [PacketHandler(RealmServerOpCode.CMSG_DISMISS_CONTROLLED_VEHICLE)]
        public static void HandleDismissControlledVehicle(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.Vehicle.ClearAllSeats();
            MovementHandler.HandleMovement(client, packet);
        }

        [PacketHandler(RealmServerOpCode.CMSG_REQUEST_VEHICLE_EXIT)]
        public static void HandleRequestVehicleExit(IRealmClient client, RealmPacketIn packet) 
        {
        	var chr = client.ActiveCharacter;
			var vehicle = chr.Vehicle;
			if (vehicle != null)
			{
				chr.VehicleSeat.ClearSeat();
			}
        }

        [PacketHandler(RealmServerOpCode.CMSG_REQUEST_VEHICLE_NEXT_SEAT)]
        public static void HandleRequestVehicleNextSeat(IRealmClient client, RealmPacketIn packet) 
        {
            // empty
        }

        [PacketHandler(RealmServerOpCode.CMSG_REQUEST_VEHICLE_SWITCH_SEAT)]
        public static void HandleRequestVehicleSwitchSeat(IRealmClient client, RealmPacketIn packet) 
        {
            var guid = packet.ReadPackedEntityId();
            byte seat = packet.ReadByte();
        }

        [PacketHandler(RealmServerOpCode.CMSG_CHANGE_SEATS_ON_CONTROLLED_VEHICLE)]
        public static void HandleChangeSeatsOnControlledVehicle(IRealmClient client, RealmPacketIn packet)
        {
            
        }

		public static void SendBreakTarget(IPacketReceiver rcvr, IEntity target)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BREAK_TARGET, 8))
			{
				packet.Write(target.EntityId);
				rcvr.Send(packet);
			}
		}

        public static void Send_SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ON_CANCEL_EXPECTED_RIDE_VEHICLE_AURA, 0))
            {
				chr.Send(packet);
            }
        }

    }
}