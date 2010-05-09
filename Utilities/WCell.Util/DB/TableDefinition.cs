using System.Collections.Generic;
using System.Data;
using System.Linq;
using WCell.Util.DB.Xml;
using WCell.Util.Data;
using WCell.Util.Variables;

namespace WCell.Util.DB
{
	/// <summary>
	/// A table definition has an array of Columns and an array of DataFields whose indices
	/// correspond to each other.
	/// </summary>
	public class TableDefinition
	{
		public delegate object GetIdHandler(IDataReader reader);

		public readonly Dictionary<string, ArrayConstraint> ArrayConstraints;

		private string m_Name;
		private string[] m_allColumns;
		private SimpleDataColumn[] m_ColumnDefinitions;
		private DataHolderDefinition m_mainDataHolder;
		private DataHolderDefinition[] m_dataHolderDefinitions;
		private PrimaryColumn[] m_primaryColumns;
		private bool m_singlePrimaryCol;
		private bool m_isDefaultTable;

		/// <summary>
		/// The handler that returns the Id (or compound Id) for each row, read from the DB.
		/// </summary>
		public GetIdHandler GetId;

		public VariableDefinition[] Variables;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="primaryColumns"></param>
		/// <param name="arrayConstraints"></param>
		public TableDefinition(string name, PrimaryColumn[] primaryColumns,
			Dictionary<string, ArrayConstraint> arrayConstraints,
			VariableDefinition[] variables)
		{
			m_Name = name;
			m_primaryColumns = primaryColumns;
			ArrayConstraints = arrayConstraints;
			GetId = GetPrimaryId;
			m_singlePrimaryCol = m_primaryColumns.Count() == 1;
			Variables = variables;
		}

		/// <summary>
		/// Whether this is a DefaultTable of its <see cref="MainDataHolder"/>.
		/// DefaultTables are the tables that contain the core data of each DataHolder.
		/// It is ensured that a DataHolder is only valid if it exists in all its DefaultTables.
		/// </summary>
		public bool IsDefaultTable
		{
			get { return m_isDefaultTable; }
		}

		internal string MainDataHolderName
		{
			get;
			set;
		}


		/// <summary>
		/// The DataHolder to which this table primarily belongs.
		/// It is used for variables and undefined key-references.
		/// </summary>
		public DataHolderDefinition MainDataHolder
		{
			get { return m_mainDataHolder; }
		}

		public string[] AllColumns
		{
			get
			{
				if (m_allColumns == null)
				{
					var cols = new List<string>();
					for (var i = 0; i < m_ColumnDefinitions.Length; i++)
					{
						if (!m_ColumnDefinitions[i].IsEmpty)
						{
							cols.Add(m_ColumnDefinitions[i].ColumnName);
						}
					}
					m_allColumns = cols.ToArray();
				}
				return m_allColumns;
			}
		}

		public string Name
		{
			get { return m_Name; }
			internal set { m_Name = value; }
		}

		public PrimaryColumn[] PrimaryColumns
		{
			get { return m_primaryColumns; }
		}

		public DataHolderDefinition[] DataHolderDefinitions
		{
			get { return m_dataHolderDefinitions; }
			internal set { m_dataHolderDefinitions = value; }
		}

		/// <summary>
		/// Set of columns and corresponding DataFields.
		/// Array must not be empty and the PrimaryKey must always be the first column.
		/// </summary>
		public SimpleDataColumn[] ColumnDefinitions
		{
			get { return m_ColumnDefinitions; }
			internal set
			{
				m_ColumnDefinitions = value;
				m_allColumns = null;
				//if (!m_Fields[0].IsPrimaryKey)
				//{
				//    throw new DataHolderException("The first column of Table \"" + m_Name + "\" must be the PrimaryKey.");
				//}
			}
		}

		public override string ToString()
		{
			return Name;
		}

		public object GetPrimaryId(IDataReader rs)
		{
			if (m_singlePrimaryCol)
			{
				var col = m_primaryColumns[0].DataColumn;
				return col.ReadValue(rs);
			}
			else
			{
				var count = m_primaryColumns.Count();
				var ids = new object[count];
				SimpleDataColumn col;
				for (var i = 0; i < count; i++)
				{
					col = m_primaryColumns[i].DataColumn;
					ids[i] = col.ReadValue(rs);
				}
				return ids;
			}
		}

		// TODO: Fix up default values
		internal void SetDefaults(object id, IDataHolder holder)
		{
			if (id is object[])
			{
				var ids = (object[])id;
				for (var i = 0; i < PrimaryColumns.Length; i++)
				{
					var col = PrimaryColumns[i].DataColumn;
					col.SetSingleValue(ids[i], holder);
				}
			}
			else
			{
				PrimaryColumns[0].DataColumn.SetSingleValue(id, holder);
			}
		}

		internal void SetMainDataHolder(DataHolderDefinition dataDef, bool isDefaultTable)
		{
			if (MainDataHolder != null)
			{
				return;
				//throw new DataHolderException("Table was set as DefaultTable for more than one DataHolder: \"" + dataDef + "\" and \"" + MainDataHolder + "\"");
			}
			m_mainDataHolder = dataDef;
			m_isDefaultTable = isDefaultTable;
		}
	}
}