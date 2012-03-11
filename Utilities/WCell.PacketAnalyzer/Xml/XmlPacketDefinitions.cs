using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.Util;

namespace WCell.PacketAnalysis.Xml
{
    [XmlRoot("Definitions")]
    public class XmlPacketDefinitions : XmlFile<XmlPacketDefinitions>
    {
        /// <summary>
        /// Used for compliancy
        /// </summary>
        public const int CurrentVersion = 4;

        int m_version;

        public XmlPacketDefinitions()
            : base("")
        {
        }

        public XmlPacketDefinitions(string filename)
            : base(filename)
        {
            m_version = CurrentVersion;
        }

        public XmlPacketDefinitions(string filename, PacketDefinition[] defs)
            : this(filename)
        {
            Definitions = new XmlPacketDefinition[defs.Length];
            for (var i = 0; i < defs.Length; i++)
            {
                Definitions[i] = defs[i].Serialize();
            }
            Version = CurrentVersion;
        }

        [XmlAttribute]
        public int Version
        {
            get
            {
                return m_version;
            }
            set
            {
                m_version = value;
            }
        }

        [XmlAttribute("schemaLocation", Namespace = "xsi")]
        public string SchemaLocation
        {
            get;
            set;
        }

        [XmlElement("RealmPacket", typeof(RealmPacketDefinition))]
        [XmlElement("AuthPacket", typeof(AuthPacketDefinition))]
        public XmlPacketDefinition[] Definitions
        {
            get;
            set;
        }

        protected override void OnLoad()
        {
            if (Version != CurrentVersion)
            {
                string changes;
                if (Version < CurrentVersion)
                {
                    changes = ChangeLog.GetChangeLog(Version, CurrentVersion);
                }
                else
                {
                    changes = "XML Packet Definition probably requires a newer version of the PacketAnalyzer.";
                }
                var msg = string.Format("Could not load definitions from \"" + FileName + "\"."
                + " Found Version: {0} - Expected Version: {1}\n{2}", Version, CurrentVersion, changes);
                Console.WriteLine(msg);
                throw new Exception(msg);
            }
        }

        private void SaveTest()
        {
            var msgSegment = new PacketSegmentStructure(SimpleType.CString, "Message");
            var targetMsgSegment = new PacketSegmentStructure[] {
				new PacketSegmentStructure(SimpleType.CString, "Target"),
				new PacketSegmentStructure(SimpleType.CString, "Message")
			}.ToList();

            SwitchPacketSegmentStructure msgTypeSwitch;

            var defs = new RealmPacketDefinition();
            Definitions = new XmlPacketDefinition[] {
				new RealmPacketDefinition(RealmServerOpCode.CMSG_CAST_SPELL,
					//new ComplexPacketSegmentStructure(
					//new PacketSegmentStructure[] {
						new PacketSegmentStructure(SimpleType.UInt, "Spell Id", typeof(SpellId))
					//})
				),
				defs
			};
            defs.OpCodes = new[] { RealmServerOpCode.CMSG_MESSAGECHAT };
            defs.Structure = new[] {
				new PacketSegmentStructure(SimpleType.UInt, "Type", typeof(ChatMsgType)),
				new PacketSegmentStructure(SimpleType.UInt, "Language", typeof(ChatLanguage)),
				msgTypeSwitch = new SwitchPacketSegmentStructure("TargetAndMessage", "Type",
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Say, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Yell, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Emote, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Party, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Raid, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.RaidLeader, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.RaidWarn, msgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Whisper, targetMsgSegment),
					new SwitchCase(ComparisonType.Equal, ChatMsgType.Channel, targetMsgSegment)
				)
			}.ToList();
            //defs.Structure.Init();
            Save();
        }

        public static PacketDefinition[] LoadDefinitions(string file)
        {
            var defs = Load(file);
            if (defs != null && defs.Definitions != null)
            {
                var packetDefs = new PacketDefinition[defs.Definitions.Length];

                for (int i = 0; i < packetDefs.Length; i++)
                {
                    var def = defs.Definitions[i];
                    var packetDef = new PacketDefinition(def.GetPacketIds(), def.Sender, def.Structure);
                    packetDef.Init();
                    packetDefs[i] = packetDef;
                }

                return packetDefs;
            }

            return PacketDefinition.Empty;
        }

        public IEnumerator<XmlPacketDefinition> GetEnumerator()
        {
            return (IEnumerator<XmlPacketDefinition>)Definitions.GetEnumerator();
        }
    }

    public class RealmPacketDefinition : XmlPacketDefinition
    {
        public RealmPacketDefinition()
        {
        }

        public RealmPacketDefinition(RealmServerOpCode opCode, ComplexPacketSegmentStructure segments)
        {
            OpCode = opCode;
            Structure = segments.Segments.ToList();
        }

        public RealmPacketDefinition(RealmServerOpCode opCode, params PacketSegmentStructure[] segments)
        {
            OpCode = opCode;
            Structure = segments.ToList();
        }

        public RealmPacketDefinition(RealmServerOpCode[] opCodes, ComplexPacketSegmentStructure segments)
        {
            OpCodes = opCodes;
            Structure = segments.Segments.ToList();
        }

        public RealmPacketDefinition(RealmServerOpCode[] opCodes, params PacketSegmentStructure[] segments)
        {
            OpCodes = opCodes;
            Structure = segments.ToList();
        }

        RealmServerOpCode OpCode
        {
            get
            {
                return (RealmServerOpCode)Enum.Parse(typeof(RealmServerOpCode), OpCode_);
            }
            set
            {
                OpCode_ = value.ToString();
            }
        }

        [XmlIgnore]
        public RealmServerOpCode[] OpCodes
        {
            get
            {
                if (OpCodes_ == null)
                {
                    return new[] { OpCode };
                }
                return OpCodes_.TransformArray(opcode => (RealmServerOpCode)Enum.Parse(typeof(RealmServerOpCode), opcode));
            }
            set
            {
                OpCodes_ = value.TransformArray(opcode => opcode.ToString());
            }
        }

        public override PacketId[] GetPacketIds()
        {
            return OpCodes.TransformArray(opcode => (PacketId)opcode);
        }
    }

    public class AuthPacketDefinition : XmlPacketDefinition
    {
        public AuthPacketDefinition()
        {
        }

        public AuthPacketDefinition(AuthServerOpCode opCode, ComplexPacketSegmentStructure segments)
        {
            OpCode = opCode;
            Structure = segments.Segments.ToList();
        }

        public AuthPacketDefinition(AuthServerOpCode opCode, params PacketSegmentStructure[] segments)
        {
            OpCode = opCode;
            Structure = segments.ToList();
        }

        [XmlIgnore]
        public AuthServerOpCode OpCode
        {
            get
            {
                return (AuthServerOpCode)Enum.Parse(typeof(AuthServerOpCode), base.OpCode_);
            }
            set
            {
                base.OpCode_ = value.ToString();
            }
        }

        [XmlIgnore]
        public AuthServerOpCode[] OpCodes
        {
            get
            {
                return OpCodes_.TransformArray(opcode => (AuthServerOpCode)Enum.Parse(typeof(AuthServerOpCode), opcode));
            }
            set
            {
                OpCodes_ = value.TransformArray(opcode => opcode.ToString());
            }
        }

        public override PacketId[] GetPacketIds()
        {
            return OpCodes.TransformArray(opcode => (PacketId)opcode);
        }
    }

    [XmlInclude(typeof(RealmServerOpCode))]
    [XmlInclude(typeof(AuthServerOpCode))]
    public abstract class XmlPacketDefinition
    {
        protected string _opcode;
        protected string[] _opcodes;

        [XmlAttribute("OpCode")]
        public string OpCode_
        {
            get
            {
                return _opcode;
            }
            set
            {
                _opcode = value;
            }
        }

        [XmlArray("OpCodes", Order = 1)]
        [XmlArrayItem("OpCode")]
        public string[] OpCodes_
        {
            get
            {
                return _opcodes;
            }
            set
            {
                _opcodes = value;
            }
        }

        [XmlElement("StaticList", typeof(StaticListPacketSegmentStructure), Order = 2)]
        [XmlElement("List", typeof(ListPacketSegmentStructure), Order = 2)]
        [XmlElement("FinalList", typeof(FinalListPacketSegmentStructure))]
        [XmlElement("Complex", typeof(ComplexPacketSegmentStructure), Order = 2)]
        [XmlElement("Switch", typeof(SwitchPacketSegmentStructure), Order = 2)]
        [XmlElement("Simple", typeof(PacketSegmentStructure), Order = 2)]
        public List<PacketSegmentStructure> Structure
        {
            get;
            set;
        }

        [XmlAttribute()]
        public PacketSender Sender
        {
            get;
            set;
        }

        public abstract PacketId[] GetPacketIds();
    }
}