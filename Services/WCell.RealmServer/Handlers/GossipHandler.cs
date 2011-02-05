using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Network;
using System.IO;

namespace WCell.RealmServer.Handlers
{
	public static class GossipHandler
	{
		/// <summary>
		/// Handles gossip hello packet (client requests Gossip Menu)
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="packet">packet incoming</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GOSSIP_HELLO)]
		public static void HandleGossipHello(IRealmClient client, RealmPacketIn packet)
		{
			var targetEntityId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var target = chr.Map.GetObject(targetEntityId) as Unit;

			if (target == null)
				return;
			
			chr.GossipConversation = null;

			var menu = target.GossipMenu;
			if (menu == null)
				return;

			if (target is NPC && !((NPC) target).CheckVendorInteraction(chr) && !chr.Role.IsStaff)
			{
				return;
			}

			chr.OnInteract(target);
			chr.StartGossip(menu, target);
		}

		/// <summary>
		/// Handles option selecting in gossip menu
		/// </summary>
		/// <param name="client">realm client</param>
		/// <param name="packet">packet incoming</param>
		[ClientPacketHandler(RealmServerOpCode.CMSG_GOSSIP_SELECT_OPTION)]
		public static void HandleGossipSelectOption(IRealmClient client, RealmPacketIn packet)
		{
			var targetEntityId = packet.ReadEntityId();
			var unknown = packet.ReadUInt32();				// usually Zero, sometimes in the thousands for quest givers, Same as the first int sent in SMSG_GOSSIP_MESSAGE
			var selectedOption = packet.ReadUInt32();

		    string extra = string.Empty;
			if (packet.Position < packet.Length)
			{
				extra = packet.ReadCString();
			}

			var chr = client.ActiveCharacter;
			var worldObject = chr.Map.GetObject(targetEntityId);

			if (worldObject == null)
				return;

			var conversation = chr.GossipConversation;

			if (conversation == null || conversation.Speaker != worldObject)
				return;

			conversation.HandleSelectedItem(selectedOption, extra);
		}

		#region Out
		/// <summary>
		/// Sends a page to the character
		/// </summary>
		/// <param name="chr">recieving character</param>
		/// <param name="owner">EntityID of sender</param>
		public static void SendPageToCharacter(GossipConversation convo,
											   IList<QuestMenuItem> questItems)
		{
			var speaker = convo.Speaker;
			var chr = convo.Character;

			var menu = convo.CurrentMenu;
			var gossipItems = menu.GossipItems;
			var gossipEntry = menu.GossipEntry;

			if (gossipEntry.IsDynamic)
			{
				// not cached
				QueryHandler.SendNPCTextUpdate(chr, gossipEntry);
			}

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GOSSIP_MESSAGE))
			{
				packet.Write(speaker.EntityId);
				packet.Write(0);				// new Flag field since 2.4.0 - menu id
				packet.Write(gossipEntry.GossipId);

				var countPos = packet.Position;
				packet.Position += 4;
				var count = 0;
				if (gossipItems != null)
				{
					for (var i = 0; i < gossipItems.Count; i++)
					{
						var item = gossipItems[i];
						if (item.Action != null && !item.Action.CanUse(convo))
						{
							continue;
						}

						packet.Write(i);
						packet.Write((byte)item.Icon);
						packet.Write(item.Input);
						packet.Write((uint)item.RequiredMoney);
						packet.WriteCString(item.GetText(convo));
						packet.WriteCString(item.GetConfirmText(convo));
						count++;
					}
				}

				if (questItems != null)
				{
					packet.WriteUInt(questItems.Count);
					for (int i = 0; i < questItems.Count; i++)
					{
						var item = questItems[i];
						packet.Write(item.ID);
						packet.Write(item.Status);
						packet.Write(item.Level);
					    packet.Write(0); // quest flags
					    packet.Write((byte)0); // 3.3.3 flag (blue question or yelloe exclamation mark)
						packet.WriteCString(item.Text);
					}
				}
				else
					packet.Write(0);

				packet.Position = countPos;
				packet.Write(count);

				chr.Client.Send(packet);
			}
		}

		/// <summary>
		/// Sends a page to the character
		/// </summary>
		/// <param name="rcv">recieving character</param>
		public static void SendConversationComplete(IPacketReceiver rcv)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GOSSIP_COMPLETE))
			{
				rcv.Send(packet);
			}
		}

		/// <summary>
		/// Send Point of interest which will then appear on the minimap
		/// </summary>
        public static void SendGossipPOI(IPacketReceiver rcv, GossipPOIFlags Flags, float X, float Y, int Data, int Icon, string Name)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_GOSSIP_POI))
			{
				packet.Write((uint)Flags);
				packet.Write(X);
				packet.Write(Y);
				packet.Write(Data);
				packet.Write(Icon);
				packet.WriteCString(Name);
				rcv.Send(packet);
			}
		}
		#endregion
	}
}