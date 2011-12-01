namespace WCell.MPQTool.DBC
{
	public delegate int CellReader(DBCReader reader, byte[] bytes, uint index, out object value);
}