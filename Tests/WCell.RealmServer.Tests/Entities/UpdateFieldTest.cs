using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.PacketAnalysis.Updates;

namespace WCell.RealmServer.Tests.Entities
{
	/// <summary>
	/// Summary description for UpdateFieldTest
	/// </summary>
	[TestClass]
	public class UpdateFieldTest
	{
		public UpdateFieldTest()
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
			if (UpdateFieldMgr.Collections[0] == null)
			{
				UpdateFieldMgr.Init();
			}
			FieldRenderUtil.Init();
		}

		[TestMethod]
		public void TestUpdateFieldSetup()
		{
			var collection = UpdateFieldMgr.Collections[(int)ObjectTypeId.Player];
			var i = 0;
			foreach (var field in collection.Fields)
			{
				Assert.IsNotNull(field, "Field {0} is null", FieldRenderUtil.GetFieldStr(ObjectTypeId.Player, i));
				i++;
			}
		}
	}
}
