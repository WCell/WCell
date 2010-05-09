using System;
using System.IO;
using System.Net;
using System.Text;
using Cell.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WCell.Core.Cryptography;
using WCell.Core.Network;

namespace WCell.Core.Tests
{
    public class TestPacketIn : PacketIn
	{

        private const int HEADER_SIZE = 2;

        public override int HeaderSize
        {
            get { return HEADER_SIZE; }
		}

		public TestPacketIn(byte[] buffer)
			: this(buffer, buffer.Length)
		{
		}

		public TestPacketIn(byte[] buffer, int bufLength)
			: base(BufferSegment.CreateSegment(buffer, 0, bufLength), 0, bufLength)
		{
		}

		public TestPacketIn(BufferSegment buffer, int bufLength)
			: base(buffer, 0, bufLength)
		{
		}

		public TestPacketIn(BufferSegment buffer, int offset, int bufLength)
			: base(buffer, offset, bufLength)
		{
		}

        public void SetPacketID(PacketId id)
        {
            _packetID = id;
        }
    }

    /// <summary>
    /// Test for PacketIn.
    /// </summary>
    [TestClass]
    public class PacketInTest
	{
		public static readonly BufferManager SmallBuffers = new BufferManager(1, 128);

        public PacketInTest()
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
            get { return testContextInstance; }
            set { testContextInstance = value; }
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
        public void TestPacketReading()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter bWriter = new BinaryWriter(memStream);

            bWriter.Write((byte)122);
            bWriter.Write((sbyte)-122);
            bWriter.Write((short)-14323);
            bWriter.Write((ushort)14323);
            bWriter.Write((int)-4124444);
            bWriter.Write((uint)4124444);
            bWriter.Write((ulong)412124553);
            bWriter.Write((long)-412124553);
            bWriter.Write((float)450.2500f);
            bWriter.Write((double)90350.45350d);
            bWriter.Write("test");

            bWriter.Write(UTF8Encoding.UTF8.GetBytes(new char[] { 't', 'e', 's', 't' }));
            bWriter.Write((byte)0);

            byte[] testBytes = new byte[] { 0x01, 0x02, 0x03, 0x02, 0x01 };
            bWriter.Write(testBytes);

            BigInteger bi = new BigInteger(1000000);
            byte[] biBytes = bi.GetBytes();
            bWriter.Write(biBytes);

            bWriter.Write((byte)biBytes.Length);
            bWriter.Write(biBytes);

            bWriter.Write(false);

            byte[] addrBytes = IPAddress.Parse("192.168.1.100").GetAddressBytes();
            bWriter.Write(addrBytes);

            bWriter.Write(UTF8Encoding.UTF8.GetBytes("tset"));
            bWriter.Write((byte)0);

            bWriter.Write(UTF8Encoding.UTF8.GetBytes(new char[] { 'z' }));

            bWriter.Write(UTF8Encoding.UTF8.GetBytes("rabooftset"));

			byte[] bytes = memStream.ToArray();

			TestPacketIn packet = new TestPacketIn(bytes);

            Assert.AreEqual((byte)122, packet.ReadByte());
            Assert.AreEqual((sbyte)-122, packet.ReadSByte());
            Assert.AreEqual((short)-14323, packet.ReadInt16());
            Assert.AreEqual((ushort)14323, packet.ReadUInt16());
            Assert.AreEqual((int)-4124444, packet.ReadInt32());
            Assert.AreEqual((uint)4124444, packet.ReadUInt32());
            Assert.AreEqual((ulong)412124553, packet.ReadUInt64());
            Assert.AreEqual((long)-412124553, packet.ReadInt64());
            Assert.AreEqual((float)450.2500f, packet.ReadFloat());
            Assert.AreEqual((double)90350.45350d, packet.ReadDouble());
            Assert.AreEqual("test", packet.ReadPascalString());
            Assert.AreEqual("test", packet.ReadCString());
            Assert.IsTrue(ByteArrayEquals(testBytes, packet.ReadBytes(5)));
            Assert.AreEqual(bi, packet.ReadBigInteger(biBytes.Length));
            Assert.AreEqual(bi, packet.ReadBigIntegerLengthValue());
            Assert.AreEqual(false, packet.ReadBoolean());
            Assert.AreEqual("192.168.1.100", packet.ReadIPAddress().Address);
            Assert.AreEqual("test", packet.ReadReversedString());
            Assert.AreEqual('z', packet.ReadChar());
            Assert.AreEqual("testfoobar", packet.ReadReversedPascalString(10));

            Assert.AreEqual(memStream.Length, packet.Length);
            Assert.AreEqual(memStream.Position, packet.Position);
        }

        [TestMethod]
        public void TestPacketReadingWithTooLittleData()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter bWriter = new BinaryWriter(memStream);

            byte[] firstTwentyBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 
                                                   0x01, 0x02, 0x03, 0x04, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05 };
            byte[] lastThreeBytes = new byte[] { 0x09, 0x13, 0x41 };

            // we'll write 23 random bytes.
            bWriter.Write(firstTwentyBytes);
            bWriter.Write(lastThreeBytes);

			byte[] memStreamArr = memStream.ToArray();

			ArraySegment<byte> buf = new ArraySegment<byte>(memStreamArr, 0, memStreamArr.Length);

			TestPacketIn packet = new TestPacketIn(memStreamArr);

            Assert.IsTrue(ByteArrayEquals(firstTwentyBytes, packet.ReadBytes(20)));
            Assert.AreEqual(float.NaN, packet.ReadFloat());
            Assert.AreEqual(0, packet.ReadInt32());
            Assert.AreEqual<uint>(0, packet.ReadUInt32());
            Assert.AreEqual(double.NaN, packet.ReadDouble());
            Assert.AreEqual(0, packet.ReadInt64());
            Assert.AreEqual<ulong>(0, packet.ReadUInt64());

            Assert.AreEqual(22, packet.Seek(2, SeekOrigin.Current));
            Assert.AreEqual(0, packet.ReadInt16());
            Assert.AreEqual(0, packet.ReadUInt16());

            Assert.AreEqual(23, packet.Seek(0, SeekOrigin.End));
            Assert.AreEqual(0, packet.ReadByte());
            Assert.AreEqual(0, packet.ReadSByte());
            Assert.IsFalse(packet.ReadBoolean());
            Assert.AreEqual('\0', packet.ReadChar());
            Assert.AreEqual(0, packet.ReadBigInteger(1).LongValue());
            Assert.AreEqual(0, packet.ReadChars(1).Length);

            MemoryStream memStreamTwo = new MemoryStream();
            BinaryWriter bWriterTwo = new BinaryWriter(memStreamTwo);

            byte[] truncatedStrBytes = UTF8Encoding.UTF8.GetBytes("thisisatruncatedstr");

            // we'll write 23 random bytes.
            bWriterTwo.Write((byte)truncatedStrBytes.Length);
            bWriterTwo.Write(truncatedStrBytes);

            byte[] truncatedBytes = new byte[] { 0x03, 0x04, 0x05, 0x09 };
            bWriterTwo.Write(truncatedBytes);

			byte[] arr2 = memStreamTwo.ToArray();

			TestPacketIn packetTwo = new TestPacketIn(arr2);

            Assert.AreEqual(packetTwo.ReadPascalString(), "thisisatruncatedstr");
            Assert.IsTrue(ByteArrayEquals(packetTwo.ReadBytes(10), truncatedBytes));
        }

        [TestMethod]
        public void TestPacketManagementFunctions()
        {
            MemoryStream memStream = new MemoryStream();
            BinaryWriter bWriter = new BinaryWriter(memStream);

            byte[] firstTwentyBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05, 
                                                   0x01, 0x02, 0x03, 0x04, 0x05, 0x01, 0x02, 0x03, 0x04, 0x05 };
            byte[] lastThreeBytes = new byte[] { 0x09, 0x13, 0x41 };

            // we'll write 23 random bytes.
            bWriter.Write(firstTwentyBytes);
            bWriter.Write(lastThreeBytes);

			byte[] memStreamArr = memStream.ToArray();

			TestPacketIn packet = new TestPacketIn(memStreamArr);

            Assert.AreEqual((long)10, packet.Seek(10, SeekOrigin.Begin));

            Assert.AreEqual((long)20, packet.Seek(10, SeekOrigin.Current));

            Assert.AreEqual((long)23, packet.Seek(0, SeekOrigin.End));

            packet.Position = 0;

            Assert.AreEqual(0, packet.Position);

            packet.SkipBytes(9);

            Assert.AreEqual(9, packet.Position);

            PacketId serviceId = new PacketId(ServiceType.Authentication, 1);

            packet.SetPacketID(serviceId);

            Assert.AreEqual(serviceId, packet.PacketId);
        }

        public bool ByteArrayEquals(byte[] arrayOne, byte[] arrayTwo)
        {
            if (arrayOne.LongLength != arrayTwo.LongLength)
            {
                return false;
            }

            for (int i = 0; i < arrayOne.Length; i++)
            {
                if (arrayOne[i] != arrayTwo[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}