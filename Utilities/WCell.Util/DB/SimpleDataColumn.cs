using System;
using System.Collections.Generic;
using System.Data;
using WCell.Util.Conversion;
using WCell.Util.Data;

namespace WCell.Util.DB
{
	/// <summary>
	/// Maps a table-column to a DataField
	/// </summary>
	public class SimpleDataColumn : BaseDataColumn
	{
		private object m_DefaultValue;
		//private readonly int m_Index;
		//private NestedDataColumn m_parent;
		internal readonly List<IFlatDataFieldAccessor> FieldList = new List<IFlatDataFieldAccessor>();
		private bool m_IsPrimaryKey;
		private IFieldReader m_reader;
		internal int m_index;

		public SimpleDataColumn(string column, object defaultValue)
			: base(column)
		{
			m_DefaultValue = defaultValue;
		}

		public SimpleDataColumn(string column, IFieldReader reader, int index)
			: base(column)
		{
			m_reader = reader;
			m_index = index;
		}

		public SimpleDataColumn(string column, IFieldReader reader)
			: base(column)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			m_reader = reader;
		}

		public object DefaultValue
		{
			get { return m_DefaultValue; }
			set { m_DefaultValue = value; }
		}

		/// <summary>
		/// The index of this column within the query-result
		/// </summary>
		public int Index
		{
			get { return m_index; }
		}

		public IFieldReader Reader
		{
			get { return m_reader; }
			internal set { m_reader = value; }
		}

		/// <summary>
		/// An empty DataColumn has no reader and thus is not necessarily mapped.
		/// </summary>
		public bool IsEmpty
		{
			get { return m_reader == null; }
		}

		public IFlatDataFieldAccessor[] Fields
		{
			get { return FieldList.ToArray(); }
		}

		public bool IsPrimaryKey
		{
			get { return m_IsPrimaryKey; }
			internal set { m_IsPrimaryKey = value; }
		}

		public void SetSingleValue(object value, IDataHolder holder)
		{
			for (var i = 0; i < FieldList.Count; i++)
			{
				var dataField = FieldList[i];
				// set value
				dataField.Set(holder, value);
			}
		}

		public override string ToString()
		{
			return m_ColumnName + " (" + m_index + ")" + (m_IsPrimaryKey ? " (PrimaryKey)" : "");
		}

		public object ReadValue(IDataReader rs)
		{
			if (m_DefaultValue != null)
			{
				return m_DefaultValue;
			}
			return Reader.Read(rs, m_index);
		}
	}
}