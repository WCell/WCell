namespace WCell.Util.DB.Xml
{
	/// <summary>
	/// Indicates that all parts of this field are in the table with the given Name.
	/// </summary>
	public interface IFlatField : IDataFieldDefinition
	{
		string Table { get; }

		//IEnumerable<string> GetColumns();

		//void AddColumns(List<string> columnList);
	}
}