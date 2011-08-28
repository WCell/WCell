/*************************************************************************
 *
 *   file		: PrimitiveWriter.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-21 10:17:04 +0100 (lø, 21 mar 2009) $
 
 *   revision		: $Rev: 818 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;
using System.Net;
using System.Text;
using WCell.Core.Cryptography;
using WCell.Util;

namespace WCell.Core.Network
{
    /// <summary>
    /// An extension of <seealso cref="BinaryWriter"/>, which provides overloads
    /// for writing primitives to a stream, including special WoW structures
    /// </summary>
    public class PrimitiveWriter : BinaryWriter
    {
    	public static Encoding DefaultEncoding = Encoding.UTF8;

        public PrimitiveWriter(Stream stream) : base(stream)
        {
        }

        #region WriteByte

        /// <summary>
        /// Writes a byte to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(byte val)
        {
            Write(val);
        }

        /// <summary>
        /// Writes a byte to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(ushort val)
        {
            Write((byte) val);
        }

        /// <summary>
        /// Writes a byte to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(short val)
        {
            Write((byte) val);
        }

        /// <summary>
        /// Writes a byte to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteByte(uint val)
        {
            Write((byte) val);
		}

		/// <summary>
		/// Writes a byte to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteByte(int val)
		{
			Write((byte)val);
		}

		/// <summary>
		/// Writes a byte to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteByte(bool val)
		{
			Write(val);
		}

        #endregion

        #region WriteShort

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(byte val)
        {
            Write((short) val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(ushort val)
        {
            Write(val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(short val)
        {
            Write(val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(uint val)
        {
            Write((short) val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShort(int val)
        {
            Write((short) val);
        }

        #endregion

        #region WriteInt

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(byte val)
        {
            Write((int) val);
        }

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(ushort val)
        {
            Write((int) val);
        }

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(short val)
        {
            Write((int) val);
        }

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(uint val)
        {
            Write(val);
        }

        /// <summary>
        /// Writes an int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteInt(int val)
        {
            Write(val);
        }

        #endregion

        #region WriteFloat

        /// <summary>
        /// Writes a float to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(byte val)
        {
            Write((float) val);
        }

        /// <summary>
        /// Writes a float to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(ushort val)
        {
            Write((float) val);
        }

        /// <summary>
        /// Writes a float to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(short val)
        {
            Write((int) val);
        }

        /// <summary>
        /// Writes a float to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(uint val)
        {
            Write((float) val);
        }

        /// <summary>
        /// Writes a float to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(int val)
        {
            Write((float) val);
        }

		/// <summary>
		/// Writes a float to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteFloat(double val)
		{
			Write((float)val);
		}

        /// <summary>
        /// Writes a float to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteFloat(float val)
        {
            Write(val);
        }

        #endregion

        #region WriteUShort

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(byte val)
        {
            Write((ushort) val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(ushort val)
        {
            Write(val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(short val)
        {
            Write((ushort) val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(uint val)
        {
            Write((ushort) val);
        }

        /// <summary>
        /// Writes a short to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUShort(int val)
        {
            Write((ushort) val);
        }

        #endregion

        #region WriteUInt

        /// <summary>
        /// Writes an unsigned int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(byte val)
        {
            Write((uint) val);
        }

        /// <summary>
        /// Writes an unsigned int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(ushort val)
        {
            Write((uint) val);
        }

        /// <summary>
        /// Writes an unsigned int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(short val)
        {
            Write((uint) val);
        }

        /// <summary>
        /// Writes an unsigned int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(uint val)
        {
            Write(val);
        }

        /// <summary>
        /// Writes an unsigned int to the stream
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteUInt(int val)
        {
            Write((uint) val);
        }

		/// <summary>
		/// Writes an int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteUInt(long val)
		{
			Write((uint)val);
		}

        #endregion

		#region WriteULong

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(byte val)
		{
			Write((ulong)val);
		}

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(ushort val)
		{
			Write((ulong)val);
		}

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(short val)
		{
			Write((ulong)val);
		}

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(uint val)
		{
			Write((ulong)val);
		}

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(int val)
		{
			Write((ulong)val);
		}

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(ulong val)
		{
			Write(val);
		}

		/// <summary>
        /// Writes an eight byte unsigned int to the stream
		/// </summary>
		/// <param name="val">the value to write</param>
		public virtual void WriteULong(long val)
		{
			Write((ulong)val);
		}

		#endregion

		public override void Write(string str)
		{
			Write(DefaultEncoding.GetBytes(str));
			Write((byte)0);
		}

        /// <summary>
        /// Writes a C-style string to the stream (actual string is null-terminated)
        /// </summary>
        /// <param name="str">String to write</param>
        public virtual void WriteCString(string str)
        {
			Write(DefaultEncoding.GetBytes(str));
			Write((byte)0);
        }

        /// <summary>
        /// Writes a C-style UTF8 string to the stream (actual string is null-terminated)
        /// </summary>
        /// <param name="str">String to write</param>
        public virtual void WriteUTF8CString(string str)
        {
            Write(Encoding.UTF8.GetBytes(str));
			Write((byte)0);
        }

        /// <summary>
        /// Writes a BigInteger to the stream
        /// </summary>
        /// <param name="bigInt">BigInteger to write</param>
        public virtual void WriteBigInt(BigInteger bigInt)
        {
            byte[] data = bigInt.GetBytes();

            base.Write(data);
        }

        /// <summary>
        /// Writes a BigInteger to the stream
        /// </summary>
        /// <param name="bigInt">BigInteger to write</param>
        /// <param name="length">maximum numbers of bytes to write for the BigInteger</param>
        public virtual void WriteBigInt(BigInteger bigInt, int length)
        {
            byte[] data = bigInt.GetBytes(length);

            base.Write(data);
        }

        /// <summary>
        /// Writes a BigInteger to the stream, while writing the length before it
        /// </summary>
        /// <param name="bigInt">BigInteger to write</param>
        public virtual void WriteBigIntLength(BigInteger bigInt)
        {
            byte[] data = bigInt.GetBytes();

            base.Write((byte) data.Length);
            base.Write(data);
        }

        /// <summary>
        /// Writes a BigInteger to the stream, while writing the length before it
        /// </summary>
        /// <param name="bigInt">BigInteger to write</param>
        /// <param name="length">maximum numbers of bytes to write for th BigInteger</param>
        public virtual void WriteBigIntLength(BigInteger bigInt, int length)
        {
            byte[] data = bigInt.GetBytes(length);

            base.Write((byte) length);
            base.Write(data);
        }

        #region WriteShortBE

        /// <summary>
        /// Writes a short to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShortBE(byte val)
        {
            Write(IPAddress.HostToNetworkOrder(val));
        }

        /// <summary>
        /// Writes a short to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShortBE(short val)
        {
            Write(IPAddress.HostToNetworkOrder(val));
        }

        /// <summary>
        /// Writes a short to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteShortBE(int val)
        {
            Write(IPAddress.HostToNetworkOrder((byte) val));
        }

		#endregion

		#region WriteUShortBE
		public void WriteUShortBE(ushort val)
		{
			Write((byte)((val & 0xFF00) >> 8));
			Write((byte)(val & 0x00FF));
		}
		#endregion

		#region WriteIntBE

		/// <summary>
        /// Writes an int to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteIntBE(byte val)
        {
            Write(IPAddress.HostToNetworkOrder((int) val));
        }

        /// <summary>
        /// Writes an int to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteIntBE(short val)
        {
            Write(IPAddress.HostToNetworkOrder((int) val));
        }

        /// <summary>
        /// Writes an int to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public virtual void WriteIntBE(int val)
        {
            Write(IPAddress.HostToNetworkOrder(val));
        }

        #endregion

        /// <summary>
        /// Writes a long to the stream, in network order
        /// </summary>
        /// <param name="val">the value to write</param>
        public void WriteLongBE(long val)
        {
            Write(IPAddress.HostToNetworkOrder(val));
        }

        /// <summary>
        /// Writes a date time to the stream, in WoW format
        /// </summary>
        /// <param name="dateTime">the time to write</param>
        public void WriteDateTime(DateTime dateTime)
        {
            Write(Utility.GetDateTimeToGameTime(dateTime));
        }

        public void InsertByteAt(byte value, long pos, bool returnOrigPos)
        {
            if (returnOrigPos)
            {
                long orig = BaseStream.Position;

                BaseStream.Position = pos;

                Write(value);

                BaseStream.Position = orig;
            }
            else
            {
                BaseStream.Position = pos;

                Write(value);
            }
        }

        public void InsertShortAt(short value, long pos, bool returnOrigPos)
        {
            if (returnOrigPos)
            {
                long orig = BaseStream.Position;

                BaseStream.Position = pos;

                Write(value);

                BaseStream.Position = orig;
            }
            else
            {
                BaseStream.Position = pos;

                Write(value);
            }
        }

        public void InsertIntAt(int value, long pos, bool returnOrigPos)
        {
            if (returnOrigPos)
            {
                long orig = BaseStream.Position;

                BaseStream.Position = pos;

                Write(value);

                BaseStream.Position = orig;
            }
            else
            {
                BaseStream.Position = pos;

                Write(value);
            }
        }
    }
}