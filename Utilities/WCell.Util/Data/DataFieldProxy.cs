namespace WCell.Util.Data
{
	public class DataFieldProxy : IFlatDataFieldAccessor
	{
		private readonly string m_FieldName;
		private readonly DataHolderDefinition m_DataHolderDef;

		private IDataField m_field;

		public DataFieldProxy(string fieldName, DataHolderDefinition dataHolderDef)
		{
			m_FieldName = fieldName;
			m_DataHolderDef = dataHolderDef;
		}

		public string FieldName
		{
			get { return m_FieldName; }
		}

		public IDataField Field
		{
			get
			{
				if (m_field == null)
				{
					m_field = m_DataHolderDef.GetField(m_FieldName);
				}
				return m_field;
			}
		}

		public DataHolderDefinition DataHolderDefinition
		{
			get { return m_DataHolderDef; }
		}

		public object Get(IDataHolder obj)
		{
			return Field.Accessor.Get(obj);
		}

		public void Set(IDataHolder obj, object value)
		{
			//throw new DataHolderException("Proxy field is not settable.");
		}

		public override string ToString()
		{
			return "Proxy for: " + m_DataHolderDef;
		}
	}
}