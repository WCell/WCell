//
// Author: James Nies
// Date: 3/22/2005
// Description: This class is a NUnit test for the PropertyAccessor class and
//		requires the NUnit framework.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

/*
using System;
using System.Collections;
using System.Reflection;


using NUnit.Framework;

using FastDynamicPropertyAccessor;

namespace WCell.Util.DynamicProperties
{
	/// <summary>
	/// Test fixture for the PropertyAccessor class.
	/// </summary>
	[TestFixture]
	public class PropertyAccessorTest
	{
		#region Test Constants

		private static readonly int TEST_INTEGER = 319;
		private static readonly string TEST_STRING = "Test string.";
		private static readonly sbyte TEST_SBYTE = 12;
		private static readonly byte TEST_BYTE = 234;
		private static readonly char TEST_CHAR = 'a';
		private static readonly short TEST_SHORT = -673;
		private static readonly ushort TEST_USHORT = 511;
		private static readonly long TEST_LONG = 8798798798;
		private static readonly ulong TEST_ULONG = 918297981798;
		private static readonly bool TEST_BOOL = false;
		private static readonly double TEST_DOUBLE = 789.12;
		private static readonly float TEST_FLOAT = 123.12F;
		private static readonly DateTime TEST_DATETIME = new DateTime(2005, 3, 6);
		private static readonly decimal TEST_DECIMAL = new decimal(98798798.1221);
		private static readonly int[] TEST_ARRAY = new int[]{1,2,3};

		#endregion Test Constants
		
		#region Test Get And Set

		[Test]
		public void TestGetInteger()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Int");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Int, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetInteger()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Int");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			int testInt = 2345;
			propertyAccessor.Set(testObject, testInt);
			Assert.AreEqual(testInt, testObject.Int);			
		}

		[Test]
		public void TestGetString()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"String");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.String, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetString()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"String");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			string testString = "New string";
			propertyAccessor.Set(testObject, testString);
			Assert.AreEqual(testString, testObject.String);			
		}

		[Test]
		public void TestGetBool()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Bool");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Bool, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetBool()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Bool");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			bool testBool = true;
			propertyAccessor.Set(testObject, testBool);
			Assert.AreEqual(testBool, testObject.Bool);			
		}

		[Test]
		public void TestGetByte()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Byte");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Byte, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetByte()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Byte");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			byte testByte = 16;
			propertyAccessor.Set(testObject, testByte);
			Assert.AreEqual(testByte, testObject.Byte);			
		}

		[Test]
		public void TestGetChar()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Char");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Char, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetChar()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Char");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			char testChar = 'Z';
			propertyAccessor.Set(testObject, testChar);
			Assert.AreEqual(testChar, testObject.Char);			
		}

		[Test]
		public void TestGetDateTime()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"DateTime");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.DateTime, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetDateTime()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"DateTime");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			DateTime testDateTime = DateTime.Now;
			propertyAccessor.Set(testObject, testDateTime);
			Assert.AreEqual(testDateTime, testObject.DateTime);			
		}

		[Test]
		public void TestGetDecimal()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Decimal");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Decimal, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetDecimal()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Decimal");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			decimal testDecimal = new decimal(123123.12);
			propertyAccessor.Set(testObject, testDecimal);
			Assert.AreEqual(testDecimal, testObject.Decimal);			
		}

		[Test]
		public void TestGetDouble()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Double");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Double, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetDouble()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Double");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			double testDouble = 9999.99;
			propertyAccessor.Set(testObject, testDouble);
			Assert.AreEqual(testDouble, testObject.Double);			
		}

		[Test]
		public void TestGetFloat()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Float");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Float, propertyAccessor.Get(testObject));			
		}
		
		[Test]
		public void TestSetFloat()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Float");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			float testFloat = 1982.12F;
			propertyAccessor.Set(testObject, testFloat);
			Assert.AreEqual(testFloat, testObject.Float);			
		}
		
		[Test]
		public void TestGetLong()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Long");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Long, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetLong()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Long");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			long testLong = 92873987232;
			propertyAccessor.Set(testObject, testLong);
			Assert.AreEqual(testLong, testObject.Long);			
		}

		[Test]
		public void TestGetSbyte()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Sbyte");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Sbyte, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetSbyte()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Sbyte");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			sbyte testSbyte = 19;
			propertyAccessor.Set(testObject, testSbyte);
			Assert.AreEqual(testSbyte, testObject.Sbyte);			
		}

		[Test]
		public void TestGetShort()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Short");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.Short, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetShort()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Short");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			short testShort = 2378;
			propertyAccessor.Set(testObject, testShort);
			Assert.AreEqual(testShort, testObject.Short);			
		}
		
		[Test]
		public void TestGetULong()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"ULong");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.ULong, propertyAccessor.Get(testObject));			
		}

		[Test]
		public void TestSetULong()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"ULong");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			ulong testULong = 29837987298;
			propertyAccessor.Set(testObject, testULong);
			Assert.AreEqual(testULong, testObject.ULong);			
		}
		
		[Test]
		public void TestGetUShort()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"UShort");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.UShort, propertyAccessor.Get(testObject));			
		}
		
		[Test]
		public void TestSetUShort()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"UShort");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			ushort testUShort = 789;
			propertyAccessor.Set(testObject, testUShort);
			Assert.AreEqual(testUShort, testObject.UShort);			
		}

		[Test]
		public void TestGetList()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"List");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			Assert.AreEqual(testObject.List, propertyAccessor.Get(testObject));			
		}
		
		[Test]
		public void TestSetList()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"List");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			ArrayList list = new ArrayList(new int[]{4,5,6});
			propertyAccessor.Set(testObject, list);
			Assert.AreEqual(list, testObject.List);			
		}


		#endregion Test Get And Set

		[Test]
		public void TestTargetType()
		{
			PropertyAccessor propertyAccessor
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Int");

			Assert.AreEqual(typeof(PropertyAccessorTestObject),
				propertyAccessor.TargetType);
		}

		[Test]
		public void TestPropertyType()
		{
			PropertyAccessor propertyAccessor
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Int");

			Assert.AreEqual(typeof(int),
				propertyAccessor.PropertyType);
		}

		[Test]
		public void TestCanRead()
		{
			PropertyAccessor propertyAccessor;

			//
			// Can read
			//
			propertyAccessor = new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Int");

			Assert.IsTrue(propertyAccessor.CanRead);

			//
			// Cannot read
			//
			propertyAccessor = new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"WriteOnlyInt");

			Assert.IsFalse(propertyAccessor.CanRead);
		}

		[Test]
		public void TestCanWrite()
		{
			PropertyAccessor propertyAccessor;

			//
			// Can read
			//
			propertyAccessor = new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"Int");

			Assert.IsTrue(propertyAccessor.CanWrite);

			//
			// Cannot write
			//
			propertyAccessor = new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"ReadOnlyInt");

			Assert.IsFalse(propertyAccessor.CanWrite);
		}

		[Test]
		[ExpectedException(typeof(PropertyAccessorException),
			 "Property \"ReadOnlyInt\" does not have a set method.")]
		public void TestSetNotSupported()
		{
			PropertyAccessor propertyAccessor 
			= new PropertyAccessor(typeof(PropertyAccessorTestObject),
			"ReadOnlyInt");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			//
			// Attempt to set a propert that is read only
			//
			propertyAccessor.Set(testObject, 123);	
		}

		[Test]
		[ExpectedException(typeof(PropertyAccessorException),
			 "Property \"WriteOnlyInt\" does not have a get method.")]
		public void TestGetNotSupported()
		{
			PropertyAccessor propertyAccessor 
				= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"WriteOnlyInt");

			PropertyAccessorTestObject testObject =
				this.CreateTestObject();

			//
			// Attempt read a write-only property
			//
			int test = (int)propertyAccessor.Get(testObject);	
		}

		[Test]
		[ExpectedException(typeof(PropertyAccessorException), 
			"Property \"NonExistantProperty\" does not exist for "
			+ "type FastDynamicPropertyAccessor.PropertyAccessorTestObject.")]
		public void TestNonExistantProperty()
		{
			PropertyAccessor propertyAccessor 
			= new PropertyAccessor(typeof(PropertyAccessorTestObject),
				"NonExistantProperty");
		}

		[Test]
		public void TestGetIntegerPerformance()
		{
			const int TEST_ITERATIONS = 1000000;

			PropertyAccessorTestObject testObject 
				= this.CreateTestObject();

			int test;

			//
			// Property accessor
			//
			DateTime start = DateTime.Now;

			PropertyAccessor propertyAccessor = 
				new PropertyAccessor(typeof(PropertyAccessorTestObject), "Int");
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				test = (int)propertyAccessor.Get(testObject);
			}

			DateTime end = DateTime.Now;
			
			TimeSpan time = end - start;
			double propertyAccessorMs = time.TotalMilliseconds;

			//
			// Direct access
			//
			start = DateTime.Now;

			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				test = testObject.Int;
			}

			end = DateTime.Now;
			
			time = end - start;
			double directAccessMs = time.TotalMilliseconds;

			//
			// Reflection
			//
			start = DateTime.Now;
			Type type = testObject.GetType();
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				test = (int)type.InvokeMember("Int", 
					BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance, 
					null, testObject, null);
			}

			end = DateTime.Now;
			
			time = end - start;
			double reflectionMs = time.TotalMilliseconds;
			
			//
			// Print results
			//
			Console.WriteLine(
				TEST_ITERATIONS.ToString() + " property gets on integer..." 
				+ "\nDirect access ms: \t\t\t\t\t" + directAccessMs.ToString()
				+ "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() 
				+ "\nReflection ms: \t\t\t\t\t" + reflectionMs.ToString());
		}

		[Test]
		public void TestSetIntegerPerformance()
		{
			const int TEST_ITERATIONS = 1000000;
			const int TEST_VALUE = 123;

			PropertyAccessorTestObject testObject 
				= this.CreateTestObject();

			//
			// Property accessor
			//
			DateTime start = DateTime.Now;

			PropertyAccessor propertyAccessor = 
				new PropertyAccessor(typeof(PropertyAccessorTestObject), "Int");
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				propertyAccessor.Set(testObject, TEST_VALUE);
			}

			DateTime end = DateTime.Now;
			
			TimeSpan time = end - start;
			double propertyAccessorMs = time.TotalMilliseconds;

			//
			// Direct access
			//
			start = DateTime.Now;

			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				testObject.Int = TEST_VALUE;
			}

			end = DateTime.Now;
			
			time = end - start;
			double directAccessMs = time.TotalMilliseconds;

			//
			// Reflection
			//
			start = DateTime.Now;
			Type type = testObject.GetType();
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				type.InvokeMember("Int", 
					BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance, 
					null, testObject, new object[]{TEST_VALUE});
			}

			end = DateTime.Now;
			
			time = end - start;
			double reflectionMs = time.TotalMilliseconds;
			
			//
			// Print results
			//
			Console.WriteLine(
				TEST_ITERATIONS.ToString() + " property sets on integer..." 
				+ "\nDirect access ms: \t\t\t\t\t" + directAccessMs.ToString()
				+ "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() 
				+ "\nReflection ms: \t\t\t\t\t" + reflectionMs.ToString());
		}

		[Test]
		public void TestGetStringPerformance()
		{
			const int TEST_ITERATIONS = 1000000;

			PropertyAccessorTestObject testObject 
				= this.CreateTestObject();

			string test;

			//
			// Property accessor
			//
			DateTime start = DateTime.Now;

			PropertyAccessor propertyAccessor = 
				new PropertyAccessor(typeof(PropertyAccessorTestObject), "String");
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				test = (string)propertyAccessor.Get(testObject);
			}

			DateTime end = DateTime.Now;
			
			TimeSpan time = end - start;
			double propertyAccessorMs = time.TotalMilliseconds;

			//
			// Direct access
			//
			start = DateTime.Now;

			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				test = testObject.String;
			}

			end = DateTime.Now;
			
			time = end - start;
			double directAccessMs = time.TotalMilliseconds;

			//
			// Reflection
			//
			start = DateTime.Now;
			Type type = testObject.GetType();
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				test = (string)type.InvokeMember("String", 
					BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance, 
					null, testObject, null);
			}

			end = DateTime.Now;
			
			time = end - start;
			double reflectionMs = time.TotalMilliseconds;
			
			//
			// Print results
			//
			Console.WriteLine(
				TEST_ITERATIONS.ToString() + " property gets on string..." 
				+ "\nDirect access ms: \t\t\t\t\t" + directAccessMs.ToString()
				+ "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() 
				+ "\nReflection ms: \t\t\t\t\t" + reflectionMs.ToString());
		}

		[Test]
		public void TestSetStringPerformance()
		{
			const int TEST_ITERATIONS = 1000000;
			const string TEST_VALUE = "Test";

			PropertyAccessorTestObject testObject 
				= this.CreateTestObject();

			//
			// Property accessor
			//
			DateTime start = DateTime.Now;

			PropertyAccessor propertyAccessor = 
				new PropertyAccessor(typeof(PropertyAccessorTestObject), "String");
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				propertyAccessor.Set(testObject, TEST_VALUE);
			}

			DateTime end = DateTime.Now;
			
			TimeSpan time = end - start;
			double propertyAccessorMs = time.TotalMilliseconds;

			//
			// Direct access
			//
			start = DateTime.Now;

			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				testObject.String = TEST_VALUE;
			}

			end = DateTime.Now;
			
			time = end - start;
			double directAccessMs = time.TotalMilliseconds;

			//
			// Reflection
			//
			start = DateTime.Now;
			Type type = testObject.GetType();
			for(int i = 0; i < TEST_ITERATIONS; i++)
			{
				type.InvokeMember("String", 
					BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance, 
					null, testObject, new object[]{TEST_VALUE});
			}

			end = DateTime.Now;
			
			time = end - start;
			double reflectionMs = time.TotalMilliseconds;
			
			//
			// Print results
			//
			Console.WriteLine(
				TEST_ITERATIONS.ToString() + " property sets on string..." 
				+ "\nDirect access ms: \t\t\t\t\t" + directAccessMs.ToString()
				+ "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() 
				+ "\nReflection ms: \t\t\t\t\t" + reflectionMs.ToString());
		}

		#region Private Methods

		private PropertyAccessorTestObject CreateTestObject()
		{
			PropertyAccessorTestObject testObject = new PropertyAccessorTestObject();

			testObject.Int = TEST_INTEGER;
			testObject.String = TEST_STRING;
			testObject.Bool = TEST_BOOL;
			testObject.Byte = TEST_BYTE;
			testObject.Char = TEST_CHAR;
			testObject.DateTime = TEST_DATETIME;
			testObject.Decimal = TEST_DECIMAL;
			testObject.Double = TEST_DOUBLE;
			testObject.Float = TEST_FLOAT;
			testObject.Long = TEST_LONG;
			testObject.Sbyte = TEST_SBYTE;
			testObject.Short = TEST_SHORT;
			testObject.ULong = TEST_ULONG;
			testObject.UShort = TEST_USHORT;
			testObject.List = new ArrayList(TEST_ARRAY);

			return testObject;
		}

		#endregion Private Methods
	}
}
*/