/*************************************************************************
 *
 *   file		: DBCReader.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-23 20:07:17 +0100 (on, 23 dec 2009) $
 
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

namespace WCell.Core.DBC
{

	public class MappedDBCReader<TEntry, TConverter> : DBCReader<TConverter>
		where TConverter : AdvancedDBCRecordConverter<TEntry>, new()
	{
		public Dictionary<int, TEntry> Entries;

		public MappedDBCReader(string fileName)
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
			var entry = ((AdvancedDBCRecordConverter<TEntry>)converter).ConvertTo(bytes, ref id);

			Entries.Add(id, entry);
		}

		protected override void InitReader()
		{
			Entries = new Dictionary<int, TEntry>(m_recordCount);
		}
	}

	public class ListDBCReader<TEntry, TConverter> : DBCReader<TConverter>
		where TConverter : AdvancedDBCRecordConverter<TEntry>, new()
	{
		public List<TEntry> EntryList;

		public ListDBCReader(string fileName)
			: base(fileName)
		{
		}

		protected override void Convert(byte[] bytes)
		{
			var id = currentIndex;
			var entry = ((AdvancedDBCRecordConverter<TEntry>)converter).ConvertTo(bytes, ref id);

			EntryList.Add(entry);
		}

		protected override void InitReader()
		{
			EntryList = new List<TEntry>(m_recordCount);
		}
	}

	public class DBCReader<TConverter>
        where TConverter : DBCRecordConverter, new()
    {
		public const int DBCHeader = 0x43424457;	// WDBC

		public static void ReadDBC(string fileName)
		{
			new DBCReader<TConverter>(fileName);
		}

		protected readonly int m_recordSize;
        protected readonly int m_recordCount;
		protected readonly int m_fieldCount;
		protected readonly string m_fileName;
		protected DBCRecordConverter converter;
		protected int currentIndex;

		public DBCReader(string fileName)
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
                    if (binReader.ReadUInt32() != DBCHeader)
                        throw new InvalidDataException("Not a (W)DBC file.");

                    m_recordCount = binReader.ReadInt32();
                    m_fieldCount = binReader.ReadInt32();
                    m_recordSize = binReader.ReadInt32();
                    var stringTableSize = binReader.ReadInt32();

                    binReader.BaseStream.Position = binReader.BaseStream.Length - stringTableSize;

                    var stringTable = binReader.ReadBytes(stringTableSize);

                    using (converter = new TConverter())
					{
						converter.Init(stringTable);
                    	InitReader();
                        MapRecords(binReader);
                    }
                }
            }
		}

		public int RecordSize
		{
			get { return m_recordSize; }
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

		private void MapRecords(BinaryReader binReader)
        {
			try
			{
				binReader.BaseStream.Position = 20;

				for (currentIndex = 0; currentIndex < m_recordCount; currentIndex++)
				{
					Convert(binReader.ReadBytes(m_recordSize));
				}
			}
			catch (Exception e)
			{
				throw new Exception("Error when reading DBC-file \"" + m_fileName + "\" (Required client version: " + WCellInfo.RequiredVersion + ")", e);
			}
		}

		protected virtual void Convert(byte[] bytes)
		{
			converter.Convert(bytes);
		}
    }
}