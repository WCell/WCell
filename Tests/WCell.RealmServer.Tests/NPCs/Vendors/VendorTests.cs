using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Tests.Tools;

namespace WCell.RealmServer.Tests.NPCs.Vendors
{
	/// <summary>
	/// Summary description for VendorTests
	/// </summary>
	[TestClass]
	public class VendorTests
	{
        private static TestCharacter Customer;
        private static NPC Vendor;

	    private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion
		
		[ClassInitialize()]
		public static void VendorTestClassInitialize(TestContext testContext)
		{
            Setup.EnsureNPCsLoaded();
			Setup.EnsureItemsLoaded();

		    EnsureChar(ref Customer, Setup.AllianceCharacterPool);
		    EnsureNPC(ref Vendor, Setup.NPCPool);
		}

        private static void EnsureChar(ref TestCharacter chr, CharacterPool pool)
        {
            chr = pool.Create();
            chr.SetMoney(100000);
			chr.GodMode = true;
			chr.EnsureInWorldAndLiving();
        }

        private static void EnsureNPC(ref NPC npc, NPCPool pool)
        {
            npc = pool.CreateVendor();
            npc.EnsureInWorldAndLiving();
        }

		//[TestMethod]
		public void TestNewXmlLoading()
		{
			//NPCMgr.LoadVendorLists();
		}

        [TestMethod]
        public void TestSellItems()
        {
            FillInventory();

            var buyBackSlot = (int)InventorySlot.BuyBack1;
            for (var slot = InventorySlot.BackPack1; slot < InventorySlot.BackPack14; slot++)
            {
                SellItem((int)slot, buyBackSlot);
                if (buyBackSlot < (int)InventorySlot.BuyBackLast)
                {
                    buyBackSlot++;
                }
            }
        
        }

        [TestMethod]
        public void TestBuyBackItems()
        {
            // Put some items into the BuyBack
            TestSellItems();

            var item = Customer.Inventory.BuyBack[(int)InventorySlot.BuyBack1];
            Assert.IsNotNull(item);
            var id = item.Template.ItemId;
            SendBuyBackPacket(InventorySlot.BuyBack1);
            // The other items will shift up to fill the void.
            Assert.IsNull(Customer.Inventory.BuyBack[(int)InventorySlot.BuyBackLast]);
            // The item retreived earlier may have been consumed in the buyback, so we have to search by ItemId.
            Assert.IsNotNull(Customer.Inventory.GetItemByItemId(id, false));
        }

	    private void SellItem(int slot, int buyBackSlot)
	    {
	        var item = Customer.Inventory.GetItem(InventorySlot.Invalid, slot, false);
	        Assert.IsNotNull(item);
	        SendSellItemPacket(item, item.Amount);

	        var packet = Customer.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_SELL_ITEM);
	        var error = (SellItemError)(packet["Error Code"].ByteValue);
	        Assert.AreEqual(SellItemError.Success, error);
	        Assert.IsNull(Customer.Inventory.GetItem(InventorySlot.Invalid, slot, false));
	        Assert.AreEqual(item, Customer.Inventory.BuyBack[buyBackSlot]);
	        Assert.AreEqual(item.Slot, buyBackSlot);
	    }

	    /// <summary>
        /// Adds some items to the Customer to sell, empties Inventory first.
        /// </summary>
	    private void FillInventory()
	    {
            for (var i = InventorySlot.BackPack1; i < InventorySlot.BackPackLast; i++)
            {
                Customer.Inventory.Remove(i, false);
            }
	        var amount = 20;
            InventoryError error;
            error = Customer.Inventory.TryAdd(ItemId.DalaranSharp, ref amount);
            Assert.IsTrue(error == InventoryError.OK);

	        amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.DwarvenMild, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.ShinyRedApple, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.SnapvineWatermelon, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.TelAbimBanana, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.GoldenbarkApple, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.ToughHunkOfBread, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.FreshlyBakedBread, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.MoistCornbread, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.RefreshingSpringWater, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 20;
	        error = Customer.Inventory.TryAdd(ItemId.SilkCloth, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 1;
	        error = Customer.Inventory.TryAdd(ItemId.BentStaff, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 1;
	        error = Customer.Inventory.TryAdd(ItemId.WornShortsword, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 1;
	        error = Customer.Inventory.TryAdd(ItemId.WornMace, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 1;
	        error = Customer.Inventory.TryAdd(ItemId.WornAxe, ref amount);
	        Assert.IsTrue(error == InventoryError.OK);

            amount = 1;
	        error = Customer.Inventory.TryAdd(ItemId.RecruitsShirt, ref amount);
            Assert.IsTrue(error == InventoryError.OK);
	    }

        private void SendSellItemPacket(Item item, int numToSell)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_SELL_ITEM))
            {
                packet.Write(Vendor.EntityId);
                packet.Write(item.EntityId);
                packet.Write((byte)numToSell);

                Customer.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }

        private void SendBuyBackPacket(InventorySlot buybackSlot)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.CMSG_BUYBACK_ITEM))
            {
                packet.Write(Vendor.EntityId);
                packet.Write((uint)buybackSlot);

                Customer.FakeClient.ReceiveCMSG(packet, true, true);
            }
        }
	}
}