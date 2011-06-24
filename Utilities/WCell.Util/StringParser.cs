using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Strings;

namespace WCell.Util
{
	public class StringParser
	{
		static StringParser()
		{
			// init all operators

			// "|" is an escape character for the WoW client, they always get doubled
			IntOperators["||"] = BinaryOrHandler;

			IntOperators["|"] = BinaryOrHandler;
			IntOperators["^"] = BinaryXOrHandler;
			IntOperators["&"] = BinaryAndHandler;
			IntOperators["+"] = PlusHandler;
			IntOperators["-"] = MinusHandler;
			IntOperators["*"] = DivideHandler;
			IntOperators["/"] = MultiHandler;
		}


		public static Dictionary<Type, Func<string, object>> TypeParsers =
			new Func<Dictionary<Type, Func<string, object>>>(() =>
			{
				var parsers =
					new Dictionary<Type, Func<string, object>>();

				parsers.Add(typeof(int),
							strVal => int.Parse(strVal));

				parsers.Add(typeof(float),
							strVal => float.Parse(strVal));

				parsers.Add(typeof(long),
							strVal => long.Parse(strVal));

				parsers.Add(typeof(ulong),
							strVal => ulong.Parse(strVal));

				parsers.Add(typeof(bool),
							strVal =>
							strVal.Equals("true",
										  StringComparison.
											  InvariantCultureIgnoreCase) ||
							strVal.Equals("1",
										  StringComparison.
											  InvariantCultureIgnoreCase) ||
							strVal.Equals("yes",
										  StringComparison.
											  InvariantCultureIgnoreCase));

				parsers.Add(typeof(double),
							strVal => double.Parse(strVal));

				parsers.Add(typeof(uint),
							strVal => uint.Parse(strVal));

				parsers.Add(typeof(short),
							strVal => short.Parse(strVal));

				parsers.Add(typeof(ushort),
							strVal => short.Parse(strVal));

				parsers.Add(typeof(byte),
							strVal => byte.Parse(strVal));

				parsers.Add(typeof(char), strVal => strVal[0]);

				return parsers;
			})();

		public static object Parse(string stringVal, Type type)
		{
			object obj = null;
			if (!Parse(stringVal, type, ref obj))
			{
				throw new Exception(string.Format("Unable to parse string-Value \"{0}\" as Type \"{1}\"", stringVal,
												  type));
			}
			return obj;
		}

		public static bool Parse(string str, Type type, ref object obj)
		{
			if (type.IsArray)
			{
				var reader = new StringStream(str);
				// iterate over string twice:
				// first get the count
				var count = 0;
				while (reader.HasNext)
				{
					string value;
					count++;
					if (!reader.NextString(out value))
					{
						// no more terminator
						break;
					}
				}

				// start again
				reader.Position = 0;

				// then fill the array
				var elemType = type.GetElementType();
				var arr = Array.CreateInstance(elemType, count);
				for (var i = 0; i < count; i++)
				{
					object value = null;
					string strVal;
					reader.NextString(out strVal);

					if (ParseSingleValue(strVal, elemType, ref value))
					{
						arr.SetValue(value, i);
					}
					else
					{
						return false;
					}
				}
				obj = arr;
				return true;
			}
			else
			{
				return ParseSingleValue(str, type, ref obj);
			}
		}

		public static bool ParseSingleValue(string str, Type type, ref object obj)
		{
			if (type == typeof(string))
			{
				obj = str;
			}
			else if (type.IsEnum)
			{
				try
				{
					obj = Enum.Parse(type, str, true);
				}
				catch
				{
					return false;
				}
			}
			else
			{
				Func<string, object> parser;
				if (TypeParsers.TryGetValue(type, out parser))
				{
					try
					{
						obj = parser(str);
						return obj != null;
					}
					catch
					{
						return false;
					}
				}
				return false;
			}
			return true;
		}

		#region Evaluate etc

		public delegate T OperatorHandler<T>(T x, T y);

		public static readonly OperatorHandler<long> BinaryOrHandler =
			(x, y) => x | y;

		public static readonly OperatorHandler<long> BinaryXOrHandler =
			(x, y) => x & ~y;

		public static readonly OperatorHandler<long> BinaryAndHandler =
			(x, y) => x & y;

		public static readonly OperatorHandler<long> PlusHandler =
			(x, y) => x + y;

		public static readonly OperatorHandler<long> MinusHandler =
			(x, y) => x - y;

		public static readonly OperatorHandler<long> DivideHandler =
			(x, y) => x / y;

		public static readonly OperatorHandler<long> MultiHandler =
			(x, y) => x * y;

		public static readonly Dictionary<string, OperatorHandler<long>> IntOperators =
			new Dictionary<string, OperatorHandler<long>>();

		/// <summary>
		/// Evaluates the given (simple) expression
		/// 
		/// TODO: Use Polish Notation to allow more efficiency and complexity
		/// TODO: Add operator priority
		/// </summary>
		public static bool Eval(Type valType, ref long val, string expr, ref object error, bool startsWithOperator)
		{
			// syntax: <val> <op> <value> [<op> <value> [<op> <value>...]]
			var args = expr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var isOp = startsWithOperator;
			OperatorHandler<long> op = null;
			foreach (var argument in args)
			{
				var arg = argument.Trim();
				if (isOp)
				{
					if (!IntOperators.TryGetValue(arg, out op))
					{
						error = "Invalid operator: " + arg;
						return false;
					}
				}
				else
				{
					object argVal = null;
					if (!Parse(arg, valType, ref argVal))
					{
						error = "Could not convert value \"" + arg + "\" to Type \"" + valType + "\"";
						return false;
					}

					var longVal = (long)Convert.ChangeType(argVal, typeof(long));
					if (op != null)
					{
						val = op(val, longVal);
					}
					else
					{
						val = longVal;
					}
				}
				isOp = !isOp;
			}
			return true;
		}

		#endregion
	}
}
