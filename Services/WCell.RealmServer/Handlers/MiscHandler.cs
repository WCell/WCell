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
	/// <summary>
	/// Packets that don't fit into any other category
	/// </summary>
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
			    //chr.IsPvPTimerActive = !pvpFlagState;
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
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_POWER_UPDATE, 17))
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

        public static void SendPlaySoundToMap(Map map, uint sound)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_SOUND, 4))
            {
                packet.WriteUInt(sound);

                map.SendPacketToMap(packet);
            }
        }

		public static void SendPlayMusic(WorldObject obj, uint sound, float range)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_MUSIC, 4))
			{
				packet.WriteUInt(sound);
				obj.SendPacketToArea(packet, range != 0 ? range : WorldObject.BroadcastRange, true);
			}
		}

		public static void SendGameObjectTextPage(IPacketReceiver rcv, IEntity obj)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GAMEOBJECT_PAGETEXT, 8))
			{
				packet.Write(obj.EntityId);

				rcv.Send(packet);
			}
		}
		#endregion
	}


}