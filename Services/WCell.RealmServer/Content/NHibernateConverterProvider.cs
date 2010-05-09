using System;
using System.Reflection;
using NHibernate;
using NHibernate.Type;
using WCell.Util.Conversion;
using WCell.Util.Data;
using System.Data;
using System.Collections.Generic;

namespace WCell.RealmServer.Content
{
	public class NullableTypeReader : IFieldReader
	{
		private readonly NullableType m_Type;

		public NullableTypeReader(NullableType type)
		{
			m_Type = type;
		}

		public NullableType Type
		{
			get { return m_Type; }
		}

		public object Read(IDataReader reader, int index)
		{
			return m_Type.NullSafeGet(reader, index);
		}
	}

	public class NHibernateConverterProvider : IConverterProvider
	{
		public static readonly Dictionary<Type, IConverter> StandardConverters = new Dictionary<Type, IConverter>();

		static NHibernateConverterProvider()
		{
			StandardConverters[typeof(uint)] = new ToUIntConverter();
			StandardConverters[typeof(string)] = new ToStringConverter();
		}

		public IConverter GetStandardConverter(Type type)
		{
			IConverter conv;
			StandardConverters.TryGetValue(type, out conv);
			return conv;
		}

		public IFieldReader GetReader(Type type)
		{
			var stdConv = GetStandardConverter(type);
			if (stdConv != null)
			{
				return new CustomReader(stdConv);
			}

			if (type.IsEnum && (Enum.GetUnderlyingType(type)) == typeof (uint))
			{
				return new CustomReader(new ToUIntEnumConverter(type));
			}

			var hibernateType = TypeFactory.Basic(type.FullName);
			if (type.IsEnum)
			{
				hibernateType = NHibernateUtil.Enum(type);
			}
			else if (TypeFactory.IsNullableEnum(type))
			{
				hibernateType = NHibernateUtil.Enum(type.GetGenericArguments()[0]);
			}

			if (hibernateType != null)
			{
				if (!(hibernateType is NullableType))
				{
					throw new ArgumentException("Invalid Type must be nullable - Found: " + hibernateType);
				}
				return new NullableTypeReader((NullableType) hibernateType);
			}

			return null;
		}
	}
}