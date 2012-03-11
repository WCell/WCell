using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.PacketAnalysis.Updates;

namespace WCell.PacketAnalysis.Logs
{
    /// <summary>
    /// A log parser that can be used to easily extract information from packet-logs.
    /// Make sure that all Packets that you use are defined in the XML files.
    /// </summary>
    public abstract class AdvancedLogParser
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        //private readonly Action<ParsedUpdatePacket> m_UpdateHandler;
        private LogHandler[] m_Handlers;
        private LogParser m_Parser;

        /// <summary>
        ///
        /// </summary>
        /// <param name="parser"></param>
        protected AdvancedLogParser(LogParser parser)
            : this(parser, null)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="parser"></param>
        protected AdvancedLogParser(LogParser parser, params LogHandler[] handlers)
        {
            m_Parser = parser;
            Handlers = handlers;
        }

        public LogParser Parser
        {
            get { return m_Parser; }
            set
            {
                m_Parser = value;
            }
        }

        public LogHandler[] Handlers
        {
            get { return m_Handlers; }
            private set
            {
                m_Handlers = value;
                if (m_Handlers != null)
                {
                    foreach (var handler in m_Handlers)
                    {
                        handler.PacketParser = info => DoParse(info, handler);
                    }
                }
            }
        }

        public void Parse(DirectoryInfo logDir)
        {
            Parse(logDir.GetFiles());
        }

        public void Parse(IEnumerable<FileInfo> logFiles)
        {
            foreach (var file in logFiles)
            {
                Parse(file);
            }
        }

        public void Parse(FileInfo inputFile)
        {
            if (m_Handlers == null)
            {
                throw new InvalidDataException("m_Handlers is not set.");
            }
            m_Parser(inputFile.FullName, m_Handlers);
        }

        protected virtual void DoParse(ParsablePacketInfo info, LogHandler handler)
        {
            var rawPacket = info.Packet;
            if (rawPacket.PacketId.IsUpdatePacket)
            {
                var len = rawPacket.Length - rawPacket.HeaderSize;
                var bytes = rawPacket.ReadBytes(len);
                if (bytes.Length != len)
                {
                    log.Warn("BinaryReader.ReadBytes failed: {0} / {1}", bytes.Length, len);
                    return;
                }
                var updatePacket = ParsedUpdatePacket.Create(info.Timestamp,
                    bytes,
                    false,
                    rawPacket.PacketId == RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT);

                if (handler.UpdatePacketHandler != null)
                {
                    handler.UpdatePacketHandler(updatePacket);
                }
            }
            else
            {
                var parser = new PacketParser(info);
                parser.Parse();

                if (handler.NormalPacketHandler != null)
                {
                    handler.NormalPacketHandler(parser);
                }
            }
        }
    }

    public class GenericLogParser : AdvancedLogParser
    {
        public GenericLogParser(LogParser parser)
            : base(parser)
        {
        }

        public GenericLogParser(LogParser parser, OpCodeValidator validator, Action<PacketParser> packetHandler)
            : base(parser, new LogHandler(validator, packetHandler))
        {
        }

        public GenericLogParser(LogParser parser, params LogHandler[] handlers)
            : base(parser, handlers)
        {
        }

        public static void ParseFile(LogParser parser, string inputFile, OpCodeValidator filter,
                                     Action<PacketParser> packetHandler)
        {
            var extractor = new GenericLogParser(parser, filter, packetHandler);
            extractor.Parse(new FileInfo(inputFile));
        }

        public static void ParseDir(LogParser parser, string inputDir, OpCodeValidator filter,
                                    Action<PacketParser> packetHandler)
        {
            var extractor = new GenericLogParser(parser, filter, packetHandler);
            extractor.Parse(new DirectoryInfo(inputDir));
        }

        public static void ParseDir(LogParser parser, string inputDir, Action<ParsedUpdatePacket> packetHandler)
        {
            var extractor = new GenericLogParser(parser, new LogHandler(packetHandler));
            extractor.Parse(new DirectoryInfo(inputDir));
        }

        public static void ParseDir(LogParser parser, string inputDir, params LogHandler[] handlers)
        {
            var extractor = new GenericLogParser(parser, handlers);
            extractor.Parse(new DirectoryInfo(inputDir));
        }

        public static void ParseFiles(LogParser parser, FileInfo[] files, OpCodeValidator filter,
                                      Action<PacketParser> packetHandler)
        {
            var extractor = new GenericLogParser(parser, filter, packetHandler);
            extractor.Parse(files);
        }

        public static void DumpLogDir(LogParser parser, string inputDir, OpCodeValidator filter,
                                      Action<PacketParser> packetHandler)
        {
            //var extractor = new GenericLogParser(parser, filter, packetHandler, updatePacketHandler);
            //extractor.Parse(new DirectoryInfo(inputDir));
            throw new NotImplementedException();
        }

        public static void DumpLogFile(LogParser parser, string inputFile, OpCodeValidator filter,
                                       Action<PacketParser> packetHandler)
        {
            //var extractor = new GenericLogParser(parser, filter, packetHandler, updatePacketHandler);
            //extractor.Parse(new FileInfo(inputFile));
            throw new NotImplementedException();
        }

        public static void DumpLogFiles(LogParser parser, FileInfo[] files, OpCodeValidator filter)
        {
            //var extractor = new GenericLogParser(parser, filter, packetHandler, updatePacketHandler);
            //extractor.Parse(files);
            throw new NotImplementedException();
        }
    }
}