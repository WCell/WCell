using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Network;
using WCell.Constants;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;
using WCell.Util;
using WCell.RealmServer.Entities;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Tests.Misc
{
	/// <summary>
	/// Summary description for MiscTest
	/// </summary>
	[TestClass]
	public class MiscTest
	{
		public MiscTest()
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

		[ClassInitialize]
		public static void Init(TestContext context)
		{
			Setup.EnsureMinimalSetup();
		}

		#region Character Name
		/// <summary>
		/// Method tests IsCharacterNameValid method with various character names. 
		/// </summary>
		[TestMethod]
		public void TestIsCharacterNameValid()
		{
			//CharacterErrorCodes createErrorCode;
			//AccountCharacterErrorCodes renameErrorCode;
			LoginErrorCode errorCode;

			RealmServerConfiguration.CapitalizeCharacterNames = true;

			string charName = "Djbobo";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);

			charName = "oscar";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);
			Assert.AreEqual("Oscar", charName);

			charName = "oScaR";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);
			Assert.AreEqual("Oscar", charName);

			charName = "Djbobo12";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.CHAR_NAME_INVALID_CHARACTER, errorCode);

			charName = "Dj bobo";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.CHAR_NAME_INVALID_SPACE, errorCode);

			// this is allowed, but client wont allow apostrophe, so ..
			charName = "Dj'bobo";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);

			charName = "'Djbobo";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.CHAR_NAME_INVALID_APOSTROPHE, errorCode);

			// set blizz like names flag to false
			RealmServerConfiguration.CapitalizeCharacterNames = false;

			charName = "Djbobo";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);

			charName = "oscar";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);
			Assert.AreEqual("oscar", charName);

			charName = "oScaR";
			errorCode = CharacterHandler.IsNameValid(ref charName);
			Assert.AreEqual(LoginErrorCode.RESPONSE_SUCCESS, errorCode);
			Assert.AreEqual("oScaR", charName);

			// default is true, so set it
			RealmServerConfiguration.CapitalizeCharacterNames = true;
		}

		[TestMethod]
		public void TestDirections()
		{
			var v = new Vector3(100, 100, 1);
			var v2 = new Vector3(0, 0, 2);

			v.GetPointXY(MathUtil.PI, 10, out v2);

			Assert.AreEqual(new Vector3(100, 90, 2), v2);

			var v3 = new Vector3(20, 20, 5);
			v3.GetPointXY(WorldObject.BehindAngleMax, 10, out v2);
		}

		[TestMethod]
		public void TextExploBits()
		{
			var bit = (126 - 1) % 32;
			var bit2 = (127 - 1) % 32;

			uint i1 = (uint) (1 << bit);
			uint i2 = (uint) (1 << bit2);

			Assert.AreEqual(0x20000000u, i1);
			Assert.AreEqual(0x40000000u, i2);
		}
		#endregion
	}
}