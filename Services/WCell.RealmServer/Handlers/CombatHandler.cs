using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
    public static class CombatHandler
    {
        [ClientPacketHandler(RealmServerOpCode.CMSG_ATTACKSWING)]
        public static void HandleAttackSwing(IRealmClient client, RealmPacketIn packet)
        {
        	var chr = client.ActiveCharacter;
			if (chr.CanDoHarm)
            {
                var targetId = packet.ReadEntityId();
                var target = chr.Region.GetObject(targetId) as Unit;

                if (target != null && chr.CanHarm(target) &&
                    chr.CanSee(target))
                {
					chr.Target = target;
					chr.IsFighting = true;

					// inform everyone
					SendCombatStart(chr, target, true);
                    SendAIReaction(client, target.EntityId, AIReaction.Hostile);
                }
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SETSHEATHED)]
        public static void HandleSheathedStateChanged(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.SheathType = (SheathType)packet.ReadByte();
        }

		/// <summary>
		/// The client signals stop melee fighting
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_ATTACKSTOP)]
        public static void HandleAttackStop(IRealmClient client, RealmPacketIn packet)
        {
			if (client.ActiveCharacter.AutorepeatSpell == null)
			{
				// ignore when using ranged
				client.ActiveCharacter.IsFighting = false;
			}
        }

        public static void SendCombatStart(Unit chr, Unit opponent, bool includeSelf)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSTART, 16))
            {
                packet.Write(chr.EntityId);
                packet.Write(opponent.EntityId);

                chr.SendPacketToArea(packet, includeSelf);
            }
        }

        public static void SendCombatStop(Unit attacker, Unit opponent, int extraArg)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSTOP, 22))
            {
                attacker.EntityId.WritePacked(packet);

                if (opponent != null)
                {
                    opponent.EntityId.WritePacked(packet);
                }
                else
                {
                    packet.Write((byte)0); // null packed guid mask
                }

                packet.Write(extraArg);

                attacker.SendPacketToArea(packet, true);
            }
        }

        public static void SendAttackerStateUpdate(DamageAction action)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKERSTATEUPDATE, 100))
            {
                var evade = action.VictimState == VictimState.Evade;
                if (evade)
                {
                    //action.HitFlags |= HitFlags.HitFlag_0x800 | HitFlags.Absorb_1;
                }
                packet.Write((uint)action.HitFlags);
                action.Attacker.EntityId.WritePacked(packet);
                action.Victim.EntityId.WritePacked(packet);

                var dmg = action.ActualDamage;

                packet.Write(dmg);
                packet.Write(0);					// unknown (overkill?)

                //damage count
                const byte damageCount = 1;
                packet.Write(damageCount);

                for (byte i = 0; i < damageCount; i++)
                {
                    packet.Write((uint) action.Schools);
                    packet.Write((float) dmg);
                    packet.Write(dmg);
                }

				if (action.HitFlags.HasAnyFlag(HitFlags.Absorb_1 | HitFlags.Absorb_2))
                {
                    for (byte i = 0; i < damageCount; i++)
                    {
                        packet.Write(action.Absorbed);
                    }
                }

				if (action.HitFlags.HasAnyFlag(HitFlags.Resist_1 | HitFlags.Resist_2))
                {
                    for (byte i =0;i<damageCount;i++)
                    {
                        packet.Write(action.Resisted);
                    }
                }

                packet.Write((byte)action.VictimState);
                if (evade)
                {
                    packet.Write(0x1000002);
                }
                else
                {
                    packet.Write(dmg > 0 ? -1 : 0); // 0 if no damage, else -1 or 1000 or very rarely something else (eg when evading)
                }

                packet.Write(0);// this is a spell id

                if (action.HitFlags.HasAnyFlag(HitFlags.Block))
                {
                    packet.Write(action.Blocked);
                }

                //if ((hitFlags & HitFlags.HitFlag_0x800000) != 0)
                //{
                //    packet.Write(0);
                //}

                //if ((hitFlags & HitFlags.HitFlag_0x1) != 0)
                //{
                //    packet.Write(0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    packet.Write((float)0);
                //    for (int i = 0; i < 5; i++)
                //    {
                //        packet.Write(0);
                //        packet.Write(0);
                //    }
                //    packet.Write(0);
                //}

                action.Victim.SendPacketToArea(packet, true);
            }
        }


        public static void SendAttackSwingBadFacing(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSWING_BADFACING))
            {
                chr.Send(packet);
            }
        }

        public static void SendAttackSwingNotInRange(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSWING_NOTINRANGE))
            {
                chr.Send(packet);
            }
		}

		/// <summary>
		/// Updates the player's combo-points, will be called automatically whenever the combo points or combotarget change.
		/// </summary>
		public static void SendComboPoints(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_COMBO_POINTS, 9))
			{
				if (chr.ComboTarget != null)
				{
					chr.ComboTarget.EntityId.WritePacked(packet);
				}
				else
				{
					packet.Write((byte)0);
				}
				packet.Write((byte)chr.ComboPoints);

				chr.Client.Send(packet);
			}
		}

        public static void SendAIReaction(IPacketReceiver recv, EntityId guid, AIReaction reaction)
        {
            using (var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_AI_REACTION, 8 + 4))
            {
                pkt.Write(guid);
                pkt.Write((uint)reaction);
                recv.Send(pkt);
            }
        }
    }
}