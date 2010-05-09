using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Database;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Debugging;
using WCell.Core;
using WCell.Constants;
using WCell.Util.Threading;
using Castle.ActiveRecord;
using WCell.Constants.Updates;
using System.Threading;
using WCell.Constants.Items;
using WCell.Constants.Talents;
using System.Collections;
using WCell.RealmServer.Database;
using WCell.RealmServer.Items;
using WCell.RealmServer.Chat;

namespace WCell.RealmServer.Tests.Entities
{
	/// <summary>
	/// Summary description for CharacterTest
	/// </summary>
	[TestClass]
	public class CharacterTest
	{
		public CharacterTest()
		{
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
		public static void Initialize(TestContext testContext)
		{
			Setup.EnsureBasicSetup();
		}

		[ClassCleanup]
		public static void TearDown()
		{
			// make sure we get rid of the Characters again (since we create so many)
			DatabaseUtil.DropSchema();
			DatabaseUtil.CreateSchema();
		}

		#region Loading
		[TestMethod]
		public void TestLoading()
		{
			//    var chr = Setup.DefaultCharacter;

			//    Setup.EnsureItemsLoaded();

			//    chr.Skills.Clear();
			//    chr.Spells.Clear();

			//    Assert.AreEqual(0, chr.Skills.Count);
			//    Assert.AreEqual(0, chr.Spells.Count);

			//    var reputationCount = chr.Reputations.Count;

			//    chr.Skills.Add(SkillId.Fire, 100, 200, false, true);
			//    chr.Spells.Add(SpellId.Firebolt);

			//    // no faction has initial reputation with Booty Bay
			//    var rep = chr.Reputations.SetValue(FactionRepListId.BootyBay, 1111);
			//    Assert.IsNotNull(rep);
			//    Assert.AreEqual(1111, rep.Value);

			//    Assert.AreEqual(1, chr.Skills.Count);
			//    Assert.AreEqual(1, chr.Spells.Count);
			//    Assert.AreEqual(reputationCount + 1, chr.Reputations.Count);

			//    // TODO: Wait on I/O Thread
			//    chr.LogoutLater();

			//    var charRecord2 = CharacterRecord.GetRecordByEntityId(chr.EntityId.Low);

			//    Assert.IsNotNull(charRecord2, "Character was not saved!");

			//    var char2 = new Character(chr.Account, charRecord2, chr.Client);

			//    Assert.AreEqual(1, char2.Skills.Count, "Loading of Skills failed.");
			//    Assert.AreEqual(1, char2.Spells.Count, "Loading of Spells failed.");
			//    Assert.AreEqual(reputationCount + 1, char2.Reputations.Count, "Loading of Reputations failed.");

			//    Assert.AreEqual(100, char2.Skills[SkillId.Fire].CurrentValue);
			//    Assert.AreEqual(200, char2.Skills[SkillId.Fire].MaxValue);

			//    Assert.IsTrue(char2.Spells.Contains(SpellId.Firebolt));

			//    Assert.AreEqual(reputationCount + 1, chr.Reputations.Count);
			//    rep = char2.Reputations[FactionRepListId.BootyBay];
			//    Assert.IsNotNull(rep);
			//    Assert.AreEqual(1111, rep.Value);
		}
		#endregion

		#region Movement
		[TestMethod]
		public void TestTeleport()
		{
			PacketUtil.GetUpdatePackets = true;

			var chr = Setup.AllianceCharacterPool.Create();

			var newRegion = World.Outland;
			Assert.AreNotEqual(newRegion, chr.Region);

			// make sure we are in the world
			chr.EnsureAloneInWorldAndLiving();
			Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);

			// wait a little until all updates have been sent
			World.Outland.WaitTicks(2);

			var blocks = chr.FakeClient.GetUpdateBlocks(UpdateType.CreateSelf);
			chr.FakeClient.PurgeUpdatePackets();

			Assert.AreEqual(1, blocks.Count, "CreateSelf Update Packet-count is " + blocks.Count);

			// teleport
			chr.TeleportTo(World.Outland, true);

			Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);
			// wait a little until all updates have been sent
			World.Outland.WaitTicks(2);

			Assert.AreEqual(World.Outland, chr.Region);

			// make sure we got our CreateSelf update-packet after teleporting
			blocks = chr.FakeClient.GetUpdateBlocks(UpdateType.CreateSelf);
			chr.FakeClient.PurgeUpdatePackets();

			Assert.AreEqual(1, blocks.Count, "CreateSelf Update Packet-count is " + blocks.Count);
		}
		#endregion

		#region Regeneration
		[TestMethod]
		public void TestRegen()
		{
			var chr = Setup.DefaultCharacter;

			chr.EnsureInWorld();
			chr.EnsureHealth(100);
			chr.EnsurePower(100);
			chr.Power = (int)chr.MaxPower - 10;
			chr.PowerRegenPerTick = 20;
			chr.RegenerationDelay = 0.0001f;

			// wait one region tick to regen
			chr.Region.AddMessageAndWait(new Message(() => {
				Assert.AreEqual(chr.Power, (int)chr.MaxPower);
			}));
		}
		#endregion

		#region Chat
		[TestMethod]
		public void TestSimpleChat()
		{
			var chr1 = Setup.AllianceCharacterPool.Create();
			var chr2 = Setup.AllianceCharacterPool.Create();

			DebugUtil.Dumps = true;

			chr1.EnsureInWorldAndLiving();
			chr2.EnsureLiving();

			chr2.EnsureXDistance(chr1, 1, true);

			Thread.Sleep(1000);

			chr1.Region.AddMessageAndWait(new Message1<Region>(
				chr1.Region, (rgn) => ChatMgr.SendSystemMessage(rgn.Characters, "huhu")
				));
		}
		#endregion

		#region Stress Tests
		[TestMethod]
		public void TestManyCharsLogin()
		{
			DebugUtil.Dumps = false;
			PacketUtil.GetUpdatePackets = false;

			Setup.AllianceCharacterPool.DefaultIsNew = true;

			// 3k will result into OutOfMemoryException
			Setup.AllianceCharacterPool.AddCharacters(1000);
		}

		[TestMethod]
		public void TestManyCharsLoginLogout()
		{
			Setup.WriteRamUsage("Starting Login/LogoutLater Test");

			//var mb = 1024 * 1024;
			//var allowedDifference = 5 * 1024 * 1024;	// 5 MB
			var num = 10;								// Amount of to add chars
			var repititions = 3;						// repeat several times

			DebugUtil.Dumps = false;
			PacketUtil.GetUpdatePackets = false;

			Setup.AllianceCharacterPool.Clear();
			Setup.AllianceCharacterPool.DefaultIsNew = true;

			GC.Collect();
			long initialRam = 0;
			for (int i = 0; i < repititions + 1; i++)
			{
				Setup.AllianceCharacterPool.AddCharacters(num);
				Setup.AllianceCharacterPool.Clear();

				// enqueue IO-task, so that by the time, the task is executed, all Characters have been saved to DB (and fully logged out)
				RealmServer.Instance.AddMessage(new Message(() => {
					Setup.WriteRamUsage("Removed {0} Characters", num);
					lock (this)
					{
						Monitor.PulseAll(this);
					}
				}));
				lock (this)
				{
					Monitor.Wait(this);
				}
				if (i == 0)
				{
					initialRam = GC.GetTotalMemory(true);
				}
			}
			GC.Collect();
			var currentRam = GC.GetTotalMemory(true);
			//Asser.GreaterOrEqual(initialRam + allowedDifference, currentRam,
			//    "Memory Leak detected: After adding and removing {0} Characters {1} times, Memory usage increased by more than {2}MB ({3}MB)",
			//    num, repititions, allowedDifference / (float)mb, (currentRam - initialRam) / (float)mb);
		}

		/// <summary>
		/// 
		/// </summary>
		//[TestMethod]
		public void TestMutualVisibility()
		{
			DebugUtil.Dumps = false;
			PacketUtil.GetUpdatePackets = true;

			Setup.AllianceCharacterPool.DefaultIsNew = false;

			// don't set this too high or it will run out of memory because of all the ParsedUpdatePackets being created
			var num = 30;
			Setup.Kalimdor.CleanSweep();
			var chars = Setup.AllianceCharacterPool.AddCharacters(num);

			// make sure we got 1 CreateSelf and (num-1) Create update-packets for each Char

			for (var i = 0; i < chars.Length; i++)
			{
				var chr = chars[i];
				var createSelfUpdates = chr.FakeClient.GetUpdateBlocks(UpdateType.CreateSelf);
				Assert.AreEqual(1, createSelfUpdates.Count, "{0} received {1} CreateSelf updates", chr.Name, createSelfUpdates.Count);

				var createUpdates = chr.FakeClient.GetUpdateBlocks(UpdateType.Create);
				Assert.AreEqual(num - 1, createUpdates.Count, "{0} ({3}) received {1}/{2} creation packets", chr.Name, createUpdates.Count,
				                num - 1, i);

				chr.FakeClient.PurgeUpdatePackets();
			}

			DebugUtil.Dumps = true;
		}

		#endregion

		#region Death
		[TestMethod]
		public void TestDeath()
		{
			PacketUtil.GetUpdatePackets = true;

			var chr = Setup.AllianceCharacterPool.Create();
			var client = chr.FakeClient;

			chr.EnsureInWorldAndLiving();

			// make sure, Character is initialized
			Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);
			chr.Region.WaitTicks(2);
			client.PurgeUpdatePackets();

			// lets die
			chr.Region.AddMessageAndWait(new Message(() => {
				chr.Health = 0;
				chr.Region.ForceUpdateCharacters(true);
			}));

			Assert.IsFalse(chr.IsAlive);
			Assert.IsFalse(chr.CanMove);

			var valueBlocks = client.GetUpdateBlocks(chr.EntityId, UpdateType.Values);
			Assert.AreEqual(1, valueBlocks.Count);

			var healthUpdate = valueBlocks.First();
			Assert.AreEqual(0, healthUpdate.GetInt(UnitFields.HEALTH));

			//var ghostAura = chr.Auras.GhostAura;
			//Assert.IsNotNull(ghostAura);

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_REPOP_REQUEST, 0);
			client.ReceiveCMSG(packet, true);

			chr.Region.WaitTicks(2);

			Assert.IsNotNull(chr.Corpse);
			Assert.IsFalse(chr.IsAlive);
			Assert.IsTrue(chr.CanMove);

			Assert.IsFalse(chr.IsRegenerating);
			Assert.AreEqual(1, chr.Health);

			client.PurgeSMSGs();

			chr.Resurrect();

			Assert.IsNull(chr.Auras.GhostAura);
			Assert.IsNull(chr.Corpse);
			Assert.AreEqual(true, chr.IsAlive);
			Assert.AreEqual(true, chr.CanMove);
			Assert.AreEqual(true, chr.Regenerates);
		}
		#endregion

		#region Inspect
		[TestMethod]
		public void TestInspect()
		{
			Setup.EnsureItemsLoaded();

			var swordTempl = ItemMgr.GetTemplate(ItemId.Shortsword);

			if (swordTempl.EquipmentSlots == null)
			{
				swordTempl.EquipmentSlots = new[] { EquipmentSlot.MainHand };
				swordTempl.IsWeapon = true;
			}

			var chr1 = Setup.AllianceCharacterPool.Create();
			var learntTalent = chr1.Talents.Trees[0].Talents[10];
			var learntRank = 2;

			chr1.EnsureInWorldAndLiving();
			chr1.Talents.Owner.FreeTalentPoints = 12;
			chr1.Talents.Set(learntTalent, learntRank);

			var chr2 = Setup.AllianceCharacterPool.Create();
			chr2.EnsureLiving();
			chr2.EnsureXDistance(chr1, 1.0f, true);

			var client2 = chr2.FakeClient;

			chr1.Inventory.TryAdd(swordTempl, InventorySlot.MainHand);
			var sword = chr1.Inventory[InventorySlot.MainHand];
			Assert.IsNotNull(sword, "Dummy Sword was not added to MainHand slot.");

			Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);
			chr1.Region.WaitTicks(2);
			client2.PurgeUpdatePackets();

			PacketUtil.GetUpdatePackets = true;

			var packet = new RealmPacketOut(RealmServerOpCode.CMSG_INSPECT);
			packet.Write(chr1.EntityId);
			client2.ReceiveCMSG(packet, true);

			Thread.Sleep(Region.CharacterUpdateEnvironmentTicks * Region.DefaultUpdateDelay);
			chr1.Region.WaitTicks(2);

			// Expected response: 1. send creation of all items to the new observer + 2. Send all talents
			var swordCreations = client2.GetUpdateBlocks(sword.EntityId, UpdateType.Create);
			Assert.IsNotNull(swordCreations);
			Assert.AreEqual(1, swordCreations.Count);


			var talentInfo = client2.DequeueSMSGInfo(RealmServerOpCode.SMSG_INSPECT_TALENT);
			Assert.AreEqual(chr1.Talents.Owner.FreeTalentPoints, (int)talentInfo.Parser.ParsedPacket["FreePoints"].UIntValue);

			var talentParser = talentInfo.Parser.Packet;
			var bytes = talentParser.ReadBytes(((int)learntTalent.Index / 8) + 1);
			var arr = new BitArray(bytes);
			Assert.AreEqual(true, arr[(int)(learntTalent.Index + learntRank - 1)]);
		}
		#endregion

        [TestMethod]
        public void TestLevelUp()
        {
            var chr = Setup.AllianceCharacterPool.Create();

            var str = chr.Strength;
            var stam = chr.Stamina;
            var agi = chr.Agility;
            var spi = chr.Spirit;
            var intel = chr.Intellect;

            chr.GainXp(chr.NextLevelXP - chr.XP, false);

            var newStr = chr.Strength;
            var newStam = chr.Stamina;
            var newAgi = chr.Agility;
            var newSpi = chr.Spirit;
            var newIntel = chr.Intellect;
            
        }
	}
}
