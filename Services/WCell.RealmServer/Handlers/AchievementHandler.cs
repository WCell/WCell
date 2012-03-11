using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Core.Network;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
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
        public static void SendAchievementEarned(uint achievementEntryId, Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ACHIEVEMENT_EARNED, 8 + 4 + 4))
            {
                chr.EntityId.WritePacked(packet);
                packet.WriteUInt(achievementEntryId);
                packet.WriteDateTime(DateTime.Now);
                packet.WriteUInt(0);
                chr.SendPacketToArea(packet, true);
            }
        }

        //SMSG_SERVER_FIRST_ACHIEVEMENT
        public static void SendServerFirstAchievement(uint achievementEntryId, Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SERVER_FIRST_ACHIEVEMENT, chr.Name.Length + 1 + 8 + 4 + 4))
            {
                packet.WriteCString(chr.Name);
                packet.Write(chr.EntityId);
                packet.WriteUInt(achievementEntryId);
                packet.WriteUInt(0);
                World.Broadcast(packet);
            }
        }

        public static RealmPacketOut CreateAchievementEarnedToGuild(uint achievementEntryId, Character chr)
        {
            // Must be a better way to do this.
            const string msg = "|Hplayer:$N|h[$N]|h has earned the achievement $a!";
            var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MESSAGECHAT);
            packet.WriteByte((byte)ChatMsgType.Achievment);
            packet.WriteUInt((uint)ChatLanguage.Universal);
            packet.Write(chr.EntityId);
            packet.WriteUInt(5);
            packet.Write(chr.EntityId);
            packet.WriteUIntPascalString(msg);
            packet.WriteByte(0);
            packet.WriteUInt(achievementEntryId);
            return packet;
        }

        // SMSG_CRITERIA_UPDATE
        public static void SendAchievmentStatus(AchievementProgressRecord achievementProgressRecord, Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CRITERIA_UPDATE, 4 * 5 + 8 * 2))
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

        // CMSG_QUERY_INSPECT_ACHIEVEMENTS
        [PacketHandler(RealmServerOpCode.CMSG_QUERY_INSPECT_ACHIEVEMENTS)]
        public static void HandleInspectAchievements(IRealmClient client, RealmPacketIn packet)
        {
            var targetGuid = packet.ReadPackedEntityId();
            var targetChr = World.GetCharacter(targetGuid.Low);
            if (targetChr != null && targetChr.IsInContext)
            {
                SendRespondInspectAchievements(targetChr);
            }
        }

        // SMSG_RESPOND_INSPECT_ACHIEVEMENTS
        public static void SendRespondInspectAchievements(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESPOND_INSPECT_ACHIEVEMENTS, chr.Achievements.AchievementsCount * 2 * 4 + 4 + 8))
            {
                chr.EntityId.WritePacked(packet);
                CreateAchievementData(packet, chr);
                chr.Client.Send(packet);
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