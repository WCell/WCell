using System.Collections.Generic;
using WCell.RealmServer.Database.Entities;
using WCell.Util.Collections;
using WCell.RealmServer.Database;

namespace WCell.RealmServer.NPCs.Auctioneer
{
	public class AuctionHouse
	{
		private readonly SynchronizedDictionary<uint, Auction> auctions;
		private readonly SynchronizedList<uint> items;

        public SynchronizedDictionary<uint, Auction> Auctions {
            get { return auctions; }
        }

		public AuctionHouse()
		{
			auctions = new SynchronizedDictionary<uint, Auction>(10000);
			items = new SynchronizedList<uint>(10000);
		}

		public void AddAuction(Auction newAuction)
		{
			if (newAuction == null)
				return;
			auctions.Add(newAuction.ItemLowId, newAuction);
			items.Add(newAuction.ItemLowId);
            //if this is a new one created by player, it should be save in database
            if (newAuction.IsNew) 
            {
                newAuction.Create();
            }
		}

		public void RemoveAuction(Auction auction)
		{
			if (auction == null)
				return;
            if (!auctions.ContainsKey(auction.AuctionId))
                return;
			auctions.Remove(auction.ItemLowId);
			items.Remove(auction.ItemLowId);
            AuctionMgr mgr = AuctionMgr.Instance;
            ItemRecord record = null;
            if (mgr.AuctionItems.ContainsKey(auction.ItemLowId)) 
            {
                record = mgr.AuctionItems[auction.ItemLowId];
                mgr.AuctionItems.Remove(auction.ItemLowId);
            }
            
            
            //remove from database
            RealmServer.IOQueue.AddMessage(() => {
                if (record != null) {
                    record.IsAuctioned = false;
                    record.Save();
                }
                auction.Delete(); });
		}

		public void RemoveAuctionById(uint auctionId)
		{
			Auction auction;
			if (TryGetAuction(auctionId, out auction))
			{
				RemoveAuction(auction);
			}
		}

		public bool HasItemById(uint itemEntityLowId)
		{
            return AuctionMgr.Instance.AuctionItems.ContainsKey(itemEntityLowId);
		}

		public bool TryGetAuction(uint auctionId, out Auction auction)
		{
			return auctions.TryGetValue(auctionId, out auction);
		}
	}
}