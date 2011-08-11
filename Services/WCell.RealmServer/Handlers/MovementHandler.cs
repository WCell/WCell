/*************************************************************************
 *
 *   file		: MovementHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-05-08 23:23:29 +0200 (lø, 08 maj 2010) $
 *   last author	: $LastChangedBy: XTZGZoReX $
 *   revision		: $Rev: 1290 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.Core.Paths;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Core.Terrain.Paths;
using WCell.Util;
using WCell.RealmServer.Spells;
using WCell.RealmServer.NPCs.Vehicles;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Handlers
{
	public static class MovementHandler
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		#region Default Client Movement
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_HEARTBEAT)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_JUMP)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_FORWARD)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_BACKWARD)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_SET_FACING)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_STOP)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_STRAFE_LEFT)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_STRAFE_RIGHT)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_STOP_STRAFE)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_TURN_LEFT)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_TURN_RIGHT)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_STOP_TURN)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_PITCH_UP)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_PITCH_DOWN)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_STOP_PITCH)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_SET_RUN_MODE)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_SET_WALK_MODE)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_SET_PITCH)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_SWIM)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_STOP_SWIM)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_FALL_LAND)]
		[ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_SET_FLY)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_HOVER)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_KNOCK_BACK)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_START_ASCEND)]
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_STOP_ASCEND)]
		[ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_CHNG_TRANSPORT)]
		[ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_FALL_RESET)]
		public static void HandleMovement(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
            var mover = chr.MoveControl.Mover as Unit;

		    if (mover == null ||
				!mover.UnitFlags.HasFlag(UnitFlags.PlayerControlled) ||
				mover.UnitFlags.HasFlag(UnitFlags.Influenced))
			{
				// don't accept Player input while not under the Player's control
				return;
			}

			mover.CancelEmote();

			var guid = packet.ReadPackedEntityId();

            if (packet.PacketId.RawId == (uint)RealmServerOpCode.CMSG_MOVE_NOT_ACTIVE_MOVER)
            {
                mover = client.ActiveCharacter.Map.GetObject(guid) as Unit;
                if (mover == null)
                    return;
            }
			uint clientTime;
			if(ReadMovementInfo(packet, chr, mover, out clientTime))
				BroadcastMovementInfo(packet, chr, mover, clientTime);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_NOT_ACTIVE_MOVER)]
		public static void HandleMoveNotActiveMover(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var guid = packet.ReadPackedEntityId();

			var mover = client.ActiveCharacter.Map.GetObject(guid) as Unit;
			if (mover == null)
				return;

			mover.CancelEmote();

			uint clientTime;
			if (ReadMovementInfo(packet, chr, mover, out clientTime))
				BroadcastMovementInfo(packet, chr, mover, clientTime);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="packet">The packet to read the info from</param>
		/// <param name="chr">The active character in the client that send the movement packet</param>
		/// <param name="mover">The unit we want this movement info to affect</param>
		/// <param name="clientTime">Used to return the read client time</param>
		/// <returns>A boolean value used to determine broadcasting of this movement info to other clients</returns>
		public static bool ReadMovementInfo(RealmPacketIn packet, Character chr, Unit mover, out uint clientTime)
		{
			var moveFlags = (MovementFlags) packet.ReadInt32();
			//var moveFlags2 = (MovementFlags2)packet.ReadInt32();
			var moveFlags2 = (MovementFlags2) packet.ReadInt16();
			//var moveFlags2 = MovementFlags2.None;

			clientTime = packet.ReadUInt32();
			//var delay = Utility.GetEpochTime() - clientTime;

			var newPosition = packet.ReadVector3();
			var orientation = packet.ReadFloat();

			if (moveFlags.HasFlag(MovementFlags.OnTransport))
			{
				var transportId = packet.ReadPackedEntityId();
				var transportPosition = packet.ReadVector3();
				var transportOrientation = packet.ReadFloat();
				var transportTime = packet.ReadUInt32();
				var transportSeat = packet.ReadByte();

				var transport = mover.Map.GetObject(transportId) as ITransportInfo;
				var isVehicle = transport is Vehicle;
				//if (transport == null || !transport.IsInRadius(chr, 100f))
				if (transport == null)
				{
					if (mover.Transport != null)
					{
						mover.Transport.RemovePassenger(mover);
					}
					//client.ActiveCharacter.Kick("Invalid Transport flag");
					return false;
				}

				if (mover.TransportInfo != transport)
				{
					if (!isVehicle)
					{
						((Transport) transport).AddPassenger(mover);
					}
					else
					{
						return false;
					}
				}

				if (!isVehicle)
				{
					mover.TransportPosition = transportPosition;
					mover.TransportOrientation = transportOrientation;
					mover.TransportTime = transportTime;
				}
			}
			else if (mover.Transport != null)
			{
				mover.Transport.RemovePassenger(mover);
			}

			if (moveFlags.HasFlag(MovementFlags.Swimming | MovementFlags.Flying) ||
			    moveFlags2.HasFlag(MovementFlags2.AlwaysAllowPitching))
			{
				if (moveFlags.HasFlag(MovementFlags.Flying) && !chr.CanFly)
				{
					return false;
				}

				// pitch, ranges from -1.55 to 1.55
				var pitch = packet.ReadFloat();

				if (chr == mover)
					chr.MovePitch(pitch);
			}

			var airTime = packet.ReadUInt32();

			if (moveFlags.HasFlag(MovementFlags.Falling))
			{
				// constant, but different when jumping in water and on land?                
				var jumpFloat1 = packet.ReadFloat();

				// sin + cos of angle between orientation and players orientation
				var jumpSinAngle = packet.ReadFloat();
				var jumpCosAngle = packet.ReadFloat();
				// speed of xy movement
				var jumpXYSpeed = packet.ReadFloat();

				//chr.OnFalling(airTime, jumpXYSpeed);
			}

			if (packet.PacketId.RawId == (uint) RealmServerOpCode.MSG_MOVE_FALL_LAND && chr == mover)
			{
				chr.OnFalling();
			}

			if (moveFlags.HasFlag(MovementFlags.Swimming) && chr == mover)
			{
				chr.OnSwim();
			}
			else if (chr.IsSwimming && chr == mover)
			{
				chr.OnStopSwimming();
			}

			if (moveFlags.HasFlag(MovementFlags.SplineElevation))
			{
				var spline = packet.ReadFloat();
			}

			// it is only orientation if it is none of the packets below, and has no flags but orientation flags
			var onlyOrientation = newPosition == mover.Position;

			if (!onlyOrientation && (mover.IsInWorld && !mover.SetPosition(newPosition, orientation)))
			{
				// rather unrealistic case
			}
			else
			{
				if (onlyOrientation)
				{
					mover.Orientation = orientation;
				}
				else
				{
					mover.OnMove();
				}

				mover.MovementFlags = moveFlags;
				mover.MovementFlags2 = moveFlags2;

				if (onlyOrientation)
				{
					mover.Orientation = orientation;
				}
				else
				{
					if (!mover.CanMove)
					{
						// cannot kick, since sometimes packets simply have bad timing
						//chr.Kick("Illegal movement.");
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private static void BroadcastMovementInfo(PacketIn packet, Character chr, Unit mover, uint clientTime)
		{
			var clients = chr.GetNearbyClients(false);
			if (clients.Count <= 0) return;

			using (var outPacket = new RealmPacketOut(packet.PacketId))
			{
				var guidLength = mover.EntityId.WritePacked(outPacket);

				// set position to start of data
				packet.Position = packet.HeaderSize + guidLength;

				outPacket.Write(packet.ReadBytes(packet.RemainingLength));

				foreach (var outClient in clients)
				{
					// 4 packet header + 4 moveflags + 2 moveflags2 + packed guid length
					SendMovementPacket(outClient, outPacket, 10 + guidLength, clientTime);
				}
			}
		}

		public static void SendMovementPacket(IRealmClient client, RealmPacketOut pak, int moveTimePos, uint clientMoveTime)
		{
			//var correctedMoveTime = (MovementDelay + delay) + OutOfSyncDelay + Utility.GetEpochTime();
			//var correctedMoveTime = (MovementDelay + delay) + OutOfSyncDelay + Utility.GetSystemTime();
			//var correctedMoveTime = clientMoveTime - Utility.GetSystemTime();
			var correctedMoveTime = Utility.GetSystemTime() +
				(client.OutOfSyncDelay * 800);

			//if (client.LastClientMoveTime > 0)
			//{
			//    correctedMoveTime += clientMoveTime - client.LastClientMoveTime;
			//}

			var originalPos = pak.Position;
			pak.Position = moveTimePos;
			pak.Write(correctedMoveTime);
			pak.Position = originalPos;

			client.LastClientMoveTime = clientMoveTime;
			client.Send(pak);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_MOVE_TIME_SKIPPED)]
		public static void HandleTimeSkipped(IRealmClient client, RealmPacketIn packet)
		{
			packet.ReadPackedEntityId();

			client.OutOfSyncDelay = packet.ReadUInt32();
		}
		#endregion

		#region Movement Acks
		/// <summary>
		/// The client sends this after map-change (when the loading screen finished)
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_WORLDPORT_ACK, IsGamePacket = false)]
		public static void HandleWorldPortAck(IRealmClient client, RealmPacketIn packet)
		{
			client.TickCount = 0;

			var chr = client.ActiveCharacter;
			if (chr != null && chr.Map != null)
			{
				var zone = chr.Map.GetZone(chr.Position.X, chr.Position.Y);
				if (zone != null)
				{
					chr.SetZone(zone);
				}
				else
				{
					chr.SetZone(chr.Map.DefaultZone);
				}

				// resend some initial packets
				CharacterHandler.SendBindUpdate(chr, chr.BindLocation);
				CharacterHandler.SendTickQuery(client);
				SpellHandler.SendSpellsAndCooldowns(chr);
				CharacterHandler.SendActionButtons(chr);
				FactionHandler.SendFactionList(chr);
				CharacterHandler.SendTimeSpeed(chr);
				AuraHandler.SendAllAuras(chr);
				// SMSG_SET_PHASE_SHIFT
				// SMSG_PARTY_MEMBER_STATS
				// MSG_BATTLEGROUND_PLAYER_POSITIONS
			}
		}


		/// <summary>
		/// The client sends this after he was rooted
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_FORCE_MOVE_ROOT_ACK, IsGamePacket = false)]
		public static void HandleRootAck(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			var chrEntityId = packet.ReadPackedEntityId();
			var unknown1 = packet.ReadUInt32();
			var unknown2 = packet.ReadUInt32();
			var posX = packet.ReadFloat();
			var posY = packet.ReadFloat();
			var posZ = packet.ReadFloat();
			var orientation = packet.ReadFloat();

			// TODO: find something to do with this information
		}

		[ClientPacketHandler(RealmServerOpCode.MSG_MOVE_TELEPORT_ACK)]
		public static void HandlerTeleportAck(IRealmClient client, RealmPacketIn packet)
		{
			var teleporteeId = packet.ReadPackedEntityId();
			var flags = packet.ReadUInt32();
			var time = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			var teleportee = chr.Map.GetObject(teleporteeId) as Character;

			// TODO: find something to do with this information.
		}
		#endregion

		public static void SendEnterTransport(Unit unit)
		{
			var transport = unit.TransportInfo;
			SendMonsterMoveTransport(unit, transport.Position - unit.Position, unit.TransportPosition);
		}

		public static void SendLeaveTransport(Unit unit)
		{
			var transport = unit.TransportInfo;
			SendMonsterMoveTransport(unit, unit.TransportPosition, transport.Position);
		}

		/// <summary>
		/// Move from current position  to
		/// new position on the Transport (relative to transport)
		/// </summary>
		/// <param name="unit"></param>
		public static void SendMonsterMoveTransport(Unit unit, Vector3 from, Vector3 to)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE_TRANSPORT))
			{
				var transport = unit.TransportInfo;

				unit.EntityId.WritePacked(packet);
				transport.EntityId.WritePacked(packet);
				packet.Write((ushort)0);				// unknown flags
				packet.Write(from);
				packet.Write(Utility.GetSystemTime());
				if (unit is Character)
				{
					packet.Write((byte) MovementState.WalkOnLand);
					packet.Write(unit.TransportOrientation);
					packet.Write((uint) MonsterMoveFlags.Flag_0x800000);
				}
				else
				{
					packet.Write((byte)0);
					packet.Write((uint) MonsterMoveFlags.Walk);
				}
				packet.Write(0);
				packet.Write(1);
				packet.Write(to);

				unit.SendPacketToArea(packet);
			}
		}

		/// <summary>
		/// Jumping while not moving when mounted
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_MOUNTSPECIAL_ANIM)]
		public static void HandleMountSpecialAnim(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			if (chr.MoveControl.Mover is Unit && ((Unit)chr.MoveControl.Mover).IsMounted)
			{
				SendMountSpecialAnim(client);
			}
		}

		public static void SendMountSpecialAnim(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOUNTSPECIAL_ANIM, 8))
			{
				client.ActiveCharacter.EntityId.WritePacked(packet);
				client.ActiveCharacter.SendPacketToArea(packet, false);
			}
		}

		#region SEND
		public static void SendMoveToPacket(Unit movingUnit, ref Vector3 pos, float orientation, uint moveTime, MonsterMoveFlags moveFlags)
		{
			if (!movingUnit.IsAreaActive && movingUnit.CharacterMaster == null)
			{
				return;
			}
			//log.Debug("Monster Move: O={0}, Time={1}, Flags={2}, Pos={3}", orientation, moveTime, moveFlags, pos);
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE, 53))
			{
				movingUnit.EntityId.WritePacked(packet);			// 8
				packet.Write((byte)0);
				packet.Write(movingUnit.Position);	// + 12 = 20
				packet.Write(Utility.GetSystemTime());			// + 4 = 24
				if (orientation != 0.0f)
				{
					packet.Write((byte)MonsterMoveType.FinalFacingAngle);							// + 1 = 25
					packet.Write(orientation);					// + 4 = 29
				}
				else
				{
					packet.Write((byte)MonsterMoveType.Normal);							// + 1 = 25
				}

				packet.Write((uint)moveFlags);					// + 4 = 33
				packet.Write(moveTime);							// + 4 = 37
				packet.Write(1);								// + 4 = 41
				packet.Write(pos);							// + 12 = 53

				movingUnit.SendPacketToArea(packet, true);
			}
		}

		public static void SendFacingPacket(Unit movingUnit, float orientation, uint moveTimeMillis)
		{
			if (!movingUnit.IsAreaActive)
			{
				return;
			}
			//log.Debug("Monster Move: O={0}, Time={1}, Flags={2}, Pos={3}", orientation, moveTime, moveFlags, pos);
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE, 53))
			{
				movingUnit.EntityId.WritePacked(packet); // 8
				packet.Write((byte)0);
				packet.Write(movingUnit.Position); // + 12 = 20
				packet.Write(Utility.GetSystemTime()); // + 4 = 24

				packet.Write((byte)MonsterMoveType.FinalFacingAngle); // + 1 = 25
				packet.Write(orientation); // + 4 = 29

				packet.Write((uint)MonsterMoveFlags.None); // + 4 = 33
				packet.Write(moveTimeMillis); // + 4 = 37
				packet.Write(1);								// + 4 = 41
				packet.Write(movingUnit.Position);

				movingUnit.SendPacketToArea(packet, true);
			}
		}

		public static void SendMoveToPacket<T>(Unit movingUnit, uint moveTime,
											   MonsterMoveFlags moveFlags, IEnumerable<T> waypoints)
			where T : IPathVertex
		{
			if (!movingUnit.IsAreaActive)
			{
				return;
			}
			using (var packet = ConstructMultiWaypointMovePacket(movingUnit, moveTime, moveFlags, waypoints))
			{
				movingUnit.SendPacketToArea(packet, true);
			}
		}

		public static void SendMoveToPacket<T>(Unit movingUnit, int speed,
											   MonsterMoveFlags moveFlags, LinkedListNode<T> firstNode)
			where T : IPathVertex
		{
			if (!movingUnit.IsAreaActive)
			{
				return;
			}
			using (var packet = ConstructMultiWaypointMovePacket(movingUnit, speed, moveFlags, firstNode))
			{
				movingUnit.SendPacketToArea(packet, true);
			}
		}

		public static void SendMoveToPacketToSingleClient<T>(IRealmClient client, Unit movingUnit, uint moveTime,
															 MonsterMoveFlags moveFlags, IEnumerable<T> waypoints)
			where T : IPathVertex
		{
			using (var packet = ConstructMultiWaypointMovePacket(movingUnit, moveTime, moveFlags, waypoints))
			{
				client.Send(packet);
			}
		}

		public static RealmPacketOut ConstructMultiWaypointMovePacket<T>(Unit movingUnit, uint moveTime,
																		  MonsterMoveFlags moveFlags, IEnumerable<T> waypoints)
			where T : IPathVertex
		{
			var numWaypoints = waypoints.Count();
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE,
											(9 + 12 + 4 + 1 + 4 + 4 + 4 + (numWaypoints * 4 * 3)));


			movingUnit.EntityId.WritePacked(packet);
			//if (movingUnit.IsOnTransport)
			//{
			//  packet.OpCode = RealmServerOpCode.SMSG_MONSTER_MOVE_TRANSPORT;
			//  movingUnit.Transport.EntityId.WritePacked(packet);
			//  packet.Write((byte)movingUnit.TransportSeat);
			//}
			packet.Write(false); // some boolean flag

			packet.Write(movingUnit.Position);// OnMonsterMove_serverLoc
			packet.Write(Utility.GetSystemTime());

			const MonsterMoveType moveType = MonsterMoveType.Normal;
			packet.Write((byte)moveType);
			switch (moveType)
			{
				case MonsterMoveType.Normal:
					break;
				// TODO: implement other cases
				//case MonsterMoveType.Stop:
				//    {
				//        return packet;
				//    }
				//case MonsterMoveType.FinalFacingPoint:
				//    {
				//        // OnMonsterMove_final_facingSpot
				//        packet.Write(0.0f);
				//        packet.Write(0.0f);
				//        packet.Write(0.0f);
				//    }
				//case MonsterMoveType.FinalFacingGuid:
				//    {
				//        packet.Write(0ul);
				//    }
				//case MonsterMoveType.FinalFacingAngle:
				//    {
				//        // OnMonsterMove_final_facingAngle
				//        packet.Write(movingUnit.Orientation);
				//    }
			}

			packet.Write((uint)moveFlags);
			if (moveFlags.HasFlag(MonsterMoveFlags.Flag_0x200000))
			{
				// TODO: what does this flag mean?
				packet.Write((byte)0);
				packet.Write(0);
			}

			packet.Write(moveTime);

			if (moveFlags.HasFlag(MonsterMoveFlags.Flag_0x800))
			{
				// TODO: what does this flag mean?
				packet.Write(0.0f);
				packet.Write(0);
			}

			packet.Write(numWaypoints);

			if (moveFlags.HasAnyFlag(MonsterMoveFlags.Flag_0x2000_FullPoints_1 | MonsterMoveFlags.Flag_0x40000_FullPoints_2))
			{
				foreach (IPathVertex waypoint in waypoints)
				{
					packet.Write(waypoint.Position); // + 12*numWaypoints
				}
			}
			else
			{
				// OnMonsterMove_pathPoints_compressed
				foreach (IPathVertex waypoint in waypoints)
				{
					packet.Write(waypoint.Position.ToDeltaPacked(movingUnit.Position, waypoints.First().Position)); // + 12*numWaypoints
				}
			}

			return packet;
		}

		/// <summary>
		/// Constructs a waypoint packet, starting with the given firstNode (until the end of the LinkedList).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="unit"></param>
		/// <param name="speed">The speed that the Unit should move with in yards/second</param>
		/// <param name="moveFlags"></param>
		/// <param name="firstNode"></param>
		/// <returns></returns>
		public static RealmPacketOut ConstructMultiWaypointMovePacket<T>(Unit unit,
			int speed,
			MonsterMoveFlags moveFlags,
			LinkedListNode<T> firstNode)

			where T : IPathVertex
		{
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE,
											(9 + 1 + 12 + 4 + 1 + 4 + 4 + 4 + (firstNode.List.Count * 4 * 3)));

			unit.EntityId.WritePacked(packet);			// 8

			packet.Write(false); // unknown flag

			packet.Write(unit.Position);
			packet.Write(Utility.GetSystemTime());
			packet.Write((byte)MonsterMoveType.Normal);
			packet.Write((uint)moveFlags);

			if (moveFlags.HasFlag(MonsterMoveFlags.Flag_0x200000))
			{
				packet.Write((byte)0);
				packet.Write(0);
			}

			var timePos = packet.Position;
			packet.Position += 4;

			if (moveFlags.HasFlag(MonsterMoveFlags.Flag_0x800))
			{
				packet.Write(0.0f);
				packet.Write(0);
			}

			var countPos = packet.Position;
			packet.Position += 4;

			var moveTime = (int)(1000 * unit.Position.GetDistance(firstNode.Value.Position) / speed);
			var count = 0;
			var current = firstNode;
			while (true)
			{
				count++;
				packet.Write(current.Value.Position);// + 12*numWaypoints
				var next = current.Next;
				if (next != null)
				{
					moveTime += (int)(1000 * current.Value.GetDistanceToNext() / speed);
					current = next;
				}
				else
				{
					break;
				}
			}

			packet.Position = timePos;
			packet.Write(moveTime);
			packet.Position = countPos;
			packet.Write(count);
			return packet;
		}

		public static void SendStopMovementPacket(Unit movingUnit)
		{
			if (!movingUnit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MONSTER_MOVE, 25))
			{
				movingUnit.EntityId.WritePacked(packet);
				packet.Write(false); // new 3.1
				packet.Write(movingUnit.Position);
				packet.Write(Utility.GetSystemTime());
				packet.Write((byte)MonsterMoveType.Stop);

                if(movingUnit is Character)
                {
                    ((Character)movingUnit).Send(packet);
                    return;
                }

				movingUnit.SendPacketToArea(packet);
			}
		}

		public static void SendHeartbeat(Unit unit, Vector3 pos, float orientation)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_HEARTBEAT, 31))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendKnockBack(WorldObject source, WorldObject target, float horizontalSpeed, float verticalSpeed)
		{
			var horizontalAngle = source == target ? source.Orientation + MathUtil.PI : target.GetAngleTowards(source);

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_KNOCK_BACK, 28))
			{
				target.EntityId.WritePacked(packet);
				packet.Write(1);
				packet.Write((float)Math.Sin(horizontalAngle));
				packet.Write((float)Math.Cos(horizontalAngle));
				packet.Write(horizontalSpeed);
				packet.Write(-verticalSpeed);

				target.SendPacketToArea(packet, true);
			}
		}

		public static void SendRooted(Character chr, int unk)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_FORCE_MOVE_ROOT, 12))
			{
				chr.EntityId.WritePacked(packet);
				packet.WriteUInt(unk);

				chr.SendPacketToArea(packet, true);
			}
		}

		public static void SendUnrooted(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_FORCE_MOVE_UNROOT, 12))
			{
				chr.EntityId.WritePacked(packet);
				packet.WriteUInt(5);

				chr.SendPacketToArea(packet, true);
			}
		}

		public static void SendWaterWalk(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_WATER_WALK, 12))
			{
				chr.EntityId.WritePacked(packet);
				packet.WriteUInt(4);

				chr.SendPacketToArea(packet, true);
			}
		}

		public static void SendWalk(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_LAND_WALK, 12))
			{
				chr.EntityId.WritePacked(packet);
				packet.WriteUInt(8);

				chr.SendPacketToArea(packet, true);
			}
		}

		public static void SendMoved(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_TELEPORT_ACK, 40))
			{
				chr.WriteTeleportPacketInfo(packet, 2);

				chr.Send(packet);
			}
		}

		public static void SendNewWorld(IRealmClient client, MapId map, ref Vector3 pos, float orientation)
		{
			// opens loading screen
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRANSFER_PENDING, 4))
			{
				packet.WriteUInt((uint)map);

				var chr = client.ActiveCharacter;
				var trans = chr.Transport;
				if (trans != null)
				{
					packet.Write((uint)trans.Entry.Id);
					packet.Write((uint)chr.MapId);
				}

				client.Send(packet);
			}

			// sends new world info
			using (var outPacket = new RealmPacketOut(RealmServerOpCode.SMSG_NEW_WORLD, 20))
			{
				outPacket.WriteUInt((uint)map);
				outPacket.Write(pos);
				outPacket.WriteFloat(orientation);

				client.Send(outPacket);
			}

			// client will ask for re-initialization afterwards
		}

		/// <summary>
		/// TODO: Find the difference between SMSG_MOVE_ABANDON_TRANSPORT and MSG_MOVE_ABANDON_TRANSPORT
		/// </summary>
		/// <param name="unit"></param>
		/// <param name="value"></param>
		public static void Send_SMSG_MOVE_ABANDON_TRANSPORT(Unit unit, ushort value)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_ABANDON_TRANSPORT, 2))
			{
				packet.Write(value);
			}
		}

		public static void SendHoverModeStart(Unit unit)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_SET_HOVER, 12))
			{
				unit.EntityId.WritePacked(packet);
				packet.WriteUInt(2);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendHoverModeStop(Unit unit)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_UNSET_HOVER, 12))
			{
				unit.EntityId.WritePacked(packet);
				packet.WriteUInt(5);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendFeatherModeStart(Unit unit)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_FEATHER_FALL, 12))
			{
				unit.EntityId.WritePacked(packet);
				packet.WriteUInt(0);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendFeatherModeStop(Unit unit)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_NORMAL_FALL, 12))
			{
				unit.EntityId.WritePacked(packet);
				packet.WriteUInt(0);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendFlyModeStart(Unit unit)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_SET_CAN_FLY, 12))
			{
				unit.EntityId.WritePacked(packet);
				packet.WriteUInt(2);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendFlyModeStop(Unit unit)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOVE_UNSET_CAN_FLY, 12))
			{
				unit.EntityId.WritePacked(packet);
				packet.WriteUInt(5);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendTransferFailure(IPacketReceiver client, MapId mapId, MapTransferError reason)
		{
			SendTransferFailure(client, mapId, reason, 0);
		}

		public static void SendTransferFailure(IPacketReceiver client, MapId mapId, MapTransferError reason, byte arg)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRANSFER_ABORTED, 8))
			{
				packet.Write((uint)mapId);
				packet.Write((byte)reason);

				switch (reason)
				{
					case MapTransferError.TRANSFER_ABORT_INSUF_EXPAN_LVL:
					case MapTransferError.TRANSFER_ABORT_DIFFICULTY:
					case MapTransferError.TRANSFER_ABORT_UNIQUE_MESSAGE:
						// only for these 3 cases!
						packet.Write((byte)arg);
						break;
				}

				client.Send(packet);
			}
		}

		#endregion

		#region Set Speed Packets (Client handles all of these in the same handler)

		public static void SendSetWalkSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_WALK_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.WalkSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetRunSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_RUN_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.RunSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetRunBackSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_RUN_BACK_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.RunBackSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetSwimSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_SWIM_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.SwimSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetSwimBackSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_SWIM_BACK_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.SwimBackSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetFlightSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_FLIGHT_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.FlightSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetFlightBackSpeed(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_FLIGHT_BACK_SPEED, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.FlightBackSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetTurnRate(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_TURN_RATE, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.WriteFloat(unit.TurnSpeed);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendSetPitchRate(Unit unit)
		{
			if (!unit.IsAreaActive)
			{
				return;
			}
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_MOVE_SET_PITCH_RATE, 43))
			{
				unit.EntityId.WritePacked(packet);
				unit.WriteMovementPacketInfo(packet);
				packet.Write(unit.PitchRate);

				unit.SendPacketToArea(packet, true);
			}
		}

		#endregion
	}
}