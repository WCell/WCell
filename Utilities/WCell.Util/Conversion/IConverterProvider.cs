using System;

namespace WCell.Util.Conversion
{
	public interface IConverterProvider
	{
		IFieldReader GetReader(Type type);
	}
}