using WCell.Core.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace WCell.Core.Tests
{
    /// <summary>
    ///This is a test class for BoundingBoxTest and is intended
    ///to contain all BoundingBoxTest Unit Tests
    ///</summary>
	[TestClass()]
	public class GraphicsTest
	{
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
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		/// <summary>
		/// Tests intersection of boxes.
		///</summary>
		[TestMethod()]
		public void TestBBoxIntersection()
		{
			BoundingBox targetBox = new BoundingBox(0, 0, 0, 20, 20, 20);
			BoundingBox intersectBox = new BoundingBox(-10, -10, -10, 10, 10, 10);
			BoundingBox containBox = new BoundingBox(0, 0, 0, 20, 20, 20);
			BoundingBox disjointBox = new BoundingBox(-30, -30, -30, -10, -10, -10);
			BoundingSphere intersectSphere = new BoundingSphere(new Vector3(0, 0, 0), 15);
			BoundingSphere containSphere = new BoundingSphere(new Vector3(10, 10, 10), 3);
			BoundingSphere disjointSphere = new BoundingSphere(new Vector3(-30, -30, -30), 5);
			BoundingBox intersectBoxExpected = intersectBox;
			BoundingBox containBoxExpected = containBox;
			BoundingBox disjointBoxExpected = disjointBox;
			BoundingSphere intersectSphereExpected = intersectSphere;
			BoundingSphere containSphereExpected = containSphere;
			BoundingSphere disjointSphereExpected = disjointSphere;

			IntersectionType intersectBoxTypeExpected = IntersectionType.Intersects;
			IntersectionType intersectBoxTypeActual;
			intersectBoxTypeActual = targetBox.Intersects(ref intersectBox);

			IntersectionType containBoxTypeExpected = IntersectionType.Contained;
			IntersectionType containBoxTypeActual;
			containBoxTypeActual = targetBox.Intersects(ref containBox);

			IntersectionType disjointBoxTypeExpected = IntersectionType.NoIntersection;
			IntersectionType disjointBoxTypeActual;
			disjointBoxTypeActual = targetBox.Intersects(ref disjointBox);

			IntersectionType intersectSphereTypeExpected = IntersectionType.Intersects;
			IntersectionType intersectSphereTypeActual;
			intersectSphereTypeActual = targetBox.Intersects(ref intersectSphere);

			IntersectionType containSphereTypeExpected = IntersectionType.Contained;
			IntersectionType containSphereTypeActual;
			containSphereTypeActual = targetBox.Intersects(ref containSphere);

			IntersectionType disjointSphereTypeExpected = IntersectionType.NoIntersection;
			IntersectionType disjointSphereTypeActual;
			disjointSphereTypeActual = targetBox.Intersects(ref disjointSphere);

			Assert.AreEqual(intersectBoxExpected, intersectBox);
			Assert.AreEqual(intersectBoxTypeExpected, intersectBoxTypeActual);
			Assert.AreEqual(containBoxExpected, containBox);
			Assert.AreEqual(containBoxTypeExpected, containBoxTypeActual);
			Assert.AreEqual(disjointBoxExpected, disjointBox);
			Assert.AreEqual(disjointBoxTypeExpected, disjointBoxTypeActual);
			Assert.AreEqual(intersectSphereExpected, intersectSphere);
			Assert.AreEqual(intersectSphereTypeExpected, intersectSphereTypeActual);
			Assert.AreEqual(containSphereExpected, containSphere);
			Assert.AreEqual(containSphereTypeExpected, containSphereTypeActual);
			Assert.AreEqual(disjointSphereExpected, disjointSphere);
			Assert.AreEqual(disjointSphereTypeExpected, disjointSphereTypeActual);
		}

		/// <summary>
		///A test for Contains
		///</summary>
		[TestMethod()]
		public void TestBoxContainment()
		{
			BoundingBox target = new BoundingBox(-10, -10, -10, 10, 10, 10);
			BoundingBox containedBox = new BoundingBox(-5, -5, -5, 5, 5, 5);
			BoundingBox intersectsBox = new BoundingBox(5, 5, 5, 15, 15, 15);
			BoundingBox containedBoxExpected = containedBox;
			BoundingBox intersectsBoxExpected = intersectsBox;

			bool containedExpected = true;
			bool containedActual;
			containedActual = target.Contains(ref containedBox);

			bool intersectsExpected = false;
			bool intersectsActual;
			intersectsActual = target.Contains(ref intersectsBox);

			Assert.AreEqual(containedBoxExpected, containedBox);
			Assert.AreEqual(containedExpected, containedActual);
			Assert.AreEqual(intersectsBoxExpected, intersectsBox);
			Assert.AreEqual(intersectsExpected, intersectsActual);
		}

		/// <summary>
		///A test for Contains
		///</summary>
		[TestMethod()]
		public void TestVector3Containment()
		{
			BoundingBox target = new BoundingBox(-10, -10, -10, 10, 10, 10);
			Vector3 point = new Vector3(-2.4432f, 7f, 4.55025f);
			Vector3 failPoint = new Vector3(-5.8f, 10.0001f, 0.4445f);
			Vector3 pointExpected = point;
			Vector3 failPointExpected = failPoint;

			bool expected = true;
			bool actual;
			actual = target.Contains(ref point);

			bool failExpected = false;
			bool failActual;
			failActual = target.Contains(ref failPoint);

			Assert.AreEqual(pointExpected, point);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(failPointExpected, failPoint);
			Assert.AreEqual(failExpected, failActual);
		}

		/// <summary>
		///A test for Contains
		///</summary>
		[TestMethod()]
		public void TestVector4Containment()
		{
			BoundingBox target = new BoundingBox(-10, -10, -10, 10, 10, 10);
			Vector4 point = new Vector4(-6.66666f, 7.8989898989f, -3f, -27.56999f);
			Vector4 failPoint = new Vector4(4.3555f, -10.00001f, 5.271f, -1000.334f);
			Vector4 pointExpected = point;
			Vector4 failPointExpected = failPoint;

			bool expected = true;
			bool actual;
			actual = target.Contains(ref point);

			bool failExpected = false;
			bool failActual;
			failActual = target.Contains(ref failPoint);

			Assert.AreEqual(pointExpected, point);
			Assert.AreEqual(expected, actual);
			Assert.AreEqual(failPointExpected, failPoint);
			Assert.AreEqual(failExpected, failActual);
		}
	}
}
