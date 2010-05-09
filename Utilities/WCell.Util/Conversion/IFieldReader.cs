using System.Data;

namespace WCell.Util.Conversion
{
	public interface IFieldReader
	{
		object Read(IDataReader reader, int index);
	}

	public class CustomReader : IFieldReader
	{
		private readonly IConverter m_Converter;

		public CustomReader(IConverter converter)
		{
			m_Converter = converter;
		}

		public IConverter Converter
		{
			get { return m_Converter; }
		}

		public object Read(IDataReader reader, int index)
		{
			return m_Converter.Convert(reader[index]);
		}
	}
}