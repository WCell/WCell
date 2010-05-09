using System;
using WCell.Core.Network;
using WCell.Util;

namespace WCell.PacketAnalysis.Logs
{
	public delegate void PacketParseHandler(ParsablePacketInfo info);

	public delegate void LogParser(string logFile, params LogHandler[] m_Handlers);

	public delegate bool OpCodeValidator(PacketId packetId);

	public struct ParsablePacketInfo
	{
		public ParsablePacketInfo(PacketIn packet, PacketSender sender, DateTime timestamp)
		{
			Packet = packet;
			Sender = sender;
			Timestamp = timestamp;
		}

		public DateTime Timestamp;
		public PacketSender Sender;
		public PacketIn Packet;
	}

	public static class LogConverter
	{
		public static OpCodeValidator DefaultValidator = (opCode) => true;

		public static readonly LogParser[] AvailableParsers;

		static LogConverter()
		{
			AvailableParsers = new LogParser[(int)LogParserType.End];

			AvailableParsers[(int)LogParserType.KSniffer] = KSnifferLogConverter.ExtractMultiLine;
			AvailableParsers[(int)LogParserType.KSnifferSingleLine] = KSnifferLogConverter.ExtractSingleLine;
			AvailableParsers[(int)LogParserType.Sniffitzt] = SniffitztLogConverter.Extract;
		}

		public static LogParser GetParser(LogParserType type)
		{
			return AvailableParsers.Get((uint)type);
		}

		public static void ParsePacket(ParsablePacketInfo info, IndentTextWriter writer)
		{
			PacketAnalyzer.Dump(info, writer);
			writer.WriteLine();

			((IDisposable)info.Packet).Dispose();
		}
	}

	public enum LogParserType
	{
		KSniffer,
		KSnifferSingleLine,
		Sniffitzt,
		End
	}
}