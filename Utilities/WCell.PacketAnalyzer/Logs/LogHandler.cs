using System;
using WCell.PacketAnalysis.Updates;

namespace WCell.PacketAnalysis.Logs
{
	public class LogHandler
	{
		public readonly OpCodeValidator Validator;
		public Action<ParsablePacketInfo> PacketParser;

		public Action<PacketParser> NormalPacketHandler;
		public Action<ParsedUpdatePacket> UpdatePacketHandler;

		public LogHandler(OpCodeValidator validator, Action<PacketParser> packetHandler) : this(validator, null, packetHandler, null)
		{
		}

		public LogHandler(OpCodeValidator validator, Action<ParsablePacketInfo> parser) : this(validator, parser, null, null)
		{
		}

		public LogHandler(Action<ParsedUpdatePacket> updatePacketHandler) : this(opcode => opcode.IsUpdatePacket, updatePacketHandler)
		{
		}

		public LogHandler(OpCodeValidator validator, Action<ParsedUpdatePacket> updatePacketHandler) : this(validator, null, updatePacketHandler)
		{
		}

		public LogHandler(OpCodeValidator validator, Action<PacketParser> packetHandler, Action<ParsedUpdatePacket> updatePacketHandler) :
			this(validator, null, packetHandler, updatePacketHandler)
		{
		}

		public LogHandler(OpCodeValidator validator, Action<ParsablePacketInfo> parser,
			Action<PacketParser> packetHandler, Action<ParsedUpdatePacket> updatePacketHandler)
		{
			Validator = validator;
			PacketParser = parser;
			NormalPacketHandler = packetHandler;
			UpdatePacketHandler = updatePacketHandler;
		}
	}
}