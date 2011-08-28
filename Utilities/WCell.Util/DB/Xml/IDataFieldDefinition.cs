namespace WCell.Util.DB.Xml
{
	public interface IDataFieldDefinition
	{
		string Name
		{
			get;
			set;
		}

		DataFieldType DataFieldType
		{
			get;
		}
	}
}