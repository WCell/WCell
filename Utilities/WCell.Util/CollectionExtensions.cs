using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace WCell.Util
{
	public static class CollectionExtensions
	{
		#region Dictionary Extensions
		public static V GetOrCreate<K, V>(this IDictionary<K, V> map, K key) where V : new()
		{
			V val;
			if (!map.TryGetValue(key, out val))
			{
				map.Add(key, val = new V());
			}
			return val;
		}

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
		#endregion

		#region Transform
		public static List<TOutput> TransformList<TInput, TOutput>(this IEnumerable<TInput> enumerable,
																   Func<TInput, TOutput> transformer)
		{
			var output = new List<TOutput>(enumerable.Count());

			foreach (var input in enumerable)
			{
				output.Add(transformer(input));
			}

			return output;
		}

		public static TOutput[] TransformArray<TInput, TOutput>(this IEnumerable<TInput> enumerable,
																Func<TInput, TOutput> transformer)
		{
			var output = new TOutput[enumerable.Count()];

			var enumerator = enumerable.GetEnumerator();
			for (var i = 0; i < output.Length; i++)
			{
				enumerator.MoveNext();
				output[i] = transformer(enumerator.Current);
			}

			return output;
		}
		#endregion

		public static void AddUnique<T>(this IList<T> items, T item)
		{
			if (!items.Contains(item))
			{
				items.Add(item);
			}
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

		public static T RemoveFirst<T>(this IList<T> items, Func<T, bool> filter)
		{
			for (var i = 0; i < items.Count; i++)
			{
				var item = items[i];
				if (filter(item))
				{
					items.RemoveAt(i);
					return item;
				}
			}
			return default(T);
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

		public static string GetFullMemberName(this MemberInfo member)
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
			Type rawType;
			if (!(member is Type))
			{
				rawType = member.GetVariableType();
			}
			else
			{
				rawType = (Type)member;
			}

			Type memberType;
			if (rawType.IsArray)
			{
				memberType = rawType.GetElementType();
				if (memberType == null)
				{
					throw new Exception(string.Format("Unable to get Type of Array {0} ({1}).", rawType, member.GetFullMemberName()));
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

        public static unsafe uint GetUInt32AtByte(this byte[] data, uint startIndex)
        {
            if (startIndex + 1 >= data.Length)
                return uint.MaxValue;

            fixed (byte* pData = &data[startIndex])
            {
                return *(uint*) pData;
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