using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Tests.Tools;

namespace WCell.RealmServer.Tests.Talents
{
    /// <summary>
    /// Summary description for TalentTests
    /// </summary>
    [TestClass]
    public class TalentTests
    {
        private TestContext testContextInstance;
        private static TestCharacter Padawan;
        private static NPC Jedi;

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
        public static void MyClassInitialize(TestContext testContext)
        {
            Setup.EnsureBasicSetup();
            Setup.EnsureNPCsLoaded();
            
            EnsureChar(ref Padawan, Setup.AllianceCharacterPool);
            EnsureNPC(ref Jedi, Setup.NPCPool);
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
            //npc = pool.CreateTrainer();
            npc.EnsureInWorldAndLiving();
        }

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic	here
            //
        }
    }
}