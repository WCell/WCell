using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.Util;

namespace WCell.RealmServer.Handlers
{
	/// <summary>
	/// Send in the packet that logs new items
	/// </summary>
	public enum ItemReceptionType : ulong
	{
		/// <summary>
		/// "You looted item:"
		/// </summary>
		Loot = 0,

		/// <summary>
		/// "You receive item:"
		/// (When buying an item etc)
		/// </summary>
		Receive = 1,

		/// <summary>
		/// "You created: "
		/// </summary>
		YouCreated = 1L << 32
	}

	public static class ItemHandler
	{

		#region CMSG_ITEM_NAME_QUERY

		[ClientPacketHandler(RealmServerOpCode.CMSG_ITEM_NAME_QUERY)]
		public static void HandleItemNameQuery(IRealmClient client, RealmPacketIn packet)
		{
			uint itemId = packet.ReadUInt32();

			ItemTemplate item = ItemMgr.Templates.Get(itemId);

			if (item != null)
			{
				SendItemNameQueryResponse(client, item);
			}
		}

		public static void SendItemNameQueryResponse(IPacketReceiver client, ItemTemplate item)
		{
			using (var outPacket = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_NAME_QUERY_RESPONSE, 4 + item.DefaultName.Length))
			{
				outPacket.WriteInt(item.Id);
				outPacket.WriteCString(item.DefaultName);

				client.Send(outPacket);
			}
		}

		#endregion

		#region CMSG_AUTOEQUIP_ITEM
		/// <summary>
		/// Auto-equips an item (can be triggered by double click etc)
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_AUTOEQUIP_ITEM)]
		public static void HandleAutoEquip(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var inv = chr.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var contSlot = (InventorySlot)packet.ReadByte();
				var slot = packet.ReadByte();

				inv.TryEquip(contSlot, slot);
			}
		}
		#endregion

		#region CMSG_DESTROYITEM
		[ClientPacketHandler(RealmServerOpCode.CMSG_DESTROYITEM)]
		public static void HandleDestroyItem(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var contSlot = (InventorySlot)packet.ReadByte();
				var slot = packet.ReadByte();

				var cont = inv.GetContainer(contSlot, inv.IsBankOpen);
				if (cont != null && cont.IsValidSlot(slot))
				{
					cont.TryDestroy(slot);
				}
			}
		}
		#endregion

		#region CMSG_USE_ITEM
		/// <summary>
		/// Use an Item
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_USE_ITEM)]
		public static void HandleUseItem(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var inv = chr.Inventory;

			if (inv.CheckInteract() != InventoryError.OK) return;

			var bagSlot = (InventorySlot)packet.ReadByte();
			var slot = packet.ReadByte();

			// get the item that is being used
			var item = inv.GetItem(bagSlot, slot, false);
			if (item != null && item.CanBeUsed)
			{
				var template = item.Template;
				var err = InventoryError.DontReport;
				if (template.UseSpell == null)
				{
					// no spell associated
					err = InventoryError.YOU_CAN_NEVER_USE_THAT_ITEM;
#if DEBUG
					client.ActiveCharacter.SendSystemMessage("Item {0} has no Spell associated with it.", item);
#endif
				}
				else if (!template.UseSpell.HasCharges ||
					item.GetSpellCharges(template.UseSpell.Index) > 0)
				{
					err = item.Template.CheckEquip(chr);
					if (err == InventoryError.OK)
					{
						var cast = chr.SpellCast;
						cast.Id = packet.ReadByte();
						var spellId = (SpellId)packet.ReadUInt32();
						var itemEid = packet.ReadEntityId();
						var glyphSlot = packet.ReadUInt32();
						var unkFlag = packet.ReadByte();

						cast.TargetItem = cast.CasterItem = item;

						if (item.Template.UseSpell.Id == spellId)
						{
							cast.Start(item.Template.UseSpell.Spell, packet, cast.Id, unkFlag, glyphSlot);
						}
					}
				}

				if (err != InventoryError.OK)
				{
					SendInventoryError(client, item, null, err);
				}
			}
		}
		#endregion

		#region CMSG_OPEN_ITEM
		/// <summary>
		/// Open an Item
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_OPEN_ITEM)]
		public static void HandleOpenItem(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var inv = chr.Inventory;

			if (inv.CheckInteract() == InventoryError.OK)
			{
				var bagSlot = (InventorySlot)packet.ReadByte();
				var slot = packet.ReadByte();

				// get the item that is being used
				var item = inv.GetItem(bagSlot, slot, false);
				if (item != null && item.CanBeUsed)
				{
					chr.SendSystemMessage("Opening {0}...", item.Name);
					item.TryLoot(chr);
				}
			}
		}
		#endregion

		#region Moving Items
		/// <summary>
		/// Swap items on the character
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_SWAP_INV_ITEM)]
		public static void HandleSwapInvItem(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var destSlot = packet.ReadByte();
				var srcSlot = packet.ReadByte();

				inv.TrySwap(inv, srcSlot, inv, destSlot);
			}
		}

		/// <summary>
		/// Swap items within bags
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_SWAP_ITEM)]
		public static void HandleSwapItem(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var destCont = (InventorySlot)packet.ReadByte();
				var destSlot = packet.ReadByte();
				var srcCont = (InventorySlot)packet.ReadByte();
				var srcSlot = packet.ReadByte();

				inv.TrySwap(srcCont, srcSlot, destCont, destSlot);
			}

			//SendInventoryError(client, null, null, InventoryError.NO_EQUIPMENT_SLOT_AVAILABLE3);
		}

		/// <summary>
		/// Set Ammo
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_AUTOSTORE_BAG_ITEM)]
		public static void HandleAutostoreBagItem(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var srcCont = (InventorySlot)packet.ReadByte();
				var srcSlot = packet.ReadByte();
				var destCont = (InventorySlot)packet.ReadByte();

				inv.TryMove(srcCont, srcSlot, destCont);
			}
		}

		/// <summary>
		/// Set Ammo
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_AMMO)]
		public static void HandleSetAmmo(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var templId = packet.ReadUInt32();

				inv.SetAmmo(templId);
			}
		}

		/// <summary>
		/// Split up a stack of items
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_SPLIT_ITEM)]
		public static void HandleSplitItem(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{

				var srcBagSlot = (InventorySlot)packet.ReadByte();
				var srcSlot = packet.ReadByte();
				var destBagSlot = (InventorySlot)packet.ReadByte();
				var destSlot = packet.ReadByte();
				var amount = packet.ReadByte();

				inv.Split(srcBagSlot, srcSlot, destBagSlot, destSlot, amount);
			}
		}
		#endregion

		/// <summary>
		/// Socket an item
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_SOCKET_GEMS)]
		public static void HandleSocketGem(IRealmClient client, RealmPacketIn packet)
		{
			var inv = client.ActiveCharacter.Inventory;
			if (inv.CheckInteract() == InventoryError.OK)
			{
				var itemId = packet.ReadEntityId();
				var item = inv.GetItem(itemId);
				if (item == null)
				{
					return;
				}

				var gems = new Item[ItemConstants.MaxSocketCount];

				for (var i = 0; i < ItemConstants.MaxSocketCount; i++)
				{
					var id = packet.ReadEntityId();
					if (id != EntityId.Zero)
					{
						gems[i] = inv.GetItem(id);
					}
				}

				item.ApplyGems(gems);
			}
		}

		/// <summary>
		/// Sends the Item's PushResult (required after adding items).
		/// </summary>
		public static void SendItemPushResult(Character owner, Item item, ItemTemplate templ, int amount, ItemReceptionType reception)
		{
			bool isStacked;
			int contSlot;
			uint propertySeed, randomPropid;
			if (item != null)
			{
				contSlot = item.Container.Slot;
				isStacked = item.Amount != amount; // item.Amount == amount means that it was not added to an existing stack
				propertySeed = item.PropertySeed;
				randomPropid = item.RandomPropertiesId;
			}
			else
			{
				contSlot = BaseInventory.INVALID_SLOT;
				isStacked = true;													// we did not have an item -> stacked
				propertySeed = 0;
				randomPropid = 0;
			}

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_PUSH_RESULT, 44))
			{
				packet.Write(owner.EntityId);
				packet.Write((ulong)reception);

				//packet.Write(received ? 1 : 0);										// 0 = "You looted...", 1 = "You received..."
				//packet.Write(isNew ? 1 : 0);										// 0 = "You received/looted...", 1 = "You created..."

				packet.Write(1);													// log message
				packet.Write((byte)contSlot);
				packet.Write(isStacked ? -1 : item.Slot);
				packet.Write(templ.Id);
				packet.Write(propertySeed);
				packet.Write(randomPropid);
				packet.Write(amount);												// amount added
				packet.Write(owner.Inventory.GetAmount(templ.ItemId));				// amount of that type of item in inventory

				owner.Send(packet);
			}
		}

		/// <summary>
		/// Send a simple "Can't do that right now" message
		/// </summary>
		/// <param name="client"></param>
		public static void SendCantDoRightNow(IRealmClient client)
		{
			SendInventoryError(client, null, null, InventoryError.CANT_DO_RIGHT_NOW);
		}

		/// <summary>
		/// item1 and item2 can be null, but item1 must be set in case of YOU_MUST_REACH_LEVEL_N.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="item1"></param>
		/// <param name="item2"></param>
		/// <param name="error"></param>
		public static void SendInventoryError(IPacketReceiver client, Item item1, Item item2, InventoryError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INVENTORY_CHANGE_FAILURE,
				error == InventoryError.YOU_MUST_REACH_LEVEL_N ? 22 : 18))
			{
				packet.WriteByte((byte)error);

				if (item1 != null)
				{
					packet.Write(item1.EntityId.Full);
				}
				else
				{
					packet.Write((long)0);
				}

				if (item2 != null)
				{
					packet.Write(item2.EntityId.Full);
				}
				else
				{
					packet.Write((long)0);
				}

				packet.Write((byte)0);

				if (error == InventoryError.YOU_MUST_REACH_LEVEL_N && item1 != null)
				{
					packet.WriteUInt(item1.Template.RequiredLevel);
				}

				client.Send(packet);
			}
		}

		/// <summary>
		/// item1 and item2 can be null, but item1 must be set in case of YOU_MUST_REACH_LEVEL_N.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="error"></param>
		public static void SendInventoryError(IPacketReceiver client, InventoryError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INVENTORY_CHANGE_FAILURE,
				error == InventoryError.YOU_MUST_REACH_LEVEL_N ? 22 : 18))
			{
				packet.WriteByte((byte)error);

				packet.WriteULong(0);
				packet.WriteULong(0);
				packet.WriteByte(0);

				client.Send(packet);
			}
		}

		public static void SendDurabilityDamageDeath(IPacketReceiver client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DURABILITY_DAMAGE_DEATH, 0))
			{
				client.Send(packet);
			}
		}

		public static void SendEnchantLog(IPacketReceivingEntity owner, ItemId entryId, uint enchantId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ENCHANTMENTLOG, 25))
			{
				packet.Write(owner.EntityId);
				packet.Write(owner.EntityId);
				packet.Write((uint)entryId);
				packet.Write((uint)enchantId); // cast maybe unneeded
				packet.Write((byte)0);

				owner.Send(packet);
			}
		}

		public static void SendEnchantTimeUpdate(IPacketReceivingEntity owner, Item item, int duration)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_ENCHANT_TIME_UPDATE, 24))
			{
				packet.Write(item.EntityId);
				packet.Write(item.Slot);
				packet.Write(duration);
				packet.Write(owner.EntityId);

				owner.Send(packet);
			}
		}


		#region CMSG_ITEM_QUERY_SINGLE

		[ClientPacketHandler(RealmServerOpCode.CMSG_ITEM_QUERY_SINGLE)]
		public static void HandleItemSingleQuery(IRealmClient client, RealmPacketIn packet)
		{
			uint templateId = packet.ReadUInt32();

			ItemTemplate template = ItemMgr.Templates.Get(templateId);
			if (template == null)
			{
				return;
			}

			SendItemQueryResponse(client, template);
		}

		public static void SendItemQueryResponse(IRealmClient client, ItemTemplate item)
		{
			var locale = client.Info.Locale;
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_QUERY_SINGLE_RESPONSE, 630))
			{
				packet.Write(item.Id);
				packet.Write((uint)item.Class);
				packet.Write((uint)item.SubClass);
				packet.Write(item.Unk0); // unknown

				packet.WriteCString(item.Names.Localize(locale));
				packet.Write((byte)0);// name2
				packet.Write((byte)0);// name3
				packet.Write((byte)0);// name4

				packet.Write(item.DisplayId);
				packet.Write((uint)item.Quality);
				packet.Write((uint)item.Flags);
				packet.Write((uint)item.Flags2);		// new 3.2.0
				packet.Write(item.BuyPrice);
				packet.Write(item.SellPrice);
				packet.Write((uint)item.InventorySlotType);
				packet.Write((uint)item.RequiredClassMask);
				packet.Write((uint)item.RequiredRaceMask);
				packet.Write(item.Level);
				packet.Write(item.RequiredLevel);
				packet.Write(item.RequiredSkill != null ? (int)item.RequiredSkill.Id : 0);
				packet.Write(item.RequiredSkillValue);
				packet.Write(item.RequiredProfession != null ? item.RequiredProfession.Id : 0);
				packet.Write(item.RequiredPvPRank);
				packet.Write(item.UnknownRank);// PVP Medal
				packet.Write(item.RequiredFaction != null ? (int)item.RequiredFaction.Id : 0);
				packet.Write((uint)item.RequiredFactionStanding);
				packet.Write(item.UniqueCount);
				packet.Write(item.MaxAmount);
				packet.Write(item.ContainerSlots);

				packet.Write(item.Mods.Length);
				for (var m = 0; m < item.Mods.Length; m++)
				{
					packet.Write((uint)item.Mods[m].Type);
					packet.Write(item.Mods[m].Value);
				}

				packet.Write(item.ScalingStatDistributionId);// NEW 3.0.2 ScalingStatDistribution.dbc
				packet.Write(item.ScalingStatValueFlags);// NEW 3.0.2 ScalingStatFlags


				// In 3.1 there are only 2 damages instead of 5
				for (var i = 0; i < 2; i++)
				{
                    if(i >= item.Damages.Length)
                    {
                        packet.WriteFloat(0f);
                        packet.WriteFloat(0f);
                        packet.WriteUInt(0u);
                        continue;
                    }

					var dmg = item.Damages[i];

					packet.Write(dmg.Minimum);
					packet.Write(dmg.Maximum);
					packet.Write((uint)dmg.School);
				}

				for (var i = 0; i < ItemConstants.MaxResCount; i++)
				{
					var res = item.Resistances[i];
					packet.Write(res);
				}

				packet.Write(item.AttackTime);
				packet.Write((uint)item.ProjectileType);
				packet.Write(item.RangeModifier);

				for (var s = 0; s < ItemConstants.MaxSpellCount; s++)
				{
				    ItemSpell spell;
                    if(s < item.Spells.Length && (spell = item.Spells[s]) != null)
                    {
                        packet.Write((uint)spell.Id);
                        packet.Write((uint)spell.Trigger);
                        packet.Write(spell.Charges);
                        packet.Write(spell.Cooldown);
                        packet.Write(spell.CategoryId);
                        packet.Write(spell.CategoryCooldown);
                    }
                    else
                    {
                        packet.WriteUInt(0u);
                        packet.WriteUInt(0u);
                        packet.WriteUInt(0u);
                        packet.Write(-1);
                        packet.WriteUInt(0u);
                        packet.Write(-1);
                    }
				}

				packet.Write((uint)item.BondType);
				packet.WriteCString(item.Descriptions.Localize(locale));

				packet.Write(item.PageTextId);
				packet.Write(item.PageCount);
				packet.Write((uint)item.PageMaterial);
				packet.Write(item.QuestId);
				packet.Write(item.LockId);
				packet.Write((int)item.Material);
				packet.Write((uint)item.SheathType);
				packet.Write(item.RandomPropertiesId);
				packet.Write(item.RandomSuffixId);
				packet.Write(item.BlockValue);
				packet.Write((uint)item.SetId);
				packet.Write(item.MaxDurability);
				packet.Write((uint)item.ZoneId);
				packet.Write((uint)item.MapId);
				packet.Write((uint)item.BagFamily);
				packet.Write((uint)item.ToolCategory);

				for (var i = 0; i < ItemConstants.MaxSocketCount; i++)
				{
					packet.Write((uint)item.Sockets[i].Color);
					packet.Write(item.Sockets[i].Content);
				}

				packet.Write(item.SocketBonusEnchantId);
				packet.Write(item.GemPropertiesId);
				packet.Write(item.RequiredDisenchantingLevel);
				packet.Write(item.ArmorModifier);

				packet.Write(item.Duration);// Exisiting duration in seconds
				packet.Write(item.ItemLimitCategoryId);// NEW 3.0.2 ItemLimitCategory.dbc

				packet.Write(item.HolidayId); // NEW 3.1.0 Holidays.dbc

				client.Send(packet);
			}
		}
		#endregion

		#region Equipment Sets

		public static void SendEquipmentSetList(IPacketReceiver client, IList<EquipmentSet> setList)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_EQUIPMENT_SET_LIST))
			{
				packet.Write(setList.Count);
				foreach (var set in setList)
				{
					set.SetGuid.WritePacked(packet);
					packet.Write(set.Id);
					packet.Write(set.Name);
					packet.Write(set.Icon);

					var items = set.Items ?? new EquipmentSetItemMapping[19];
					for (var i = 0; i < 19; i++)
					{
						var item = items[i];
						if (item != null)
						{
							item.ItemEntityId.WritePacked(packet);
							continue;
						}

						EntityId.Zero.WritePacked(packet);
					}
				}

				client.Send(packet);
			}
		}

		public static void SendEquipmentSetSaved(IPacketReceiver client, EquipmentSet set)
		{
			if (set == null) return;

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_EQUIPMENT_SET_SAVED))
			{
				packet.Write(set.Id);
				packet.Write(set.SetGuid);

				client.Send(packet);
			}
		}

		public static void SendUseEquipmentSetResult(IPacketReceiver client, UseEquipmentSetError error)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_USE_EQUIPMENT_SET_RESULT))
			{
				packet.Write((byte)error);

				client.Send(packet);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_SET_EQUIPMENT_SET)]
		public static void HandleSetEquipmentSet(IRealmClient client, RealmPacketIn packet)
		{
			var setEntityId = packet.ReadPackedEntityId();
			var setId = packet.ReadInt32();
			var name = packet.ReadCString();
			var icon = packet.ReadCString();

			var itemList = new EntityId[19];
			for (var i = 0; i < 19; i++)
			{
				itemList[i] = packet.ReadPackedEntityId();
			}

			var chr = client.ActiveCharacter;

			chr.Inventory.SetEquipmentSet(setEntityId, setId, name, icon, itemList);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_DELETE_EQUIPMENT_SET)]
		public static void HandleDeleteEquipmentSet(IRealmClient client, RealmPacketIn packet)
		{
			var setGuid = packet.ReadPackedEntityId();

			var chr = client.ActiveCharacter;

			chr.Inventory.DeleteEquipmentSet(setGuid);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_USE_EQUIPMENT_SET)]
		public static void HandleUseEquipmentSet(IRealmClient client, RealmPacketIn packet)
		{
			var equipmentSwap = new EquipmentSwapHolder[19];
			for (var i = 0; i < 19; i++)
			{
				equipmentSwap[i] = new EquipmentSwapHolder
				{
					ItemGuid = packet.ReadPackedEntityId(),
					SrcContainer = (InventorySlot)packet.ReadByte(),
					SrcSlot = packet.ReadByte()
				};
			}

			var chr = client.ActiveCharacter;
			chr.Inventory.UseEquipmentSet(equipmentSwap);
		}
		#endregion
	}
}