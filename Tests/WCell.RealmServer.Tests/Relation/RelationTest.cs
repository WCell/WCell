using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants;
using WCell.Constants.World;
using WCell.Core;
using WCell.PacketAnalysis;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Tests.Misc;
using WCell.RealmServer.Interaction;

namespace WCell.RealmServer.Tests.Relation
{
	/// <summary>
	/// Summary description for WhoListTest
	/// </summary>
	[TestClass]
	public class RelationTest : PacketUtil
	{
		private static List<TestCharacter> _allianceChars = new List<TestCharacter>(10);
		private static List<TestCharacter> _hordeChars = new List<TestCharacter>(10);
		
		public RelationTest()
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
		public static void TestRelationClassInitialize(TestContext testContext)
		{
			TestCharacter character;
			for (int i = 0; i < _allianceChars.Capacity; i++)
			{
				character = Setup.AllianceCharacterPool.Create();
				character.EnsureInWorld();

				_allianceChars.Add(character);
			}

			for (int i = 0; i < _hordeChars.Capacity; i++)
			{
				character = Setup.HordeCharacterPool.Create();
				character.EnsureInWorld();

				_hordeChars.Add(character);
			}

			Assert.AreEqual(10, _allianceChars.Count);
			Assert.AreEqual(10, _hordeChars.Count);
		}

		[Owner("Nosferatus"), TestMethod]
		public void TestRelationAdd()
		{
			BaseRelation relation = null;
			BaseRelation storedRelation = null;
			int i = 0;

			foreach (CharacterRelationType type in Enum.GetValues(typeof(CharacterRelationType)))
			{
				if (type != CharacterRelationType.Invalid && type != CharacterRelationType.Count)
				{
					relation = RelationMgr.CreateRelation(_allianceChars[i].EntityId.Low,
						_allianceChars[i + 1].EntityId.Low, type);

					RelationMgr.Instance.AddRelation(relation);

					storedRelation = RelationMgr.Instance.GetRelation(_allianceChars[i].EntityId.Low,
						_allianceChars[i + 1].EntityId.Low, type);

					Assert.AreEqual(relation, storedRelation);
					i++;
				}
			}
		}

		[TestCleanup()]
		public void TestRelationAddCleanup() 
		{
			BaseRelation relation = null;
			BaseRelation storedRelation = null;
			int i = 0;

			foreach (CharacterRelationType type in Enum.GetValues(typeof(CharacterRelationType)))
			{
				if (type != CharacterRelationType.Invalid && type != CharacterRelationType.Count)
				{
					relation = RelationMgr.Instance.GetRelation(_allianceChars[i].EntityId.Low,
						_allianceChars[i + 1].EntityId.Low, type);

					RelationMgr.Instance.RemoveRelation(relation);

					storedRelation = RelationMgr.Instance.GetRelation(_allianceChars[i].EntityId.Low,
						_allianceChars[i + 1].EntityId.Low, type);

					Assert.IsNull(storedRelation);
					i++;
				}
			}
		}
	}
}