/*************************************************************************
 *
 *   file		: TradeHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-27 10:06:23 +0100 (on, 27 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1227 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Trade;

namespace WCell.RealmServer.Handlers
{
	public class TradeHandler
	{
		[ClientPacketHandler(RealmServerOpCode.CMSG_INITIATE_TRADE)]
		public static void HandleProposeTrade(IRealmClient client, RealmPacketIn packet)
		{
			var targetGuid = packet.ReadEntityId();
			var targetChr = World.GetCharacter(targetGuid.Low);

			if (TradeMgr.MayProposeTrade(client.ActiveCharacter, targetChr))
			{
				TradeMgr.Propose(client.ActiveCharacter, targetChr);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_BEGIN_TRADE)]
		public static void HandleBeginTrade(IRealmClient client, RealmPacketIn packet)
		{
			var trade = client.ActiveCharacter.TradeWindow;

			if (trade == null)
			{
				SendTradeStatus(client, TradeStatus.PlayerNotFound);

				return;
			}

			trade.AcceptTradeProposal();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_BUSY_TRADE)]
		public static void HandleBusyTrade(IRealmClient client, RealmPacketIn packet)
		{
			var tradeInfo = client.ActiveCharacter.TradeWindow;

			if (tradeInfo == null)
			{
				SendTradeStatus(client, TradeStatus.PlayerNotFound);

				return;
			}

			tradeInfo.DeclineBusy();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_IGNORE_TRADE)]
		public static void HandleIgnoreTrade(IRealmClient client, RealmPacketIn packet)
		{
			var tradeInfo = client.ActiveCharacter.TradeWindow;

			if (tradeInfo == null)
			{
				SendTradeStatus(client, TradeStatus.PlayerNotFound);

				return;
			}

			tradeInfo.DeclineIgnore();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_CANCEL_TRADE, IsGamePacket = false, RequiresLogin = false)]
		public static void HandleCancelTrade(IRealmClient client, RealmPacketIn packet)
		{
			if (client.ActiveCharacter == null || client.ActiveCharacter.TradeWindow == null)
			{
				return;
			}

			var tradeInfo = client.ActiveCharacter.TradeWindow;

			tradeInfo.Cancel();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_ACCEPT_TRADE)]
		public static void HandleAcceptTrade(IRealmClient client, RealmPacketIn packet)
		{
			var tradeInfo = client.ActiveCharacter.TradeWindow;

			if (tradeInfo == null)
			{
				return;
			}

			tradeInfo.AcceptTrade();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_UNACCEPT_TRADE)]
		public static void HandleUnacceptTrade(IRealmClient client, RealmPacketIn packet)
		{
			var tradeInfo = client.ActiveCharacter.TradeWindow;

			if (tradeInfo == null)
			{
				return;
			}

			tradeInfo.UnacceptTrade();
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_TRADE_GOLD)]
		public static void HandleSetTradeGold(IRealmClient client, RealmPacketIn packet)
		{
			var trade = client.ActiveCharacter.TradeWindow;

			if (trade == null)
				return;

			var money = packet.ReadUInt32();

			trade.SetMoney(money);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_TRADE_ITEM)]
		public static void HandleSetTradeItem(IRealmClient client, RealmPacketIn packet)
		{
			var trade = client.ActiveCharacter.TradeWindow;

			if (trade == null)
				return;

			var tradeSlot = packet.ReadByte();
			var bag = packet.ReadByte();
			var slot = packet.ReadByte();

			trade.SetTradeItem(tradeSlot, bag, slot);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_CLEAR_TRADE_ITEM)]
		public static void HandleClearTradeItem(IRealmClient client, RealmPacketIn packet)
		{
			var tradeInfo = client.ActiveCharacter.TradeWindow;

			if (tradeInfo == null)
				return;

			var tradeSlot = packet.ReadByte();

			tradeInfo.ClearTradeItem(tradeSlot);
		}

		public static void SendTradeProposal(IPacketReceiver client, Character initiater)
		{
			using (var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS))
			{
				pkt.WriteUInt((uint)TradeStatus.Proposed);
				pkt.Write(initiater.EntityId);

				client.Send(pkt);
			}
		}

		public static void SendTradeStatus(IPacketReceiver client, TradeStatus tradeStatus)
		{
			using (var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS))
			{
				pkt.WriteUInt((uint)tradeStatus);

				client.Send(pkt);
			}
		}

		public static void SendTradeProposalAccepted(IPacketReceiver client)
		{
			using (var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS))
			{
				pkt.WriteUInt((uint)TradeStatus.Proposed);
				pkt.Write(0);

				client.Send(pkt);
			}
		}

		/// <summary>
		/// Sends the new state of the trading window to other party
		/// </summary>
		/// <param name="client">receiving party</param>
		/// <param name="money">new amount of money</param>
		/// <param name="items">new items</param>
		public static void SendTradeUpdate(IPacketReceiver client, uint money, Item[] items)
		{
			using (var pkt = new RealmPacketOut(RealmServerOpCode.SMSG_TRADE_STATUS_EXTENDED))
			{
				pkt.WriteByte(1);
				pkt.Write(0);					// Trade id
				pkt.Write(items.Length);
				pkt.Write(items.Length);
				pkt.Write(money);
				pkt.Write(0);

				for (var i = 0; i < items.Length; i++)
				{
					pkt.WriteByte(i);
					var item = items[i];

					if (item != null)
					{
						pkt.Write(item.EntryId);
						pkt.Write(item.Template.DisplayId);
						pkt.Write(item.Amount);
						pkt.Write(0);

						pkt.Write(item.GiftCreator);

						var enchant = item.GetEnchantment(EnchantSlot.Permanent);
						pkt.Write(enchant != null ? enchant.Entry.Id : 0);

						pkt.Zero(4 * 3);

						pkt.Write(item.Creator);
						pkt.Write(item.SpellCharges);
						pkt.Write(item.Template.RandomSuffixFactor);
						pkt.Write(item.RandomPropertiesId);

						var itemLock = item.Lock;
						pkt.Write(itemLock != null ? itemLock.Id : 0);

						pkt.Write(item.MaxDurability);
						pkt.Write(item.Durability);
					}
					else
					{
						pkt.Zero(18 * 4);
					}
				}

				client.Send(pkt);
			}
		}
	}
}