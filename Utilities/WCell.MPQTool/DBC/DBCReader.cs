using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WCell.Util;

namespace WCell.MPQTool.DBC
{
    public class DBCReader
    {
        public static CellReader[] Readers;

        static DBCReader()
        {
            Readers = new CellReader[(int)Utility.GetMaxEnum<CellType>() + 1];
            Readers[(int)CellType.Byte] = delegate(DBCReader reader, byte[] bytes, uint index, out object value)
            {
                value = bytes[index];
                return 1;
            };

            Readers[(int)CellType.Float] = delegate(DBCReader reader, byte[] bytes, uint index, out object value)
            {
                value = bytes.GetFloat(index);
                return 4;
            };

            Readers[(int)CellType.Int] = delegate(DBCReader reader, byte[] bytes, uint index, out object value)
            {
                value = bytes.GetInt32(index);
                return 4;
            };

            Readers[(int)CellType.UInt] = delegate(DBCReader reader, byte[] bytes, uint index, out object value)
            {
                value = bytes.GetUInt32(index);
                return 4;
            };

            Readers[(int)CellType.SByte] = delegate(DBCReader reader, byte[] bytes, uint index, out object value)
            {
                value = (sbyte)bytes[index];
                return 1;
            };

            Readers[(int)CellType.String] = delegate(DBCReader reader, byte[] bytes, uint index, out object value)
            {
                var offset = bytes.GetUInt32(index);
                value = reader.GetString(offset);
                return 4;
            };
        }

        public const uint DBCFormatID = 0x43424457;

        private readonly int m_recordSize;
        private readonly int m_recordCount;
        private readonly int m_fieldCount;
        private readonly string m_fileName;
        private byte[] stringTable;
        string[] strings;
        byte[][] rows;

        public DBCReader(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("The required DBC file was not found: {0}", fileName);
            }

            m_fileName = fileName;

            FileStream fileStream = new FileStream(m_fileName, FileMode.Open);

            if (fileStream.Length < 4)
            {
                return;
            }

            var binReader = new BinaryReader(fileStream);
            if (binReader.ReadUInt32() != DBCFormatID)
            {// WDBC
                binReader.Close();
                throw new FormatException("Invalid file format: " + fileName + " does not contain DBC information.");
            }

            m_recordCount = binReader.ReadInt32();
            m_fieldCount = binReader.ReadInt32();
            m_recordSize = binReader.ReadInt32();
            int stringTableSize = binReader.ReadInt32();

            binReader.BaseStream.Position = binReader.BaseStream.Length - stringTableSize;

            stringTable = binReader.ReadBytes(stringTableSize);
            strings = Encoding.UTF8.GetString(stringTable).Split(new[] { '\0' }, StringSplitOptions.None);

            binReader.BaseStream.Position = 20;

            rows = new byte[m_recordCount][];
            for (int i = 0; i < m_recordCount; i++)
            {
                rows[i] = binReader.ReadBytes(m_recordSize);
            }
            binReader.Close();
        }

        /// <summary>
        /// Gets the percentage of rows that might have strings in the given col.
        /// Returns 0 if there isnt at least <code>uniquePct</code> percent of unique string-indeces in this column.
        /// </summary>
        public float GetStringMatchPct(uint col, float uniquePct)
        {
            var foundIndeces = new HashSet<uint>();

            var maxIdenticalCount = (int)((m_recordCount / 100f) * (100f - uniquePct));

            var identicalCount = 0;

            int matches = 0;
            foreach (var row in rows)
            {
                var index = row[col];
                if (index != 0)
                {
                    if (!foundIndeces.Contains(index))
                    {
                        foundIndeces.Add(index);
                    }
                    else
                    {
                        //identicalCount++;
                    }

                    var chr = stringTable.Get(index);
                    if (chr != 0)
                    {
                        matches++;
                    }
                }
                else
                {
                    identicalCount++;
                }

                if (identicalCount >= maxIdenticalCount)
                {
                    return 0.0f;
                }
            }

            return (matches / (float)m_recordCount) * 100f;
        }

        /// <summary>
        /// Irregular means that we don't only have 4 byte fields (its only a guess)
        /// </summary>
        public bool IrregularColumnSize
        {
            get
            {
                return m_fieldCount != m_recordSize / 4;
            }
        }

        /// <summary>
        /// Size of each record
        /// </summary>
        public int RecordSize
        {
            get { return m_recordSize; }
        }

        /// <summary>
        /// Amount of existing records
        /// </summary>
        public int RecordCount
        {
            get { return m_recordCount; }
        }

        /// <summary>
        /// Amount of columns
        /// </summary>
        public int ColumnCount
        {
            get { return m_fieldCount; }
        }

        /// <summary>
        /// The name of the underlying file
        /// </summary>
        public string FileName
        {
            get { return m_fileName; }
        }

        public byte[] GetRow(uint rowNum)
        {
            return rows[rowNum];
        }

        public T GetValue<T>(uint col, uint row, CellType type)
            where T : class
        {
            object value;
            Readers[(int)type](this, rows[row], col, out value);
            return (T)value;
        }

        public string GetString(uint offset)
        {
            StringBuilder builder = new StringBuilder();
            byte b;
            while ((b = stringTable[offset++]) != 0)
            {
                builder.Append((char)b);
            }
            return builder.ToString();
        }

        public bool HasStrings
        {
            get
            {
                return stringTable.Length > 0;
            }
        }
    }
}