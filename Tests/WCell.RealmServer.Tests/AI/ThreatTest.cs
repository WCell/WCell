using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Tests.AI
{
	[TestClass]
	public class ThreatTest
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

		[TestMethod]
		public void TestThreatCollection()
		{
			var collection = new ThreatCollection();

			var npc1 = CreateDummy(1);
			var npc2 = CreateDummy(2);
			var npc3 = CreateDummy(3);

			collection.AddNew(npc1);

			Assert.AreEqual(npc1, collection.CurrentAggressor);

			collection.AddNew(npc2);

			Assert.AreEqual(npc1, collection.CurrentAggressor);

			collection[npc2] = 10000;

			Assert.AreEqual(npc2, collection.CurrentAggressor);

			collection[npc3] = 1000000;

			Assert.AreEqual(npc3, collection.CurrentAggressor);

			collection[npc3] = 1;

			Assert.AreEqual(npc2, collection.CurrentAggressor);
			Assert.AreEqual(3, collection.Size);

			collection.Remove(npc2);
			Assert.AreEqual(npc3, collection.CurrentAggressor);

			collection.Remove(npc3);
			Assert.AreEqual(npc1, collection.CurrentAggressor);

			collection.Remove(npc1);
			Assert.AreEqual(null, collection.CurrentAggressor);

		}
	}
}