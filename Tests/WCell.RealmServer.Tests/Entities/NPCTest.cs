using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		public NPCTest()
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
			Setup.EnsureNPCsLoaded();

			// register the test packet handlers
			//RealmPacketManager.Instance.Register(typeof(CharacterTest));
			//FakePacketMgr.Instance.Register(typeof(CharacterTest));

		}

		[TestMethod]
		public void TestCreation()
		{
		}


		[TestMethod]
		public void TestXpDistribution()
		{
			var npc = Setup.NPCPool.CreateDummy();
			npc.EnsureInWorldAndLiving();

			var chr = Setup.AllianceCharacterPool.Create();

			var oldExp = new Experience(chr.Level, chr.XP);

			chr.Level = npc.Level = 1;

			// be close in order to gain any xp
			chr.EnsureXDistance(npc, 1.0f, true);

			// calc the xp that chr will gain
			var gainedXp = npc.Region.XpCalculator(chr.Level, npc);
			Asser.GreaterThan(gainedXp, 0u);

			// let the npc die and chr be the killer
			npc.Region.AddMessageAndWait(() => {
				npc.FirstAttacker = chr;
				npc.Health = 0;

				var currentExp = chr.GetExperience();
				Assert.AreEqual(oldExp + gainedXp, currentExp);
			});
		}

	}
}
