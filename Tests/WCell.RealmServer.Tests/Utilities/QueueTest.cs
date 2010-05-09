using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cell.Core.Collections;

namespace WCell.RealmServer.Tests.Utilities
{
	/// <summary>
	/// Summary description for QueueTest
	/// </summary>
	[TestClass]
	public class QueueTest
	{
		public QueueTest()
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
		public void TestEnqueueAndDequeue()
		{
			LockfreeQueue<int> q = new LockfreeQueue<int>();

			q.Enqueue(1);
			q.Enqueue(2);
			q.Enqueue(3);

			// queue count
			Assert.AreEqual(3, q.Count);

			int value;

			Assert.AreEqual(1, q.Dequeue());
			Assert.AreEqual(2, q.Dequeue());
			Assert.AreEqual(3, q.Dequeue());
			Assert.IsFalse(q.TryDequeue(out value));
		}
	}
}
