using System;

namespace WCell.Util.Conversion
{
	public class ToUIntConverter : IConverter
	{
	    public virtual object Convert(object input)
		{
            if (input is DBNull)
            {
                return (uint)0;
            }
			if (input is uint)
			{
				return input;
			}
			if (input is int)
			{
				return (uint)(int)input;
			}
			if (input is sbyte)
			{
				return (uint)(sbyte)input;
			}
			if (input is short)
			{
				return (uint)(short)input;
			}
			if (input is long)
			{
				return (uint)(long)input;
			}
			if (input is byte)
			{
				return (uint)(byte)input;
			}
			if (input is ushort)
			{
				return (uint)(ushort)input;
			}
			if (input is ulong)
			{
				return (uint)(ulong)input;
			}
			if (input is string)
			{
				long longVal;
				if (long.TryParse((string)input, out longVal))
				{
					return (uint)longVal;
				}
			}
			throw new Exception("Could not convert value to UInt: " + input);
		}
	}

	public class ToUIntEnumConverter : ToUIntConverter
	{
		private readonly Type m_EnumType;

		public ToUIntEnumConverter(Type enumType)
		{
			m_EnumType = enumType;
		}

		public Type EnumType
		{
			get { return m_EnumType; }
		}

		public override object Convert(object input)
		{
			var num = (uint)base.Convert(input);
			
			//return System.Convert.ChangeType(num, m_EnumType);
			//return Enum.Parse();
			return num;
		}
	}

	public class ToStringConverter : IConverter
	{
		public object Convert(object input)
		{
			if (input == null)
			{
				return "";
			}
			return input.ToString();
		}
	}
}