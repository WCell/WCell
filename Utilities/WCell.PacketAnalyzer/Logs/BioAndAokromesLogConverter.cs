using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using WCell.Constants;
using WCell.Core.Network;
using WCell.PacketAnalysis.Updates;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.PacketAnalysis.Logs
{
    class BioAndAokromesLogConverter
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();


        public static void Extract(string logFile, Action<ParsablePacketInfo> parser, OpCodeValidator validator)
        {
            Extract(logFile, new LogHandler(validator, parser));
        }

        /// <summary>
        /// Extracts all Packets out of the given logged and default-formatted lines
        /// </summary>
        /// <param name="singleLinePackets">Whether the packet content is one single line (or false in case of the fancy ksniffer format)</param>
        public static void Extract(string logFile, params LogHandler[] handlers)
        {
            try
            {
                var lines = File.ReadAllLines(logFile);
                var lineNo = -1;
                var realmServerOpCode = (RealmServerOpCode) uint.MaxValue;
                var sender = PacketSender.Any;
                var timeStrLen = "Time".Length;
                for (lineNo = 0; lineNo < lines.Length;)
                {
                    try
                    {
                        var timestamp = DateTime.Now;
                        var line = lines[lineNo];

                        if (line.Length == 0)
                        {
                            lineNo++;
                            continue;
                        }

                        // find sender
                        if (line.IndexOf("SERVERTO", StringComparison.InvariantCultureIgnoreCase) > -1)
                        {
                            sender = PacketSender.Server;
                        }
                        else
                        {
                            sender = PacketSender.Client;
                        }

                        // find opcode and timestamp
                        var match = Regex.Match(line, @"\(0x(.{4})\s\-\>");
                        if (match.Success)
                        {
                            var timestampIndex = line.IndexOf("Time", StringComparison.InvariantCultureIgnoreCase) +
                                                 timeStrLen;
                            if (timestampIndex >= 0)
                            {
                                uint x;
                                while (!uint.TryParse(line[timestampIndex].ToString(), out x))
                                {
                                    timestampIndex++;
                                }
                                var timestampStr = line.Substring(timestampIndex).Trim();

                                if (!DateTime.TryParse(timestampStr, out timestamp))
                                {
                                    timestamp = DateTime.Now;
                                }
                            }
                            realmServerOpCode = (RealmServerOpCode) Int32.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
                        }

                        line = lines[++lineNo]; // one line for the packet-header

                        //Copy to local variable (access to modified closure)
                        var opCode = realmServerOpCode;
                        var opcodeHandler = handlers.Where(handler => handler.Validator(opCode)).FirstOrDefault();
                        var buildPacket = opcodeHandler != null;

                        var sb = new StringBuilder();
                        
                        if (buildPacket)
                        {
                            while(!string.IsNullOrEmpty(line = lines[lineNo++]))
                                sb.Append(line);
                        }

                        if (buildPacket)
                        {
                            if (!Enum.IsDefined(typeof (RealmServerOpCode), realmServerOpCode))
                            {
                                log.Warn("Packet at line #{0} had undefined Opcode: " + realmServerOpCode, lineNo);
                                continue;
                            }

                            var bytes = UpdateFieldsUtil.HexStringConverter.ToByteArray(sb.ToString());
                            var packet = DisposableRealmPacketIn.Create(realmServerOpCode, bytes);
                            if (packet != null)
                            {
                                if (packet.PacketId == RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT &&
                                    packet.Length < 20)
                                {
                                    throw new Exception("SMSG_COMPRESSED_UPDATE_OBJECT - Format error");
                                }

                                opcodeHandler.PacketParser(new ParsablePacketInfo(packet, sender, timestamp));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogUtil.ErrorException(e, "Error in Sniffer log at line {0} ({1})", lineNo, realmServerOpCode);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Failed to parse log-file \"{0}\"", logFile), e);
            }
        }


        /// <summary>
        /// Renders the given log file to the given output.
        /// </summary>
        /// <param name="outputFile">The location of the file that the result should be written to</param>
        public static void ConvertLog(string logFile, string outputFile)
        {
            new FileInfo(outputFile).MKDirs();

            using (var stream = new StreamWriter(outputFile, false))
            {
                ConvertLog(logFile, stream);
            }
        }

        /// <summary>
        /// Renders the given log file to the given output.
        /// </summary>
        /// <param name="output">A StreamWriter or Console.Out etc</param>
        public static void ConvertLog(string logFile, TextWriter output)
        {
            var writer = new IndentTextWriter(output);
                Extract(logFile, info => LogConverter.ParsePacket(info, writer),
                        LogConverter.DefaultValidator);
        }
    }
}
