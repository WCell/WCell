using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Core.Initialization;
using WCell.Core;
using WCell.Core.Network;
using WCell.PacketAnalysis.Xml;

namespace WCell.PacketAnalysis.PacketDefinitions
{
	public static class Misc
	{
		//[Initialization(InitializationPass.Second, Name = "Misc Packet Definition")]
		public static void Init()
		{
			var msgSegment = new PacketSegment(SimpleType.CString, "Message");
			var targetMsgSegment = new ComplexPacketSegment(
				new PacketSegment(SimpleType.CString, "Target"),
				new PacketSegment(SimpleType.CString, "Message")
			);

			SwitchPacketSegment msgTypeSwitch;
			PacketAnalyzer.RegisterDefintion(new PacketDefinition(RealmServerOpCode.CMSG_MESSAGECHAT, 
				msgTypeSwitch = new SwitchPacketSegment(SimpleType.UInt, "Type", (segment, parser) => {
					parser.ParsedSegments.SetCurrentMoveNext((ChatMsgType)parser.Packet.ReadUInt32());
					},
					new Condition(ComparisonType.Equal, ChatMsgType.Say, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.Yell, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.Emote, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.Party, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.Raid, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.RaidLeader, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.RaidWarn, msgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.Whisper, targetMsgSegment),
					new Condition(ComparisonType.Equal, ChatMsgType.Channel, targetMsgSegment)
				),
				new PacketSegment(SimpleType.UInt, "Language", (segment, parser) => {
					parser.ParsedSegments.SetCurrentMoveNext((ChatLanguage)parser.Packet.ReadUInt32());
				}),
				new ConditionalPacketSegment(msgTypeSwitch)
			));
		}
	}
}
