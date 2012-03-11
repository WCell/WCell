using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs.Vehicles;

namespace WCell.RealmServer.Handlers
{
    public static class VehicleHandler
    {
        [PacketHandler(RealmServerOpCode.CMSG_DISMISS_CONTROLLED_VEHICLE)]
        public static void HandleDismissControlledVehicle(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.Vehicle.ClearAllSeats(true);
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
            var guid = packet.ReadPackedEntityId();
            var oldVehicle = client.ActiveCharacter.Map.GetObject(guid) as Vehicle;
            if (oldVehicle == null)
                return;
            uint clientTime;
            MovementHandler.ReadMovementInfo(packet, client.ActiveCharacter, oldVehicle, out clientTime);

            var newVehicleGuid = packet.ReadPackedEntityId();
            var newVehicle = client.ActiveCharacter.Map.GetObject(newVehicleGuid) as Vehicle;
            if (newVehicle == null)
                return;

            var passenger = client.ActiveCharacter;
            var oldSeat = passenger.m_vehicleSeat;

            //shouldnt need this, but fall back just in case
            if (oldSeat == null)
                oldVehicle.FindSeatOccupiedBy(passenger);

            //uh oh!
            if (oldSeat == null)
                return;

            var seatId = packet.ReadByte();
            var newSeat = newVehicle.Seats[seatId];
            //something went wrong
            if (newSeat == null)
                return;

            //cheater?!
            if (newSeat.Passenger != null)
                return;

            oldSeat.ClearSeat();
            newSeat.Enter(passenger);
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