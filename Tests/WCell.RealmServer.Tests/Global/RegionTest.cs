using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Global;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.GameObjects;
using System.Diagnostics;
using System.Threading;

namespace WCell.RealmServer.Tests.Global
{
	/// <summary>
	/// Summary description for RegionTest
	/// </summary>
	[TestClass]
	public class RegionTest
	{
		public RegionTest()
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

		//[TestMethod]
		//public void TestSpawning()
		//{
		//    Setup.EnsureBasicSetup();

		//    NPCMgr.ForceInitialize();
		//    GOMgr.LoadAll();

		//    var region = WorldMgr.Kalimdor;

		//    // we need a player to start the update loop
		//    Setup.AllianceCharacterPool.First.TeleportTo(region, true);

		//    region.SpawnRegionLater();

		//    var startTime = DateTime.Now;
		//    region.WaitOneTick();	// spawns default objects
		//    Debug.Assert(region.ObjectCount > 0, "No objects were spawned");

		//    region.WaitOneTick();	// does initial update to default objects

		//    Thread.Sleep(100000);

		//    Assert.AreEqual(0, region.PendingUpdateCount);
		//    Setup.WriteLine("Spawned all of Kalimdor in: " + (DateTime.Now - startTime));
		//}
	}
}