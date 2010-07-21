using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.Core;
using WCell.Constants;
using WCell.RealmServer.AI;
using WCell.RealmServer.Items;
using WCell.RealmServer.Factions;
using WCell.Constants.Factions;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Tests.Entities
{
	/// <summary>
	/// Summary description for NPCTest
	/// </summary>
	[TestClass]
	public class NPCTest
	{
		static NPC CreateDummy(int i)
		{
			var npc = Setup.NPCPool.CreateDummy();
			npc.EnsureInWorldAndLiving();
			npc.IsEvading = false;
			npc.Name = i.ToString();

			Assert.IsTrue(npc.CanGenerateThreat);
			return npc;
		}

		public NPCTest()
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

		[ClassInitialize()]
		public static void Initialize(TestContext testContext)
		{
			//Setup.EnsureNPCsLoaded();

			// register the test packet handlers
			//RealmPacketManager.Instance.Register(typeof(CharacterTest));
			//FakePacketMgr.Instance.Register(typeof(CharacterTest));

		}

		[TestMethod]
		public void TestDisarm()
		{
			var mob = CreateDummy(1);

			mob.Entry.MinDamage = mob.Entry.MaxDamage = 100;
			mob.MainWeapon = mob.Entry.CreateMainHandWeapon();

			Assert.AreEqual(mob.MinDamage, 100);

			mob.SetDisarmed(InventorySlotType.WeaponMainHand);
			Assert.AreEqual(mob.MainWeapon, GenericWeapon.Fists);

			mob.UnsetDisarmed(InventorySlotType.WeaponMainHand);

			Assert.AreEqual(mob.MinDamage, 100);
		}
	}
}