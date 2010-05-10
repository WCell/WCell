using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core.Network;
using WCell.PacketAnalysis.Logs;
using WCell.PacketAnalysis.Updates;
using WCell.RealmServer;
using System.IO;
using WCell.Util;
using OF = WCell.Constants.Updates.ObjectFields;
using GOF = WCell.Constants.Updates.GameObjectFields;

namespace WCell.PacketAnalysis.Samples
{
	public class GOSerializer : AdvancedLogParser, IDisposable
	{
		private static IndentTextWriter s_Writer;

		public GOSerializer(LogParser parser, string outputFile)
			: this(parser, new IndentTextWriter(new StreamWriter(outputFile)))
		{
		}

		public GOSerializer(LogParser parser, IndentTextWriter writer)
			: base(parser, new LogHandler(ValidateOpCode, HandlePacket))
		{
			// just make sure to have the XML-definition of the packet before using this Class
			if (!PacketAnalyzer.IsDefined(RealmServerOpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE, PacketSender.Server))
			{
				throw new InvalidOperationException("SMSG_GAMEOBJECT_QUERY_RESPONSE is not defined.");
			}
			s_Writer = writer;
		}

		private static bool ValidateOpCode(PacketId packetId)
		{
			return packetId == RealmServerOpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE;
		}

		protected static void HandlePacket(PacketParser parser)
		{
			var pac = parser.ParsedPacket;
			s_Writer.WriteLine(@"INSERT INTO `gameobject_roots` KEYS (entry, Type, displayid, Name) " +
				"VALUES ({0}, {1}, {2}, {3})", pac["Entry"].UIntValue, pac["Type"].UIntValue, pac["DisplayId"].UIntValue, pac["Name"].StringValue);
		}

		public static void ExtractGOs(LogParser parser, string inputDir, string outputFile)
		{
			var extractor = new GOSerializer(parser, outputFile);
			extractor.Parse(new DirectoryInfo(inputDir));
		}

		public void Dispose()
		{
			s_Writer.Close();
		}
	}
}
