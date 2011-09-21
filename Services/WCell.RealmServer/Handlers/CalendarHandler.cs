namespace WCell.RealmServer.Handlers{}
/*{
    public class CalendarHandler
    {
        // Todo: Implement Calendar
        #region IN

        [PacketHandler(RealmServerOpCode.CMSG_CALENDAR_GET_NUM_PENDING)]
        public static void HandleCalendarGetNumPending(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;

            SendHasPendingInvites(chr, false);
        }

        [PacketHandler(RealmServerOpCode.CMSG_CALENDAR_GET_CALENDAR)]
        public static void HandleCalendarGetCalendar(IRealmClient client, RealmPacketIn packet)
        {
            //Console.WriteLine("RECEIVED CMSG_CALENDAR_GET_CALENDAR");
            //SendCalendar(client);
        }

        [PacketHandler(RealmServerOpCode.CMSG_CALENDAR_ADD_EVENT)]
        public static void HandleCalendarAddEvent(IRealmClient client, RealmPacketIn packet)
        {
            //Console.WriteLine("RECEIVED CMSG_CALENDAR_ADD_EVENT");
            //Console.WriteLine(packet);
            //Console.WriteLine(packet.ToHexDump());
        }

        #endregion


        #region OUT

        public static void SendHasPendingInvites(IPacketReceiver client, bool hasPendingInvites)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CALENDAR_SEND_NUM_PENDING))
            {
                packet.Write(hasPendingInvites ? 1u : 0u); // 0 = no pending invites, 1 = some pending invites

                client.Send(packet);
            }
        }

        public static void SendCalendar (IPacketReceiver client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CALENDAR_SEND_CALENDAR))
            {
                var realmClient= (RealmClient)client;

                packet.write(realmclient.ActiveCharacter.EventInvites.Count);
                foreach (var eventInvite in realmclient.activecharacter.EventInvites)
                {
                    packet.Write(eventInvite.Id);
                    packet.Write(eventInvite.Invitee);
                    packet.Write(eventInvite.Byte1);
                    packet.Write(eventInvite.Byte2);
                    packet.Write(eventInvite.Byte3);
                    packet.Write(eventInvite.PackedGuid);
                }
                packet.Write(realmClient.ActiveCharacter.Events.Count);
                foreach (var evnt in realmClient.ActiveCharacter.Events)
                {
                    packet.Write((ulong)evnt.Id);
                    packet.Write(evnt.Name);
                    packet.Write((uint)0); // unknown1
                    packet.Write((uint)0); // unknown2
                    packet.Write((uint)evnt.Flags);
                    packet.Write((uint)0); // unknown3
                    packet.Write(evnt.PackedGuid); // player id or date or invitee id
                }
                packet.Write(Utility.GetSystemTimeLong());// probably wrong
                packet.Write((uint)Utility.GetDateTimeToGameTime(new DateTime()));

                packet.Write(realmClient.ActiveCharacter.Instances.Instances.Count);
                foreach (var instance in realmClient.ActiveCharacter.Instances)
                {
                    packet.Write((uint)instance.Id);
                    packet.Write((uint)instance.DungeonMode);
                    packet.Write(Utility.GetDateTimeToGameTime(instance.ExpiryTime));
                    packet.Write(instance.InstanceId);
                }

                packet.Write(realmClient.ActiveCharacter.InstancesToReset.Count); //or raids
                foreach (var raid in instancestoreset)
                {
                    packet.Write(raid.Id);
                    packet.Write(raid.resettime);
                    packet.Write((uint)0); // unknown
                }
                packet.Write(count5);
                foreach (var count5 in count5)
                {
                    packet.Write((uint)0); //unknown1
                    packet.Write((uint)0); //unknown2
                    packet.Write((uint)0); //unknown3
                    packet.Write((uint)0); //unknown4
                    packet.Write((uint)0); //unknown5
                    for (int i = 0; i < 26; i++)
                    {
                        packet.Write(0); //unknown
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        packet.Write(0); //unknown
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        packet.Write(0); //unknown
                    }
                    packet.Write("unknown string");
                }
            }
        }

        public static void SendCalendarCommandResult(IPacketReceiver client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CALENDAR_COMMAND_RESULT))
            {
                packet.Write((UInt32)0);
                packet.Write((UInt32)0);
                packet.Write((UInt16)0);

                client.Send(packet);
            }
        }

    }
}

        #endregion
/*
        void WorldSession::HandleCalendarGetCalendar(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_GET_CALENDAR");
    recv_data.hexlike();
 
    time_t cur_time = time(NULL);
 
    WorldPacket data(SMSG_CALENDAR_SEND_CALENDAR,4+4*0+4+4*0+4+4);
 
    // TODO: calendar invite event output
    data << (uint32) 0; //invite node count
    // TODO: calendar event output
    data << (uint32) 0; //event count
 
    data << (uint32) 0; //wtf??
    data << (uint32) secsToTimeBitFields(cur_time); // current time
 
    uint32 counter = 0;
    size_t p_counter = data.wpos();
    data << uint32(counter); // instance save count
 
    for(int i = 0; i < TOTAL_DIFFICULTIES; ++i)
    {
        for (Player::BoundInstancesMap::const_iterator itr = _player->m_boundInstances[i].begin(); itr != _player->m_boundInstances[i].end(); ++itr)
        {
            if(itr->second.perm)
            {
                InstanceSave *save = itr->second.save;
                data << uint32(save->GetMapId());
                data << uint32(save->GetDifficulty());
                data << uint32(save->GetResetTime() - cur_time);
                data << uint64(save->GetInstanceId()); // instance save id as unique instance copy id
                ++counter;
            }
        }
    }
    data.put<uint32>(p_counter,counter);
 
    data << (uint32) 1135753200; //wtf?? (28.12.2005 12:00)
    data << (uint32) 0; // unk counter 4
    data << (uint32) 0; // unk counter 5
    //sLog.outDebug("Sending calendar");
    //data.hexlike();
    SendPacket(&data);
}
 
void WorldSession::HandleCalendarGetEvent(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_GET_EVENT");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarGuildFilter(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_GUILD_FILTER");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarArenaTeam(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_ARENA_TEAM");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarAddEvent(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_ADD_EVENT");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarUpdateEvent(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_UPDATE_EVENT");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarRemoveEvent(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_REMOVE_EVENT");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarCopyEvent(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_COPY_EVENT");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarEventInvite(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_EVENT_INVITE");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarEventRsvp(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_EVENT_RSVP");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarEventRemoveInvite(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_EVENT_REMOVE_INVITE");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarEventStatus(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_EVENT_STATUS");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarEventModeratorStatus(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_EVENT_MODERATOR_STATUS");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarComplain(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_COMPLAIN");
    recv_data.hexlike();
}
 
void WorldSession::HandleCalendarGetNumPending(WorldPacket &recv_data)
{
    sLog.outDebug("WORLD: CMSG_CALENDAR_GET_NUM_PENDING");
    recv_data.hexlike();
 
    WorldPacket data(SMSG_CALENDAR_SEND_NUM_PENDING, 4);
    data << uint32(0); // 0 - no pending invites, 1 - some pending invites
    SendPacket(&data);
}
    }
}*/