using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants;
using WCell.Core.Network;
using WCell.PacketAnalysis;
using WCell.RealmServer.Achievement;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// Stub class for containing achievement packets
	/// </summary>
	public static class AchievementHandler
	{
        private static Logger log = LogManager.GetCurrentClassLogger();

        //SMSG_ALL_ACHIEVEMENT_DATA
       /* public static void SendAchievementData(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ALL_ACHIEVEMENT_DATA, ))
            {
                packet.Write();

                chr.Client.Send(packet);
            }
            throw new NotImplementedException();
        }*/
	}
}