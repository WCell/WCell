/*************************************************************************
 *
 *   file		: DB2Reader.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 *   last author	: $LastChangedBy: pepsi1x1 $
 *   revision		: $Rev: 1151 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using WCell.Constants;

namespace WCell.Core.ClientDB
{

    public class MappedDB2Reader<TEntry, TConverter> : DB2Reader<TConverter>
        where TConverter : AdvancedClientDBRecordConverter<TEntry>, new()
    {
        public Dictionary<int, TEntry> Entries;

        public MappedDB2Reader(string fileName)
            : base(fileName)
        {
        }

        public TEntry this[int id]
        {
            get
            {
                TEntry entry;
                Entries.TryGetValue(id, out entry);
                return entry;
                //return default(TEntry);
            }
        }

        public TEntry this[uint id]
        {
            get { return this[(int)id]; }
        }

        protected override void Convert(byte[] bytes)
        {
            var id = currentIndex;
            var entry = ((AdvancedClientDBRecordConverter<TEntry>)converter).ConvertTo(bytes, ref id);

            Entries.Add(id, entry);
        }

        protected override void InitReader()
        {
            Entries = new Dictionary<int, TEntry>(m_recordCount);
        }
    }

    public class ListDB2Reader<TEntry, TConverter> : DB2Reader<TConverter>
        where TConverter : AdvancedClientDBRecordConverter<TEntry>, new()
    {
        public List<TEntry> EntryList;

        public ListDB2Reader(string fileName)
            : base(fileName)
        {
        }

        protected override void Convert(byte[] bytes)
        {
            var id = currentIndex;
            var entry = ((AdvancedClientDBRecordConverter<TEntry>)converter).ConvertTo(bytes, ref id);

            EntryList.Add(entry);
        }

        protected override void InitReader()
        {
            EntryList = new List<TEntry>(m_recordCount);
        }
    }

    public class DB2Reader<TConverter>
        where TConverter : ClientDBRecordConverter, new()
    {
        public const int DB2Header = 0x32424457;    // WDB2

        public static void ReadDBC(string fileName)
        {
            new DBCReader<TConverter>(fileName);
        }

        protected readonly int m_recordSize;
        protected readonly int m_recordCount;
        protected readonly int m_fieldCount;
        protected readonly string m_fileName;
        protected ClientDBRecordConverter converter;
        protected int currentIndex;

        public DB2Reader(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("The required DBC file \"" + fileName + "\" was not found.");
            }

            m_fileName = fileName;

            using (var fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.Read))
            {
                using (var binReader = new BinaryReader(fileStream))
                {
                    var header = binReader.ReadUInt32();
                    if (header != DB2Header)
                        throw new InvalidDataException("Not a (W)DB2 file.");

                    m_recordCount = binReader.ReadInt32();
                    m_fieldCount = binReader.ReadInt32();
                    m_recordSize = binReader.ReadInt32();
                    var stringTableSize = binReader.ReadInt32();

                    var tableHash = binReader.ReadUInt32();
                    var build = binReader.ReadUInt32();
                    var lastUpdated = binReader.ReadUInt32();
                    var minId = binReader.ReadInt32();
                    var maxId = binReader.ReadInt32();
                    var locale = binReader.ReadInt32();
                    var unk = binReader.ReadInt32();

                    if (maxId != 0)
                    {
                        var diff = maxId - minId + 1; // blizzard is weird people...
                        binReader.ReadBytes(diff*4); // an index for rows
                        binReader.ReadBytes(diff*2); // a memory allocation bank
                    }

                    var recordStart = binReader.BaseStream.Position;
                    binReader.BaseStream.Position += RecordSize * RecordCount;

                    var stringTable = binReader.ReadBytes(stringTableSize);

                    using (converter = new TConverter())
                    {
                        converter.Init(stringTable);
                        InitReader();
                        MapRecords(binReader, recordStart);
                    }
                }
            }
        }

        public int RecordSize
        {
            get { return m_recordSize; }
        }

        public int RecordCount
        {
            get { return m_recordCount; }
        }

        public int FieldCount
        {
            get { return m_fieldCount; }
        }

        public string FileName
        {
            get { return m_fileName; }
        }

        protected virtual void InitReader()
        {
        }

        private void MapRecords(BinaryReader binReader, long recordStart)
        {
            try
            {
                binReader.BaseStream.Position = recordStart;

                for (currentIndex = 0; currentIndex < m_recordCount; currentIndex++)
                {
                    Convert(binReader.ReadBytes(m_recordSize));
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error when reading DB2-file \"" + m_fileName + "\" (Required client version: " + WCellInfo.RequiredVersion + ")", e);
            }
        }

        protected virtual void Convert(byte[] bytes)
        {
            converter.Convert(bytes);
        }
    }
}