using System;
using System.Collections.Generic;
using System.Data;
using NHibernate;
using NHibernate.Type;
using WCell.Util.Conversion;

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
            return m_Type.Get(reader, index);
        }
    }

    public class NHibernateConverterProvider : IConverterProvider
    {
        public static readonly Dictionary<Type, IConverter> StandardConverters = new Dictionary<Type, IConverter>();

        static NHibernateConverterProvider()
        {
            StandardConverters[typeof(int)] = new ToIntConverter();
            StandardConverters[typeof(uint)] = new ToUIntConverter();
            StandardConverters[typeof(string)] = new ToStringConverter();
        }

        private static Boolean IsNullableEnum(Type typeClass)
        {
            if (!typeClass.IsGenericType) return false;
            Type nullable = typeof(Nullable<>);
            if (!nullable.Equals(typeClass.GetGenericTypeDefinition())) return false;

            Type genericClass = typeClass.GetGenericArguments()[0];
            return genericClass.IsSubclassOf(typeof(Enum));
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

            if (type.IsEnum && (Enum.GetUnderlyingType(type)) == typeof(uint))
            {
                return new CustomReader(new ToUIntEnumConverter(type));
            }

            var hibernateType = TypeFactory.Basic(type.FullName);
            if (type.IsEnum)
            {
                hibernateType = NHibernateUtil.Enum(type);
            }
            else if (IsNullableEnum(type))
            {
                hibernateType = NHibernateUtil.Enum(type.GetGenericArguments()[0]);
            }

            if (hibernateType != null)
            {
                if (!(hibernateType is NullableType))
                {
                    throw new ArgumentException("Invalid Type must be nullable - Found: " + hibernateType);
                }
                return new NullableTypeReader((NullableType)hibernateType);
            }

            return null;
        }
    }
}