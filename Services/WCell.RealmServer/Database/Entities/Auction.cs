using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.RealmServer.Mail;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Database.Entities
{
	public class Auction
	{
		public DateTime TimeEnds
		{
			get;
			set;
		}

		public AuctionHouseFaction HouseFaction;

		public uint AuctionId
		{
			get { return ItemLowId; }
		}

		public int ItemTemplateId;

		public uint ItemLowId;

		public uint OwnerLowId;

		public uint BidderLowId;

		public uint CurrentBid;

		public uint BuyoutPrice;

		public uint Deposit;

		public bool IsNew;

		public void SendMail(MailAuctionAnswers response, uint money)
		{
			var subject = String.Format("{0}:0:{1}", ItemTemplateId, response);
			SendMail(subject, money, null, "");
		}

		public void SendMail(MailAuctionAnswers response, uint money, string body)
		{
			var subject = String.Format("{0}:0:{1}", ItemTemplateId, response);
			SendMail(subject, money, null, body);
		}

		public void SendMail(MailAuctionAnswers response, ItemRecord item)
		{
			var subject = String.Format("{0}:0:{1}", ItemTemplateId, response);
			SendMail(subject, 0, item, "");
		}

		public void SendMail(MailAuctionAnswers response, uint money, ItemRecord item, string body)
		{
			var subject = String.Format("{0}:0:{1}", ItemTemplateId, response);
			SendMail(subject, money, item, body);
		}

		public void SendMail(string subject, uint money, ItemRecord item, string body)
		{
			var msg = new MailMessage(subject, body)
				{
					SenderId = (uint)HouseFaction,
					ReceiverId = OwnerLowId,
					MessageStationary = MailStationary.Auction,
					MessageType = MailType.Auction,
					IncludedMoney = money,
                    LastModifiedOn = null,
                    SendTime = DateTime.Now,
                    DeliveryTime = DateTime.Now
				};

            if(item != null)
			    msg.AddItem(item);

			msg.Send();
		}

		public static IEnumerable<Auction> GetAffiliatedAuctions(AuctionHouseFaction houseFaction)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<Auction>(auction => auction.HouseFaction == houseFaction);
		}

		public static IEnumerable<Auction> GetAuctionsForCharacter(uint charLowId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<Auction>(auction => auction.OwnerLowId == charLowId);
		}

		public static IEnumerable<Auction> GetBidderAuctionsForCharacter(uint charLowId)
		{
			return RealmWorldDBMgr.DatabaseProvider.FindAll<Auction>(auction => auction.BidderLowId == charLowId);
		}
	}
}