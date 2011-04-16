using System;
using WCell.Constants.Spells;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.NPCs.Armorer
{
	public static class ArmorerMgr
	{
		private static DurabilityCost[] itemClassRepairModifiers;
		private static DurabilityQuality[] itemQualityRepairModifiers;

		public static void RepairItem(IRealmClient client, NPC armorer, EntityId itemId, bool useGuildFunds)
		{
			var curChar = client.ActiveCharacter;

			uint totalCost = 0;
			if (itemId.Low != 0)
			{
				// Repairing an individual item.
				var item = GetItemByEntityId(curChar, itemId);
				if (item == null)
					return;
				if (!ArmorerCheatChecks(curChar, armorer, item))
					return;
				totalCost += RepairItem(curChar, armorer, item, useGuildFunds);
			}
			else
			{
				// Case Repair all
				if (!ArmorerCheatChecks(curChar, armorer))
					return;
				totalCost += RepairAllItems(curChar, armorer, useGuildFunds);
			}

			if (useGuildFunds)
			{
				/****************************
				 * TODO: record the funds usage in the guild log
				 ****************************/
			}
		}

		private static uint RepairAllItems(Character curChar, NPC armorer, bool useGuildFunds)
		{
			uint totalCost = 0;

			// Repair all items in the Backpack and Bags
			curChar.Inventory.Iterate(false, invItem => {
				if (invItem.MaxDurability > 0)
					totalCost += RepairItem(curChar, armorer, invItem, useGuildFunds);
				return true;
			});
			return totalCost;
		}


		/// <summary>
		/// Repairs the item of the given Character at the given armorer and returns the costs
		/// </summary>
		/// <returns></returns>
		private static uint RepairItem(Character curChar, NPC armorer, Item item, bool useGuildFunds)
		{
			var cost = GetCostToRepair(item);

			cost = curChar.Reputations.GetDiscountedCost(armorer.Faction.ReputationIndex, cost);

			if (useGuildFunds)
			{
				////////////////////////
				// TODO: Implement after Guilds are done...
				// Check that the character is a member of a guild, that they have guild fund usage priveleges
				// and that there is enough money in the guild bank.
				////////////////////////
			}
			else if (curChar.Money < cost)
			{
				return 0;
			}
			else curChar.Money -= cost;

			item.RepairDurability();

			return cost;
		}

		private static bool ArmorerCheatChecks(Character curChar, NPC armorer, Item item)
		{
			return item != null && ArmorerCheatChecks(curChar, armorer);
		}

		private static bool ArmorerCheatChecks(Character curChar, NPC armorer)
		{
			if (curChar == null)
				return false;
			if (armorer == null)
				return false;
			if (!armorer.CheckVendorInteraction(curChar))
				return false;

			// Remove Auras not compatible with NPC interaction
			curChar.Auras.RemoveByFlag(AuraInterruptFlags.OnStartAttack);

			return true;
		}

		private static Item GetItemByEntityId(Character curChar, EntityId itemId)
		{
			// Look through the Character's inventory to find the item with the given EntityId
			// Equipment first
			foreach (var item in curChar.Inventory.Equipment.Items)
			{
				if (item != null && item.EntityId == itemId)
					return item;
			}

			// Then backpack and bags
			return curChar.Inventory.GetItem(itemId, false);
		}

		private static uint GetCostToRepair(Item item)
		{
			if (item == null || item.Template == null)
				return 0;
			if (!(item.MaxDurability > 0))
				return 0;

			var lostDurability = (uint)(item.MaxDurability - item.Durability);
			if (!(lostDurability > 0))
				return 0;

			uint classMod;
			var costModArray = itemClassRepairModifiers[item.Template.Level];
			if (costModArray != null)
			{
				classMod = costModArray.GetModifierBySubClassId(item.Template.Class, item.Template.SubClass);
			}
			else
			{
				classMod = 1;
			}

			uint qualityMod;
			var qualModArray = itemQualityRepairModifiers[((int)item.Template.Quality + 1) * 2];
			if (qualModArray != null)
			{
				qualityMod = qualModArray.CostModifierPct;
			}
			else
			{
				qualityMod = 100;
			}

			var costToRepair = (lostDurability * classMod * qualityMod) / 100;

			return Math.Max(costToRepair, 1u); // Fix for ITEM_QUALITY_ARTIFACT
		}

		[Initialization(InitializationPass.Fifth, "Initialize Repair Costs")]
		public static void Initialize()
		{
			var durabilityCostsReader = new ListDBCReader<DurabilityCost, DBCDurabilityCostsConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_DURABILITYCOSTS));


			var durabilityQualityReader = new ListDBCReader<DurabilityQuality, DBCDurabilityQualityConverter>(
				RealmServerConfiguration.GetDBCFile(WCellConstants.DBC_DURABILITYQUALITY));


			itemClassRepairModifiers = durabilityCostsReader.EntryList.ToArray();
			itemQualityRepairModifiers = durabilityQualityReader.EntryList.ToArray();
		}
	}

}