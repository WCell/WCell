using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;
using WCell.Util.Logging;

namespace WCell.RealmServer.Handlers
{
    public static class TotemHandler
    {
        public static Logger Log = LogManager.GetCurrentClassLogger();

        [ClientPacketHandler(RealmServerOpCode.CMSG_TOTEM_DESTROYED)]
        public static void HandleDestroyTotem(IRealmClient client, RealmPacketIn packet)
        {
            var totemSlot = packet.ReadUInt32();
            Log.Debug("Received CMSG_TOTEM_DESTROYED for Slot {0}", totemSlot);
        }

        public static bool SendTotemCreated(IPacketReceiver client, Spell totemSpell, EntityId totemEntityId)
        {
            var chr = client as Character;
            if (chr == null)
                return false;

            var effect = totemSpell.GetEffect(SpellEffectType.Summon);
            if (effect == null)
                return false;

            var slot = effect.SummonEntry.Slot - 1;

            using(var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TOTEM_CREATED))
            {
                packet.Write(slot);
                packet.Write(totemEntityId);
                packet.Write(totemSpell.GetDuration(chr.SharedReference));
                packet.Write(totemSpell.Id);

                client.Send(packet);
            }
            return true;
        }
    }
}
