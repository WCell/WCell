using System;
using System.Collections.Generic;
using Castle.ActiveRecord;
using WCell.Constants;
using WCell.RealmServer.Database;
using WCell.RealmServer.Mail;
using WCell.RealmServer.NPCs.Auctioneer;

namespace WCell.RealmServer.NPCs.Auctioneer
{
	[ActiveRecord]
	public class Auction : ActiveRecordBase<Auction>
	{
		/// <summary>
		/// TODO: NPC entityid must not be saved to DB
		/// TODO: Don't save uints as long - int is fine
		/// </summary>
		[Field("AuctionId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _auctionId;

		[Field("ItemTemplateId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _itemTemplateId;

		[Field("OwnerEntityLowId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _ownerEntityLowId;

		[Field("BidderEntityLowId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _bidderEntityLowId;

		[Field("CurrentBid", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _currentBid;

		[Field("BuyoutPrice", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _buyoutPrice;

		[Field("Deposit", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _deposit;

		[Field("AuctionHouseFaction", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _houseFaction;

		//this field don't need save in database
		private bool _isNew = false;

		[Property(NotNull = true)]
		public DateTime TimeEnds
		{
			get;
			set;
		}

		[PrimaryKey(PrimaryKeyType.Assigned, "ItemEntityLowId")]
		private int _ItemEntityLowId
		{
			get;
			set;
		}

		public AuctionHouseFaction HouseFaction
		{
			get
			{
				return (AuctionHouseFaction)Enum.ToObject(typeof(AuctionHouseFaction), _houseFaction);
			}
			set
			{
				_houseFaction = (int)value;
			}
		}

		public uint AuctionId
		{
			get { return (uint)_auctionId; }
		}

		public uint ItemTemplateId
		{
			get { return (uint)_itemTemplateId; }
			set { _itemTemplateId = (int)value; }
		}

		public uint ItemLowId
		{
			get { return (uint)_ItemEntityLowId; }
			set
			{
				_auctionId = (int)value;
				_ItemEntityLowId = (int)value;
			}
		}

		public uint OwnerLowId
		{
			get { return (uint)_ownerEntityLowId; }
			set { _ownerEntityLowId = (int)value; }
		}

		public uint BidderLowId
		{
			get { return (uint)_bidderEntityLowId; }
			set { _bidderEntityLowId = (int)value; }
		}

		public uint CurrentBid
		{
			get { return (uint)_currentBid; }
			set { _currentBid = (int)value; }
		}

		public uint BuyoutPrice
		{
			get { return (uint)_buyoutPrice; }
			set { _buyoutPrice = (int)value; }
		}

		public uint Deposit
		{
			get { return (uint)_deposit; }
			set { _deposit = (int)value; }
		}

		public bool IsNew
		{
			get { return _isNew; }
			set { _isNew = value; }
		}

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
			var msg = new MailMessage
				{
					SenderId = (uint)HouseFaction,
					ReceiverId = OwnerLowId,
					MessageStationary = MailStationary.Auction,
					MessageType = MailType.Auction,
					Subject = subject,
					Body = body,
					IncludedMoney = money
				};
			msg.AddItem(item);
			msg.Send();
		}

		public static IEnumerable<Auction> GetAffiliatedAuctions(AuctionHouseFaction houseFaction)
		{
			return FindAllByProperty("_houseFaction", (int)houseFaction);
		}

		public static IEnumerable<Auction> GetAuctionsForCharacter(uint charLowId)
		{
			return FindAllByProperty("_ownerEntityLowId", (int)charLowId);
		}

		public static IEnumerable<Auction> GetBidderAuctionsForCharacter(uint charLowId)
		{
			return FindAllByProperty("_bidderEntityLowId", (int)charLowId);
		}
	}
}