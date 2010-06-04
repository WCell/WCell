/*************************************************************************
 *
 *   file		: PacketOut.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-02 17:46:41 +0100 (lø, 02 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1166 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.IO;
using Cell.Core;
using NLog;

namespace WCell.Core.Network
{
    /// <summary>
    /// Writes data to an outgoing packet stream
    /// </summary>
    public abstract class PacketOut : PrimitiveWriter
    {
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected PacketId m_id;

        #region Constructors

        /// <summary>
        /// Constructs an empty packet with the given packet ID.
        /// </summary>
        /// <param name="id">the ID of the packet</param>
        protected PacketOut(PacketId id)
            : base(BufferManager.Default.CheckOutStream())
			//: base(new MemoryStream())
        {
            m_id = id;
		}

		/// <summary>
		/// Constructs an empty packet with an initial capacity of exactly or greater than the specified amount.
		/// </summary>
		/// <param name="id">the ID of the packet</param>
		/// <param name="maxCapacity">the minimum space required for the packet</param>
		protected PacketOut(PacketId id, int maxCapacity)
			: base(BufferManager.GetSegmentStream(maxCapacity))
			//: base(new MemoryStream(maxCapacity))
        {
            m_id = id;
        }

        #endregion

        /// <summary>
        /// The packet header size.
        /// </summary>
        /// <returns>The header size in bytes.</returns>
        public abstract int HeaderSize { get; }

        /// <summary>
        /// The ID of this packet.
        /// </summary>
        /// <example>RealmServerOpCode.SMSG_QUESTGIVER_REQUEST_ITEMS</example>
        public PacketId PacketId
        {
            get { return m_id; }
        }

        /// <summary>
        /// The position within the current packet.
        /// </summary>
        public long Position
        {
            get { return BaseStream.Position; }
            set
            {
            	BaseStream.Position = value;
            }
        }

        /// <summary>
		/// The length of this packet in bytes
        /// </summary>
        public int TotalLength
        {
            get { return (int)BaseStream.Length; }
			set
			{
				BaseStream.SetLength(value);
			}
		}

    	public int ContentLength
    	{
    		get
    		{
    			return (int) BaseStream.Length - HeaderSize;
    		}
			set
			{
				BaseStream.SetLength(value + HeaderSize);
			}
    	}

		public void Fill(byte val, int num)
		{
			for (int i = 0; i < num; ++i)
			{
				Write(val);
			}
		}

		public void Zero(int len)
		{
			for (var i = 0; i < len; ++i)
			{
				Write((byte)0);
			}
		}

        /// <summary>
        /// Finalize packet data
        /// </summary>
        protected virtual void FinalizeWrite()
        {
        }

        /// <summary>
        /// Finalizes and copies the content of the packet
        /// </summary>
        /// <returns>Packet data</returns>
        public byte[] GetFinalizedPacket()
        {
            FinalizeWrite();
        	var bytes = new byte[TotalLength];
        	BaseStream.Position = 0;
        	BaseStream.Read(bytes, 0, TotalLength);
        	return bytes;
        }

        /// <summary>
        /// Reverses the contents of an array
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="buffer">array of objects to reverse</param>
        protected static void Reverse<T>(T[] buffer)
        {
            Reverse(buffer, buffer.Length);
        }

        /// <summary>
        /// Reverses the contents of an array
        /// </summary>
        /// <typeparam name="T">type of the array</typeparam>
        /// <param name="buffer">array of objects to reverse</param>
        /// <param name="length">number of objects in the array</param>
        protected static void Reverse<T>(T[] buffer, int length)
        {
            for (int i = 0; i < length/2; i++)
            {
                T temp = buffer[i];
                buffer[i] = buffer[length - i - 1];
                buffer[length - i - 1] = temp;
            }
        }

        /// <summary>
        /// Dumps the packet to string form, using hexadecimal as the formatter
        /// </summary>
        /// <returns>hexadecimal representation of the data parsed</returns>
        public string ToHexDump()
        {
            FinalizeWrite();
        	//var segment = Segment;
			//return Utility.ToHex(PacketID, segment.Buffer.Array, segment.Offset + HeaderSize, ContentLength);
        	var content = ((MemoryStream) BaseStream).ToArray();
			return WCellUtil.ToHex(PacketId, content, HeaderSize, ContentLength);
        }

        /// <summary>
        /// Gets the name of the packet ID. (ie. CMSG_PING)
        /// </summary>
        /// <returns>a string containing the packet's canonical name</returns>
        public override string ToString()
        {
            return PacketId.ToString();
        }

		/// <summary>
		/// String preceeded by uint length
		/// </summary>
		/// <param name="message"></param>
		public void WriteUIntPascalString(string message)
		{
			if (message.Length > 0)
			{
				var bytes = DefaultEncoding.GetBytes(message);
				base.WriteUInt(bytes.Length + 1);
				Write(bytes);
				Write((byte) 0);
			}
			else
			{
				base.WriteUInt(0);
			}
		}
    }
}