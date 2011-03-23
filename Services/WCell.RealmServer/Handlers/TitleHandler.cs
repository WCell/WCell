using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Titles;

namespace WCell.RealmServer.Handlers
{
    public static class TitleHandler
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static void SendTitleEarned(Character character, CharacterTitleEntry titleEntry, bool lost)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TITLE_EARNED, 4 + 4))
            {
                packet.WriteUInt((uint) titleEntry.BitIndex);
                packet.WriteUInt(lost ? 0 : 1);
                character.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_TITLE)]
        public static void HandleChooseTitle(IRealmClient client, RealmPacketIn packet)
        {
            var titleBit = packet.ReadInt32();

            if (titleBit > 0)
            {
                if (!client.ActiveCharacter.HasTitle((TitleBitId)titleBit))
                {
                    return;
                }
            }
            else
            {
                titleBit = 0;
            }


            client.ActiveCharacter.ChosenTitle = (TitleBitId)titleBit;
        }
    }
}
