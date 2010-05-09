using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Tests.Misc;

namespace WCell.RealmServer.Tests.Trade
{
	[TestClass]
	public class TradeTest
	{
		public TradeTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

	    /// <summary>
	    ///Gets or sets the test context which provides
	    ///information about and functionality for the current test run.
	    ///</summary>
	    public TestContext TestContext
	    {
	        get;
	        set;
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

	    private static TestCharacter chr1;
	    private static TestCharacter chr2;

		[ClassInitialize()]
		public static void Initialize(TestContext testContext)
		{
			Setup.EnsureBasicSetup();
            Setup.EnsureItemsLoaded();

            chr1 = Setup.AllianceCharacterPool.Create();
            chr2 = Setup.AllianceCharacterPool.Create();
		}

        [TestInitialize]
        public void TestInitialize()
        {
            chr1.EnsureInWorldAndLiving();
            chr2.EnsureInWorldAndLiving();

            chr1.Inventory.Purge();
            chr2.Inventory.Purge();

            chr1.SetMoney(10000);
            chr2.SetMoney(1000);
        }

		[TestMethod]
		public void TestTradeMoney()
		{
			StartTrade();

			chr1.TradeInfo.SetMoney(1000);

			Accept();

			Assert.AreEqual(chr1.Money, 9000);
			Assert.AreEqual(chr2.Money, 2000);
		}

        [TestMethod]
        public void TestTradeItems()
        {
            var inv1 = chr1.Inventory;
            var inv2 = chr2.Inventory;

            var amount = 5;
            var silverOre = AddItem(chr1, ItemId.SilverOre, ref amount);
            
            Assert.IsNotNull(silverOre);

            amount = 5;
            var goldOre = AddItem(chr1, ItemId.GoldOre, ref amount);
            Assert.IsNotNull(goldOre);

            amount = 5;
            var silkCloth = AddItem(chr2, ItemId.SilkCloth, ref amount);
            Assert.IsNotNull(silkCloth);

            amount = 5;
            var mageCloth = AddItem(chr2, ItemId.MageweaveCloth, ref amount);
            Assert.IsNotNull(mageCloth);


            StartTrade();

            chr1.TradeInfo.SetTradeItem(0x00, silverOre.Container.Slot, (byte)silverOre.Slot);
            chr1.TradeInfo.SetTradeItem(0x01, goldOre.Container.Slot, (byte)goldOre.Slot);

            chr2.TradeInfo.SetTradeItem(0x00, silkCloth.Container.Slot, (byte)silkCloth.Slot);
            chr2.TradeInfo.SetTradeItem(0x01, mageCloth.Container.Slot, (byte)mageCloth.Slot);

            Accept();

            var item = inv1.GetItem(silkCloth.EntityId);
            Assert.IsNotNull(item);
            Assert.AreEqual(chr1, item.Owner);

            item = inv2.GetItem(silkCloth.EntityId);
            Assert.IsNull(item);

            item = inv1.GetItem(mageCloth.EntityId);
            Assert.IsNotNull(item);
            Assert.AreEqual(chr1, item.Owner);

            item = inv2.GetItem(mageCloth.EntityId);
            Assert.IsNull(item);

            item = inv2.GetItem(silverOre.EntityId);
            Assert.IsNotNull(item);
            Assert.AreEqual(chr2, item.Owner);

            item = inv1.GetItem(silverOre.EntityId);
            Assert.IsNull(item);

            item = inv2.GetItem(goldOre.EntityId);
            Assert.IsNotNull(item);
            Assert.AreEqual(chr2, item.Owner);

            item = inv1.GetItem(goldOre.EntityId);
            Assert.IsNull(item);
        }

        private static Item AddItem(Character chr, ItemId itemId, ref int amount)
        {
            chr.Inventory.TryAdd(ItemMgr.GetTemplate(itemId), ref amount);
            return chr.Inventory.GetItemByItemId(itemId);
        }

		private void StartTrade()
		{
			WCell.RealmServer.Misc.TradeInfo.Propose(chr1, chr2);

			//var proposalPacket = chr1.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_TRADE_STATUS);
			//var proposersEntityId = proposalPacket["Proposer"].EntityIdValue;
			//Assert.AreEqual(proposersEntityId, chr2.EntityId);

			//var beginTradePacket = new RealmPacketOut(RealmServerOpCode.CMSG_BEGIN_TRADE);

			//chr2.FakeClient.ReceiveCMSG(beginTradePacket);

			Assert.IsNotNull(chr2.TradeInfo);

			chr2.TradeInfo.AcceptTradeProposal();

			CheckTrade();
		}

		private void Accept()
		{
			CheckTrade();

			chr1.TradeInfo.AcceptTrade();
			chr2.TradeInfo.AcceptTrade();

			Assert.IsNull(chr1.TradeInfo);
			Assert.IsNull(chr2.TradeInfo);
		}

		private void CheckTrade()
		{
			Assert.IsNotNull(chr1.TradeInfo);
			Assert.IsNotNull(chr2.TradeInfo);

			Assert.AreEqual(chr1.TradeInfo.Other, chr2.TradeInfo);
			Assert.AreEqual(chr1.TradeInfo, chr2.TradeInfo.Other);
		}
	}
}
