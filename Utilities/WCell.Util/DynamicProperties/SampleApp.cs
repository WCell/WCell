//
// Author: James Nies
// Date: 3/22/2005
// Description: Sample application that tests the PropertyAccessor class.
//
// *** This code was written by James Nies and has been provided to you, ***
// *** free of charge, for your use.  I assume no responsibility for any ***
// *** undesired events resulting from the use of this code or the		 ***
// *** information that has been provided with it .						 ***
//

using System;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using WCell.Util.DynamicProperties;

namespace WCell.Util.DynamicProperties
{
    /// <summary>
    /// Sample application for the fast dynamic property
    /// accesssor.
    /// </summary>
    class SampleApp
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
        private static readonly int[] TEST_ARRAY = new int[] { 1, 2, 3 };

        const int TEST_ITERATIONS = 100000000;


        #endregion Test Constants

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //
            // Test getting an integer property.
            //
            Thread.Sleep(500);
            TestGetIntegerPerformance();
            Thread.Sleep(500);
            //
            // Test setting an integer property.
            //
            TestSetIntegerPerformance();
            Thread.Sleep(500);

            //
            // Test getting a string property.
            //
            TestGetStringPerformance();
            Thread.Sleep(500);

            //
            // Test setting an integer property.
            //
            TestSetStringPerformance();

            Console.Read();
        }

        /// <summary>
        /// Test the performance of getting an integer property.
        /// </summary>
        public static void TestGetIntegerPerformance()
        {
            PropertyAccessorTestObject testObject
                = CreateTestObject();

            int test = 0;

            //
            // Generic Property accessor
            //
            Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Reset(); watch.Start();

            GenericPropertyAccessor<PropertyAccessorTestObject, int> propertyAccessor =
                new GenericPropertyAccessor<PropertyAccessorTestObject, int>("Int");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                test = propertyAccessor.Get(testObject);
            }

            watch.Stop();
            double genericPropertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Direct access
            //
            watch.Reset(); watch.Start();

            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                test = testObject.Int;
            }

            watch.Stop();
            double directAccessMs = watch.ElapsedMilliseconds;

            //
            // Property accessor
            //
            Type type = testObject.GetType();
            watch.Reset(); watch.Start();

            PropertyAccessor propertyAccessor2 = new PropertyAccessor(typeof(PropertyAccessorTestObject), "Int");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                test = (int)propertyAccessor2.Get(testObject);
            }

            watch.Stop();
            double propertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Print results
            //
            Console.WriteLine(
                TEST_ITERATIONS.ToString() + " property gets on integer..."
                + "\nDirect access ms: \t\t\t\t" + directAccessMs.ToString()
                + "\nGenericPropertyAccessor (Reflection.Emit) ms: \t" + genericPropertyAccessorMs.ToString()
                + "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() + "\n\n");
        }

        /// <summary>
        /// Test the performance of setting an integer property.
        /// </summary>
        public static void TestSetIntegerPerformance()
        {
            const int TEST_VALUE = 123;

            PropertyAccessorTestObject testObject
                = CreateTestObject();

            //
            // Generic Property accessor
            //
            Stopwatch watch = new System.Diagnostics.Stopwatch();

            watch.Reset(); watch.Start();

            GenericPropertyAccessor<PropertyAccessorTestObject, int> propertyAccessor =
                new GenericPropertyAccessor<PropertyAccessorTestObject, int>("Int");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                propertyAccessor.Set(testObject, TEST_VALUE);
            }

            watch.Stop();
            double genericPropertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Direct access
            //
            watch.Reset(); watch.Start();

            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                testObject.Int = TEST_VALUE;
            }

            watch.Stop();
            double directAccessMs = watch.ElapsedMilliseconds;

            //
            // Property accessor
            //
            Type type = testObject.GetType();
            watch.Reset(); watch.Start();

            PropertyAccessor propertyAccessor2 = new PropertyAccessor(typeof(PropertyAccessorTestObject), "Int");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                propertyAccessor2.Set(testObject, TEST_VALUE);
            }

            watch.Stop();
            double propertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Print results
            //
            Console.WriteLine(
                TEST_ITERATIONS.ToString() + " property sets on integer..."
                + "\nDirect access ms: \t\t\t\t" + directAccessMs.ToString()
                + "\nGenericPropertyAccessor (Reflection.Emit) ms: \t" + genericPropertyAccessorMs.ToString()
                + "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() + "\n\n");
        }

        /// <summary>
        /// Test the performance of getting a string property.
        /// </summary>
        public static void TestGetStringPerformance()
        {

            PropertyAccessorTestObject testObject
                = CreateTestObject();

            string test = "Unset";

            //
            // Generic Property accessor
            //
            Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Reset(); watch.Start();

            GenericPropertyAccessor<PropertyAccessorTestObject, string> propertyAccessor =
                new GenericPropertyAccessor<PropertyAccessorTestObject, string>("String");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                test = propertyAccessor.Get(testObject);
            }

            watch.Stop();
            double genericPropertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Direct access
            //
            watch.Reset(); watch.Start();

            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                test = testObject.String;
            }

            watch.Stop();
            double directAccessMs = watch.ElapsedMilliseconds;

            //
            // Property accessor
            //
            Type type = testObject.GetType();
            watch.Reset(); watch.Start();

            PropertyAccessor propertyAccessor2 = new PropertyAccessor(typeof(PropertyAccessorTestObject), "String");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                test = (string)propertyAccessor2.Get(testObject);
            }

            watch.Stop();
            double propertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Print results
            //
            Console.WriteLine(
                TEST_ITERATIONS.ToString() + " property gets on string..."
                + "\nDirect access ms: \t\t\t\t" + directAccessMs.ToString()
                + "\nGenericPropertyAccessor (Reflection.Emit) ms: \t" + genericPropertyAccessorMs.ToString()
                + "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() + "\n\n");
        }

        /// <summary>
        /// Test the performance of setting a string property.
        /// </summary>
        public static void TestSetStringPerformance()
        {
            const string TEST_VALUE = "Test";

            PropertyAccessorTestObject testObject
                = CreateTestObject();

            //
            // Generic Property accessor
            //
            Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Reset(); watch.Start();

            GenericPropertyAccessor<PropertyAccessorTestObject, string> propertyAccessor =
                new GenericPropertyAccessor<PropertyAccessorTestObject, string>("String");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                propertyAccessor.Set(testObject, TEST_VALUE);
            }

            watch.Stop();
            double genericPropertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Direct access
            //
            watch.Reset(); watch.Start();

            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                testObject.String = TEST_VALUE;
            }

            watch.Stop();
            double directAccessMs = watch.ElapsedMilliseconds;

            //
            // Property accessor
            //
            Type type = testObject.GetType();
            watch.Reset(); watch.Start();

            PropertyAccessor propertyAccessor2 = new PropertyAccessor(typeof(PropertyAccessorTestObject), "String");
            for (int i = 0; i < TEST_ITERATIONS; i++)
            {
                propertyAccessor2.Set(testObject, TEST_VALUE);
            }

            watch.Stop();
            double propertyAccessorMs = watch.ElapsedMilliseconds;

            //
            // Print results
            //
            Console.WriteLine(
                TEST_ITERATIONS.ToString() + " property sets on string..."
                + "\nDirect access ms: \t\t\t\t" + directAccessMs.ToString()
                + "\nGenericPropertyAccessor (Reflection.Emit) ms: \t" + genericPropertyAccessorMs.ToString()
                + "\nPropertyAccessor (Reflection.Emit) ms: \t\t" + propertyAccessorMs.ToString() + "\n\n");
        }

        /// <summary>
        /// Creates an object for testing the PropertyAccessor class.
        /// </summary>
        /// <returns></returns>
        public static PropertyAccessorTestObject CreateTestObject()
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

    }
}
