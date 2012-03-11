using WCell.Constants;
using WCell.Constants.Updates;
using WCell.PacketAnalysis.Logs;
using WCell.PacketAnalysis.Updates;
using WCell.Util;

namespace WCell.PacketAnalysis.Samples
{
    public class MixedPASample
    {
        private static IndentTextWriter questWriter = new IndentTextWriter("ParsedQuestOutput.txt");

        private static IndentTextWriter goWriter = new IndentTextWriter("GODefinitions.sql");

        private static IndentTextWriter level80MobsWriter = new IndentTextWriter("Level80Mobs.txt");

        private static void Main(string[] args)
        {
            // Parse all files in the dir "/logs" using the KSniffer-log-format and
            // hand all SMSG_GAMEOBJECT_QUERY_RESPONSE packets to the HandleGOQueryPackets - method
            GenericLogParser.ParseDir(KSnifferLogConverter.ExtractSingleLine, "/logs",
                                      opcode => opcode == RealmServerOpCode.SMSG_GAMEOBJECT_QUERY_RESPONSE,
                                      HandleGOQueryPackets);

            // Parse all files in the dir "/logs" using the KSniffer-log-format (with all of a packet's content in a single line) and
            // hand all Quest-related packets to the HandleQuestPackets - method
            GenericLogParser.ParseDir(KSnifferLogConverter.ExtractSingleLine, "/logs",
                                      opcode => opcode.ToString().Contains("QUEST"),
                                      HandleQuestPackets);

            // Parse all files in the dir "/logs" using the Sniffitzt-log-format and
            // hand all Update-packets to the HandleUpdatePackets - method
            // NOTE: Update packets are different from all other kinds of packets and therefore get special treatment
            GenericLogParser.ParseDir(SniffitztLogConverter.Extract, "/logs",
                                      HandleUpdatePackets);
        }

        /// <summary>
        /// Writes the content of the given parsed Packet to an SQL file
        /// </summary>
        /// <param name="parser">Parsed SMSG_GAMEOBJECT_QUERY_RESPONSE packet</param>
        public static void HandleGOQueryPackets(PacketParser parser)
        {
            var pac = parser.ParsedPacket;
            goWriter.WriteLine(@"INSERT INTO `gameobject_root` KEYS (entry, Type, displayid, Name) " +
                "VALUES ({0}, {1}, {2}, {3})", pac["Entry"].UIntValue, pac["Type"].UIntValue, pac["DisplayId"].UIntValue, pac["Name"].StringValue);
        }

        /// <summary>
        /// Write human-readable version of log to ParsedQuestOutput.txt
        /// </summary>
        /// <param name="parser">Any kind of Quest-packet</param>
        public static void HandleQuestPackets(PacketParser parser)
        {
            parser.Dump(questWriter);
            questWriter.WriteLine();		// empty line in between entries
        }

        /// <summary>
        /// Write human-readable version of log to ParsedQuestOutput.txt
        /// </summary>
        /// <param name="packet">Any kind of Update-packet</param>
        public static void HandleUpdatePackets(ParsedUpdatePacket packet)
        {
            // iterate over all Creation-UpdateBlocks in the UpdatePacket
            foreach (var block in packet.GetBlocks(UpdateType.Create))
            {
                // We only want NPC-information
                if (block.ObjectType == ObjectTypeId.Unit)
                {
                    // write all NPCs that I encountered and have Level > 80 to a file
                    // add a "u" (for unsigned in uint) to the number, since the Level is a uint
                    if (block.GetUInt(UnitFields.LEVEL) > 80u)
                    {
                        packet.Dump(level80MobsWriter);
                        level80MobsWriter.WriteLine();		// empty line in between entries
                    }
                }
            }
        }
    }
}