using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Tests.Entities
{
	/// <summary>
	/// Summary description for RangeTest
	/// </summary>
	[TestClass]
	public class RangeTest
	{
		public RangeTest()
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
		/// <summary>
		/// Gets the angle between this object and the given position, in relation to the north-south axis
		/// </summary>
		public float GetAngleTowards(Vector3 v, Vector3 thisPos)
		{
			var angleTowards = (float)Math.Atan2((v.Y - thisPos.Y), (v.X - thisPos.X));
			if (angleTowards < 0)
			{
				angleTowards = (2 * MathUtil.PI) + angleTowards;
			}
			return angleTowards;
		}

		/// <summary>
		/// Returns whether this Object is behind the given obj
		/// </summary>
		public bool IsBehind(Vector3 v, Vector3 thisPos, float orientation)
		{
			var angle = GetAngleTowards(v, thisPos);
			return Math.Abs(orientation - angle) > 3f * MathUtil.PI / 2f;		// difference is pi if they are looking in the same direction
		}

		/// <summary>
		/// Returns whether a is in front of b
		/// </summary>
		public bool IsInFrontOf(Vector3 a, Vector3 b, float orientation)
		{
			var angleTowards = GetAngleTowards(b, a);
			if (angleTowards < 0)
			{
				angleTowards = (2*MathUtil.PI) + angleTowards;
			}
			var angle = Math.Abs(orientation - angleTowards);
			return angle < MathUtil.PI / 3f ||
				angle > 5f * MathUtil.PI / 3f;		// difference is close to 0 (or 2 pi) if they are looking in opposite directions
		}

		[TestMethod]
		public void TestFacing()
		{
			var center = new Vector3(0, 0, 0);
			var north = new Vector3(1, 0, 0);
			var south = new Vector3(-1, 0, 0);
			var west = new Vector3(0, 1, 0);
			var east = new Vector3(0, -1, 0);

			var centerToNorth = GetAngleTowards(north, center);
			var centerToSouth = GetAngleTowards(south, center);
			var centerToWest = GetAngleTowards(west, center);
			var centerToEast = GetAngleTowards(east, center);
			var northToSouth = GetAngleTowards(south, north);
			var northToWest = GetAngleTowards(west, north);
			var northToEast = GetAngleTowards(east, north);
			var southToNorth = GetAngleTowards(north, south);
			var southToWest = GetAngleTowards(west, south);
			var southToEast = GetAngleTowards(east, south);

			Assert.IsTrue(IsInFrontOf(south, north, 0));
			Assert.IsTrue(IsInFrontOf(south, north, 6));
			Assert.IsTrue(IsInFrontOf(north, south, 3));
			Assert.IsFalse(IsInFrontOf(north, south, 1));
		}
	}
}