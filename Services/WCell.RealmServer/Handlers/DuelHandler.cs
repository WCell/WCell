using WCell.Constants;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    public static class DuelHandler
    {
        [ClientPacketHandler(RealmServerOpCode.CMSG_DUEL_ACCEPTED)]
        public static void HandleAccept(IRealmClient client, RealmPacketIn packet)
        {
            // var flagId = packet.ReadEntityId();
            if (client.ActiveCharacter.Duel != null)
            {
                client.ActiveCharacter.Duel.Accept(client.ActiveCharacter);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_DUEL_CANCELLED)]
        public static void HandleCancel(IRealmClient client, RealmPacketIn packet)
        {
            if (client.ActiveCharacter.Duel != null)
            {
                client.ActiveCharacter.Duel.Finish(DuelWin.Knockout, client.ActiveCharacter);
            }
        }

        public static void SendCountdown(Character duelist, uint millis)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_COUNTDOWN, 4))
            {
                packet.Write(millis);

                duelist.Send(packet);
            }
        }

        public static void SendRequest(GameObject duelFlag, Character challenger, Character rival)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_REQUESTED))
            {
                packet.Write(duelFlag.EntityId);
                packet.Write(challenger.EntityId);

                rival.Client.Send(packet);
                challenger.Client.Send(packet);
            }
        }

        public static void SendOutOfBounds(Character duelist, int cancelDelayMillis)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_OUTOFBOUNDS, 4))
            {
                packet.Write(cancelDelayMillis);

                duelist.Send(packet);
            }
        }

        public static void SendInBounds(Character duelist)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_INBOUNDS, 4))
            {
                duelist.Send(packet);
            }
        }

        public static void SendComplete(Character duelist1, Character duelist2, bool complete)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_COMPLETE))
            {
                packet.Write((byte)(complete ? 1 : 0));

                if (duelist1 != null) duelist1.Client.Send(packet);
                if (duelist2 != null) duelist2.Client.Send(packet);
            }
        }

        public static void SendWinner(DuelWin win, Unit winner, INamed loser)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DUEL_WINNER))
            {
                packet.Write((byte)win);
                packet.Write(winner.Name);
                packet.Write(loser.Name);

                winner.SendPacketToArea(packet);
            }
        }
    }
}