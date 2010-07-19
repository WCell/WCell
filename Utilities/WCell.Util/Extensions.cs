using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace WCell.Util
{
	public static class Extensions
	{
		public static List<V> GetOrCreate<K, V>(this IDictionary<K, List<V>> map, K key)
		{
			List<V> list;
			if (!map.TryGetValue(key, out list))
			{
				map.Add(key, list = new List<V>());
			}
			return list;
		}

		public static HashSet<V> GetOrCreate<K, V>(this IDictionary<K, HashSet<V>> map, K key)
		{
			HashSet<V> dict;
			if (!map.TryGetValue(key, out dict))
			{
				map.Add(key, dict = new HashSet<V>());
			}
			return dict;
		}

		public static IDictionary<K2, V> GetOrCreate<K, K2, V>(this IDictionary<K, IDictionary<K2, V>> map, K key)
		{
			IDictionary<K2, V> dict;
			if (!map.TryGetValue(key, out dict))
			{
				map.Add(key, dict = new Dictionary<K2, V>());
			}
			return dict;
		}

		public static Dictionary<K2, V> GetOrCreate<K, K2, V>(this IDictionary<K, Dictionary<K2, V>> map, K key)
		{
			Dictionary<K2, V> dict;
			if (!map.TryGetValue(key, out dict))
			{
				map.Add(key, dict = new Dictionary<K2, V>());
			}
			return dict;
		}

		public static V GetValue<K, V>(this IDictionary<K, V> map, K key)
		{
			V val;
			map.TryGetValue(key, out val);
			return val;
		}

		public static bool Iterate<T>(this IEnumerable<T> items, Func<T, bool> action)
		{
			foreach (var item in items)
			{
				if (!action(item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// For unexplainable reasons, HashSet's ToArray method is internal
		/// </summary>
		public static T[] MakeArray<T>(this HashSet<T> set)
		{
			var arr = new T[set.Count];
			var i = 0;
			foreach (var item in set)
			{
				arr[i++] = item;
			}
			return arr;
		}

		public static bool RemoveFirst<T>(this ICollection<T> items, Func<T, bool> filter)
		{
			foreach (var item in items)
			{
				if (filter(item))
				{
					items.Remove(item);
					return true;
				}
			}
			return false;
		}

		public static bool Contains<T>(this IEnumerable<T> list, Func<T, bool> predicate)
		{
			foreach (var item in list)
			{
				if (predicate(item))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Returns either the list or a new List if list is null.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static List<T> NotNull<T>(this List<T> list)
		{
			if (list == null)
			{
				list = new List<T>();
			}
			return list;
		}

		public static List<string> GetAllMessages(this Exception ex)
		{
			var msgs = new List<string>();
			do
			{
				if (!(ex is TargetInvocationException))
				{
					msgs.Add(ex.Message);
					if ((ex is ReflectionTypeLoadException))
					{
						msgs.Add("###########################################");
						msgs.Add("LoaderExceptions:");
						foreach (var lex in ((ReflectionTypeLoadException)ex).LoaderExceptions)
						{
							msgs.Add(lex.GetType().FullName + ":");
							msgs.AddRange(lex.GetAllMessages());
							if (lex is FileNotFoundException)
							{
								var asmName = ((FileNotFoundException)lex).FileName;
								var asms = Utility.GetMatchingAssemblies(asmName);
								if (asms.Count() > 0)
								{
									msgs.Add("Found matching Assembly: " + asms.ToString("; ") + 
										" - Make sure to compile against the correct version.");
								}
								else
								{
									msgs.Add("Did not find any matching Assembly - Make sure to load the required Assemblies before loading this one.");
								}
							}
							msgs.Add("");
						}
						msgs.Add("#############################################");
					}
				}
				ex = ex.InnerException;
			} while (ex != null);
			return msgs;
		}

		#region Types

		public static void SetUnindexedValue(this MemberInfo member, object obj, object value)
		{
			if (member is FieldInfo)
			{
				((FieldInfo)member).SetValue(obj, value);
			}
			else if (member is PropertyInfo)
			{
				((PropertyInfo)member).SetValue(obj, value, null);
			}
			else
			{
				throw new Exception("Can only get Values of Fields and Properties");
			}
		}

		public static object GetUnindexedValue(this MemberInfo member, object obj)
		{
			if (member is FieldInfo)
			{
				return ((FieldInfo)member).GetValue(obj);
			}
			else if (member is PropertyInfo)
			{
				return ((PropertyInfo)member).GetValue(obj, null);
			}
			else
			{
				throw new Exception("Can only get Values of Fields and Properties");
			}
		}

		public static Type GetVariableType(this MemberInfo member)
		{
			if (member is FieldInfo)
			{
				return ((FieldInfo)member).FieldType;
			}
			if (member is PropertyInfo)
			{
				return ((PropertyInfo)member).PropertyType;
			}
			throw new Exception("Can only get VariableType of Fields and Properties");
		}

		public static string GetMemberName(this MemberInfo member)
		{
			var str = member.DeclaringType.FullName + "." + member.Name;
			if (member is MethodInfo)
			{
				str += "(" + ((MethodInfo)member).GetParameters().ToString(", ", (param) => param.ParameterType.Name) + (")");
			}
			return str;
		}

		public static Type GetActualType(this MemberInfo member)
		{
			var rawType = member.GetVariableType();
			var isArr = rawType.IsArray;

			Type memberType;
			if (isArr)
			{
				memberType = rawType.GetElementType();
				if (memberType == null)
				{
					throw new Exception(string.Format("Unable to get Type of Array {0} ({1}).", rawType, member.GetMemberName()));
				}
			}
			else
			{
				memberType = rawType;
			}
			return memberType;
		}

		/// <summary>
		/// Simple types are primitive-types and strings
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsSimpleType(this Type type)
		{
			return type.IsEnum || type.IsPrimitive || type == typeof(string);
		}

		public static bool IsFieldOrProp(this MemberInfo member)
		{
			return member is FieldInfo || member is PropertyInfo;
		}

		public static bool IsReadonly(this MemberInfo member)
		{
			if (member is FieldInfo)
			{
				return ((FieldInfo)member).IsInitOnly || ((FieldInfo)member).IsLiteral;
			}
			else if (member is PropertyInfo)
			{
				return !((PropertyInfo)member).CanWrite || ((PropertyInfo)member).GetSetMethod() == null ||
					!((PropertyInfo)member).GetSetMethod().IsPublic;
			}
			return true;
		}

		public static bool IsNumericType(this Type type)
		{
			return type.IsInteger() || type.IsFloatingPoint();
		}

		public static bool IsFloatingPoint(this Type type)
		{
			return type == typeof (float) || type == typeof (double);
		}

		public static bool IsInteger(this Type type)
		{
			return type.IsEnum ||
			       type == typeof (int) || type == typeof (uint) || type == typeof (short) || type == typeof (ushort) ||
			       type == typeof (byte) || type == typeof (sbyte) || type == typeof (long) || type == typeof (ulong);
		}
		#endregion

        #region ByteArray Helpers

        public static unsafe ushort GetUInt16(this byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 2 > data.Length)
                return ushort.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(ushort*)pData;
            }
        }

        public static unsafe ushort GetUInt16AtByte(this byte[] data, uint startIndex)
        {
            if (startIndex + 1 >= data.Length)
                return ushort.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(ushort*)pData;
            }
        }

        public static unsafe uint GetUInt32(this byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                return uint.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(uint*)pData;
            }
        }

        public static unsafe int GetInt32(this byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                return int.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(int*)pData;
            }
        }

        public static unsafe float GetFloat(this byte[] data, uint field)
        {
            uint startIndex = field * 4;
            if (startIndex + 4 > data.Length)
                return float.NaN;

            fixed (byte* pData = &data[startIndex])
            {
                return *(float*)pData;
            }
        }

        public static unsafe ulong GetUInt64(this byte[] data, uint startingField)
        {
            uint startIndex = startingField * 4;
            if (startIndex + 8 > data.Length)
                return ulong.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(ulong*)pData;
            }
        }

        public static byte[] GetBytes(this byte[] data, uint startingField, int amount)
        {
            byte[] bytes = new byte[amount];

            uint startIndex = startingField * 4;
            if (startIndex + amount > data.Length)
                return bytes;

            for (int i = 0; i < amount; i++)
            {
                bytes[i] = data[startIndex + i];
            }
            return bytes;
        }
        #endregion
	}
}