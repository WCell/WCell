using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.AI.Actions;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Tests.Tools;

namespace WCell.RealmServer.Tests.AI
{
	/// <summary>
	/// Summary description for AIComplexActionTest
	/// </summary>
	[TestClass]
	public class AIComplexActionTest
	{
		public AIComplexActionTest()
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
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			Setup.EnsureNPCsLoaded();
		}
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

		private const int ACTIONS_COUNT = 5;

		private const int REPEAT_COUNT = 5;

		[TestMethod]
		public void TestAIComplexAction()
		{
			//var factory = new AICompositeActionFactory();
			//var npc = Setup.NPCPool.CreateDummy();
			//npc.Brain = new BaseBrain(npc);

			//AIActionStub[] actions = new AIActionStub[ACTIONS_COUNT];

			//for (int i = 0; i < ACTIONS_COUNT; i++)
			//    actions[i] = new AIActionStub(npc.Brain);

			//actions[ACTIONS_COUNT - 1].Executing = true;

			//var complexAction = factory.Compose(npc.Brain, actions);

			//complexAction.Start();

			//for (int i = 0; i < ACTIONS_COUNT; i++)
			//{
			//    Assert.IsTrue(actions[i].Started);
			//    Assert.AreEqual(actions[i].UpdateCount, 1);
			//}

			//for (int i = 0; i < REPEAT_COUNT; i++)
			//{
			//    complexAction.Update();
			//}

			//for (int i = 0; i < ACTIONS_COUNT - 1; i++)
			//{
			//    Assert.IsTrue(actions[i].Started);
			//    Assert.AreEqual(actions[i].UpdateCount, 1);
			//}

			//Assert.IsTrue(actions[ACTIONS_COUNT - 1].Started);
			//Assert.AreEqual(actions[ACTIONS_COUNT - 1].UpdateCount, REPEAT_COUNT + 1);

			//complexAction.Stop();

			//Assert.IsFalse(actions[ACTIONS_COUNT - 1].Started);
		}
	}
}