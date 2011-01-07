/*************************************************************************
 *
 *   file		: MiscHandlers.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-30 16:30:19 +0100 (lø, 30 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1235 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/


using WCell.Constants;
using WCell.Constants.Login;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Constants.World;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Handlers
{
	public static class MiscHandler
	{
		#region IN

		[ClientPacketHandler(RealmServerOpCode.CMSG_REALM_SPLIT, IsGamePacket = false, RequiresLogin = false)]
		public static void HandleRealmStateRequest(IRealmClient client, RealmPacketIn packet)
		{
			uint unknown = packet.ReadUInt32();
			SendRealmStateResponse(client, unknown);
		}

		/// <summary>
		/// Handles an incoming ping request.
		/// </summary>
		/// <param name="client">the Session the incoming packet belongs to</param>
		/// <param name="packet">the full packet</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_PING, IsGamePacket = false, RequiresLogin = false)]
		public static void PingRequest(IRealmClient client, RealmPacketIn packet)
		{
			SendPingReply(client, packet.ReadUInt32());

			client.Latency = packet.ReadInt32();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_TOGGLE_PVP)]
		public static void HandleTogglePvP(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			if (packet.ContentLength > 0)
			{
				var pvpFlagState = packet.ReadBoolean();
				chr.SetPvPFlag(pvpFlagState);
				return;
			}
			chr.TogglePvPFlag();
		}

		#endregion

		#region OUT
		public static void SendRealmStateResponse(IPacketReceiver client, uint realmNo)
		{
			//uint realmSplitState = 0;
			// realmNo = 0;
			const RealmState realmState = RealmState.Normal;
			var splitDate = "01/01/01";
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_REALM_SPLIT, 8 + 1 + splitDate.Length))
			{
				packet.WriteUInt(realmNo);
				packet.WriteUInt((uint)realmState);
				packet.WriteCString(splitDate);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a ping reply to the client.
		/// </summary>
		/// <param name="client">the client to send to</param>
		/// <param name="sequence">the sequence number sent by client</param>
		public static void SendPingReply(IPacketReceiver client, uint sequence)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PONG, 4))
			{
				packet.Write(sequence);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Sends the given list of messages as motd (displays as a regular system-msg)
		/// </summary>
		public static void SendMotd(IPacketReceiver client, params string[] messages)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_MOTD, 4 + messages.Length * 20))
			{
				packet.Write(messages.Length);
				foreach (var msg in messages)
				{
					packet.WriteCString(msg);
				}

				client.Send(packet);
			}
		}

		/// <summary>
		/// Flashes a message in the middle of the screen.
		/// </summary>
		public static void SendNotification(IPacketReceiver client, string msg)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_NOTIFICATION))
			{
				packet.WriteCString(msg);
				client.Send(packet);
			}
		}

		public static void SendCancelAutoRepeat(IPacketReceiver client, IEntity entity)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CANCEL_AUTO_REPEAT))
			{
				entity.EntityId.WritePacked(packet);
				client.Send(packet);
			}
		}

		public static void SendHealthUpdate(Unit unit, int health)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_HEALTH_UPDATE, 13))
			{
				unit.EntityId.WritePacked(packet);
				packet.Write(health);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendPowerUpdate(Unit unit, PowerType type, int value)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_POWER_UPDATE, 13))
			{
				unit.EntityId.WritePacked(packet);
				packet.Write((byte)type);
				packet.Write(value);

				unit.SendPacketToArea(packet, true);
			}
		}

		public static void SendPlayObjectSound(WorldObject obj, uint sound)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_OBJECT_SOUND, 13))
			{
				packet.Write(sound);
				packet.Write(obj.EntityId);

				obj.SendPacketToArea(packet, true);
			}
		}

        public static void SendPlaySoundToRegion(Region region, uint sound)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_SOUND, 4))
            {
                packet.WriteUInt(sound);

                region.SendPacketToRegion(packet);
            }
        }
		public static void SendInitWorldStates(Character rcv, WorldStateCollection states, Zone newZone)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INIT_WORLD_STATES, 300))
			{
				packet.Write((uint)rcv.Region.Id);
				packet.Write((uint)newZone.ParentZoneId);
				packet.Write((uint)newZone.Id);

				var countPos = packet.Position;
				packet.Position += 2;

				var count = AppendWorldStates(packet, newZone);

				packet.Position = countPos;
				packet.Write((ushort)count);

				rcv.Send(packet);
			}
		}

		static int AppendWorldStates(RealmPacketOut packet, IWorldSpace space)
		{
			var count = 0;
			if (space.ParentSpace != null)
			{
				count += AppendWorldStates(packet, space.ParentSpace);
			}
			if (space.WorldStates != null)
			{
				count += space.WorldStates.FieldCount;
				packet.Write(space.WorldStates.CompiledState);
			}
			return count;
		}

		public static void SendInitWorldStates(IPacketReceiver rcv, MapId map, ZoneId zone, uint areaId, params WorldState[] states)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INIT_WORLD_STATES, 300))
			{
				packet.Write((uint)map);
				packet.Write((uint)zone);
				packet.Write(areaId);
				packet.Write((ushort)states.Length);
				foreach (var state in states)
				{
					packet.Write((uint)state.Key);
					packet.Write(state.DefaultValue);
				}
				rcv.Send(packet);
			}
		}

		public static void SendUpdateWorldState(IPacketReceiver rcv, WorldState state)
		{
			SendUpdateWorldState(rcv, state.Key, state.DefaultValue);
		}

		public static void SendUpdateWorldState(IPacketReceiver rcv, WorldStateId key, int value)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UPDATE_WORLD_STATE, 300))
			{
				packet.Write((uint) key);
				packet.Write(value);
				rcv.Send(packet);
			}
		}

		#endregion
	}


}