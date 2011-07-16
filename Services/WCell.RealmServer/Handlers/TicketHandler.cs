using WCell.Constants;
using WCell.Constants.Tickets;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// TODO: Check for existing tickets before creating/deleting
	/// TODO: Always set Character.Ticket and Ticket.Owner correspondingly
	/// TODO: Commands
	/// TODO: Save to DB
	/// TODO: Enable staff to list any ticket that was issued since serverstart and maybe save a backup copy to Ticket archive
	/// TODO: Attach Tickets to mail system
	/// TODO: Enforced Help-chat for fast ticket-handling etc
	/// </summary>
	public static class TicketHandler
	{
		#region IN
		[PacketHandler(RealmServerOpCode.CMSG_GMTICKET_SYSTEMSTATUS)]
		public static void HandleSystemStatusPacket(IRealmClient client, RealmPacketIn packet)
		{
			using (var repPacket = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_SYSTEMSTATUS, 4))
			{
				// TODO: Add indicator to Account for whether person may use ticket system
				repPacket.Write(1);
				client.Send(repPacket);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GMTICKET_CREATE)]
		public static void HandleCreateTicketPacket(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			if (chr.Ticket == null)
			{
				var map = (MapId)packet.ReadUInt32();
				var x = packet.ReadFloat();
				var y = packet.ReadFloat();
				var z = packet.ReadFloat();
				var msg = packet.ReadCString();
				var type = (TicketType)packet.ReadUInt32(); // prev. unk0

				var unk1 = packet.ReadByte(); // unk1, 1
				var unk2 = packet.ReadUInt32(); // unk2, 0
				var unk3 = packet.ReadUInt32(); // unk3, 0

				var ticket = new Ticket(chr, msg, type);

				TicketMgr.Instance.AddTicket(ticket);
				chr.Ticket = ticket;
				SendCreateResponse(client, TicketInfoResponse.Saved);
			}
			else
			{
				SendCreateResponse(client, TicketInfoResponse.Fail);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GM_REPORT_LAG)]
		public static void HandleReportLagTicket(IRealmClient client, RealmPacketIn packet)
		{
			var type = (TicketReportLagType)packet.ReadUInt32();
			var unk0 = packet.ReadUInt32(); // Seems to be always 0
			var posX = packet.ReadFloat();
			var posY = packet.ReadFloat();
			var posZ = packet.ReadFloat();
		}

		[PacketHandler(RealmServerOpCode.CMSG_GMTICKET_GETTICKET)]
		public static void HandleGetTicketPacket(IRealmClient client, RealmPacketIn packet)
		{
			SendGetTicketResponse(client, client.ActiveCharacter.Ticket);
		}

		[PacketHandler(RealmServerOpCode.CMSG_GMTICKET_DELETETICKET)]
		public static void HandleDeleteTicketPacket(IRealmClient client, RealmPacketIn packet)
		{
			var ticket = client.ActiveCharacter.Ticket;
			if (ticket != null)
			{
				ticket.Delete();
				SendDeleteResponse(client, TicketInfoResponse.Deleted);
			}
			else
			{
				SendDeleteResponse(client, TicketInfoResponse.NoTicket);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GMTICKET_UPDATETEXT)]
		public static void HandleUpdateTicketPacket(IRealmClient client, RealmPacketIn packet)
		{
			var ticket = client.ActiveCharacter.Ticket;
			if (ticket != null)
			{
				ticket.Message = packet.ReadCString();
				SendUpdateResponse(client, TicketInfoResponse.Saved);
			}
			else
			{
				SendUpdateResponse(client, TicketInfoResponse.Fail);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_GMTICKET_RESOLVE_RESPONSE)]
		public static void HandleResolveResponsePacket(IRealmClient client, RealmPacketIn packet)
		{
			SendResolveResponse(client, client.ActiveCharacter.Ticket);
		}
		#endregion


		#region OUT
		/// <summary>
		/// 
		/// </summary>
		/// <param name="client"></param>
		/// <param name="ticket">Can be null</param>
		public static void SendGetTicketResponse(IPacketReceiver client, Ticket ticket)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_GETTICKET))
			{
				if (ticket != null)
				{
					packet.Write((uint)TicketInfoResponse.Pending);
					packet.WriteCString(ticket.Message);
					packet.Write((byte)ticket.Type);
					packet.Write((float)0);
					packet.Write((float)0);
					packet.Write((float)0);
					packet.Write((ushort)0);
					client.Send(packet);
				}
				else
				{
					packet.Write((uint)TicketInfoResponse.NoTicket);
				}
			}
		}

		public static void SendUpdateResponse(IPacketReceiver client, TicketInfoResponse response)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_UPDATETEXT))
			{
				packet.Write((uint)response);
				client.Send(packet);
			}
		}

		public static void SendCreateResponse(IPacketReceiver client, TicketInfoResponse response)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_CREATE))
			{
				packet.Write((uint)response);
				client.Send(packet);
			}
		}

		public static void SendDeleteResponse(IPacketReceiver client, TicketInfoResponse response)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_DELETETICKET))
			{
				// packet.Write(9);	// deleted successfully
				packet.Write((uint)response);
				client.Send(packet);
			}
		}

		public static void SendResolveResponse(IPacketReceiver client, Ticket ticket)
		{
			ticket.Delete();
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GMTICKET_RESOLVE_RESPONSE, 4))
			{
				packet.WriteByte(0);
				client.Send(packet);
			}
		#endregion
		}
	}
}