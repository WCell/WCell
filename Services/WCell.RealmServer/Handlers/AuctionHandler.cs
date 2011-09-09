using System;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Auctioneer;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public static class AuctionHandler
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		#region IN
		[ClientPacketHandler(RealmServerOpCode.MSG_AUCTION_HELLO)]
		public static void HandleAuctionHello(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();

			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;
			AuctionMgr.Instance.AuctionHello(chr, auctioneer);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_SELL_ITEM)]
		public static void HandleAuctionSellItem(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();
		    var unknown = packet.ReadUInt32();
			var itemId = packet.ReadEntityId();
		    var stackSize = packet.ReadUInt32();
			var bid = packet.ReadUInt32();
			var buyout = packet.ReadUInt32();
			var time = packet.ReadUInt32();

			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;
			AuctionMgr.Instance.AuctionSellItem(chr, auctioneer, itemId, bid, buyout, time, stackSize);
		}

        [ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_LIST_PENDING_SALES)]
        public static void HandleAuctionListPendingSales(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            var auctioneerId = packet.ReadEntityId();
            var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;

            var count = 1u;
            using (var packetOut = new RealmPacketOut(RealmServerOpCode.SMSG_AUCTION_LIST_PENDING_SALES, 14 * (int)count))
            {
               
                packetOut.Write(count);
                for (var i = 0; i < count; ++i)
                {
                    packetOut.Write("");
                    packetOut.Write("");
                    packetOut.WriteUInt(0);
                    packetOut.WriteUInt(0);
                    packetOut.WriteFloat(0f);
                    client.Send(packetOut);
                }
            }
        }

		[ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_PLACE_BID)]
		public static void HandleAuctionPlaceBid(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();
			var auctionId = packet.ReadUInt32();
			var bid = packet.ReadUInt32();

			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;
			AuctionMgr.Instance.AuctionPlaceBid(chr, auctioneer, auctionId, bid);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_REMOVE_ITEM)]
		public static void HandleAuctionRemoveItem(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();
			var auctionId = packet.ReadUInt32();

			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;
			AuctionMgr.Instance.CancelAuction(chr, auctioneer, auctionId);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_LIST_OWNER_ITEMS)]
		public static void HandleAuctionListOwnerItems(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();

			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;
			AuctionMgr.Instance.AuctionListOwnerItems(chr, auctioneer);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_LIST_BIDDER_ITEMS)]
		public static void HandleAuctionListBidderItems(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();
		    var outbiddedCount = packet.ReadUInt32();
            for(var i = 0; i < outbiddedCount; i++)
            {
                //packet.ReadUInt32(); //auction id
            }
			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;
			AuctionMgr.Instance.AuctionListBidderItems(chr, auctioneer);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_AUCTION_LIST_ITEMS)]
		public static void HandleAuctionListItems(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var auctioneerId = packet.ReadEntityId();
			var auctioneer = chr.Map.GetObject(auctioneerId) as NPC;

			var searcher = new AuctionSearch()
			{
				StartIndex = packet.ReadUInt32(),
				Name = packet.ReadCString(),
				LevelRange1 = packet.ReadByte(),
				LevelRange2 = packet.ReadByte(),
				InventoryType = (InventorySlotType)packet.ReadUInt32(),
				ItemClass = (ItemClass)packet.ReadUInt32(),
				ItemSubClass = (ItemSubClass)packet.ReadUInt32(),
				Quality = packet.ReadInt32(),
				IsUsable = packet.ReadBoolean()
			};

			AuctionMgr.Instance.AuctionListItems(chr, auctioneer, searcher);

		}

		#endregion

		#region OUT
		public static void SendAuctionHello(IPacketReceiver client, NPC auctioneer)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_AUCTION_HELLO, 12))
			{
				packet.Write(auctioneer.EntityId);
				packet.Write((uint)auctioneer.AuctioneerEntry.LinkedHouseFaction); // id from AuctionHouse.dbc
			    packet.Write(true); // auction enabled/disabled

				client.Send(packet);
			}
		}

		public static void SendAuctionCommandResult(IPacketReceiver client, Auction auction, AuctionAction action, AuctionError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUCTION_COMMAND_RESULT, 12))
			{
				if (auction != null)
				{
					packet.Write(auction.ItemLowId);
				}
				else
				{
					packet.Write(0u);
				}
				packet.Write((uint)action);
				packet.Write((uint)error);

				client.Send(packet);
			}
		}

		public static void SendAuctionOutbidNotification(IPacketReceiver client, Auction auction, uint newBid, uint minBidInc)
		{
			if (auction == null)
				return;

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUCTION_BIDDER_NOTIFICATION, 8 * 4))
			{
				packet.Write((uint)auction.HouseFaction);
				packet.Write(auction.ItemLowId);
				packet.Write(auction.BidderLowId);
				packet.Write(newBid);
				packet.Write(minBidInc);
				packet.Write(auction.ItemTemplateId);
				packet.Write(0u);

				client.Send(packet);
			}
		}

		public static void SendAuctionListOwnerItems(IPacketReceiver client, Auction[] auctions)
		{
			if (auctions == null || auctions.Length < 1)
				return;

			//can not use "using" block if a ref parameter in there.
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUCTION_OWNER_LIST_RESULT, 1024);
			packet.Write(auctions.Length);

			foreach (var auction in auctions)
			{
				BuildAuctionPacket(auction, packet);
			}
			packet.Write(auctions.Length);
			packet.Write(0);
			client.Send(packet);
			packet.Close();

		}

		public static void SendAuctionListBidderItems(IPacketReceiver client, Auction[] auctions)
		{
			if (auctions == null || auctions.Length < 1)
				return;

			//can not use "using" block if a ref parameter in there.
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUCTION_BIDDER_LIST_RESULT, 1024);
			packet.Write(auctions.Length);

			foreach (var auction in auctions)
			{
				BuildAuctionPacket(auction, packet);
			}
			packet.Write(auctions.Length);
			packet.Write(0);
			client.Send(packet);
			packet.Close();
		}

		public static void SendAuctionListItems(IPacketReceiver client, Auction[] auctions)
		{
			if (auctions == null || auctions.Length < 1)
				return;
			var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUCTION_LIST_RESULT, 7000);
            var count = 0;
            packet.Write(auctions.Length);
			foreach (var auction in auctions)
			{
				if(BuildAuctionPacket(auction, packet))
                    count++;
			}
            //packet.InsertIntAt(count, 0, true);
			packet.Write(count);
			packet.Write(300);
			client.Send(packet);
			packet.Close();

		}
		#endregion

		#region Packet Helper
		public static bool BuildAuctionPacket(Auction auction, RealmPacketOut packet)
		{
			var item = AuctionMgr.Instance.AuctionItems[auction.ItemLowId];

			if (item == null)
				return false;

			var timeleft = auction.TimeEnds - DateTime.Now;
			if (timeleft.TotalMilliseconds < 0)
				return false;

			packet.Write(auction.ItemLowId);
			packet.Write(item.Template.Id);

			for (var i = 0; i < 7; i++)
			{
				if (item.EnchantIds != null)
				{
					packet.Write(item.EnchantIds[i]);
					packet.Write(i);					// enchant duration
					packet.Write(item.GetEnchant((EnchantSlot)i).Charges);	// TODO: Fix enchant charges
				}
				else
				{
					packet.Write(0);
					packet.Write(0);
					packet.Write(0);
				}
			}

			packet.Write(item.RandomProperty);
			packet.Write(item.RandomSuffix);
			packet.Write(item.Amount);
			packet.Write((uint)item.Charges);
            packet.WriteUInt(0);                    //Unknown
            packet.WriteULong(auction.OwnerLowId);
            packet.Write(auction.CurrentBid);       //auction start bid
		    packet.WriteUInt(AuctionMgr.GetMinimumNewBidIncrement(auction));                    //amount required to outbid
            packet.Write(auction.BuyoutPrice);
            packet.Write((int)timeleft.TotalMilliseconds);
            packet.WriteULong(auction.BidderLowId);
            packet.Write(auction.CurrentBid);
		    return true;
		}
		#endregion
	}
}