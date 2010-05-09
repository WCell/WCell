using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Cell.Core.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace WCell.Core.Tests
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

		//[TestMethod]
		public void TestAll()
		{
			var num = 5000;

			bool done = false;
			var q = new LockfreeQueue<int>();
			int lastItem = -1;
			for (int i = 0; i < num; i++)
			{
				q.Enqueue(i);
			}

			Assert.AreEqual(num, q.Count);

			var values = new int[num];
			for (int i = 0; i < num; i++)
			{
				ThreadPool.QueueUserWorkItem((indexObj) => {
					var index = (int)indexObj;
					var x = q.TryDequeue();

					lock (values)
					{
						if (index == num - 1)
						{
							done = true;
							Monitor.PulseAll(values);
						}
					}
				}, i);
			}

			lock (values)
			{
				if (!done)
				{
					Monitor.Wait(values);
				}
			}

			var uniqueStrings = new HashSet<int>();
			for (int i = 0; i < values.Length; i++)
			{
				var s = values[i];

				Assert.IsNotNull(s);
				uniqueStrings.Add(s);
			}
			Assert.AreEqual(values.Length, uniqueStrings.Count);
		}
	}
}
