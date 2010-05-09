using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Tests.Misc;
using WCell.Core.Network;
using WCell.Core;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Items;

namespace WCell.RealmServer.Tests.Items
{
	/// <summary>
	/// Summary description for InventoryTest
	/// </summary>
	[TestClass]
	public class InventoryTest : PacketUtil
	{
		public InventoryTest()
		{
			//
			// TODO: Add constructor logic here
			//
		}

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
		public static void Initialize(TestContext testContext)
		{
			Setup.EnsureBasicSetup();
            Setup.EnsureItemsLoaded();
		}


		[TestMethod]
		public void TestAddItems()
		{
			ItemMgr.LoadAll();
			var chr = Setup.AllianceCharacterPool.Create();
			var inv = chr.Inventory;

			chr.Level = 70;
			chr.EnsureInWorldAndLiving();
			PlayerInventory.AutoEquipNewItems = true;

			inv.Purge();

			var err = inv.TryAdd(ItemId.SmallRedPouch, InventorySlot.Bag1);
			Assert.AreEqual(InventoryError.OK, err);

			err = inv.TryAdd(ItemId.MiningSack);
			Assert.AreEqual(InventoryError.OK, err);

			var pouch = inv[InventorySlot.Bag1] as Container;
			Assert.IsNotNull(pouch);
			Assert.AreEqual(ItemId.SmallRedPouch, pouch.Template.ItemId);

			var miningBag = inv[InventorySlot.Bag2] as Container;
			Assert.IsNotNull(miningBag);
			Assert.AreEqual(ItemId.MiningSack, miningBag.Template.ItemId);

			var stuff = new[]
			          	{
			          		new ItemStackDescription(ItemId.SlimyMurlocScale, 3),
			          		new ItemStackDescription(ItemId.WornShortsword, 3)
			          	};

			var materials = new[]
			          	{
			          		new ItemStackDescription(ItemId.SilverOre, 3),
			          		new ItemStackDescription(ItemId.GoldOre, 3)
			          	};

			err = inv.TryAddAll(stuff);
			Assert.AreEqual(InventoryError.OK, err);
			Assert.AreEqual(4, inv.Count);	// 2 bags + 2 other items
			Assert.AreEqual(0, miningBag.BaseInventory.Count);

			err = inv.TryAddAll(materials);
			Assert.AreEqual(InventoryError.OK, err);
			Assert.AreEqual(2, miningBag.BaseInventory.Count);
		}


		[TestMethod]
		public void TestSwap()
		{
		}

		[TestMethod]
		public void TestDelete()
		{
			//chr.EnsureItem(InventorySlot.Head);
		}

        [TestMethod]
        public void TestEquipmentSets()
        {
            
        }
	}
}
