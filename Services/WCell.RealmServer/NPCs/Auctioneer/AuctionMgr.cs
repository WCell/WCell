using System;
using System.Linq;
using System.Collections.Generic;
using WCell.RealmServer.Database.Entities;
using WCell.Util.Collections;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Mail;

namespace WCell.RealmServer.NPCs.Auctioneer
{
	public class AuctionMgr : Manager<AuctionMgr>
	{
		/// <summary>
		/// Whether Alliance characters can list on Horde Auctionhouses and vice-versa.
		/// </summary>
		public static bool AllowInterFactionAuctions;
		/// <summary>
		/// The percent of the auctioned item's value used to calculate the required auctionhouse deposit.
		/// This value is used for Alliance/Horde auctionhouses only.
		/// </summary>
		public static uint FactionHouseDepositPercent = 15;
		/// <summary>
		/// The percent of the auctioned item's value used to calculate the required auctionhouse deposit.
		/// This value is used for Neutral auctionhouses only.
		/// </summary>
		public static uint NeutralHouseDepositPercent = 75;
		/// <summary>
		/// The multiplier used to calculate the cut the auctionhouse takes from your auction winnings.
		/// Default = 1.00
		/// </summary>
		public static float AuctionHouseCutRate = 1.00f;

		/// <summary>
		/// All auctions listed in the Alliance Auctionhouses.
		/// </summary>
		public AuctionHouse AllianceAuctions;
		/// <summary>
		/// All auctions listed in the Horde Auctionhouses.
		/// </summary>
		public AuctionHouse HordeAuctions;
		/// <summary>
		/// All auctions listed in the Neutral auctionhouses.
		/// </summary>
		public AuctionHouse NeutralAuctions;

		private SynchronizedDictionary<uint, ItemRecord> _auctionedItems;

		private bool _hasItemLoaded = false;

		#region Manager
		[Initialization(InitializationPass.Fifth, "Initialize Auctions")]
		public static void Initialize()
		{
			Instance.Start();
		}

		protected bool Start()
		{
			_auctionedItems = new SynchronizedDictionary<uint, ItemRecord>(10000);

			if (AllowInterFactionAuctions)
			{
				NeutralAuctions = new AuctionHouse();
				AllianceAuctions = NeutralAuctions;
				HordeAuctions = NeutralAuctions;
			}
			else
			{
				AllianceAuctions = new AuctionHouse();
				HordeAuctions = new AuctionHouse();
				NeutralAuctions = new AuctionHouse();
			}

#if DEBUG
			try
			{
#endif
				FetchAuctions();
#if DEBUG
			}
			catch (Exception e)
			{
				RealmDBMgr.OnDBError(e);
				FetchAuctions();
			}
#endif
			return true;
		}

		protected AuctionMgr()
		{
		}

		private void FetchAuctions()
		{
			if (AllowInterFactionAuctions)
			{
				foreach (var auction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Neutral))
				{
					NeutralAuctions.AddAuction(auction);
				}
			}
			else
			{
				foreach (var auction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Alliance))
				{
					AllianceAuctions.AddAuction(auction);
				}
				foreach (var auction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Horde))
				{
					HordeAuctions.AddAuction(auction);
				}
				foreach (var auction in Auction.GetAffiliatedAuctions(AuctionHouseFaction.Neutral))
				{
					NeutralAuctions.AddAuction(auction);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void LoadItems()
		{
			if (!_hasItemLoaded)
			{
				try
				{
					foreach (var itemRecord in ItemRecord.LoadAuctionedItems())
					{
						_auctionedItems.Add(itemRecord.EntityLowId, itemRecord);
					}
					_hasItemLoaded = true;
				}
				catch (Exception e)
				{
					RealmDBMgr.OnDBError(e);
					foreach (var itemRecord in ItemRecord.LoadAuctionedItems())
					{
						_auctionedItems.Add(itemRecord.EntityLowId, itemRecord);
					}
					_hasItemLoaded = true;
				}
			}
		}

		public SynchronizedDictionary<uint, ItemRecord> AuctionItems
		{
			get
			{
				return _auctionedItems;
			}
		}

		public bool HasItem(uint itemid)
		{
			return _auctionedItems.ContainsKey(itemid);
		}

		public bool HasItemLoaded
		{
			get { return _hasItemLoaded; }
			set { _hasItemLoaded = value; }
		}

		public void RemoveAuction(Auction auction)
		{
			AllianceAuctions.RemoveAuction(auction);
			HordeAuctions.RemoveAuction(auction);
			NeutralAuctions.RemoveAuction(auction);
		}
		#endregion

		public void AuctionHello(Character chr, NPC auctioneer)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			AuctionHandler.SendAuctionHello(chr.Client, auctioneer);
		}

		public void AuctionSellItem(Character chr, NPC auctioneer, EntityId itemId, uint bid, uint buyout, uint time, uint stackSize)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			var item = chr.Inventory.GetItem(itemId, false);

			var msg = AuctionCheatChecks(auctioneer, item, bid, time);
			if (msg == AuctionError.Ok)
			{
			    // Check that character has enough money to cover the deposit
			    var houseFaction = auctioneer.AuctioneerEntry.LinkedHouseFaction;
			    var deposit = GetAuctionDeposit(item, houseFaction, time);
			    if (chr.Money < deposit)
			    {
			        AuctionHandler.SendAuctionCommandResult(chr.Client, null, AuctionAction.SellItem, AuctionError.NotEnoughMoney);
			        return;
			    }

			    if (item.Amount > stackSize)
			        item = item.Split((int) stackSize);

			    if (item == null)
			    {
			        AuctionHandler.SendAuctionCommandResult(chr.Client, null, AuctionAction.SellItem, AuctionError.ItemNotFound);
			        return;
			    }


			    // Charge the deposit to the character
			    chr.Money -= deposit;


			    // Create the new Auction and add it to the list.

			    var newAuction = new Auction
			                         {
			                             BidderLowId = 0,
			                             BuyoutPrice = buyout,
			                             CurrentBid = bid,
			                             Deposit = deposit,
			                             HouseFaction = houseFaction,
			                             ItemLowId = item.EntityId.Low,
			                             ItemTemplateId = item.Template.Id,
			                             OwnerLowId = chr.EntityId.Low,
			                             TimeEnds = DateTime.Now.AddMinutes(time),
			                             IsNew = true
			                         };



			    //save new auction to database and add item to items container
			    RealmServer.IOQueue.AddMessage(new Util.Threading.Message(() =>
			                                                                  {
			                                                                      ItemRecord record = item.Record;
			                                                                      record.IsAuctioned = true;
			                                                                      record.Save();
			                                                                      auctioneer.AuctioneerEntry.Auctions.AddAuction(
			                                                                          newAuction);
			                                                                      AuctionItems.Add(newAuction.ItemLowId, record);
			                                                                      item.Remove(false);
			                                                                      AuctionListOwnerItems(chr, auctioneer);
			                                                                  }));

			    // Send the all-good message
			    AuctionHandler.SendAuctionCommandResult(chr.Client, newAuction, AuctionAction.SellItem, AuctionError.Ok);

			}
			else
			{
				AuctionHandler.SendAuctionCommandResult(chr.Client, null, AuctionAction.SellItem, msg);
			}
		}

		public void AuctionPlaceBid(Character chr, NPC auctioneer, uint auctionId, uint bid)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			Auction auction = null;
			if (!auctioneer.AuctioneerEntry.Auctions.TryGetAuction(auctionId, out auction))
			{
				AuctionHandler.SendAuctionCommandResult(chr.Client, null, AuctionAction.PlaceBid, AuctionError.InternalError);
				return;
			}

			var msg = AuctionBidChecks(auction, chr, bid);
			if (msg != AuctionError.Ok)
			{
				AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.PlaceBid, msg);
				return;
			}

			if (bid < auction.BuyoutPrice || (auction.BuyoutPrice == 0))
			{
				if (auction.BidderLowId == chr.EntityId)
				{
					chr.Money -= (bid - auction.CurrentBid);
				}
				else
				{
					chr.Money -= bid;

					// Send a mail to the outbid character with their bid money
					SendOutbidMail(auction, bid);
				}

				auction.BidderLowId = chr.EntityId.Low;
				auction.CurrentBid = bid;

				AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.PlaceBid, AuctionError.Ok);
			}
			else
			{
				// This is a buyout
				if (auction.BidderLowId == chr.EntityId)
				{
					chr.Money -= (auction.BuyoutPrice - auction.CurrentBid);
				}
				else
				{
					chr.Money -= auction.BuyoutPrice;
					if (auction.BidderLowId != auction.OwnerLowId)
					{
						// Someone had already bid on this item, send them a rejection letter
						SendOutbidMail(auction, auction.BuyoutPrice);
					}
				}
				auction.BidderLowId = chr.EntityId.Low;
				auction.CurrentBid = auction.BuyoutPrice;

				SendAuctionSuccessfullMail(auction);
				SendAuctionWonMail(auction);

				AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.PlaceBid, AuctionError.Ok);

				auctioneer.AuctioneerEntry.Auctions.RemoveAuction(auction);
			}
		}

		public void CancelAuction(Character chr, NPC auctioneer, uint auctionId)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			Auction auction;
			if (auctioneer.AuctioneerEntry.Auctions.TryGetAuction(auctionId, out auction))
			{
				if (auction.OwnerLowId == chr.EntityId.Low)
				{
					var itemRecord = ItemRecord.GetRecordByID(auction.ItemLowId);
					if (itemRecord != null)
					{
						if (auction.BidderLowId != auction.OwnerLowId)
						{
							// someone has bid on the item already, his money must be returned 
							// and the cancelling player must be charged the auctionhouse's cut
							var auctionhouseCut = CalcAuctionCut(auction.HouseFaction, auction.CurrentBid);
							if (chr.Money < auctionhouseCut)
							{
								// the player is no-good for the money, the auction cannot be cancelled
								AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.CancelAuction, AuctionError.NotEnoughMoney);
								return;
							}

							//var bidder = WorldMgr.GetCharacter(auction.BidderLowId);
							//if (bidder != null)
							//{
							// send bidder a notification?
							//}
							auction.SendMail(MailAuctionAnswers.CancelledToBidder, auction.CurrentBid);
							chr.Money -= auctionhouseCut;
						}

						// return the item to the seller via mail
						auction.SendMail(MailAuctionAnswers.Cancelled, itemRecord);
						auctioneer.AuctioneerEntry.Auctions.RemoveAuction(auction);
					}
					else
					{
						// auction contains a non-existant item
						AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.CancelAuction, AuctionError.ItemNotFound);
						return;
					}
				}
				else
				{
					// trying to cancel someone else's auction -- CHEATER!
					AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.CancelAuction, AuctionError.ItemNotFound);
					return;
				}
			}
			else
			{
				// auction not found -- CHEATER?
				AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.CancelAuction, AuctionError.ItemNotFound);
				return;
			}

			AuctionHandler.SendAuctionCommandResult(chr.Client, auction, AuctionAction.CancelAuction, AuctionError.Ok);
			auctioneer.AuctioneerEntry.Auctions.RemoveAuction(auction);
		}

		public void AuctionListOwnerItems(Character chr, NPC auctioneer)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			Auction[] auctions = Auction.GetAuctionsForCharacter(chr.EntityId.Low).ToArray();
			AuctionHandler.SendAuctionListOwnerItems(chr.Client, auctions);

		}

		public void AuctionListBidderItems(Character chr, NPC auctioneer)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			Auction[] auctions = Auction.GetBidderAuctionsForCharacter(chr.EntityId.Low).ToArray();
			AuctionHandler.SendAuctionListBidderItems(chr.Client, auctions);
		}

		public void AuctionListItems(Character chr, NPC auctioneer, AuctionSearch searcher)
		{
			if (!DoAuctioneerInteraction(chr, auctioneer))
				return;

			Auction[] auctions = searcher.RetrieveMatchedAuctions(auctioneer.AuctioneerEntry.Auctions).ToArray();
			AuctionHandler.SendAuctionListItems(chr.Client, auctions);
		}

		private static void SendOutbidMail(Auction auction, uint newBid)
		{
			if (auction == null)
				return;

			// is the old bidder online?
			var oldBidder = World.GetCharacter(auction.BidderLowId);
			if (oldBidder != null)
			{
				AuctionHandler.SendAuctionOutbidNotification(oldBidder.Client,
															 auction,
															 newBid,
															 GetMinimumNewBidIncrement(auction));
			}

			auction.SendMail(MailAuctionAnswers.Outbid, auction.CurrentBid);
		}

		private static void SendAuctionSuccessfullMail(Auction auction)
		{
			if (auction == null)
				return;

			var auctionhouseCut = CalcAuctionCut(auction.HouseFaction, auction.CurrentBid);
			var body = String.Format("{0,16:X}:{1,16:D}:0:{2,16:D}:{3,16:D}",
									 auction.BidderLowId,
									 auction.CurrentBid,
									 auction.Deposit,
									 auctionhouseCut);
			var profit = auction.CurrentBid + auction.Deposit - auctionhouseCut;

			auction.SendMail(MailAuctionAnswers.Successful, profit, body);
		}

		private static void SendAuctionWonMail(Auction auction)
		{
			if (auction == null)
				return;
			var body = String.Format("{0,16:X}:{1,16:D}:{2,16:D}", auction.OwnerLowId, auction.CurrentBid, auction.BuyoutPrice);
			var record = ItemRecord.GetRecordByID(auction.ItemLowId);
			if (record != null)
			{
				auction.SendMail(MailAuctionAnswers.Won, 0, record, body);
				// log it?
			}
		}

		private static bool DoAuctioneerInteraction(Character chr, NPC auctioneer)
		{
			if (!auctioneer.IsAuctioneer || !auctioneer.CheckVendorInteraction(chr))
				return false;

			chr.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);
			return true;
		}

		private AuctionError AuctionCheatChecks(NPC auctioneer, Item item, uint bid, uint time)
		{
			if (bid == 0 || time == 0)
			{
				return AuctionError.InternalError;
			}

			if (item == null)
			{
				return AuctionError.ItemNotFound;
			}

			if (IsAlreadyAuctioned(auctioneer, item) ||
				(item.IsContainer && !((Container)item).BaseInventory.IsEmpty) ||
				!item.CanBeTraded ||
				item.Duration > 0 ||
				item.IsConjured)
			{
				return AuctionError.InternalError;
			}

			return AuctionError.Ok;
		}

		private AuctionError AuctionBidChecks(Auction auction, Character chr, uint bid)
		{
			// Cannot bid on your own auction
			if (auction.OwnerLowId == chr.EntityId)
			{
				return AuctionError.CannotBidOnOwnAuction;
			}

            if (!chr.GodMode)
            {
                // Cannot bid on your alt's auctions
                var record = chr.Account.GetCharacterRecord(auction.OwnerLowId);
                if (record != null)
                {
                    return AuctionError.CannotBidOnOwnAuction;
                }
            }

		    if (bid < GetMinimumNewBid(auction))
			{
				// the Client checks this so if you get here, you are a CHEATER!!!
				// CHEATER!!!
				return AuctionError.InternalError;
			}

			if (chr.Money < bid)
			{
				// the Client checks this so if you get here, your are a CHEATER!!!
				// CHEATER!!!
				return AuctionError.InternalError;
			}

			return AuctionError.Ok;
		}

		private bool IsAlreadyAuctioned(NPC auctioneer, ILootable item)
		{
			if (item == null)
				return true;

			var itemId = item.EntityId.Low;

			//if (AllowInterFactionAuctions)
			//    return NeutralAuctions.HasItemById(itemId);

			//if (auctioneer.AuctioneerEntry.Auctions.HasItemById(itemId))
			//    return true;

			//switch (auctioneer.AuctioneerEntry.LinkedHouseFaction)
			//{
			//    case AuctionHouseFaction.Alliance:
			//        return NeutralAuctions.HasItemById(itemId) || HordeAuctions.HasItemById(itemId);
			//    case AuctionHouseFaction.Horde:
			//        return NeutralAuctions.HasItemById(itemId) || AllianceAuctions.HasItemById(itemId);
			//    case AuctionHouseFaction.Neutral:
			//        return AllianceAuctions.HasItemById(itemId) || HordeAuctions.HasItemById(itemId);
			//    default:
			//        return true;
			//}
			return AuctionItems.ContainsKey(itemId);
		}

		private static uint GetAuctionDeposit(Item item, AuctionHouseFaction houseFaction, uint timeInMin)
		{
			if (item == null)
				return 0;

			var percent = FactionHouseDepositPercent;
			if (houseFaction == AuctionHouseFaction.Neutral && !AllowInterFactionAuctions)
				percent = NeutralHouseDepositPercent;

			var sellPrice = item.Template.SellPrice * (uint)item.Amount;
			return ((sellPrice * percent) / 100) * (timeInMin / (12 * 60)); // deposit is per 12 hour interval
		}

		public static uint GetMinimumNewBid(Auction auction)
		{
			// Bids must increase by the larger of 5% or 1 copper each time.
            return auction.CurrentBid + GetMinimumNewBidIncrement(auction);
		}

        public static uint GetMinimumNewBidIncrement(Auction auction)
        {
            uint minimumIncreaseForNextBid = 0;
            if (auction.CurrentBid > 0)
            {
                minimumIncreaseForNextBid = (auction.CurrentBid / 100) * 5;
                minimumIncreaseForNextBid = Math.Max(1, minimumIncreaseForNextBid);
            }
            return minimumIncreaseForNextBid;
        }

		private static uint CalcAuctionCut(AuctionHouseFaction houseFaction, uint bid)
		{
			if (houseFaction == AuctionHouseFaction.Neutral && !AllowInterFactionAuctions)
			{
				return ((uint)(0.15f * bid * AuctionHouseCutRate));
			}
			return ((uint)(0.05f * bid * AuctionHouseCutRate));
		}
	}
}