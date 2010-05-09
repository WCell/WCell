/*************************************************************************
 *
 *   file		: WDBReader.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-01-31 12:35:36 +0100 (to, 31 jan 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 87 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace WCell.Core.WDB
{
    public interface IWDBEntry
    {
        int EntryId { get; }
    }

    public class WDBReader<TEntry, TConverter>
        where TEntry : IWDBEntry
        where TConverter : WDBRecordConverter<TEntry>, new()
    {
        private string m_fileName;
        private readonly uint m_magic;
        private readonly uint m_build;
        private readonly uint m_locale;
        private readonly uint m_unkHeader1;
        private readonly uint m_unkHeader2;
        private readonly List<TEntry> m_list;

        public WDBReader(string fileName)
        {
            m_fileName = fileName;
            m_list = new List<TEntry>();

            using (FileStream fileStream = new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader binReader = new BinaryReader(fileStream))
                {
                    m_magic = binReader.ReadUInt32();
                    m_build = binReader.ReadUInt32();
                    m_locale = binReader.ReadUInt32();
                    m_unkHeader1 = binReader.ReadUInt32();
                    m_unkHeader2 = binReader.ReadUInt32();

                    //T record;
                    TConverter converter = new TConverter();
                    // 20 for Header, 8 0x00 Bytes at the end
                    while (binReader.BaseStream.Position < (binReader.BaseStream.Length - 20 - 8))
                    {
                        TEntry entry = converter.Convert(binReader);
                        m_list.Add(entry);
                    }
                }
            }
        }

        public uint Build
        {
            get { return m_build; }
        }

        public List<TEntry> Entries
        {
            get { return m_list; }
        }

        public void Sort()
        {
            // Returns:
            //     Value Condition Less than 0 x is less than y.0 x equals y.Greater than 0
            //     x is greater than y.
            m_list.Sort(delegate(TEntry x, TEntry y)
            {
                if (x.EntryId < y.EntryId)
                    return -1;
                if (x.EntryId > y.EntryId)
                    return 1;
                return 0;
            });
        }
    }
}
