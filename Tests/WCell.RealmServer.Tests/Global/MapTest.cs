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
	/// Summary description for MapTest
	/// </summary>
	[TestClass]
	public class MapTest
	{
		public MapTest()
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

		//    var map = WorldMgr.Kalimdor;

		//    // we need a player to start the update loop
		//    Setup.AllianceCharacterPool.First.TeleportTo(map, true);

		//    map.SpawnMapLater();

		//    var startTime = DateTime.Now;
		//    map.WaitOneTick();	// spawns default objects
		//    Debug.Assert(map.ObjectCount > 0, "No objects were spawned");

		//    map.WaitOneTick();	// does initial update to default objects

		//    Thread.Sleep(100000);

		//    Assert.AreEqual(0, map.PendingUpdateCount);
		//    Setup.WriteLine("Spawned all of Kalimdor in: " + (DateTime.Now - startTime));
		//}
	}
}