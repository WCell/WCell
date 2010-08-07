using System;
using System.Collections.Generic;
using System.IO;
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
//			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ALL_ACHIEVEMENT_DATA, 4 * 2 + chr.Achievements.AchievementsCount * 4 * 2 + chr.Achievements.m_achivement_progress .Count* 7 * 4))
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ALL_ACHIEVEMENT_DATA, chr.Achievements.AchievementsCount * 2 * 4 + 4))
            {
				if (chr.Achievements.AchievementsCount > 0)
				{
					CreateAchievementData(packet, chr);
					chr.Client.Send(packet);
				}
            }
        }

		//SMSG_ACHIEVEMENT_EARNED
		public static void SendAchievementEarned(AchievementEntryId achievementEntryId, Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ACHIEVEMENT_EARNED, 8+4+4))
			{
				chr.EntityId.WritePacked(packet);
				packet.WriteUInt((uint)achievementEntryId);
				packet.WriteDateTime(DateTime.Now);
				packet.WriteUInt(0);
				chr.SendPacketToArea(packet, true);
			}
		}

		// SMSG_CRITERIA_UPDATE
		public static void SendAchievmentStatus(AchievementProgressRecord achievementProgressRecord, Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CRITERIA_UPDATE, 4*5+8*2))
			{
				packet.WriteUInt((uint)achievementProgressRecord.AchievementCriteriaId);
				packet.WritePackedUInt64(achievementProgressRecord.Counter);					//	amount
				chr.EntityId.WritePacked(packet);
				packet.Write(0);
				packet.WriteDateTime(DateTime.Now);				// start time?
				packet.Write(0);								// Duration
				packet.Write(0);								// Duration left

				chr.Client.Send(packet);
			}
		}

		// SMSG_RESPOND_INSPECT_ACHIEVEMENTS
		public static void SendRespondInspectAchievements(Character inspectedChar, Character inspectingChar)
		{
			//using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESPOND_INSPECT_ACHIEVEMENTS, 4 * 2 + inspectedChar.Achievements.AchievementsCount * 4 * 2 + inspectedChar.Achievements.m_achivement_progress.Count * 7 * 4))
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESPOND_INSPECT_ACHIEVEMENTS, 8 + inspectedChar.Achievements.AchievementsCount * 2 * 4 + 4))
			{
				inspectingChar.EntityId.WritePacked(packet);
				CreateAchievementData(packet, inspectedChar);
				inspectingChar.Client.Send(packet);
			}
		}

		public static void CreateAchievementData(RealmPacketOut packet, Character chr)
		{
			foreach (AchievementRecord completedAchievement in chr.Achievements.m_completedAchievements.Values)
			{
				packet.WriteUInt((uint)completedAchievement.AchievementEntryId);
				packet.WriteDateTime(completedAchievement.CompleteDate);
			}
			packet.WriteInt(0xFFFFFFFFu);

			/*foreach (AchievementProgressRecord achievementProgressRecord in chr.Achievements.m_achivement_progress.Values)
			{

				packet.WriteUInt((uint)achievementProgressRecord.AchievementCriteriaId);
				packet.WritePackedUInt64(achievementProgressRecord.Counter);					//	amount
				chr.EntityId.WritePacked(packet);
				packet.Write(0);
				packet.WriteDateTime(DateTime.Now);				// start time?
				packet.Write(0);								// Duration
				packet.Write(0);

			}
			packet.Write(0xFFFFFFFFu);*/
		}
	}
}