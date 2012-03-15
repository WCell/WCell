using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.Util.NLog;

namespace WCell.PacketAnalysis.Updates
{
    public class UpdateBlock : IDisposable
    {
        protected static Logger log = LogManager.GetCurrentClassLogger();

        internal ParsedUpdatePacket packet;
        //public readonly uint Offset;

        public readonly UpdateType Type;
        public readonly EntityId EntityId;

        public readonly EntityId[] EntityIds;

        public readonly ObjectTypeId ObjectType;

        public readonly byte[] Values;
        public uint[] SetIndices;

        private MovementBlock m_movement;

        private int index;

        public UpdateBlock(ParsedUpdatePacket parser, int index)
        {
            this.index = index;
            packet = parser;
            //Offset = parser.index;

            Type = (UpdateType)ReadByte();

            if (!Enum.IsDefined(typeof(UpdateType), (byte)Type))
            {
                throw new Exception("Invalid UpdateType '" + Type + "' in Block " + this);
            }

            // Console.WriteLine("Reading {0}-Block...", Type);

            if (Type == UpdateType.OutOfRange ||
                Type == UpdateType.Near)
            {
                var count = ReadUInt();
                EntityIds = new EntityId[count];
                for (var i = 0; i < count; i++)
                {
                    EntityIds[i] = ReadPackedEntityId();
                }
            }
            else
            {
                EntityId = ReadPackedEntityId();

                if (Type == UpdateType.Create ||
                    Type == UpdateType.CreateSelf)
                {
                    ObjectType = (ObjectTypeId)ReadByte();
                }

                if (Type == UpdateType.Create ||
                    Type == UpdateType.CreateSelf ||
                    Type == UpdateType.Movement)
                {
                    m_movement = ReadMovementBlock();
                }

                if (Type != UpdateType.Movement)
                {
                    Values = ReadValues();
                }
            }

            if (Type != UpdateType.Create && Type != UpdateType.CreateSelf)
            {
                ObjectType = EntityId.ObjectType;
            }
        }

        public MovementBlock Movement
        {
            get { return m_movement; }
        }

        /// <summary>
        /// The actual amount of fields that got updated
        /// </summary>
        public uint UpdateCount
        {
            get { return SetIndices != null ? (uint)SetIndices.Length : 0; }
        }

        #region Get Values of Fields

        /// <summary>
        /// Renders the entry at the given id (or null if field is not set) as a string
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string this[ExtendedUpdateFieldId id]
        {
            get { return FieldRenderer.Render(id, this); }
        }

        public ulong GetULong(UpdateFieldId id)
        {
            return Values.GetUInt64((uint)id.RawId);
        }

        public float GetFloat(UpdateFieldId id)
        {
            return Values.GetFloat((uint)id.RawId);
        }

        public uint GetUInt(UpdateFieldId id)
        {
            return Values.GetUInt32((uint)id.RawId);
        }

        public int GetInt(UpdateFieldId id)
        {
            return Values.GetInt32((uint)id.RawId);
        }

        public ushort GetUShort(UpdateFieldId id)
        {
            return Values.GetUInt16((uint)id.RawId);
        }

        public byte GetByte(UpdateFieldId id)
        {
            var i = (uint)id.RawId * 4;
            if (Values.Length < i + 1)
            {
                return 0;
            }
            return Values[i];
        }

        public bool IsSet(UpdateFieldId id)
        {
            var i = (uint)id.RawId * 4;
            if (Values.Length < i + 1)
            {
                return false;
            }
            return SetIndices.Contains((uint)id.RawId);
        }

        #endregion Get Values of Fields

        internal byte ReadByte()
        {
            return packet.Bytes[packet.index++];
        }

        internal ushort ReadUShort()
        {
            ushort x = 0;
            for (int j = 0; j < sizeof(ushort); j++)
            {
                x = (ushort)(x + (packet.Bytes[packet.index++] << (j * 8)));
            }
            return x;
        }

        internal uint ReadUIntAmount(uint amount)
        {
            uint x = 0;
            for (int j = 0; j < amount; j++)
            {
                x = x + (uint)(packet.Bytes[packet.index++] << (j * 8));
            }
            return x;
        }

        internal int ReadIntAmount(int amount)
        {
            int x = 0;
            for (int j = 0; j < amount; j++)
            {
                x = x + (packet.Bytes[packet.index++] << (j * 8));
            }
            return x;
        }

        internal uint ReadUInt()
        {
            uint x = 0;
            for (int j = 0; j < sizeof(uint); j++)
            {
                x = x + (uint)(packet.Bytes[packet.index++] << (j * 8));
            }
            return x;
        }

        internal int ReadInt()
        {
            int x = 0;
            for (int j = 0; j < sizeof(int); j++)
            {
                x = x + (packet.Bytes[packet.index++] << (j * 8));
            }
            return x;
        }

        internal uint ReadUInt(uint index)
        {
            uint x = 0;
            for (int j = 0; j < sizeof(uint); j++)
            {
                x = x + (uint)(packet.Bytes[index++] << (j * 8));
            }
            return x;
        }

        internal int ReadInt(uint index)
        {
            int x = 0;
            for (int j = 0; j < sizeof(int); j++)
            {
                x = x + (packet.Bytes[index++] << (j * 8));
            }
            return x;
        }

        internal ulong ReadUInt64()
        {
            ulong x = 0;
            for (int i = 0; i < sizeof(ulong); i++)
            {
                x += (ulong)(packet.Bytes[packet.index++] << (i * 8));
            }
            return x;
        }

        internal float ReadFloat()
        {
            var f = BitConverter.ToSingle(packet.Bytes, (int)packet.index);
            packet.index += sizeof(float);
            return f;
        }

        internal Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        internal Vector4 ReadVector4NoO()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), 0f);
        }

        internal Vector3 ReadVector3()
        {
            //var z = ReadFloat();
            //var y = ReadFloat();
            //return new Vector3(ReadFloat(), z, y);
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        internal byte[] ReadBytes(int amount)
        {
            var bs = new byte[amount];
            for (uint j = 0; j < amount; j++)
            {
                bs[j] = packet.Bytes[packet.index++];
            }
            return bs;
        }

        internal byte[] ReadBytes(uint index, int amount)
        {
            var bs = new byte[amount];
            for (int j = 0; j < amount; j++)
            {
                bs[j] = packet.Bytes[index++];
            }
            return bs;
        }

        internal MovementBlock ReadMovementBlock()
        {
            return new MovementBlock(this);
        }

        internal EntityId ReadEntityId()
        {
            return new EntityId(ReadBytes(8));
        }

        internal EntityId ReadPackedEntityId()
        {
            var rawId = ReadMask(1, 1, "Packet EntityId", (index, bytes) => bytes[index] = ReadByte());
            return new EntityId(rawId);
        }

        internal byte[] ReadValues()
        {
            var blockCount = ReadByte();
            var list = new List<uint>();
            var values = ReadMask(blockCount, 4, "Values", (index, bytes) =>
            {
                for (int i = 0; i < 4; i++)
                {
                    bytes[index + i] = ReadByte();
                }
                list.Add(index);
            });
            SetIndices = list.ToArray();
            return values;
        }

        internal byte[] ReadMask(uint blockCount, uint valueSize, string name, Action<uint, byte[]> parser)
        {
            var bitFields = new uint[blockCount][];
            var totalCount = 0;
            for (var j = 0; j < blockCount; j++)
            {
                var mask = ReadUIntAmount(valueSize);
                bitFields[j] = Utility.GetSetIndices(mask);
                totalCount += bitFields[j].Length;
            }

            var fieldCount = 8 * valueSize;
            var blockSize = fieldCount * valueSize; // a block has 8 * valueSize fields and each field has again valueSize bytes
            var values = new byte[blockSize * bitFields.Length];

            for (uint j = 0; j < blockCount; j++)
            {
                var bitField = bitFields[j];
                for (uint k = 0; k < bitField.Length; k++)
                {
                    var bit = bitField[k];
                    var index = j * blockSize + (bit * valueSize);
                    try
                    {
                        parser(index, values);
                    }
                    catch (Exception e)
                    {
                        var field = index / valueSize;
                        var msg =
                            string.Format("Unable to parse {0} of UpdateBlock at index {1} ({7})\n\tBlocks: {2}/{3}\n\tField: {4} ({5}/{6})",
                                          name, index, j + 1, blockCount, bit, k + 1, bitField.Length,
                                          FieldRenderUtil.GetFriendlyName(ObjectType,
                                                                          field));
                        LogUtil.ErrorException(e, msg);
                        return values.ToArray();
                    }
                }
            }
            return values.ToArray();
        }

        internal void Skip(uint amount)
        {
            packet.index += amount;
        }

        public void Dump(string indent, IndentTextWriter writer)
        {
            writer.WriteLine(indent + "UpdateBlock: " + EntityId + " (FieldCount: " + UpdateCount + ")");

            writer.IndentLevel++;

            writer.WriteLine(indent + "Type: " + Type);
            if (m_movement != null)
            {
                writer.WriteLine();
                writer.WriteLine(indent + "Movement:");
                m_movement.Dump(indent + "\t", writer);
            }
            writer.WriteLine();

            if (EntityIds != null)
            {
                writer.WriteLine();
                writer.WriteLine(indent + "EntityIds:");
                foreach (var id in EntityIds)
                {
                    writer.WriteLine(indent + "\t" + id);
                }
            }

            if (Values != null)
            {
                writer.WriteLine(indent + "Fields:");
                var renderer = FieldRenderUtil.GetRenderer(EntityId.ObjectType);
                // use the exact if its available
                if (m_movement != null)
                {
                    renderer = FieldRenderUtil.GetRenderer(m_movement.ObjectTypeId); //
                }

                writer.IndentLevel++;
                uint size = 0;
                for (uint i = 0; i < SetIndices.Length; i++)
                {
                    var index = SetIndices[i];
                    size = renderer.Render(index, Values, writer);
                    while (size > 1 && SetIndices.Length > i + 1)
                    {
                        // check if we can skip the next indices
                        var next = SetIndices[i + 1];
                        if (next != index + 4)
                        {
                            break;
                        }

                        size--;
                        i++;
                    }
                }
                writer.IndentLevel--;
            }
            writer.WriteLine();

            writer.IndentLevel--;
        }

        /// <summary>
        /// Removes circular references
        /// </summary>
        public void Dispose()
        {
            m_movement = null;
        }

        public override string ToString()
        {
            return index + " " + Type + (Values != null ? " (Values: " + Values.Length / 4 + ")" : "");
        }
    }
}