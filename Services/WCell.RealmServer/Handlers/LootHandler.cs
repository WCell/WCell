using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Constants.World;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Network;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Handlers
{
	public static class LootHandler
	{
		#region IN
		/// <summary>
		/// Stores a selected item in inventory automatically
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_AUTOSTORE_LOOT_ITEM)]
		public static void HandleAutoLoot(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var looter = chr.LooterEntry;

			var loot = looter.Loot;

			if (loot != null)
			{
				var itemIndex = packet.ReadByte();

				loot.TakeItem(looter, itemIndex, chr.Inventory, BaseInventory.INVALID_SLOT);
			}
		}

		/// <summary>
		/// Stores a selected item in the given slot
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_STORE_LOOT_IN_SLOT)]
		public static void HandleStoreLoot(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var looter = chr.LooterEntry;

			var loot = looter.Loot;

			if (loot != null)
			{
				var itemIndex = packet.ReadByte();
				var contSlot = (InventorySlot)packet.ReadByte();
				var slot = packet.ReadByte();

				loot.TakeItem(looter, itemIndex, chr.Inventory.GetContainer(contSlot, false), slot);
			}
		}

		/// <summary>
		/// A client wants to loot something (usually a corpse)
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_LOOT)]
		public static void HandleLoot(IRealmClient client, RealmPacketIn packet)
		{
			var objId = packet.ReadEntityId();
			var chr = client.ActiveCharacter;
			var looter = chr.LooterEntry;
			var lootable = chr.Region.GetObject(objId);

			if (lootable != null)
			{
				looter.TryLoot(lootable);
			}
		}

		/// <summary>
		/// Gold is given automatically in group-looting. Client can only request gold when looting alone.
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_LOOT_MONEY)]
		public static void HandleLootMoney(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var looter = chr.LooterEntry;

			chr.SpellCast.Cancel();

			var loot = looter.Loot;
			if (loot != null)
			{
				loot.GiveMoney();
			}
		}

		/// <summary>
		/// Client finished looting
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_LOOT_RELEASE)]
		public static void HandleLootRelease(IRealmClient client, RealmPacketIn packet)
		{
			//var objId = packet.ReadEntityId();

			var chr = client.ActiveCharacter;
			var looter = chr.LooterEntry;

			var loot = looter.Loot;
			if (loot != null)
			{
				looter.Loot = null;
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LOOT_ROLL)]
		public static void HandleRoll(IRealmClient client, RealmPacketIn packet)
		{
			var lootedId = packet.ReadEntityId();
			var index = packet.ReadUInt32();
			var rollType = (LootRollType)packet.ReadByte();

			var chr = client.ActiveCharacter;
			var looted = chr.Region.GetObject(lootedId);

			if (looted != null && looted.Loot != null &&
				looted.Loot.Method == LootMethod.NeedBeforeGreed &&
				index < looted.Loot.Items.Length)
			{
				looted.Loot.Roll(chr, index, rollType);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_LOOT_MASTER_GIVE)]
		public static void HandleMasterGive(IRealmClient client, RealmPacketIn packet)
		{
		    var lootedId = packet.ReadEntityId();
		    var lootSlot = packet.ReadByte();
		    var playerId = packet.ReadEntityId();

		    var chr = client.ActiveCharacter;
		    var looted = chr.Region.GetObject(lootedId);
		    var player = chr.Region.GetObject(playerId) as Character;

            if (looted != null && looted.Loot != null && 
                looted.Loot.Method == LootMethod.MasterLoot &&
                lootSlot < looted.Loot.Items.Length)
            {
                looted.Loot.GiveLoot(chr, player, lootSlot);
            }
		}

        /// <summary>
        /// Sets the automatic PassOnLoot rolls option.
        /// </summary>
        [ClientPacketHandler(RealmServerOpCode.CMSG_OPT_OUT_OF_LOOT)]
        public static void HandleOptOutOfLoot(IRealmClient client, RealmPacketIn packet)
        {
            var autoOptOut = (packet.ReadUInt32() > 0);
            client.ActiveCharacter.PassOnLoot = autoOptOut;
        }
		#endregion

		#region OUT
		public static void SendLootFail(Character looter, ILootable lootable)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_RESPONSE))
			{
				packet.Write(lootable.EntityId);
				packet.Write((uint)LootResponseType.Fail);
				looter.Client.Send(packet);
			}
		}

		public static void SendLootResponse(Character looter, Loot loot)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_RESPONSE, 14 + (loot.RemainingCount * 22)))
			{
				var looterEntry = looter.LooterEntry;

				packet.Write(loot.Lootable.EntityId);
				packet.Write((byte)loot.ResponseType);
				packet.Write(loot.IsMoneyLooted ? 0 : loot.Money);

				var countPos = packet.Position;
				var count = 0;
				packet.Position++;
				for (var i = 0; i < loot.Items.Length; i++)
				{
					var item = loot.Items[i];
					var templ = item.Template;
					var looters = item.MultiLooters;
					if (!item.Taken &&
						((looters == null && templ.CheckLootRequirements(looter)) ||
						(looters != null && looters.Contains(looterEntry))))
					{

						packet.Write((byte)i);
						packet.Write(templ.Id);
						packet.Write(item.Amount);
						packet.Write(templ.DisplayId);
						packet.Write(templ.RandomSuffixFactor);
						packet.Write(templ.RandomSuffixFactor > 0 ? -(int)templ.RandomSuffixId : (int)templ.RandomPropertiesId);
						packet.Write((byte)item.Decision);
						count++;
					}
				}

				packet.Position = countPos;
				packet.Write((byte)count);

				looter.Client.Send(packet);
			}
		}

		public static void SendLootReleaseResponse(Character looter, Loot loot)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_RELEASE_RESPONSE))
			{
				packet.Write(looter.EntityId);
				packet.WriteByte(1);
				looter.Client.Send(packet);
			}
		}

		public static void SendLootRemoved(Character looter, uint index)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_REMOVED))
			{
				packet.WriteByte(index);
				looter.Client.Send(packet);
			}
		}

		/// <summary>
		/// Your share of the loot is %d money.
		/// </summary>
		public static void SendMoneyNotify(Character looter, uint amount)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_MONEY_NOTIFY))
			{
				packet.WriteUInt(amount);
				looter.Client.Send(packet);
			}
		}

		public static void SendClearMoney(Loot loot)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_CLEAR_MONEY))
			{
				foreach (var looterEntry in loot.Looters)
				{
					if (looterEntry.Owner != null)
					{
						looterEntry.Owner.Client.Send(packet);
					}
				}
			}
		}

		public static void SendStartRoll(Loot loot, LootItem item, IEnumerable<LooterEntry> looters, MapId mapid)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_START_ROLL))
			{
				packet.Write(loot.Lootable.EntityId);
			    packet.WriteUInt((uint)mapid); // TODO: actually use this
				packet.Write(item.Index);
				packet.Write(item.Template.Id);
				packet.Write(item.Template.RandomSuffixFactor);
				packet.Write((int)(item.Template.RandomSuffixFactor > 0 ? 
					-item.Template.RandomSuffixId : item.Template.RandomPropertiesId));
				packet.Write(LootMgr.DefaultLootRollTimeout);
				packet.Write((byte)0x0F);							 // since 3.3: loot roll mask

				foreach (var looter in looters)
				{
					if (looter.Owner != null)
					{
						looter.Owner.Client.Send(packet);
					}
				}
			}
		}

		public static void SendRoll(Character looter, Loot loot, LootItem item, int rollNumber, LootRollType rollType)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ROLL))
			{
				packet.Write(loot.Lootable.EntityId);
				packet.Write(item.Index);
				packet.Write(looter.EntityId);
				packet.Write(item.Template.Id);
				packet.Write(item.Template.RandomSuffixFactor);
				packet.Write((int)(item.Template.RandomSuffixFactor > 0 ? -item.Template.RandomSuffixId : item.Template.RandomPropertiesId));
				packet.Write(rollNumber);
				packet.Write((byte)rollType);

				foreach (var looterEntry in loot.Looters)
				{
					if (looterEntry.Owner != null)
					{
						looterEntry.Owner.Client.Send(packet);
					}
				}
			}
		}

		// TODO: Still unimplemented packets
		public static void SendItemNotify(Character looter, Loot loot)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ITEM_NOTIFY))
			{
				// guid, byte, byte, int, string
				looter.Client.Send(packet);
			}
		}

		public static void SendAllPassed(Character looter, Loot loot, LootItem item)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ALL_PASSED))
			{
				//packet.Write(item.);

				looter.Client.Send(packet);
			}
		}

		public static void SendRollWon(Character looter, Loot loot, LootItem item, LootRollEntry entry)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_ROLL_WON))
			{
				packet.Write(loot.Lootable.EntityId);
				packet.Write(item.Index);
				packet.Write(looter.EntityId);
				packet.Write(item.Template.Id);
				packet.Write(item.Template.RandomSuffixFactor);
				packet.Write((int)(item.Template.RandomSuffixFactor > 0 ? -item.Template.RandomSuffixId : item.Template.RandomPropertiesId));
				packet.Write(looter.EntityId);

				packet.Write(entry.Number);
				packet.Write((int)entry.Type);

				foreach (var looterEntry in loot.Looters)
				{
					if (looterEntry.Owner != null)
					{
						looterEntry.Owner.Client.Send(packet);
					}
				}
			}
		}

		public static void SendMasterList(Character looter, Loot loot)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOOT_MASTER_LIST))
			{
				looter.Client.Send(packet);
			}
		}
		#endregion
	}
}