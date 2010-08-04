using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Constants;
using WCell.Constants.Achievements;
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
        public static void SendAchievementData(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ALL_ACHIEVEMENT_DATA, chr.Achievements.AchievementsCount * 3 * 4 + 4))
            {
				if (chr.Achievements.AchievementsCount > 0)
				{
					foreach (AchievementRecord completedAchievement in chr.Achievements.m_completedAchievements.Values)
					{
						packet.WriteUInt((uint)completedAchievement.AchievementEntryId);
						packet.WriteDateTime(completedAchievement.CompleteDate);
					}
					packet.Write(0xFFFFFFFFu);
					chr.Client.Send(packet);
				}
            }
        }

		//SMSG_ACHIEVEMENT_EARNED
		public static void SendAchievementEarned(AchievementEntryId achievementEntryId, Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ACHIEVEMENT_EARNED, 30))
			{
				chr.EntityId.WritePacked(packet);
				packet.Write((uint)achievementEntryId);
				packet.WriteDateTime(DateTime.Now);
				packet.Write(0);
				chr.Client.Send(packet);
			}
		}

		public static void SendAchievmentStatus(AchievementCriteriaId achievementCriteriaId, Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CRITERIA_UPDATE, 30))
			{
				packet.WriteUInt((uint)achievementCriteriaId);
				packet.WritePackedUInt64(1);					//	amount
				chr.EntityId.WritePacked(packet);
				packet.Write(0);
				packet.WriteDateTime(DateTime.Now);				// start time?
				packet.Write(0);								// Duration
				packet.Write(0);								// Duration left

				chr.Client.Send(packet);
			}
		}
	}
}