using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Network;
using WCell.Core.Initialization;
using WCell.Core;
using WCell.RealmServer.Spells;

namespace WCell.PacketAnalysis.PacketDefinitions
{
	public static class Spells
	{
		public static SegmentParser SpellIdParser = (segment, parser) => {
			parser.ParsedSegments.SetCurrentMoveNext((SpellId)parser.Packet.ReadUInt32());
		};

		//[Initialization(InitializationPass.Second, Name = "Spell Packet Definition")]
		public static void Init()
		{
			PacketAnalyzer.RegisterDefintion(new PacketDefinition(RealmServerOpCode.CMSG_CAST_SPELL, 
				new PacketSegment(SimpleType.UInt, "SpellId", SpellIdParser)
			));

			PacketAnalyzer.RegisterDefintion(new PacketDefinition(RealmServerOpCode.CMSG_CANCEL_CAST,
				new PacketSegment(SimpleType.UInt, "SpellId", SpellIdParser)
			));
		}
	}
}