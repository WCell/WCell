using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WCell.Util.DB;
using System.Reflection;

namespace WCell.Util.Data
{
	public class BinaryContentStream
	{
		private readonly DataHolderDefinition m_Def;
		private IBinaryPersistor[] m_persistors;
		private IDataField[] m_fields;

		public BinaryContentStream(DataHolderDefinition def)
		{
			m_Def = def;
			InitPersistors();
		}

		private void InitPersistors()
		{
			m_fields = new IDataField[m_Def.Fields.Values.Count];
			m_persistors = new IBinaryPersistor[m_fields.Length];
			var i = 0;
			if (m_Def.DependingField != null)
			{
				// depending field comes first
				m_persistors[0] = BinaryPersistors.GetPersistor(m_Def.DependingField);
				m_fields[0] = m_Def.DependingField;
				i++;
			}

			foreach (var field in m_Def.Fields.Values)
			{
				if (field == m_Def.DependingField)
				{
					continue;
				}
				var persistor = BinaryPersistors.GetPersistor(field);
				m_persistors[i] = persistor;
				m_fields[i] = field;
				i++;
			}
		}

		public void WriteAll(string filename, IEnumerable holders)
		{
			var writer = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write));
			WriteAll(writer, holders);
		}

		public void WriteAll(BinaryWriter writer, IEnumerable holders)
		{
			var initPos = writer.BaseStream.Position;
			writer.BaseStream.Position += 4;

			var count = 0;
			foreach (var holder in holders)
			{
				if (holder != null)
				{
					count++;
					Write(writer, (IDataHolder)holder);
				}
			}

			writer.BaseStream.Position = initPos;
			writer.Write(count);
		}

		void Write(BinaryWriter writer, IDataHolder holder)
		{
			for (var i = 0; i < m_persistors.Length; i++)
			{
				var persistor = m_persistors[i];
				try
				{
					var val = m_fields[i].Accessor.Get(holder);
					persistor.Write(writer, val);
				}
				catch (Exception e)
				{
					throw new DataHolderException(e, "Failed to write DataHolder \"{0}\" (Persistor #{1} {2} for: {3}).",
						holder, i, persistor, m_fields[i]);
				}
			}
		}

		internal void LoadAll(BinaryReader reader, List<Action> initors)
		{
			var recordCount = reader.ReadInt32();
			for (var i = 0; i < recordCount; i++)
			{
				var holder = Read(reader);
				initors.Add(holder.FinalizeDataHolder);
			}
		}

		public IDataHolder Read(BinaryReader reader)
		{
			var firstVal = m_persistors[0].Read(reader);
			var holder = (IDataHolder)m_Def.CreateHolder(firstVal);
			m_fields[0].Accessor.Set(holder, firstVal);

			for (var i = 1; i < m_persistors.Length; i++)
			{
				var persistor = m_persistors[i];
				try
				{
					var val = persistor.Read(reader);
					m_fields[i].Accessor.Set(holder, val);
				}
				catch (Exception e)
				{
					throw new DataHolderException(e, "Failed to read DataHolder \"{0}\" (Persistor #{1} {2} for: {3}).",
						holder, i, persistor, m_fields[i]);
				}
			}
			return holder;
		}
	}
}
