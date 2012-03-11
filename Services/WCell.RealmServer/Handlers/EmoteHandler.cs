using WCell.Constants;
using WCell.Constants.Chat;
using WCell.Constants.Misc;
using WCell.Core.Network;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
    /// <summary>
    /// Handler class for emote-related packets.
    /// </summary>
    public static class EmoteHandler
    {
        [ClientPacketHandler(RealmServerOpCode.CMSG_TEXT_EMOTE)]
        public static void HandleTextEmote(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (!chr.CanMove || !chr.CanInteract) return;

            var emote = (TextEmote)packet.ReadUInt32();
            packet.SkipBytes(4);
            var targetId = packet.ReadEntityId();
            var target = chr.Map.GetObject(targetId) as INamed;
            if (target != null)
            {
                SendTextEmote(chr, emote, target);
            }

            EmoteType animation;
            EmoteDBC.EmoteRelationReader.Entries.TryGetValue((int)emote, out animation);

            switch (animation)
            {
                //The client seems to handle these on its own.
                case EmoteType.StateSleep:
                case EmoteType.StateSit:
                case EmoteType.StateKneel:
                case EmoteType.StateDance:
                    chr.EmoteState = animation;
                    break;
                default:
                    chr.Emote(animation);
                    break;
            }

            //todo: Achievement and scripting hooks/events.
        }

        // Client doesn't seem to be sending this
        //[ClientPacketHandler(RealmServerOpCode.CMSG_EMOTE)]
        //public static void HandleEmote(IRealmClient client, RealmPacketIn packet)
        //{
        //    var emote = (EmoteType)packet.ReadUInt32();

        //    if (emote != EmoteType.None)
        //    {
        //        var chr = client.ActiveCharacter;
        //        if (chr.CanMove && chr.CanInteract)
        //        {
        //            SendEmote(chr, emote);
        //        }
        //    }
        //}

        public static void SendTextEmote(WorldObject obj, TextEmote emote, INamed target)
        {
            var len = (target == null) ? 20 : target.Name.Length + 21;

            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TEXT_EMOTE, len))
            {
                packet.Write(obj.EntityId);
                packet.WriteUInt((uint)emote);
                packet.WriteInt(-1);
                packet.WriteUIntPascalString(target != null ? target.Name : "");

                obj.SendPacketToArea(packet, true);
            }
        }

        public static void SendEmote(WorldObject obj, EmoteType emote)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_EMOTE, 12))
            {
                packet.WriteUInt((uint)emote);
                packet.Write(obj.EntityId);

                obj.SendPacketToArea(packet, true);
            }
        }
    }
}