using System;
using NLog;
using WCell.Core;
using WCell.Util;
using WCell.Util.Graphics;

namespace WCell.PacketAnalysis
{
    /// <summary>
    /// Reads and renders the next segment of the given packet.
    /// </summary>
    public delegate object SegmentReader(PacketSegmentStructure segment, PacketParser parser);

    /// <summary>
    /// Reads and renders the next segment of the given packet.
    /// </summary>
    public delegate void SegmentRenderer(PacketSegmentStructure segment, PacketParser parser, IndentTextWriter writer);

    public delegate object SimpleTypeConverter(object obj);

    public static class SimpleTypes
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public static readonly Type[] SimpleTypeMap = new Type[(uint)SimpleType.Count];
        public static readonly SegmentReader[] SimpleReaders = new SegmentReader[(uint)SimpleType.Count];
        public static readonly SimpleTypeConverter[] SimpleTypeConverters = new SimpleTypeConverter[(uint)SimpleType.Count];
        public static readonly Func<string, object>[] SimpleStringReaders = new Func<string, object>[(uint)SimpleType.Count];

        static SimpleTypes()
        {
            InitTypeMap();
            InitReaders();
            InitConverters();
            InitStringReaders();
        }

        #region Init

        private static void InitReaders()
        {
            SimpleReaders[(uint)SimpleType.Byte] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 1)
                {
                    WarnLength(SimpleType.Byte, segment, parser);
                    return (byte)0;
                }
                return parser.Packet.ReadByte();
            };
            //SimpleReaders[(uint)SimpleType.SByte] = (segment, parser) => {
            //    return parser.Packet.ReadByte();
            //};
            SimpleReaders[(uint)SimpleType.UShort] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 2)
                {
                    WarnLength(SimpleType.UShort, segment, parser);
                    return (ushort)0;
                }
                return parser.Packet.ReadUInt16();
            };
            SimpleReaders[(uint)SimpleType.Short] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 2)
                {
                    WarnLength(SimpleType.Short, segment, parser);
                    return (short)0;
                }
                return parser.Packet.ReadInt16();
            };
            SimpleReaders[(uint)SimpleType.UInt] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 4)
                {
                    WarnLength(SimpleType.UInt, segment, parser);
                    return (uint)0;
                }
                return parser.Packet.ReadUInt32();
            };
            SimpleReaders[(uint)SimpleType.Int] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 4)
                {
                    WarnLength(SimpleType.Int, segment, parser);
                    return 0;
                }
                return parser.Packet.ReadInt32();
            };
            SimpleReaders[(uint)SimpleType.ULong] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 8)
                {
                    WarnLength(SimpleType.ULong, segment, parser);
                    return 0ul;
                }
                return parser.Packet.ReadUInt64();
            };
            SimpleReaders[(uint)SimpleType.Long] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 8)
                {
                    WarnLength(SimpleType.Long, segment, parser);
                    return 0L;
                }
                return parser.Packet.ReadInt64();
            };
            SimpleReaders[(uint)SimpleType.Float] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 4)
                {
                    WarnLength(SimpleType.Float, segment, parser);
                    return 0f;
                }
                return parser.Packet.ReadFloat();
            };
            SimpleReaders[(uint)SimpleType.Vector3] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 12)
                {
                    WarnLength(SimpleType.Vector3, segment, parser);
                    return new Vector3();
                }
                return new Vector3(
                    parser.Packet.ReadFloat(),
                    parser.Packet.ReadFloat(),
                    parser.Packet.ReadFloat());
            };
            SimpleReaders[(uint)SimpleType.PackedVector3] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 4)
                {
                    WarnLength(SimpleType.PackedVector3, segment, parser);
                    return new Vector3();
                }
                return Vector3.FromPacked(parser.Packet.ReadUInt32());
            };
            SimpleReaders[(uint)SimpleType.Vector4] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 16)
                {
                    WarnLength(SimpleType.Vector4, segment, parser);
                    return new Vector4();
                }
                return new Vector4(
                    parser.Packet.ReadFloat(),
                    parser.Packet.ReadFloat(),
                    parser.Packet.ReadFloat(),
                    parser.Packet.ReadFloat());
            };
            SimpleReaders[(uint)SimpleType.Guid] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 8)
                {
                    WarnLength(SimpleType.Guid, segment, parser);
                    return EntityId.Zero;
                }
                return new EntityId(parser.Packet.ReadUInt64());
            };
            SimpleReaders[(uint)SimpleType.PackedGuid] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 1)
                {
                    WarnLength(SimpleType.PackedGuid, segment, parser);
                    return EntityId.Zero;
                }
                return EntityId.ReadPacked(parser.Packet);
            };
            SimpleReaders[(uint)SimpleType.CString] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 1)
                {
                    WarnLength(SimpleType.CString, segment, parser);
                    return "";
                }
                return parser.Packet.ReadCString();
            };

            SimpleReaders[(uint)SimpleType.PascalStringByte] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 1)
                {
                    WarnLength(SimpleType.PascalStringByte, segment, parser);
                    return "";
                }
                return parser.Packet.ReadPascalString();
            };
            SimpleReaders[(uint)SimpleType.PascalStringUShort] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 2)
                {
                    WarnLength(SimpleType.PascalStringUShort, segment, parser);
                    return "";
                }
                return parser.Packet.ReadPascalStringUShort();
            };
            SimpleReaders[(uint)SimpleType.PascalStringUInt] = (segment, parser) =>
            {
                if (parser.Packet.RemainingLength < 4)
                {
                    WarnLength(SimpleType.PascalStringUInt, segment, parser);
                    return "";
                }
                return parser.Packet.ReadPascalStringUInt();
            };

            SimpleReaders[(int)SimpleType.PackedDate] =
                (segment, parser) =>
                {
                    if (parser.Packet.RemainingLength < 4)
                    {
                        WarnLength(SimpleType.PackedDate, segment, parser);
                        return "";
                    }
                    return
                        Utility.GetGameTimeToDateTime(
                            parser.Packet.ReadUInt32()).
                            ToString();
                };

            SimpleReaders[(int)SimpleType.UnixTime] =
                (segment, parser) =>
                {
                    if (parser.Packet.RemainingLength < 4)
                    {
                        WarnLength(SimpleType.UnixTime, segment, parser);
                        return "";
                    }
                    return Utility.GetDateTimeFromUnixTime(parser.Packet.ReadUInt32()).ToString();
                };
        }

        private static void WarnLength(SimpleType type, PacketSegmentStructure segment, PacketParser parser)
        {
            log.Warn(
                "Packet {0} has invalid definition and tries to read {1} with insufficient remaining length at {2}",
                parser.Packet.PacketId, type, segment);
        }

        private static void InitConverters()
        {
            // in case of enum-values, convert back to numeric type
            SimpleTypeConverters[(uint)SimpleType.Byte] = obj => (byte)obj;
            SimpleTypeConverters[(uint)SimpleType.UShort] = obj => (ushort)obj;
            SimpleTypeConverters[(uint)SimpleType.Short] = obj => (short)obj;
            SimpleTypeConverters[(uint)SimpleType.UInt] = obj => (uint)obj;
            SimpleTypeConverters[(uint)SimpleType.Int] = obj => (int)obj;
            SimpleTypeConverters[(uint)SimpleType.ULong] = obj => (ulong)obj;
            SimpleTypeConverters[(uint)SimpleType.Long] = obj => (long)obj;
            SimpleTypeConverters[(uint)SimpleType.Float] = obj => (float)obj;

            // these don't actually do anything
            SimpleTypeConverters[(uint)SimpleType.Guid] = obj => (EntityId)obj;
            SimpleTypeConverters[(uint)SimpleType.PackedGuid] = obj => (EntityId)obj;
            SimpleTypeConverters[(uint)SimpleType.CString] = obj => (string)obj;
            SimpleTypeConverters[(uint)SimpleType.PascalStringByte] = obj => (string)obj;
            SimpleTypeConverters[(uint)SimpleType.PascalStringUShort] = obj => (string)obj;
            SimpleTypeConverters[(uint)SimpleType.PascalStringUInt] = obj => (string)obj;
            SimpleTypeConverters[(uint)SimpleType.Vector3] = obj => (Vector3)obj;
            SimpleTypeConverters[(uint)SimpleType.PackedVector3] = obj => (Vector3)obj;
            SimpleTypeConverters[(uint)SimpleType.Vector4] = obj => (Vector4)obj;
        }

        private static void InitTypeMap()
        {
            SimpleTypeMap[(uint)SimpleType.Byte] = typeof(byte);
            //SimpleTypeMap[(uint)SimpleType.SByte] = typeof(sbyte);
            SimpleTypeMap[(uint)SimpleType.UShort] = typeof(ushort);
            SimpleTypeMap[(uint)SimpleType.Short] = typeof(short);
            SimpleTypeMap[(uint)SimpleType.UInt] = typeof(uint);
            SimpleTypeMap[(uint)SimpleType.Int] = typeof(int);
            SimpleTypeMap[(uint)SimpleType.ULong] = typeof(ulong);
            SimpleTypeMap[(uint)SimpleType.Long] = typeof(long);
            SimpleTypeMap[(uint)SimpleType.Float] = typeof(float);
            SimpleTypeMap[(uint)SimpleType.Vector3] = typeof(Vector3);
            SimpleTypeMap[(uint)SimpleType.PackedVector3] = typeof(Vector3);
            SimpleTypeMap[(uint)SimpleType.Vector4] = typeof(Vector4);
            SimpleTypeMap[(uint)SimpleType.Guid] = typeof(EntityId);
            SimpleTypeMap[(uint)SimpleType.PackedGuid] = typeof(EntityId);
            SimpleTypeMap[(uint)SimpleType.CString] = typeof(string);

            SimpleTypeMap[(uint)SimpleType.PascalStringByte] = typeof(string);
            SimpleTypeMap[(uint)SimpleType.PascalStringUShort] = typeof(string);
            SimpleTypeMap[(uint)SimpleType.PascalStringUInt] = typeof(string);

            SimpleTypeMap[(int)SimpleType.PackedDate] = typeof(DateTime);
            SimpleTypeMap[(int)SimpleType.UnixTime] = typeof(DateTime);
        }

        private static void InitStringReaders()
        {
            SimpleStringReaders[(uint)SimpleType.Byte] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return byte.Parse(str);
            };
            //SimpleStringReaders[(uint)SimpleType.SByte] = (str) => {
            //    writer.WriteLine(segment.Name + ": " + parser.ParsedSegments.GetCurrentMoveNext());
            //};
            SimpleStringReaders[(uint)SimpleType.UShort] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return ushort.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return ushort.Parse(str);
            };
            SimpleStringReaders[(uint)SimpleType.Short] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return short.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return short.Parse(str);
            };
            SimpleStringReaders[(uint)SimpleType.UInt] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return uint.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return uint.Parse(str);
            };
            SimpleStringReaders[(uint)SimpleType.Int] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return int.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return int.Parse(str);
            };
            SimpleStringReaders[(uint)SimpleType.ULong] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return ulong.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return ulong.Parse(str);
            };
            SimpleStringReaders[(uint)SimpleType.Long] = (str) =>
            {
                if (str.StartsWith("0x"))
                {
                    return long.Parse(str, System.Globalization.NumberStyles.HexNumber);
                }
                return long.Parse(str);
            };
            SimpleStringReaders[(uint)SimpleType.Float] = (str) => float.Parse(str);
            SimpleStringReaders[(uint)SimpleType.Vector3] = (str) =>
            {
                throw new NotImplementedException("String representation of Vector3 is NIY");
            };
            SimpleStringReaders[(uint)SimpleType.PackedVector3] = str =>
                                                                       {
                                                                           throw new NotImplementedException(
                                                                               "String representation of PackedVector3 is NIY");
                                                                       };
            SimpleStringReaders[(uint)SimpleType.Vector4] = (str) =>
            {
                throw new NotImplementedException("String representation of Vector4 is NIY");
            };
            SimpleStringReaders[(uint)SimpleType.Guid] = (str) =>
            {
                ulong val;
                if (str.StartsWith("0x"))
                {
                    val = ulong.Parse(str.Substring(2), System.Globalization.NumberStyles.HexNumber);
                }
                else
                {
                    val = ulong.Parse(str);
                }
                return new EntityId(val);
            };
            SimpleStringReaders[(uint)SimpleType.PackedGuid] = (str) =>
            {
                throw new Exception("Packed GUIDs cannot be parsed from string.");
            };

            SimpleStringReaders[(uint)SimpleType.PackedDate] = (str) =>
            {
                throw new Exception("Packed dates cannot be parsed from string.");
            };
            SimpleStringReaders[(uint)SimpleType.UnixTime] = (str) =>
            {
                throw new Exception("Packed dates cannot be parsed from string.");
            };

            SimpleStringReaders[(uint)SimpleType.CString] = str => str;
            SimpleStringReaders[(uint)SimpleType.PascalStringByte] = str => str;
            SimpleStringReaders[(uint)SimpleType.PascalStringUShort] = str => str;
            SimpleStringReaders[(uint)SimpleType.PascalStringUInt] = str => str;
        }

        #endregion Init

        public static Type GetActualType(this SimpleType type)
        {
            return SimpleTypeMap[(int)type];
        }

        public static object ReadString(SimpleType type, string str)
        {
            return SimpleStringReaders[(int)type](str);
        }

        public static SegmentReader GetReader(SimpleType type)
        {
            return SimpleReaders[(uint)type];
        }
    }
}