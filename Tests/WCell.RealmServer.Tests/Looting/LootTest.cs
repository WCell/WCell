using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Tests.Groups;
using WCell.Constants.NPCs;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Items;
using WCell.Constants.Items;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Tests.Looting
{
	/// <summary>
	/// Summary description for LootTest
	/// </summary>
	[TestClass]
	public class LootTest
	{
		public LootTest()
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

		[TestMethod]
		public void TestNeedGreedSorting()
		{
			var rolls = new SortedDictionary<LootRollEntry, int>();

			rolls.Add(new LootRollEntry(20, LootRollType.Greed), 20);
			rolls.Add(new LootRollEntry(10, LootRollType.Greed), 10);

			Assert.AreEqual(10, rolls.First().Value);
		}

		[TestMethod]
		public void TestRolling()
		{
			Setup.EnsureBasicSetup();
			ItemMgr.InitMisc();

			// create group
			var group = GroupTest.CreateGroup(3);
			group.LootMethod = LootMethod.NeedBeforeGreed;

			var leaderMember = group.Leader;
			var leader = (TestCharacter)leaderMember.Character;

			// create corpse
			var npc = Setup.NPCPool.CreateDummy();
			npc.EnsureInWorld();
			npc.FirstAttacker = leader;
			npc.Health = 0;

			// create loot
			var loot = npc.Loot = new NPCLoot(npc, 0, new[] {
				new ItemStackTemplate(new ItemTemplate
				{
					ItemId = ItemId.Shortsword,
					Id = (uint)ItemId.Shortsword,
					DefaultName = "Test Item1",
					MaxAmount = 3,
					Quality = ItemQuality.Artifact
				})
			});
			var lootItem = loot.Items[0];

			foreach (var member in group)
			{
				// let them all be next to the corpse
				((TestCharacter)member.Character).EnsureXDistance(npc, 0.5f, true);
			}

			leader.Region.AddMessageAndWait(() => {
				var looters = LootMgr.FindLooters(npc, leader);
				Assert.AreEqual(group.Count, looters.Count);

				// initialize the Loot
				loot.Initialize(leader, looters, npc.RegionId);
			});
			
			Assert.IsNotNull(lootItem.RollProgress);

			// everyone should now get the n/g box
			var i = 0;
			foreach (var member in group)
			{
				var chr = ((TestCharacter)member.Character);

				var rollStart = chr.FakeClient.DequeueSMSG(RealmServerOpCode.SMSG_LOOT_START_ROLL);
				Assert.IsNotNull(rollStart);

				Assert.AreEqual(npc.EntityId, rollStart["Looted"].Value);
				Assert.AreEqual(lootItem.Index, rollStart["ItemIndex"].Value);
				Assert.AreEqual(lootItem.Template.ItemId, rollStart["ItemTemplate"].Value);
				i++;
			}

			// let everyone roll
			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_LOOT_ROLL);
			packet.Write(npc.EntityId);
			packet.Write(lootItem.Index);
			packet.Write((byte)LootRollType.Need);		// need always before greed
			leader.FakeClient.ReceiveCMSG(packet, true);

			Assert.AreEqual(lootItem.RollProgress.HighestParticipant, leader);

			packet = new RealmPacketOut(RealmServerOpCode.CMSG_LOOT_ROLL);
			packet.Write(npc.EntityId);
			packet.Write(lootItem.Index);
			packet.Write((byte)LootRollType.Greed);
			((TestCharacter)leaderMember.Next.Character).FakeClient.ReceiveCMSG(packet, true);

			Assert.AreEqual(lootItem.RollProgress.HighestParticipant, leader);
			Assert.AreEqual(1, lootItem.RollProgress.RemainingParticipants.Count);			// one left

			packet = new RealmPacketOut(RealmServerOpCode.CMSG_LOOT_ROLL);
			packet.Write(npc.EntityId);
			packet.Write(lootItem.Index);
			packet.Write((byte)LootRollType.Greed);
			((TestCharacter)leaderMember.Next.Next.Character).FakeClient.ReceiveCMSG(packet, true);

			// everyone rolled, item should have been given to the winner
			Assert.IsNull(lootItem.RollProgress);
			Assert.IsTrue(lootItem.Taken);
			Assert.IsTrue(leader.Inventory.Contains(lootItem.Template.Id, (int)lootItem.Amount, true));

			// since that was the only item: Loot should be unset and the corpse should now decay
			Assert.IsNull(npc.Loot);
			Assert.IsTrue(npc.IsDecaying);

			// TODO: Take money
			// TODO: Be not able to take items that are being rolled for
		}
	}
}