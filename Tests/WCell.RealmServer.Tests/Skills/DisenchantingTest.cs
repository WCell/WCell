using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.Skills;
using WCell.RealmServer.Content;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Items;
using WCell.RealmServer.Looting;
using WCell.Constants.Spells;
using WCell.Constants.Items;
using WCell.RealmServer.Network;
using WCell.RealmServer.Tests.Network;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Tests.Skills
{
	/// <summary>
	/// Summary description for Disenchanting
	/// </summary>
	[TestClass]
	public class DisenchantingTest
	{
		private static TestCharacter chr;
		static TestFakeClient client;
		private static Spell disenchantSpell;
		private static ItemId itemId = ItemId.TwistedSabre;
		private static ItemId expectedLootId = ItemId.LargeGlimmeringShard;

		public DisenchantingTest()
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

		[ClassInitialize]
		public static void Init(TestContext testContext)
		{
			chr = Setup.AllianceCharacterPool.Create();
			client = chr.FakeClient;
			disenchantSpell = SpellHandler.Get(SpellId.DisenchantPT);
			disenchantSpell.CastDelay = 0;	// we don't want to wait

			ItemMgr.LoadAll();
			//LootMgr.LoadAll();
			ContentMgr.Load<ItemLootItemEntry>();
		}

		[ClassCleanup]
		public static void Cleanup()
		{
			chr.Logout(true);
		}

		[TestMethod]
		public void TestDisenchanting()
		{
			var inv = chr.Inventory;
			var item = chr.EnsureItem(itemId);
			var skill = chr.Skills.GetOrCreate(SkillId.Enchanting, true);

			skill.CurrentValue = 50;
			skill.MaxValue = 75;

			chr.SpellCast.UsedItem = item;
			
			// start disenchanting
			var err = chr.SpellCast.Start(disenchantSpell, false);
			Assert.AreEqual(SpellFailedReason.Ok, err, "Disenchanting failed: " + err);

			// Item must still be there (until looting finishes)
			item = inv.GetItemByItemId(itemId);
			Assert.IsNotNull(item);

			// check for whether we have the expected loot
			var loot = chr.LooterEntry.Loot;
			Assert.IsNotNull(loot);

			var shards = loot.Items.Where((lootItem) => lootItem.Template.ItemId == expectedLootId);
			Assert.AreEqual(1, shards.Count());

			var lootResponse = client.DequeueSMSG(RealmServerOpCode.SMSG_LOOT_RESPONSE);
			Assert.IsNotNull(lootResponse);
		}
	}
}