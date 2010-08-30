using System.Collections.Generic;
using System.Linq;
using WCell.Constants;
using WCell.Constants.ArenaTeams;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.ArenaTeams;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Items;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Armorer;
using WCell.RealmServer.NPCs.Trainers;
using WCell.RealmServer.NPCs.Vendors;
using WCell.Constants.Guilds;

namespace WCell.RealmServer.Handlers
{
	public static class NPCHandler
	{
		#region Banker

		[ClientPacketHandler(RealmServerOpCode.CMSG_BANKER_ACTIVATE)]
		public static void HandleBankActivate(IRealmClient client, RealmPacketIn packet)
		{
			var bankerId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;

			var banker = chr.Region.GetObject(bankerId) as NPC;
			if (banker != null && banker.IsBanker && banker.CheckVendorInteraction(chr))
			{
				chr.OpenBank(banker);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_BUY_BANK_SLOT)]
		public static void HandleBuyBankSlot(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var inv = chr.Inventory;
			if (inv.IsBankOpen)
			{
				var result = inv.BankBags.IncBankBagSlotCount(true);
				if (result != BuyBankBagResponse.Ok)
				{
					SendBankSlotResult(chr, result);
				}
			}
			else
			{
				SendBankSlotResult(chr, BuyBankBagResponse.NotABanker);
			}
		}

		/// <summary>
		/// Auto-move item from Inventory to Bank
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_AUTOBANK_ITEM)]
		public static void HandleAutoDeposit(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var inv = chr.Inventory;
			if (inv.IsBankOpen)
			{
				var bagSlot = (InventorySlot)packet.ReadByte();
				var slot = packet.ReadByte();

				inv.Deposit(bagSlot, slot);
			}
			else
			{
				SendBankSlotResult(chr, BuyBankBagResponse.NotABanker);
			}
		}

		/// <summary>
		/// Auto-move item from Bank to Inventory
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_AUTOSTORE_BANK_ITEM)]
		public static void HandleAutoWithdrawal(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var inv = chr.Inventory;
			if (inv.IsBankOpen)
			{
				var bagSlot = (InventorySlot)packet.ReadByte();
				var slot = packet.ReadByte();

				inv.Withdraw(bagSlot, slot);
			}
			else
			{
				SendBankSlotResult(chr, BuyBankBagResponse.NotABanker);
			}
		}

		public static void SendBankSlotResult(Character chr, BuyBankBagResponse response)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BUY_BANK_SLOT_RESULT, 4))
			{
				packet.Write((uint)response);
				chr.Send(packet);
			}
		}

		#endregion

		#region Petitioner
		[ClientPacketHandler(RealmServerOpCode.CMSG_PETITION_SHOWLIST)]
		public static void HandlePetitionerShowList(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var npc = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;
            if (npc != null && npc.NPCFlags.HasFlag(NPCFlags.Petitioner))
			{
				npc.SendPetitionList(client.ActiveCharacter);
			}
		}

		public static void SendPetitionList(this NPC petitioner, Character chr)
		{
			using (
				var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PETITION_SHOWLIST, 8 + 4 * 6))
			{
				packet.Write(petitioner.EntityId);
				if (petitioner.IsGuildPetitioner)
				{
					// Guild petitioner
					packet.Write(1);
					packet.Write((uint)PetitionerEntry.GuildPetitionEntry.ItemId);
					packet.Write(PetitionerEntry.GuildPetitionEntry.DisplayId);
					packet.Write(PetitionerEntry.GuildPetitionEntry.Cost);
					packet.Write(0);
					packet.Write(PetitionerEntry.GuildPetitionEntry.RequiredSignatures);
				}
				else if (petitioner.IsArenaPetitioner)
				{
					// Arena petitioner
					chr.SendSystemMessage("Building arena packet.");
					//packet.Write(3);

					packet.Write((uint)PetitionerEntry.ArenaPetition2v2Entry.Index);
					packet.Write((uint)PetitionerEntry.ArenaPetition2v2Entry.ItemId);
					packet.Write((uint)PetitionerEntry.ArenaPetition2v2Entry.DisplayId);
					packet.Write((uint)PetitionerEntry.ArenaPetition2v2Entry.Cost);
					//packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures);
					//packet.Write(0);
					packet.Write((uint)PetitionerEntry.ArenaPetition2v2Entry.RequiredSignatures);

					packet.Write((uint)PetitionerEntry.ArenaPetition3v3Entry.Index);
					packet.Write((uint)PetitionerEntry.ArenaPetition3v3Entry.ItemId);
					packet.Write((uint)PetitionerEntry.ArenaPetition3v3Entry.DisplayId);
					packet.Write((uint)PetitionerEntry.ArenaPetition3v3Entry.Cost);
					//packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures);
					//packet.Write(0);
					packet.Write((uint)PetitionerEntry.ArenaPetition3v3Entry.RequiredSignatures);

					packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.Index);
					packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.ItemId);
					packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.DisplayId);
					packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.Cost);
					//packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures);
					//packet.Write(0);
					packet.Write((uint)PetitionerEntry.ArenaPetition5v5Entry.RequiredSignatures);
				}
				chr.Client.Send(packet);
			}
		}
		[PacketHandler(RealmServerOpCode.CMSG_PETITION_BUY)]
		public static void HandlePetitionBuy(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;

			var petitionerId = packet.ReadEntityId();
			var petitioner = chr.Region.GetObject(petitionerId) as NPC;

			//var petitionId = packet.ReadInt32();
			//var petitionCreator = packet.ReadEntityId();
			//packet.SkipBytes(4 + 8);
			packet.Position += 4 + 8;
			var name = packet.ReadCString().Trim();
			//var bodytext = packet.ReadCString().Trim();
			//var minSignatures = packet.ReadInt32();
			//var maxSignatures = packet.ReadInt32();
			//var deadline = packet.ReadInt32();
			//var issueDate = packet.ReadInt32();
			//var allowedGuildId = packet.ReadInt32();
			//var allowedClassMask = packet.ReadInt32();
			//var allowedRaceMask = packet.ReadInt32();
			//var allowedGender = packet.ReadInt32(); // wow is sexist... :(
			//var allowedMinLevel = packet.ReadInt32();
			//var allowedMaxLevel = packet.ReadInt32();            
			//packet.SkipBytes(4 * 10);
			//packet.Position += (7 * 8) + 2 + 1 + 8;
			packet.Position += 4 * 10;
			var choice = packet.ReadInt32();
			//var petitionType = packet.ReadInt32();
			//packet.SkipBytes(4);
			packet.Position += 4;

			if (petitioner != null && petitioner.IsPetitioner && petitioner.CheckVendorInteraction(chr))
			{
				ItemId itemId = 0;
				uint cost = 0;
                PetitionType type = PetitionType.None;

				if (petitioner.IsGuildPetitioner)
				{
					if (chr.IsInGuild)
					{
						return;
					}
					if (!GuildMgr.IsValidGuildName(name))
					{
						GuildHandler.SendResult(chr, GuildCommandId.CREATE, name, GuildResult.NAME_INVALID);
						return;
					}
					else if (GuildMgr.DoesGuildExist(name))
					{
						GuildHandler.SendResult(chr, GuildCommandId.CREATE, name, GuildResult.NAME_EXISTS);
						return;
					}
					itemId = PetitionerEntry.GuildPetitionEntry.ItemId;
					cost = GuildMgr.GuildCharterCost;
                    type = PetitionType.Guild;
				}
				else if (petitioner.IsArenaPetitioner)
				{
					switch (choice)
					{
						case 1:
							itemId = PetitionerEntry.ArenaPetition2v2Entry.ItemId;
							cost = PetitionerEntry.ArenaPetition2v2Entry.Cost;
                            type = PetitionType.Arena2vs2;
							break;
						case 2:
							itemId = PetitionerEntry.ArenaPetition3v3Entry.ItemId;
							cost = PetitionerEntry.ArenaPetition3v3Entry.Cost;
                            type = PetitionType.Arena3vs3;
							break;
						case 3:
							itemId = PetitionerEntry.ArenaPetition5v5Entry.ItemId;
							cost = PetitionerEntry.ArenaPetition5v5Entry.Cost;
                            type = PetitionType.Arena5vs5;
							break;
						default:
							return;
					}
                    if (!ArenaTeamMgr.IsValidArenaTeamName(name))
                    {
                        ArenaTeamHandler.SendResult(chr, ArenaTeamCommandId.CREATE, name, string.Empty, ArenaTeamResult.NAME_INVALID);
                        return;
                    }
                    else if (ArenaTeamMgr.DoesArenaTeamExist(name))
                    {
                        ArenaTeamHandler.SendResult(chr, ArenaTeamCommandId.CREATE, name, string.Empty, ArenaTeamResult.NAME_EXISTS);
                        return;
                    }
				}
				if (itemId != 0 && cost != 0 && type != PetitionType.None)
				{
					var templ = ItemMgr.GetTemplate(itemId);
					if (templ == null)
					{
						SendBuyError(chr, petitioner, itemId, BuyItemError.CantFindItem);
					}
					else if (chr.Money < cost)
					{
						SendBuyError(chr, petitioner, itemId, BuyItemError.NotEnoughMoney);
					}
                    else if (!PetitionRecord.CanBuyPetition(chr.EntityId.Low))
                    {
                        chr.SendSystemMessage("You can't buy another petition !");
                    }
					else
					{
						var slotId = chr.Inventory.FindFreeSlot(templ, 1);
						if (slotId.Container == null)
						{
							SendBuyError(chr, petitioner, itemId, BuyItemError.CantCarryAnymore);
						}
						else
						{
							var item = slotId.Container.AddUnchecked(slotId.Slot, templ, 1, true) as PetitionCharter;
                            item.Petition = new PetitionRecord(name, chr.EntityId.Low, item.EntityId.Low, type);
                            item.Petition.Create();

							chr.Money -= cost;
                            
							item.SetEnchantId(EnchantSlot.Permanent, item.EntityId.Low);                           

						}
					}
				}
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_PETITION_SHOW_SIGNATURES)]
		public static void HandlePetitionShowSigns(IRealmClient client, RealmPacketIn packet)
		{
            var petitionGuid = packet.ReadEntityId();
            var charter = client.ActiveCharacter.Inventory.GetItem(petitionGuid) as PetitionCharter;
            SendPetitionSignatures(client, charter); 
		}

		[PacketHandler(RealmServerOpCode.CMSG_PETITION_SIGN)]
		public static void HandlePetitionSign(IRealmClient client, RealmPacketIn packet)
	    {
			var petitionGuid = packet.ReadEntityId();
             
		}

        [PacketHandler(RealmServerOpCode.MSG_PETITION_RENAME)]
        public static void HandlePetitionRename(IRealmClient client, RealmPacketIn packet)
        {
            var petitionGuid = packet.ReadEntityId();
            var newName = packet.ReadCString().Trim();

            var charter = client.ActiveCharacter.Inventory.GetItem(petitionGuid) as PetitionCharter;
            var chr = client.ActiveCharacter;

                if(charter.Petition.Type == PetitionType.Guild)
                {
                    if (!GuildMgr.IsValidGuildName(newName))
                    {
                        GuildHandler.SendResult(chr, GuildCommandId.CREATE, newName, GuildResult.NAME_INVALID);
                        return;
                    }
                    else if (GuildMgr.DoesGuildExist(newName))
                    {
                        GuildHandler.SendResult(chr, GuildCommandId.CREATE, newName, GuildResult.NAME_EXISTS);
                        return;
                    }
                }
                else
                {
                    if (!ArenaTeamMgr.IsValidArenaTeamName(newName))
                    {
                        ArenaTeamHandler.SendResult(chr, ArenaTeamCommandId.CREATE, newName, string.Empty, ArenaTeamResult.NAME_INVALID);
                        return;
                    }
                    else if (ArenaTeamMgr.DoesArenaTeamExist(newName))
                    {
                        ArenaTeamHandler.SendResult(chr, ArenaTeamCommandId.CREATE, newName, string.Empty, ArenaTeamResult.NAME_EXISTS);
                        return;
                    }
                }

            charter.Petition.Name = newName;
            charter.Petition.Update();

            SendPetitionRename(client, charter);
        }
		[PacketHandler(RealmServerOpCode.MSG_PETITION_DECLINE)]
		public static void HandlePetitionDecline(IRealmClient client, RealmPacketIn packet)
		{
			var petitionGuid = packet.ReadEntityId();

            //SendPetitionDecline(client, petition);
		}

		[PacketHandler(RealmServerOpCode.CMSG_OFFER_PETITION)]
		public static void HandlePetitionOffer(IRealmClient client, RealmPacketIn packet)
		{
			// TODO: 
		}

		[PacketHandler(RealmServerOpCode.CMSG_TURN_IN_PETITION)]
		public static void HandlePetitionTurnIn(IRealmClient client, RealmPacketIn packet)
		{
            var petitionGuid = packet.ReadEntityId();
            var petition = client.ActiveCharacter.Inventory.GetItem(petitionGuid) as PetitionCharter;
            var name = petition.Petition.Name;
            var type = petition.Petition.Type;

            if (petition.Petition.SignedIds.Count < ((uint)type - 1))
            {
                SendPetitionTurnInResults(client, PetitionTurns.NEED_MORE_SIGNATURES);
                return;
            }
            if (type == PetitionType.Guild && client.ActiveCharacter.IsInGuild)
            {
                SendPetitionTurnInResults(client, PetitionTurns.ALREADY_IN_GUILD);
                return;
            }
            else if (client.ActiveCharacter.ArenaTeamMember[(uint)ArenaTeamMgr.GetSlotByType((uint)type)] != null)
            {
                ArenaTeamHandler.SendResult(client, ArenaTeamCommandId.CREATE, name, string.Empty, ArenaTeamResult.ALREADY_IN_ARENA_TEAM);
                return;
            }
            else if (type == PetitionType.Guild && GuildMgr.DoesGuildExist(name))
            {
                GuildHandler.SendResult(client, GuildCommandId.CREATE, name, GuildResult.NAME_EXISTS);
                return;
            }
            else if (ArenaTeamMgr.DoesArenaTeamExist(name))
            {
                ArenaTeamHandler.SendResult(client, ArenaTeamCommandId.CREATE, name, string.Empty, ArenaTeamResult.NAME_EXISTS);
                return;
            }
            else
            {
                petition.Destroy();
                if (type == PetitionType.Guild)
                {
                    var guild = new Guild(client.ActiveCharacter.Record, name);
                    foreach (var chr in petition.Petition.SignedIds)
                    {
                        if(chr == 0)
                            continue;
                        else
                        {
                            var character = World.GetCharacter(chr);
                            guild.AddMember(character);
                        }
                    }
                }
                else
                {
                    var team = new ArenaTeam(client.ActiveCharacter.Record, name, (uint)type);
                    foreach (var chr in petition.Petition.SignedIds)
                    {
                        if(chr == 0)
                            continue;
                        else
                        {
                            var character = World.GetCharacter(chr);
                            team.AddMember(character);
                        }
                    }
                }

                SendPetitionTurnInResults(client, PetitionTurns.OK);
            }
		}

		[PacketHandler(RealmServerOpCode.CMSG_PETITION_QUERY)]
		public static void HandlePetitionQuery(IRealmClient client, RealmPacketIn packet)
		{
            var petitionLowId = packet.ReadUInt32();
            var petitionGuid = packet.ReadEntityId();

            var charter = client.ActiveCharacter.Inventory.GetItem(petitionGuid) as PetitionCharter;
            SendPetitionQueryResponse(client, charter);
		}

		public static void SendPetitionSignatures(IPacketReceiver client, PetitionCharter charter)
		{
            var signs = charter.Petition.SignedIds;
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PETITION_SHOW_SIGNATURES, 8+8+4+1+signs.Count*12))
			{
                packet.WriteULong(charter.EntityId.Full);
                packet.WriteULong(charter.Owner.EntityId.Full);
                packet.WriteUInt(charter.EntityId.Low);
                packet.WriteByte(signs.Count);
                
                foreach(var guid in signs)
                {
                    packet.WriteULong(guid);
                    packet.WriteUInt(0);
                }

				client.Send(packet);
			}
		}

		public static void SendPetitionSignResults(IPacketReceiver client, GuildTabardResult result)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PETITION_SIGN_RESULTS))
			{
				// TODO: 
				client.Send(packet);
			}
		}

		public static void SendPetitionDecline(IPacketReceiver client, PetitionRecord record)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_PETITION_DECLINE))
			{
                
				// TODO
				client.Send(packet);
			}
		}

		public static void SendPetitionTurnInResults(IPacketReceiver client, PetitionTurns result)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TURN_IN_PETITION_RESULTS, 4))
			{
				packet.WriteUInt((uint)result); 
				client.Send(packet);
			}
		}

        public static void SendPetitionRename(IPacketReceiver client, PetitionCharter petition)
        {
            var name = petition.Petition.Name;
            using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_PETITION_RENAME, 8 + name.Length + 1))
            {
                packet.WriteULong(petition.EntityId.Full);
                packet.WriteCString(name);

                client.Send(packet);
            }
        }

		public static void SendPetitionQueryResponse(IPacketReceiver client, PetitionCharter charter)
		{
            string name = charter.Petition.Name;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PETITION_QUERY_RESPONSE))
			{
                packet.WriteUInt(charter.EntityId.Low);
                packet.WriteULong(charter.Owner.EntityId.Full);
                packet.WriteCString(name);
                packet.WriteByte(0);

                var type = (uint)charter.Petition.Type;
                if(type == (uint)PetitionType.Guild)
                {
                    packet.WriteUInt(type);
                    packet.WriteUInt(type);
                    packet.WriteUInt(0);
                }
                else
                {
                    packet.WriteUInt(type-1);
                    packet.WriteUInt(type-1);
                    packet.WriteUInt(type);
                }
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUShort(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);
                packet.WriteUInt(0);

                for(int i = 0; i < 10; ++i)
                    packet.WriteByte(0);

                packet.WriteUInt(0);

                if(type == (uint)PetitionType.Guild)
                    packet.WriteUInt(0);
                else
                    packet.WriteUInt(1);

                client.Send(packet);
			}
		}
		#endregion

		#region Vendor
		[ClientPacketHandler(RealmServerOpCode.CMSG_LIST_INVENTORY)]
		public static void HandleVendorListInventory(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var vendor = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;
			if (vendor != null && vendor.IsVendor)
			{
				vendor.VendorEntry.UseVendor(client.ActiveCharacter);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_SELL_ITEM)]
		public static void HandleSellItem(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var itemId = packet.ReadEntityId();
			var numToSell = packet.ReadByte();

			var chr = client.ActiveCharacter;
			var vendor = chr.Region.GetObject(vendorId) as NPC;

			if (vendor != null && vendor.IsVendor)
			{
				var itemToSell = chr.Inventory.GetItem(itemId, false);
                if (itemToSell != null)
                {
                    vendor.VendorEntry.SellItem(client.ActiveCharacter, itemToSell, numToSell);
                }
                else
                {
                    SendSellError(client, vendorId, itemId, SellItemError.CantFindItem);
                }
			}
			else
			{
			    SendSellError(client, vendorId, itemId, SellItemError.CantFindVendor);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_BUYBACK_ITEM)]
		public static void HandleBuyBackItem(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var slot = packet.ReadInt32();

			var vendor = client.ActiveCharacter.Region.GetObject(vendorId) as NPC;
			if (vendor != null && vendor.IsVendor)
			{
				//client.ActiveCharacter.SendMessage("Buyback is temporarily disabled.");
				vendor.VendorEntry.BuyBackItem(client.ActiveCharacter, slot);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_BUY_ITEM_IN_SLOT)]
		public static void HandleBuyItemInSlot(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var itemEntryId = packet.ReadUInt32();
		    var slot = packet.ReadUInt32();
			var bagId = packet.ReadEntityId();
		    var bagSlot = packet.ReadByte();
            var amount = packet.ReadInt32();

			var chr = client.ActiveCharacter;
			var vendor = chr.Region.GetObject(vendorId) as NPC;
			if (vendor != null && vendor.IsVendor)
			{
				BaseInventory inv;
				if (bagId.High == HighId.Item)
				{
					var bag = chr.Inventory.GetItemByLowId(bagId.Low) as Container;
					if (bag != null)
					{
						inv = bag.BaseInventory;
					}
					else
					{
						// invalid Container
						return;
					}
				}
				else
				{
					inv = chr.Inventory;
				}
				vendor.VendorEntry.BuyItem(chr, itemEntryId, inv, amount, (int)slot);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_BUY_ITEM)]
		public static void HandleBuyItem(IRealmClient client, RealmPacketIn packet)
		{
			var vendorId = packet.ReadEntityId();
			var itemEntryId = packet.ReadUInt32();
            //var slot = packet.ReadUInt32();			// slot in the vendor list
			packet.ReadFloat();						// unknown float
            //var count = packet.ReadUInt32();
			var amount = packet.ReadInt32();

			var chr = client.ActiveCharacter;
			var vendor = chr.Region.GetObject(vendorId) as NPC;
			if (vendor != null && vendor.IsVendor)
			{
				vendor.VendorEntry.BuyItem(chr, itemEntryId, chr.Inventory, amount, BaseInventory.INVALID_SLOT);
			}
		}

		public static void SendNPCError(IPacketReceiver client, IEntity vendor, VendorInventoryError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LIST_INVENTORY, 10))
			{
				packet.Write(vendor.EntityId);
				packet.Write((byte)0);
				packet.Write((byte)error);
			}
		}

		/// <summary>
		/// Send the vendor's list of items for sale to the client. 
		/// *All allowable race and class checks should be done prior to calling this method.
		/// *All checks on number-limited items should also be done prior to calling this method.
		/// *This method can handle up to 256 items per vendor. If you try to send more items than that, 
		/// this method will send only the first 256. 
		/// </summary>
		/// <param name="client">The client to send the packet to.</param>
		/// <param name="itemsForSale">An array of items to send to the client.</param>
		public static void SendVendorInventoryList(Character buyer, NPC vendor, List<VendorItemEntry> itemsForSale)
		{
			var numItems = (itemsForSale.Count <= 0xFF) ? itemsForSale.Count : 0xFF;

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LIST_INVENTORY, 10 + (28 * numItems)))
			{
				packet.Write(vendor.EntityId);
				packet.Write((byte)numItems);
				if (numItems == 0)
				{
					packet.Write((byte)VendorInventoryError.NoInventory);
				}
				else
				{
					for (var i = 0; i < numItems; ++i)
					{
						// Write in the item number (1 - 256)
						packet.Write(i + 1);

						var item = itemsForSale[i];
						var price = buyer.Reputations.GetDiscountedCost(vendor.Faction.ReputationIndex, item.Template.BuyPrice);

						packet.Write(item.Template.Id);
						packet.Write(item.Template.DisplayId);
						packet.Write(item.RemainingStockAmount);
						packet.Write(price);
						packet.Write(item.Template.MaxDurability);
						packet.Write(item.BuyStackSize);
						packet.Write(item.ExtendedCostId);
					}
				}
				buyer.Send(packet);
			}
		}

		/// <summary>
		/// Sends a sell-error packet to the client
		/// </summary>
		/// <param name="client">The IRealmClient to send the error to.</param>
		/// <param name="error">A SellItemError</param>
		public static void SendSellError(IPacketReceiver client, EntityId vendorId, EntityId itemId, SellItemError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SELL_ITEM, 8 + 8 + 1))
			{
				packet.Write(vendorId);
				packet.Write(itemId);
				packet.Write((byte)error);

				client.Send(packet);
			}
		}

		public static void SendBuyError(IPacketReceiver client, IEntity vendor, ItemId itemEntryId, BuyItemError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BUY_FAILED, 8 + 4 + 1))
			{
				packet.Write(vendor.EntityId);
				packet.Write((uint)itemEntryId);
				packet.Write((byte)error);

				client.Send(packet);
			}
		}

		public static void SendBuyItem(IPacketReceiver client, IEntity vendor, ItemId itemId, int numItemsPurchased)
        {
			SendBuyItem(client, vendor, itemId, numItemsPurchased, 0);
        }

		public static void SendBuyItem(IPacketReceiver client, IEntity vendor, ItemId itemId, int numItemsPurchased, int remainingAmount)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BUY_ITEM, 8 + 4 + 4 + 4))
			{
				packet.Write(vendor.EntityId);
				packet.Write((uint)itemId);
				packet.Write(numItemsPurchased);
                packet.Write(remainingAmount);

				client.Send(packet);
			}
		}

		#endregion

		#region Trainer

		[ClientPacketHandler(RealmServerOpCode.CMSG_TRAINER_LIST)]
		public static void HandleListTrainerSpells(IRealmClient client, RealmPacketIn packet)
		{
			var trainerId = packet.ReadEntityId();

			var trainer = client.ActiveCharacter.Region.GetObject(trainerId) as NPC;
			if (trainer != null)
			{
				trainer.TalkToTrainer(client.ActiveCharacter);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_TRAINER_BUY_SPELL)]
		public static void HandleBuyTrainerSpell(IRealmClient client, RealmPacketIn packet)
		{
			var trainerId = packet.ReadEntityId();
			var spellEntryId = (SpellId)packet.ReadUInt32();

			var trainer = client.ActiveCharacter.Region.GetObject(trainerId) as NPC;
			if (trainer != null)
			{
				trainer.BuySpell(client.ActiveCharacter, spellEntryId);
			}
		}

        public static void SendTrainerBuyFailed(this NPC trainer, IRealmClient client, int serviceType, TrainerBuyError error)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRAINER_BUY_FAILED))
            {
                packet.Write(trainer.EntityId);
                packet.Write(serviceType);
                packet.Write((int) error);
            }
        }

		public static void SendTrainerList(this NPC trainer, Character chr, IEnumerable<TrainerSpellEntry> spells,
										   string msg)
		{
			using (
				var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRAINER_LIST, 8 + 4 + 4 + (30 * 38) + msg.Length + 1))
			{
				packet.Write(trainer.EntityId);
				packet.Write((uint)trainer.TrainerEntry.TrainerType);

				var countPos = packet.Position;
				packet.Position += 4;

				var spellCount = 0;
				foreach (var trainerSpell in spells)
				{
					if (!chr.CanLearn(trainerSpell))
					{
						continue;
					}

					var spell = trainerSpell.Spell;
					if (spell.IsTeachSpell)
					{
						spell = spell.LearnSpell;
					}

					//packet.Position = offset  + (spell.Index * entryLength);
					packet.Write(trainerSpell.Spell.Id);
					packet.Write((byte)trainerSpell.GetTrainerSpellState(chr));
					packet.Write(trainerSpell.GetDiscountedCost(chr, trainer));
					packet.Write(spell.Talent != null ? 1u : 0u);						// talent cost
					packet.Write(trainerSpell.Spell.IsProfession && trainerSpell.Spell.TeachesApprenticeAbility ? 1 : 0);	// Profession cost
					packet.Write((byte)trainerSpell.RequiredLevel);
					packet.Write((uint)trainerSpell.RequiredSkillId);
					packet.Write(trainerSpell.RequiredSkillAmount);
					packet.Write((uint)trainerSpell.RequiredSpellId);

					// The following are infrequent Ids of some sort - Possibly spell replacements?
					packet.Write(0u);
					packet.Write(0u);
					++spellCount;
				}

				packet.Write(msg);

				packet.Position = countPos;
				packet.Write(spellCount);

				chr.Send(packet);
			}
		}

		public static void SendTrainerBuySucceeded(IPacketReceiver client, NPC trainer, TrainerSpellEntry spell)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRAINER_BUY_SUCCEEDED, 8 + 4))
			{
				packet.Write(trainer.EntityId);
				packet.Write(spell.Spell.Id);

				client.Send(packet);
			}
		}

		#endregion

		#region Armorer

		[PacketHandler(RealmServerOpCode.CMSG_REPAIR_ITEM)]
		public static void HandleRepair(IRealmClient client, RealmPacketIn packet)
		{
			var armorerId = packet.ReadEntityId();
			var itemId = packet.ReadEntityId();
			var useGuildFunds = packet.ReadBoolean();

			var armorer = client.ActiveCharacter.Region.GetObject(armorerId) as NPC;
			ArmorerMgr.RepairItem(client, armorer, itemId, useGuildFunds);
		}

		#endregion

		#region InnKeeper

		[ClientPacketHandler(RealmServerOpCode.CMSG_BINDER_ACTIVATE)]
		public static void HandleBinderActivate(IRealmClient client, RealmPacketIn packet)
		{
			var binderId = packet.ReadEntityId();
			var binder = client.ActiveCharacter.Region.GetObject(binderId) as NPC;

			if (binder != null)
			{
				client.ActiveCharacter.TryBindTo(binder);
			}
		}

		public static void SendBindConfirm(Character chr, WorldObject binder, ZoneId zone)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BINDER_CONFIRM, 8 + 4))
			{
				packet.Write(binder.EntityId);
				packet.Write((uint)zone);

				chr.Client.Send(packet);
			}
		}

		public static void SendPlayerBound(Character chr, WorldObject binder, ZoneId zone)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAYERBOUND, 8 + 4))
			{
				packet.Write(binder.EntityId);
				packet.Write((uint)zone);

				chr.Client.Send(packet);
			}
		}

		#endregion
	}
}