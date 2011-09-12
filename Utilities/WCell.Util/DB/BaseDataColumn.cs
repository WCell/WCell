namespace WCell.Util.DB
{
	public abstract class BaseDataColumn
	{
		protected readonly string m_ColumnName;
		//private NestedDataColumn m_parent;

		//public BaseDataColumn(NestedDataColumn parent)
		//{
		//    m_parent = parent;
		//}

		//public NestedDataColumn Parent
		//{
		//    get { return m_parent; }
		//}

		//public int Index
		//{
		//    get { return m_Index; }
		//}

		protected BaseDataColumn(string column)
		{
			m_ColumnName = column;
		}

		/// <summary>
		/// The name of the Column
		/// </summary>
		public string ColumnName
		{
			get
			{
				return m_ColumnName;
			}
		}
	}
}