using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using WCell.Core.Network;
using WCell.Core;
using WCell.Constants;
using WCell.PacketAnalysis.Logs;
using WCell.RealmServer;
using Cell.Core;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.PacketAnalysis
{
	/// <summary>
	/// Parses Packets into a human-readable format
	/// 
	/// TODO: Special Packets (special treatment, eg. UpdatePackets, MovementPackets etc)
	/// TODO: Get rid of segment-value array and change to proper parsing
	/// TODO: 100 character simultaneously login test
	/// </summary>
	public class PacketParser : IParsedPacket, IDisposable
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();

		public static readonly ArraySegment<byte> EmptyBytes = new ArraySegment<byte>();

		public static readonly Func<DateTime, string> DefaultTimestampCreator =
			timestamp => "[" + timestamp.FormatMillis() + "] ";

		public static Func<DateTime, string> TimeStampcreator = DefaultTimestampCreator;


		public readonly PacketDefinition Definition;

		/// <summary>
		/// The packet being parsed
		/// </summary>
		public readonly PacketIn Packet;

		public readonly PacketSender Sender;

		public readonly DateTime Timestamp;

		ParsedSegment m_parsedPacket;

		Dictionary<string, ParsedSegment> m_parsedSegmentsByName;

		public PacketParser(ParsablePacketInfo info)
			: this(info.Packet, info.Sender, PacketAnalyzer.GetDefinition(info.Packet.PacketId, info.Sender))
		{
			Timestamp = info.Timestamp;
		}

		public PacketParser(PacketIn packet, PacketSender sender)
			: this(packet, sender, PacketAnalyzer.GetDefinition(packet.PacketId, sender))
		{

		}

		public PacketParser(PacketIn packet, PacketSender sender, PacketDefinition def)
		{
			if (packet.PacketId == RealmServerOpCode.SMSG_COMPRESSED_MOVES)
			{
				var packet2 = ParseCompressedMove(packet);
				if (packet2 != null)
				{
					packet = packet2;
				}
			}
			Packet = packet;
			Sender = sender;
			Definition = def;
			Timestamp = DateTime.Now;
		}

		public ParsedSegment ParsedPacket
		{
			get
			{
				return m_parsedPacket;
			}
		}

		public bool IsParsed
		{
			get { return m_parsedPacket != null; }
		}

		internal Dictionary<string, ParsedSegment> LastParsedSegmentsByName
		{
			get
			{
				return m_parsedSegmentsByName;
			}
		}

		public void Parse()
		{
			if (!PacketAnalyzer.IsInitialized)
			{
				throw new InvalidOperationException("The PacketAnalyzer has not been initialized. " +
					"Make sure to load definitions before starting to use the PA.");
			}
			if (Definition != null)
			{
				m_parsedSegmentsByName = new Dictionary<string, ParsedSegment>();
				Definition.Structure.Parse(this, m_parsedPacket = new ParsedSegment());

				m_parsedSegmentsByName.Clear();
				m_parsedSegmentsByName = null;
			}
		}

		public void Dump(IndentTextWriter writer)
		{
			var prefix = (Definition == null ? "UNDEFINED " : "");
			var length = Packet.Length - Packet.HeaderSize;

			writer.WriteLine(TimeStampcreator(Timestamp) + string.Format(prefix + Sender + " Packet #{0} ({1}), Length: {2} bytes",
				Packet.PacketId.RawId, Packet.PacketId, length));
			writer.IndentLevel++;
			if (Definition != null)
			{
				//Definition.Structure.Render(this, writer);
				m_parsedPacket.RenderTo(writer);
			}

			// display the remainder as hexadecimal byte-string
			var remainderLength = Packet.Length - Packet.Position;
			if (remainderLength > 0)
			{
				var byteStr = new List<string>();
				while (Packet.Position < Packet.Length)
				{
					byteStr.Add(string.Format("{0:X2}", Packet.ReadByte()));
				}
				writer.WriteLine("Remainder (" + remainderLength + " bytes): " + byteStr.ToString(" "));
			}
			writer.IndentLevel--;
		}

		public static ParsedSegment Parse(PacketIn packet, PacketSender sender)
		{
			var parser = new PacketParser(packet, sender);
			parser.Parse();
			return parser.ParsedPacket;
		}

		public static ParsedSegment Parse(PacketIn packet, PacketSender sender, PacketDefinition def)
		{
			var parser = new PacketParser(packet, sender, def);
			parser.Parse();
			return parser.ParsedPacket;
		}

		public static ParsedSegment Parse(RealmPacketOut realmPacketOut, PacketSender sender)
		{
			return Parse(realmPacketOut, sender, PacketAnalyzer.GetDefinition(realmPacketOut.PacketId, sender));
		}

		public static ParsedSegment Parse(RealmPacketOut realmPacketOut, PacketSender sender, PacketDefinition def)
		{
			using (var packetIn = DisposableRealmPacketIn.CreateFromOutPacket(realmPacketOut))
			{
				//if (packetIn.PacketID == RealmServerOpCode.SMSG_COMPRESSED_MOVE)
				//{
				//    using (var movePacket = ParseCompressedMove(packetIn))
				//    {
				//        return Parse(movePacket, sender, def);
				//    }
				//}
				return Parse(packetIn, sender, def);
			}
		}

		public static DisposableRealmPacketIn ParseCompressedMove(PacketIn packet)
		{
			// special treatment for the compressed move packet
			var uncompressedLength = packet.ReadInt32();

			var segment = BufferManager.GetSegment(uncompressedLength);
			var arr = segment.Buffer.Array;
		    try
			{
				Compression.DecompressZLib(packet.ReadBytes(packet.RemainingLength), arr);

				ushort length = arr[0];
				RealmServerOpCode opCode = (RealmServerOpCode) (arr[1] | arr[2] << 8);
				return new DisposableRealmPacketIn(segment, 1, length, length - 3, opCode);
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "Unable to parse packet: " + packet);
			}
			return null;
		}

		#region Move the following to a new packet-stream class (if needed even)
		// ArraySegment<byte> m_remainder;

		//public void RenderRealmStream(byte[] bytes)
		//{
		//    Parse(ServiceType.Realm, new ArraySegment<byte>(bytes));
		//}

		//public void RenderRealmStream(ArraySegment<byte> bytes)
		//{
		//    Parse(ServiceType.Realm, bytes);
		//}

		//public void Parse(ServiceType type, byte[] bytes)
		//{
		//    Parse(type, new ArraySegment<byte>(bytes, 0, bytes.Length));
		//}

		///// <summary>
		///// Renders a part of an unencrypted WoW byte-stream
		///// </summary>
		///// <param name="bytes">Part of an unencrypted WoW byte-stream</param>
		//public void Parse(ServiceType type, ArraySegment<byte> bytes)
		//{
		//    if (m_remainder.Count > 0)
		//    {
		//        // parts coming together
		//        var newBytes = new byte[bytes.Count + m_remainder.Count];
		//        Array.Copy(m_remainder.Array, m_remainder.Offset, newBytes, 0, m_remainder.Count);
		//        Array.Copy(bytes.Array, bytes.Offset, newBytes, m_remainder.Count, bytes.Count);
		//        bytes = new ArraySegment<byte>(newBytes);
		//        m_remainder = EmptyBytes;
		//    }

		//    while (bytes.Count > 2)
		//    {
		//        int length = (bytes.Array[bytes.Offset] << 8) + bytes.Array[bytes.Offset + 1] + 2;		// 2 bytes for length
		//        if (length >= bytes.Count)
		//        {
		//            var packet = PacketMgr.CreatePacket(type, bytes);
		//            packet.Initialize();
		//            Render(packet);
		//        }
		//    }

		//    // remaining bytes
		//    if (bytes.Count > 0)
		//    {
		//        m_remainder = bytes;
		//    }
		//}
		#endregion

		#region IDisposable Members

        public void Dispose()
        {
            ((IDisposable)Packet).Dispose();

            if (m_parsedPacket != null)
            {
                m_parsedPacket = null;
            }
        }

	    #endregion
	}
}