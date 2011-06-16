using System.Collections.Generic;
using WCell.Core.Network;
using WCell.Constants;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Constants.NPCs;

namespace WCell.RealmServer.Handlers
{
    /*
     -> CMSG_BATTLEMASTER_HELLO (or through Gossip)
     <- SMSG_BATTLEFIELD_LIST
     -> CMSG_BATTLEMASTER_JOIN
     <- SMSG_BATTLEFIELD_STATUS
     ...
     <- SMSG_BATTLEFIELD_STATUS (ready)
     -> CMSG_BATTLEFIELD_PORT
     <- SMSG_BATTLEFIELD_PORT_DENIED (may not port)
	 
     -> MSG_BATTLEGROUND_PLAYER_POSITIONS
     <- MSG_BATTLEGROUND_PLAYER_POSITIONS
	 
     -> CMSG_BATTLEFIELD_STATUS (on login)
     <- SMSG_BATTLEFIELD_STATUS
	 
     -> CMSG_BATTLEFIELD_LIST
     <- SMSG_BATTLEFIELD_LIST
	 
     -> CMSG_LEAVE_BATTLEFIELD
	 
     SMSG_BATTLEGROUND_PLAYER_JOINED
     SMSG_BATTLEGROUND_PLAYER_LEFT
	 
     MSG_PVP_LOG_DATA
	 
     SMSG_GROUP_JOINED_BATTLEGROUND
    */
    public static class BattlegroundHandler
    {
        [PacketHandler(RealmServerOpCode.CMSG_BATTLEMASTER_HELLO)]
        public static void HandleBattlemasterHello(IRealmClient client, RealmPacketIn packet)
        {
            var bmId = packet.ReadEntityId();

            var chr = client.ActiveCharacter;
            var bm = chr.Map.GetObject(bmId) as NPC;

            if (bm != null &&
                bm.NPCFlags.HasFlag(NPCFlags.BattleMaster)
                // && bm.CanInteractWith(chr)
                )
            {
                bm.TalkToBattlemaster(chr);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_BATTLEMASTER_JOIN)]
        public static void HandleBattlemasterJoin(IRealmClient client, RealmPacketIn packet)
        {
            var battlemasterGuid = packet.ReadEntityId();
			var bgId = (BattlegroundId)packet.ReadUInt32();
            var instanceId = packet.ReadUInt32();
            var asGroup = packet.ReadBoolean();

			// check to make sure bg id was valid
			if (bgId <= BattlegroundId.None || bgId >= BattlegroundId.End)
				return;

            var chr = client.ActiveCharacter;

			BattlegroundMgr.EnqueuePlayers(chr, bgId, instanceId, asGroup);
        }

		[ClientPacketHandler(RealmServerOpCode.CMSG_LEAVE_BATTLEFIELD)]
		public static void HandleBattlefieldLeave(IRealmClient client, RealmPacketIn packet)
		{
			// Start 64-bit BGID
			var unk1 = packet.ReadInt16();
			var bgId = (BattlegroundId)packet.ReadUInt32();
			var unk2 = packet.ReadInt16();
			// End BGID

			var bgs = client.ActiveCharacter.Battlegrounds;
			// check to make sure player is in a bg and one of the given type
			if(!bgs.IsParticipating(bgId))
				return;

			// port em out
			bgs.TeleportBack();
		}

    	[ClientPacketHandler(RealmServerOpCode.CMSG_BATTLEMASTER_JOIN_ARENA)]
        public static void HandleBattlemasterJoinArena(IRealmClient client, RealmPacketIn packet)
        {
            var battlemasterGuid = packet.ReadEntityId();

            var arenaType = (ArenaType)packet.ReadByte();
            bool joinAsGroup = packet.ReadBoolean();
            bool ratedMatch = packet.ReadBoolean();
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_BATTLEFIELD_PORT)]
        public static void HandlePort(IRealmClient client, RealmPacketIn packet)
        {
            // Start 64-bit BGID
            var unk1 = packet.ReadInt16();
            var bgId = (BattlegroundId)packet.ReadUInt32();
            var unk2 = packet.ReadInt16();
            // End BGID

            var join = packet.ReadBoolean();	// whether to join or cancel

            var chr = client.ActiveCharacter;

            if (join)
            {
                var invitation = chr.Battlegrounds.Invitation;
                if (invitation != null && invitation.Team.Battleground != null)
                {
                    var bg = invitation.Team.Battleground;
                    //if (bg.GetTeam(chr).IsFull)
                    //{
                    //    // already full
                    //    chr.Battlegrounds.Cancel(invitation);
                    //    chr.SendBattlegroundError(BattlegroundJoinError.);
                    //}
                    if (bg.Template.Id == bgId)
                    {
                        // valid request
                        bg.TeleportInside(chr);
                    }
                }
            }
            else
            {
                chr.Battlegrounds.CancelRelation(bgId);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_BATTLEGROUND_PLAYER_POSITIONS)]
        public static void HandlePlayerPositionQuery(IRealmClient client, RealmPacketIn packet)
        {
            // empty
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_BATTLEFIELD_STATUS)]
        public static void HandleBattleFieldStatusRequest(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (!chr.Battlegrounds.IsEnqueuedForBattleground) return;

            var relations = chr.Battlegrounds.Relations;
        	var count = chr.Battlegrounds.Relations.Length;
            for (var i = 0; i < count; i++)
            {
                var relation = relations[i];
				if (relation != null)
				{
					if (relation.IsEnqueued)
					{
						SendStatusEnqueued(chr, i, relation, relation.Queue.ParentQueue);
					}
					else if (chr.Map is Battleground && 
						relation.BattlegroundId == ((Battleground)chr.Map).Template.Id)
					{
						SendStatusActive(chr, (Battleground)chr.Map, i);
					}
				}
            }
        }

        #region List
        [ClientPacketHandler(RealmServerOpCode.CMSG_BATTLEFIELD_LIST)]
		public static void HandleBattlefieldList(IRealmClient client, RealmPacketIn packet)
		{
        	var bgId = (BattlegroundId)packet.ReadByte();
			
			var templ = BattlegroundMgr.GetTemplate(bgId);
        	var chr = client.ActiveCharacter;
			if (templ != null)
			{
				var queue = templ.GetQueue(chr.Level);
				if (queue != null)
				{
					SendBattlefieldList(chr, queue);
				}
			}
		}

        public static void SendBattlefieldList(Character chr, GlobalBattlegroundQueue queue)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_LIST))
            {
                var fromGUI = true;

                packet.Write((long)0);
                packet.Write(fromGUI);							// since 3.1.1
				packet.Write((uint)queue.Template.Id);
				packet.Write((byte)queue.BracketId);			// BracketId
				packet.Write((byte)0);							// since 3.3

                var pos = packet.Position;
                packet.Position += 4;

                var count = 0;
                for (var i = 0; i < queue.Instances.Count; i++)
                {
                    var bg = queue.Instances[i];
                    if (chr.Role.IsStaff || bg.CanEnter(chr))
                    {
                        packet.Write(bg.InstanceId);
                        count++;
                    }
                }

                packet.Position = pos;
                packet.Write(count);

                chr.Send(packet);
            }
        }
        #endregion

        #region Status

        public static void ClearStatus(IPacketReceiver client, int queueIndex)
        {
            // this packet makes the client clear out data for this queue
            var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS1);

            packet.Write(queueIndex);

            client.Send(packet);
            packet.Close();
        }

        public static void SendStatusEnqueued(Character chr,
            int index,
            BattlegroundRelation relation,
            BattlegroundQueue queue)
        {
            var status = BattlegroundStatus.Enqueued;

            var packet = new RealmPacketOut(RealmServerOpCode.SMSG_JOINED_BATTLEGROUND_QUEUE);

            var bgId = queue.Template.Id;

            packet.Write((byte) 0); // since 3.3
            packet.Write((byte) 0); // since 3.3
            packet.Write(queue.AverageWaitTime); // avg wait time for queue, in ms
            packet.Write(index);
            packet.Write(queue.InstanceId); // instance id
            packet.Write((byte)0); // since 3.3

            // 64-bit guid start
            packet.Write((byte)ArenaType.None);
            packet.Write((byte)1); // affects level range calculation?
            packet.Write((uint)bgId);
            packet.Write((ushort)8080);
            // 64-bit guid stop

            packet.Write(false); // bool isRatedMatch
            packet.Write((int) relation.QueueTime.TotalMilliseconds); // time in the queue, also in ms

            chr.Send(packet);
            packet.Close();
        }

        /// <summary>
        /// Make sure that <see cref="BattlegroundInfo.Invitation"/> is set.
        /// </summary>
        public static void SendStatusInvited(Character chr)
        {
            SendStatusInvited(chr, BattlegroundMgr.InvitationTimeoutMillis);
        }

        public static void SendStatusInvited(Character chr, int inviteTimeout)
        {
            var status = BattlegroundStatus.Preparing;
            var invite = chr.Battlegrounds.Invitation;

            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS3))
            {
                var bg = invite.Team.Battleground;
                var bgId = bg.Template.Id;

                packet.Write((byte)0);				// since 3.3
                packet.Write((byte)0);				// since 3.3
                packet.Write(bg.InstanceId); // instance id

                // 64-bit guid start
                packet.Write((byte)ArenaType.None);
                packet.Write((byte)1); // affects level range calculation?
                packet.Write((uint)bgId);
                packet.Write((ushort)8080);
                // 64-bit guid stop

                packet.Write(invite.QueueIndex);
                packet.Write((byte)chr.FactionGroup.GetBattlegroundSide()); // bool isRatedMatch
                packet.Write(inviteTimeout);

				packet.Write((int)bg.Id);

                packet.Write((byte)0); //Max Level
                

                chr.Send(packet);
            }
        }

        public static void SendStatusActive(Character chr, Battleground bg, int queueIndex)
        {
            var status = BattlegroundStatus.Active;
            var side = chr.FactionGroup.GetBattlegroundSide();

            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEFIELD_STATUS2))
            {
				var bgId = bg.Template.Id;

                packet.Write((byte)0); // bool isRatedMatch
                // start time, in ms. clientGetTickCount - this = instance runtime
                packet.Write(bg.RuntimeMillis);
                packet.Write(queueIndex);
                packet.Write((int)bg.Id);

                // 64-bit guid start
                packet.Write((byte)ArenaType.None);
                packet.Write((byte)1); // affects level range calculation?
                packet.Write((uint)bgId);
                packet.Write((ushort)8080);
                // 64-bit guid stop

                // the number of milliseconds before the Battlefield will close after a battle is finished.
                // This is 0 before the battle is finished
                packet.Write(bg.RemainingShutdownDelay);
                packet.Write((byte)0);				// since 3.3
                packet.Write((byte)0);				// since 3.3
                packet.Write(bg.InstanceId); // instance id
                packet.Write((byte)side); // arena faction - 0 or 1
                
                chr.Send(packet);
            }
        }
        #endregion

        #region PvP Data
        [ClientPacketHandler(RealmServerOpCode.MSG_PVP_LOG_DATA)]
        public static void PvPLogDataRequest(IRealmClient client, RealmPacketIn packet)
        {
        	var chr = client.ActiveCharacter;
			if (!chr.IsInBattleground)
			{
				return;
			}

        	var team = chr.Battlegrounds.Team;
			if (team != null)
			{
				SendPvpData(client, team.Side, team.Battleground);
			}
        }

        public static void SendPvpData(IPacketReceiver reciever, BattlegroundSide side, Battleground bg)
        {
            bg.EnsureContext();

            using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_PVP_LOG_DATA, 10 + bg.PlayerCount * 40))
            {
            	var winner = bg.Winner;

                packet.Write(bg.IsArena);
                if (bg.IsArena)
                {
                    // TODO: Arena
                    for (var i = 0; i < 2; i++)
                    {
                        packet.Write(0); // old rating
                        packet.Write(3999); // new rating (3000 + diff)
                        packet.Write(0); // matchmaking value (lvl group/rank?)
                    }

                    packet.WriteCString(string.Empty); // arena team names
                    packet.WriteCString(string.Empty);
                }

                var isFinished = bg.Winner != null;
                packet.Write(isFinished);
                if (isFinished)
                {
                    packet.Write((byte)bg.Winner.Side);
                }

				var chrs = bg.Characters;
                List<BattlegroundStats> listStats = new List<BattlegroundStats>(chrs.Count);
                chrs.ForEach(chr => listStats.Add(chr.Battlegrounds.Stats));
                packet.Write(listStats.Count);

                for (var i = 0; i < listStats.Count; i++)
                {
                	var chr = chrs[i];
					if (!chr.IsInBattleground)
					{
						continue;
					}

                	var stats = chr.Battlegrounds.Stats;

                    packet.Write(chr.EntityId); // player guid
					packet.Write(stats.KillingBlows);

                    if (bg.IsArena)
					{
                        packet.Write(winner != null && chr.Battlegrounds.Team == winner); // is on the winning team
                    }
                    else
                    {
						packet.Write(stats.HonorableKills);
						packet.Write(stats.Deaths);
						packet.Write(stats.BonusHonor);
                    }

					packet.Write(stats.TotalDamage);
					packet.Write(stats.TotalHealing);

                    packet.Write(stats.SpecialStatCount);

                	stats.WriteSpecialStats(packet);
                }

                reciever.Send(packet);
            }
        }
        #endregion

        public static void SendPlayerJoined(IPacketReceiver rcv, Character joiningCharacter)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEGROUND_PLAYER_JOINED, 8))
            {
                packet.Write(joiningCharacter.EntityId);

                rcv.Send(packet);
            }
        }

        public static void SendPlayerLeft(IPacketReceiver rcv, Character leavingCharacter)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEGROUND_PLAYER_LEFT, 8))
            {
                packet.Write(leavingCharacter.EntityId);

                rcv.Send(packet);
            }
        }

        public static void SendBattlegroundError(IPacketReceiver rcv, BattlegroundJoinError err)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_JOINED_BATTLEGROUND, 4))
            {
                packet.Write((int)err);

                if (err == BattlegroundJoinError.JoinTimedOut || err == BattlegroundJoinError.JoinFailed)
                    packet.Write((ulong)0);

                rcv.Send(packet);
            }
        }

        /// <summary>
        /// "Your group joined Name"
        /// </summary>
        /// <param name="battleground"></param>
        public static void SendGroupJoinedBattleground(IPacketReceiver rcv, BattlegroundId battleground)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GROUP_JOINED_BATTLEGROUND, 4))
            {
                packet.Write((int)battleground);

                rcv.Send(packet);
            }
        }

        public static void SendPlayerPositions(IPacketReceiver client, IList<Character> players, IList<Character> flagCarriers)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BATTLEGROUND_PLAYER_POSITIONS))
            {
                if (players != null)
                {
                    packet.Write(players.Count); // for the players side
                    foreach (var player in players)
                    {
                        packet.Write(player.EntityId); // player guid
                        packet.Write(player.Position.X); // player x
                        packet.Write(player.Position.Y); // player y
                    }
                }
                else
                {
                    packet.Write(0);
                }

                if (flagCarriers != null)
                {
                    packet.Write(flagCarriers.Count);
                    foreach (var player in flagCarriers)
                    {
                        packet.Write(player.EntityId);
                        packet.Write(player.Position.X);
                        packet.Write(player.Position.Y);
                    }
                }
                else
                {
                    packet.Write(0);
                }

                client.Send(packet);
            }
        }
    }
}