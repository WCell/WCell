using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Network;
using WCell.RealmServer.Items;
using WCell.Core;

namespace WCell.RealmServer.Entities
{
    public partial class Item
	{

        #region CMSG_ITEM_QUERY_SINGLE

        [PacketHandler(RealmServerOpCode.CMSG_ITEM_QUERY_SINGLE)]
        public static void HandleItemSingleQuery(RealmClient client, RealmPacketIn packet)
        {
            uint itemId = packet.ReadUInt32();

            ItemTemplate item = ItemMgr.GetTemplate(itemId);
            if (item == null)
            {
                return;
            }

            SendItemQueryResponse(client, item);            
        }

        public static void SendItemQueryResponse(RealmClient client, ItemTemplate item)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_QUERY_SINGLE_RESPONSE, 630))
            {
                packet.Write(item.Id);
                packet.WriteInt((int)item.ItemClass);
                packet.WriteInt((int)item.ItemSubClass);
                packet.WriteInt(-1); // unknown

                packet.WriteCString(item.Name);
                packet.WriteByte(0);// name2
                packet.WriteByte(0);// name3
                packet.WriteByte(0);// name4

                packet.WriteInt(item.DisplayId);
                packet.WriteInt((int)item.Quality);
                packet.WriteInt((int)item.Flags);
                packet.WriteInt(item.BuyPrice);
                packet.WriteInt(item.SellPrice);
                packet.WriteInt((int)item.InventorySlot);
                packet.WriteInt((int)item.RequiredClassMask);
                packet.WriteInt((int)item.RequiredRaceMask);
                packet.WriteInt(item.Level);
                packet.WriteInt(item.RequiredLevel);
                packet.WriteInt(item.RequiredSkill != null ? (int)item.RequiredSkill.Id : 0);
                packet.WriteInt(item.RequiredSkillLevel);
                packet.WriteInt(item.RequiredProfession != null ? (int)item.RequiredProfession.Id : 0);
                packet.WriteInt(item.RequiredPvPRank);
                packet.WriteInt(item.UnknownRank);// city rank?
                packet.WriteInt(item.RequiredFaction != null ? (int)item.RequiredFaction.Id : 0);
                packet.WriteInt((int)item.RequiredFactionStanding);
                packet.WriteInt(item.UniqueCount);
                packet.WriteInt(item.MaxAmount);
                packet.WriteInt(item.ContainerSlots);
               foreach (var stat in item.Mods)
                {
					packet.WriteUInt((uint)stat.Type);
                    packet.WriteInt(stat.Value);
                }

                foreach (var dmg in item.Damages)
                {
					packet.WriteFloat(dmg.Minimum);
					packet.WriteFloat(dmg.Maximum);
					packet.WriteUInt((uint)dmg.DamageSchool);
                }

                foreach (var res in item.Resistances)
                {
                    packet.WriteUInt(res);
                }

                packet.WriteUInt(item.WeaponSpeed);
				packet.WriteUInt((uint)item.ProjectileType);
                packet.WriteFloat(item.RangeModifier);

                for (int i = 0; i < 5; i++)
                {
					packet.WriteUInt(item.Spells[i].Id);
					packet.WriteUInt((int)item.Spells[i].Trigger);
					packet.WriteUInt(item.Spells[i].Charges);
                    packet.WriteInt(item.Spells[i].Cooldown);
                    packet.WriteUInt(item.Spells[i].CategoryId);
                    packet.WriteInt(item.Spells[i].CategoryCooldown);
                }

				packet.WriteUInt((int)item.BondType);
                packet.WriteCString(item.Description);

				packet.Write(item.PageTextId);
				packet.Write(item.PageCount);
				packet.Write(item.PageMaterial);
				packet.Write(item.QuestId);
				packet.Write(item.RequiredLockpickSkill);
				packet.Write(item.Material);
				packet.Write((uint)item.SheathType);
				packet.Write(item.RandomPropertyId);
				packet.Write(item.RandomSuffixId);
				packet.Write(item.BlockValue);
				packet.Write(item.SetId);
				packet.Write(item.MaxDurability);
				packet.Write((uint)item.ZoneId);
				packet.Write((uint)item.MapId);
				packet.Write((uint)item.BagFamily);
				packet.Write(item.TotemCategory);

				for (int i = 0; i < ItemTemplate.MaxSocketCount; i++)
                {
                    packet.WriteInt(item.Sockets[i].SocketColor);
                    packet.WriteInt(item.Sockets[i].Unknown);
                }
                packet.WriteInt(item.SocketBonusId);
                packet.WriteInt(item.GemProperties);
                packet.WriteInt(item.ExtendedCost);
                packet.WriteInt(item.RequiredArenaRanking);
                packet.WriteInt(item.RequiredDisenchantingLevel);
                packet.WriteFloat(item.ArmorModifier);

                client.Send(packet);
            }
        }

        #endregion

        #region CMSG_ITEM_NAME_QUERY

        [PacketHandler(RealmServerOpCode.CMSG_ITEM_NAME_QUERY)]
        public static void HandleItemNameQuery(RealmClient client, RealmPacketIn packet)
        {
            uint itemId = packet.ReadUInt32();

            ItemTemplate item = ItemMgr.GetTemplate(itemId);

            if (item == null)
            {
                return;
            }

            SendItemNameQueryResponse(client, item);
        }

        public static void SendItemNameQueryResponse(RealmClient client, ItemTemplate item)
        {
            using (RealmPacketOut outPacket = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_NAME_QUERY_RESPONSE, 4 + item.Name.Length))
            {
                outPacket.WriteInt(item.Id);
                outPacket.WriteCString(item.Name);

                client.Send(outPacket);
            }
        }

		#endregion

		/// <summary>
		/// Auto-equips an item (can be triggered by double click etc)
		/// </summary>
		[PacketHandler(RealmServerOpCode.CMSG_AUTOEQUIP_ITEM)]
		public static void HandleAutoEquip(RealmClient client, RealmPacketIn packet)
		{
			var contSlot = (InventorySlot)packet.ReadByte();
			var slot = packet.ReadByte();

			client.ActiveCharacter.SendSystemMsg("Trying to equip Item in {0}/{1}", contSlot, slot);
			client.ActiveCharacter.Inventory.TryEquip(contSlot, slot);
		}

		[PacketHandler(RealmServerOpCode.CMSG_DESTROYITEM)]
		public static void HandleDestroyItem(RealmClient client, RealmPacketIn packet)
		{
			var contSlot = (InventorySlot)packet.ReadByte();
			var slot = packet.ReadByte();

			var inv = client.ActiveCharacter.Inventory;
			var cont = inv.GetContainer(contSlot, inv.IsBankOpen);
			if (cont != null)
			{
				if (!cont.Destroy(slot))
				{
					SendInventoryError(client, null, null, InventoryError.CANT_DROP_SOULBOUND);
				}
			}
			else
			{
				SendInventoryError(client, null, null, InventoryError.ITEM_NOT_FOUND);
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_SWAP_ITEM)]
		public static void HandleSwapItem(RealmClient client, RealmPacketIn packet)
		{
			var destBagSlot = (InventorySlot)packet.ReadByte();
			var destSlot = packet.ReadByte();
			var srcBagSlot = (InventorySlot)packet.ReadByte();
			var srcSlot = packet.ReadByte();

			client.ActiveCharacter.Inventory.TrySwap(srcBagSlot, srcSlot, destBagSlot, destSlot);
		}

		/// <summary>
		/// Swap item within the backpack
		/// </summary>
		[PacketHandler(RealmServerOpCode.CMSG_SWAP_INV_ITEM)]
		public static void HandleSwapInvItem(RealmClient client, RealmPacketIn packet)
		{
			var srcSlot = packet.ReadByte();
			var destSlot = packet.ReadByte();

			//SendInventoryError(client, null, null, InventoryError.NO_EQUIPMENT_SLOT_AVAILABLE3);
			var inv = client.ActiveCharacter.Inventory;
			inv.TrySwap(inv, srcSlot, inv, destSlot);
		}

		[PacketHandler(RealmServerOpCode.CMSG_SET_AMMO)]
		public static void HandleSetAmmo(RealmClient client, RealmPacketIn packet)
		{
			var templId = packet.ReadUInt32();

			client.ActiveCharacter.Inventory.AmmoId = templId;
		}
        
        public static void SendInventoryError(RealmClient client, Item item1, Item item2, InventoryError error)
        {
            using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_INVENTORY_CHANGE_FAILURE, 
				error == InventoryError.YOU_MUST_REACH_LEVEL_N ? 22 : 18))
            {
                packet.WriteByte((byte)error);

                if (error == InventoryError.YOU_MUST_REACH_LEVEL_N)
                {
                    packet.WriteUInt(item1.Template.RequiredLevel);
                }

                if (item1 != null)
                {
                    packet.Write(item1.EntityId.Full);
                }
                else
                {
                    packet.WriteULong(0);
                }

                if (item2 != null)
                {
                    packet.Write(item2.EntityId.Full);
                }
                else
                {
                    packet.WriteULong(0);
                }

                packet.WriteByte(0);

                client.Send(packet);
            }
        }
    }
}