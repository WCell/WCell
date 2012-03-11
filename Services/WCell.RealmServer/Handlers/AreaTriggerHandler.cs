using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class AreaTriggerHandler
    {
        public static void SendAreaTriggerMessage(IPacketReceiver client, string msg)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AREA_TRIGGER_MESSAGE, (msg.Length * 2) + 4))
            {
                packet.WriteUIntPascalString(msg);
                packet.Write((byte)0);

                client.Send(packet);
            }
        }

        [PacketHandler(RealmServerOpCode.CMSG_AREATRIGGER)]
        public static void HandleAreaTrigger(IRealmClient client, RealmPacketIn packet)
        {
            var id = packet.ReadUInt32();
            var chr = client.ActiveCharacter;

            if (chr.IsAlive)
            {
                var trigger = AreaTriggerMgr.GetTrigger(id);
                if (trigger != null)
                {
                    trigger.Trigger(chr);
                }
            }
        }
    }
}