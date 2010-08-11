/*************************************************************************
 *
 *   file		: Utility.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-10 05:18:25 +0800 (Sun, 10 Aug 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 585 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Linq;
using System.Security;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Collections;
using System.Net.Sockets;

namespace WCell.Util
{
	/// <summary>
	/// Contains miscellaneous utility method used throughout the project.
	/// </summary>
	/// <remarks>Things that can't be added as extension methods, or are too miscellaneous
	/// will most likely be in this class.</remarks>
	public static class Utility
	{
		public static readonly DateTime UnixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0);

		public static readonly object[] EmptyObjectArray = new object[0];

		public static readonly Dictionary<string, Type> TypeMap =
			new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

		public static readonly Dictionary<Type, Dictionary<string, object>> EnumValueMap =
			new Dictionary<Type, Dictionary<string, object>>(300);

		static Utility()
		{
			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				InitEnums(asm);
			}
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
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

			TypeMap.Add("UInt32", typeof(uint));
			TypeMap.Add("UInt64", typeof(ulong));
			TypeMap.Add("Int32", typeof(int));
			TypeMap.Add("Int64", typeof(long));
		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			InitEnums(args.LoadedAssembly);
		}

		private static void InitEnums(Assembly asm)
		{
			AddTypesToTypeMap(asm);
		}

		/// <summary>
		/// Adds all non-standard Enum-types of the given Assembly to the TypeMap.
		/// Also caches all big enums into a dictionary to improve Lookup speed.
		/// </summary>
		/// <param name="asm"></param>
		public static void AddTypesToTypeMap(Assembly asm)
		{
			if (asm.FullName == null)
			{
				return;
			}
			if (!asm.FullName.StartsWith("System.") && !asm.FullName.StartsWith("Microsoft.")
				&& !asm.FullName.StartsWith("NHibernate")
				&& !asm.FullName.StartsWith("Castle")
				&& !asm.FullName.StartsWith("msvc")
				&& !asm.FullName.StartsWith("NLog")
				&& !asm.FullName.StartsWith("mscorlib"))
			{
				foreach (var type in asm.GetTypes())
				{
					if (!type.FullName.StartsWith("System.") && !type.FullName.StartsWith("Microsoft."))
					{
						if (type.IsValueType)
						{
							TypeMap[type.FullName] = type;
							if (type.IsEnum)
							{
								var values = Enum.GetValues(type);
								//if (values.Length >= 100)
								{
									var dict = new Dictionary<string, object>(values.Length + 100,
																			  StringComparer.InvariantCultureIgnoreCase);
									var names = Enum.GetNames(type);
									for (var i = 0; i < names.Length; i++)
									{
										dict[names[i]] = values.GetValue(i);
									}
									EnumValueMap[type] = dict;
								}
							}
						}
					}
				}
			}
		}

		#region Times
		public const int TicksPerSecond = 10000;
		private const long TICKS_SINCE_1970 = 621355968000000000; // .NET ticks for 1970

		public static int GetMilliSecondsInt(this DateTime time)
		{
			return (int)(time.Ticks / TicksPerSecond);
		}

		public static int GetMilliSecondsInt(this TimeSpan time)
		{
			return (int)(time.Ticks + TicksPerSecond/2) / TicksPerSecond;
		}

		/// <summary>
		/// Gets the system uptime.
		/// </summary>
		/// <returns>the system uptime in milliseconds</returns>
		public static uint GetSystemTime()
		{
			return (uint)Environment.TickCount;
		}

		/// <summary>
		/// Gets the time since the Unix epoch.
		/// </summary>
		/// <returns>the time since the unix epoch in seconds</returns>
		public static uint GetEpochTime()
		{
			return (uint)((DateTime.UtcNow.Ticks - TICKS_SINCE_1970) / TimeSpan.TicksPerSecond);
		}

		public static DateTime GetDateTimeFromUnixTime(uint unixTime)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);
		}

		public static DateTime GetUTCTimeSeconds(long seconds)
		{
			return UnixTimeStart.AddSeconds(seconds);
		}

		public static DateTime GetUTCTimeMillis(long millis)
		{
			return UnixTimeStart.AddMilliseconds(millis);
		}

		/// <summary>
		/// Gets the system uptime.
		/// </summary>
		/// <remarks>
		/// Even though this returns a long, the original value is a 32-bit integer,
		/// so it will wrap back to 0 after approximately 49 and half days of system uptime.
		/// </remarks>
		/// <returns>the system uptime in milliseconds</returns>
		public static long GetSystemTimeLong()
		{
			return (uint)Environment.TickCount;
		}

		/// <summary>
		/// Converts the current time and date into the time and date format of the WoW client.
		/// </summary>
		/// <returns>a packed time and date</returns>
		public static uint GetDateTimeToGameTime(DateTime n)
		{
			uint dayOfWeek = ((uint)n.DayOfWeek == 0 ? 6 : ((uint)n.DayOfWeek) - 1);

			uint gameTime = ((uint)n.Minute & 0x3F);
			gameTime |= (((uint)n.Hour << 6) & 0x7C0);
			gameTime |= ((dayOfWeek << 11) & 0x3800);
			gameTime |= (((uint)(n.Day - 1) << 14) & 0xFC000);
			gameTime |= (((uint)(n.Month - 1) << 20) & 0xF00000);
			gameTime |= (((uint)(n.Year - 2000) << 24) & 0x1F000000);

			return gameTime;
		}

		public static DateTime GetGameTimeToDateTime(uint packedDate)
		{
			int minute = (int)(packedDate & 0x3F);
			int hour = (int)((packedDate >> 6) & 0x1F);
			//DayOfWeek dayOfWeek = (DayOfWeek) ((packedDate >> 11) & 0x3800);
			int day = (int)((packedDate >> 14) & 0x3F);
			int month = (int)((packedDate >> 20) & 0xF);
			int year = (int)((packedDate >> 24) & 0x1F);

			return new DateTime(year + 2000, month + 1, day + 1, hour, minute, 0);
		}

		/// <summary>
		/// Gets the time between the Unix epich and a specific <see cref="DateTime">time</see>.
		/// </summary>
		/// <param name="time">the end time</param>
		/// <returns>the time between the unix epoch and the supplied <see cref="DateTime">time</see> in seconds</returns>
		public static uint GetEpochTimeFromDT()
		{
			return GetEpochTimeFromDT(DateTime.Now);
		}

		/// <summary>
		/// Gets the time between the Unix epich and a specific <see cref="DateTime">time</see>.
		/// </summary>
		/// <param name="time">the end time</param>
		/// <returns>the time between the unix epoch and the supplied <see cref="DateTime">time</see> in seconds</returns>
		public static uint GetEpochTimeFromDT(DateTime time)
		{
			return (uint)((time.Ticks - TICKS_SINCE_1970) / 10000000L);
		}
		#endregion

		/// <summary>
		/// Reverses the contents of an array.
		/// </summary>
		/// <typeparam name="T">type of the array</typeparam>
		/// <param name="buffer">array of objects to reverse</param>
		public static void Reverse<T>(T[] buffer)
		{
			Reverse(buffer, buffer.Length);
		}

		/// <summary>
		/// Reverses the contents of an array.
		/// </summary>
		/// <typeparam name="T">type of the array</typeparam>
		/// <param name="buffer">array of objects to reverse</param>
		/// <param name="length">number of objects in the array</param>
		public static void Reverse<T>(T[] buffer, int length)
		{
			for (int i = 0; i < length / 2; i++)
			{
				T temp = buffer[i];
				buffer[i] = buffer[length - i - 1];
				buffer[length - i - 1] = temp;
			}
		}

		/// <summary>
		/// Swaps one reference with another atomically.
		/// </summary>
		/// <typeparam name="T">the type of the reference</typeparam>
		/// <param name="originalRef">the original reference</param>
		/// <param name="newRef">the new reference</param>
		public static void SwapReference<T>(ref T originalRef, ref T newRef) where T : class
		{
			T orig;

			do
			{
				orig = originalRef;
			} while (Interlocked.CompareExchange(ref originalRef, newRef, orig) != orig);
		}

		/// <summary>
		/// Swaps one reference with another atomically, and replaces the original with the given value
		/// </summary>
		/// <typeparam name="T">the type of the reference</typeparam>
		/// <param name="originalRef">the original reference</param>
		/// <param name="newRef">the new reference</param>
		/// <param name="replacement">the value to replace the original with</param>
		public static void SwapReference<T>(ref T originalRef, ref T newRef, T replacement) where T : class
		{
			do
			{
				newRef = originalRef;
			} while (Interlocked.CompareExchange(ref originalRef, replacement, newRef) != newRef);
		}

		/// <summary>
		/// Moves memory from one array to another.
		/// </summary>
		/// <param name="src">the pointer to the source array</param>
		/// <param name="srcIndex">the index to read from in the source array</param>
		/// <param name="dest">the destination array</param>
		/// <param name="destIndex">the index to write to in the destination array</param>
		/// <param name="len">the number of bytes to move</param>
		public static unsafe void MoveMemory(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
		{
			if (len != 0)
			{
				src += srcIndex;

				fixed (byte* destRef = &dest[destIndex])
				{
					byte* pDest = destRef;

					while (len-- > 0)
					{
						*pDest++ = *src++;
					}
				}
			}
		}

		/// <summary>
		/// Moves memory from one array to another.
		/// </summary>
		/// <param name="src">the source array</param>
		/// <param name="srcIndex">the index to read from in the source array</param>
		/// <param name="dest">the pointer to the destination array</param>
		/// <param name="destIndex">the index to write to in the destination array</param>
		/// <param name="len">the number of bytes to move</param>
		public static unsafe void MoveMemory(byte[] src, int srcIndex, byte* dest, int destIndex, int len)
		{
			if (len != 0)
			{
				dest += destIndex;

				fixed (byte* srcRef = &src[srcIndex])
				{
					byte* pSrc = srcRef;

					while (len-- > 0)
					{
						*dest++ = *pSrc++;
					}
				}
			}
		}

		/// <summary>
		/// Cast one thing into another
		/// </summary>
		public static T Cast<T>(object obj)
		{
			return (T)Convert.ChangeType(obj, typeof(T));
		}

		#region String Building / Verbosity

		/// <summary>
		/// Returns the string representation of an IEnumerable (all elements, joined by comma)
		/// </summary>
		/// <param name="conj">The conjunction to be used between each elements of the collection</param>
		public static string ToString<T>(this IEnumerable<T> collection, string conj)
		{
			string vals;
			if (collection != null)
			{
				vals = string.Join(conj, ToStringArrT(collection));
			}
			else
				vals = "(null)";

			return vals;
		}

		/// <summary>
		/// Returns the string representation of an IEnumerable (all elements, joined by comma)
		/// </summary>
		/// <param name="conj">The conjunction to be used between each elements of the collection</param>
		public static string ToString<T>(this IEnumerable<T> collection, string conj, Func<T, object> converter)
		{
			string vals;
			if (collection != null)
			{
				vals = string.Join(conj, ToStringArrT(collection, converter));
			}
			else
				vals = "(null)";

			return vals;
		}

		/// <summary>
		/// Returns the string representation of an IEnumerable (all elements, joined by comma)
		/// </summary>
		/// <param name="conj">The conjunction to be used between each elements of the collection</param>
		public static string ToStringCol(this ICollection collection, string conj)
		{
			string vals;
			if (collection != null)
			{
				vals = string.Join(conj, ToStringArr(collection));
			}
			else
				vals = "(null)";

			return vals;
		}

		//public static string[] ToStringArr(ICollection collection)
		//{
		//    var strArr = new string[collection.Count];
		//    var colEnum = collection.GetEnumerator();
		//    for (var i = 0; i < strArr.Length; i++)
		//    {
		//        colEnum.MoveNext();
		//        var cur = colEnum.Current;
		//        if (cur != null)
		//        {
		//            strArr[i] = cur.ToString();
		//        }
		//    }
		//    return strArr;
		//}

		public static string ToString(this IEnumerable collection, string conj)
		{
			string vals;
			if (collection != null)
			{
				vals = string.Join(conj, ToStringArr(collection));
			}
			else
				vals = "(null)";

			return vals;
		}

		public static string[] ToStringArrT<T>(IEnumerable<T> collection)
		{
			return ToStringArrT(collection, null);
		}

		public static string[] ToStringArr(IEnumerable collection)
		{
			var strs = new List<string>();
			var colEnum = collection.GetEnumerator();
			while (colEnum.MoveNext())
			{
				var cur = colEnum.Current;
				if (cur != null)
				{
					strs.Add(cur.ToString());
				}
			}
			return strs.ToArray();
		}

		public static string[] ToStringArrT<T>(IEnumerable<T> collection, Func<T, object> converter)
		{
			var strArr = new string[collection.Count()];
			var colEnum = collection.GetEnumerator();
			var i = 0;
			while (colEnum.MoveNext())
			{
				var cur = colEnum.Current;
				if (!Equals(cur, default(T)))
				{
					strArr[i++] = (converter != null ? converter(cur) : cur).ToString();
				}
			}
			return strArr;
		}

		public static string[] ToJoinedStringArr<T>(IEnumerable<T> col, int partCount, string conj)
		{
			var strs = ToStringArrT(col);

			var list = new List<string>();
			var current = new List<string>(partCount);
			for (int index = 0, i = 0; index < strs.Length; i++, index++)
			{
				current.Add(strs[index]);
				if (i == partCount)
				{
					i = 0;
					list.Add(string.Join(conj, current.ToArray()));
					current.Clear();
				}
			}
			if (current.Count > 0)
				list.Add(string.Join(conj, current.ToArray()));

			return list.ToArray();
		}

		public static string ToString<K, V>(this IEnumerable<KeyValuePair<K, V>> args, string indent, string seperator)
		{
			string s = "";
			var i = 0;
			foreach (var arg in args)
			{
				i++;
				s += indent + arg.Key + " = " + arg.Value;

				if (i < args.Count())
				{
					s += seperator;
				}
			}
			return s;
		}

		#endregion

		public static void ReverseArr<T>(this T[] arr)
		{
			var len = arr.Length - 1;
			for (int i = 0; i < arr.Length / 2; i++)
			{
				var bottom = arr[i];
				var top = arr[len - i];

				arr[i] = top;
				arr[len - i] = bottom;
			}
		}

		/// <summary>
		/// Sets all values of the given array between offset and length to the given obj
		/// </summary>
		public static void Fill<T>(this T[] arr, T obj, int offset, int until)
		{
			for (var i = offset; i <= until; i++)
			{
				arr[i] = obj;
			}
		}

		public static void Fill(this int[] arr, int offset, int until, int startVal)
		{
			for (var i = offset; i <= until; i++)
			{
				arr[i] = startVal++;
			}
		}

		#region Random

		private static long holdrand = DateTime.Now.Ticks;

		public static int Random()
		{
			return (int)(((holdrand = holdrand * 214013L + 2531011L) >> 16) & 0x7fff);
		}

		public static uint RandomUInt()
		{
			return (uint)(((holdrand = holdrand * 214013L + 2531011L) >> 16) & 0x7fff);
		}

		public static bool Chance()
		{
			return Chance(RandomFloat());
		}

		public static bool Chance(double chance)
		{
			return chance > 1 ? true : chance < 0 ? false : Random() / (double)0x7fff <= chance;
		}

		public static bool Chance(float chance)
		{
			return chance > 1 ? true : chance < 0 ? false : RandomFloat() <= chance;
		}

		/*public static bool Chance(int chance)
		{
			return chance >= 10000 ? true : random.Next(0, 10000) <= chance;
		}*/

		public static float RandomFloat()
		{
			return (((holdrand = holdrand * 214013L + 2531011L) >> 16) & 0x7fff) / (float)0x7fff;
		}

		/// <summary>
		/// Generates a pseudo-random number in range [from, to)
		/// </summary>
		public static int Random(int from, int to)
		{
			//return from > to
			//        ? (int)Math.Round(RandomFloat() * (from - to) + to)
			//        : (int)Math.Round((RandomFloat() * (to - from) + from));
			return from == to
					   ? from
					   : (from > to
							  ? ((Random() % (from - to)) + to)
							  : ((Random() % (to - from)) + from));
		}

		public static uint Random(uint from, uint to)
		{
			//return from > to
			//        ? (uint)Math.Round((RandomFloat() * (from - to) + to))
			//        : (uint)Math.Round((RandomFloat() * (to - from) + from));return
			return from == to
					   ? from
					   : (from > to
							  ? ((RandomUInt() % (from - to)) + to)
							  : ((RandomUInt() % (to - from)) + from));
		}

		public static int Random(int max)
		{
			//return from > to
			//        ? (uint)Math.Round((RandomFloat() * (from - to) + to))
			//        : (uint)Math.Round((RandomFloat() * (to - from) + from));return
			return Random() % max;
		}

		public static uint RandomUInt(uint max)
		{
			//return from > to
			//        ? (uint)Math.Round((RandomFloat() * (from - to) + to))
			//        : (uint)Math.Round((RandomFloat() * (to - from) + from));return
			return RandomUInt() % max;
		}

		public static float Random(float from, float to)
		{
			return from > to ? RandomFloat() * (from - to) + to : (RandomFloat() * (to - from) + from);
		}

		public static double Random(double from, double to)
		{
			return from > to ? RandomFloat() * (from - to) + to : RandomFloat() * (to - from) + from;
		}

		#endregion

		#region Simple Arbitrary String Parsing

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

		#endregion

		/// <summary>
		/// Measures how long the given func takes to be executed repeats times
		/// </summary>
		public static void Measure(string name, int repeats, Action action)
		{
			var start = DateTime.Now;
			for (int i = 0; i < repeats; i++)
			{
				action();
			}
			var span = DateTime.Now - start;
			Console.WriteLine(name + " (" + repeats + " time(s)) took: " + span);
		}

		/// <summary>
		/// Gets the biggest value of a numeric enum
		/// </summary>
		public static T GetMaxEnum<T>()
		{
			var values = (T[])Enum.GetValues(typeof(T));
			return values.Max();
		}

		#region Sets of set bits

		/// <summary>
		/// Creates and returns an array of all indices that are set within the given flag field.
		/// eg. {11000011, 11000011} would result into an array containing: 0,1,6,7,8,9,14,15
		/// </summary>
		public static uint[] GetSetIndices(uint[] flagsArr)
		{
			var indices = new List<uint>();
			foreach (var flags in flagsArr)
			{
				GetSetIndices(indices, flags);
			}
			return indices.ToArray();
		}

		/// <summary>
		/// Creates and returns an array of all indices that are set within the given flag field.
		/// eg. 11000011 would result into an array containing: 0,1,6,7
		/// </summary>
		public static uint[] GetSetIndices(uint flags)
		{
			var indices = new List<uint>();
			GetSetIndices(indices, flags);
			return indices.ToArray();
		}

		public static T[] GetSetIndicesEnum<T>(T flags)
		{
			var indices = new List<uint>();
			var uintFlags = (uint)Convert.ChangeType(flags, typeof(uint));
			GetSetIndices(indices, uintFlags);
			if (indices.Count == 0)
			{
				object box = (uint)0;
				return new[] { (T)box };
			}

			var arr = new T[indices.Count];
			for (var i = 0; i < indices.Count; i++)
			{
				var index = indices[i];
				object box = (uint)(1 << (int)(index));
				arr[i] = (T)box;
			}
			return arr;
		}

		public static void GetSetIndices(List<uint> indices, uint flags)
		{
			for (uint i = 0; i < 32; i++)
			{
				if ((flags & 1 << (int)i) != 0)
				{
					indices.Add(i);
				}
			}
		}

		/// <summary>
		/// Creates and returns an array of all indices that are set within the given flag field.
		/// eg. 11000011 would result into an array containing: 0,1,6,7
		/// </summary>
		public static T[] GetSetIndices<T>(uint flags)
		{
			var indices = new List<T>(5);
			for (uint i = 0; i < 32; i++)
			{
				if ((flags & 1 << (int)i) != 0)
				{
					if (typeof(T).IsEnum)
					{
						indices.Add((T)Enum.Parse(typeof(T), i.ToString()));
					}
					else
					{
						indices.Add((T)Convert.ChangeType(i, typeof(T)));
					}
				}
			}
			return indices.ToArray();
		}

		#endregion

		public static A[] CreateEnumArray<E, A>()
		{
			var arr = new A[(int)Convert.ChangeType(GetMaxEnum<E>(), typeof(int))];
			return arr;
		}

		/// <summary>
		/// Delays the given action by the given amount of milliseconds
		/// </summary>
		/// <returns>The timer that performs the delayed call (in case that you might want to cancel earlier)</returns>
		public static Timer Delay(uint millis, Action action)
		{
			Timer timer = null;
			timer = new Timer(sender =>
								  {
									  action();
									  timer.Dispose();
								  });
			timer.Change(millis, Timeout.Infinite);
			return timer;
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool IsInRange(float sqDistance, float max)
		{
			return /*sqDistance != 0 &&*/ sqDistance <= max * max;
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

		public static string GetAbsolutePath(string file)
		{
			return new DirectoryInfo(file).FullName;
		}

		public static IPAddress ParseOrResolve(string input)
		{
			IPAddress addr;
			if (IPAddress.TryParse(input, out addr))
			{
				return addr;
			}

			// try resolve synchronously
			var addresses = Dns.GetHostAddresses(input);

			// for now only do Ipv4 address (apparently the wow client doesnt support Ipv6 yet)
			addr = addresses.Where(address => address.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();

			return addr ?? IPAddress.Loopback;
		}

		public static string FormatMoney(uint money)
		{
			var str = "";
			if (money >= 10000)
			{
				str += (money / 10000) + "g ";
				money = money % 10000;
			}
			if (money >= 100)
			{
				str += (money / 100) + "s ";
				money = money % 100;
			}
			if (money > 0)
			{
				str += money + "c";
			}
			return str;
		}

		public static string Format(this TimeSpan time)
		{
			return string.Format("{0}{1:00}h {2:00}m {3:00}s", time.TotalDays > 0 ? (int)time.TotalDays + "d " : "",
								 time.Hours, time.Minutes, time.Seconds);
		}

		public static string FormatMillis(this DateTime time)
		{
			return string.Format("{0:00}h {1:00}m {2:00}s {3:00}ms", time.Hour, time.Minute,
								 time.Second, time.Millisecond);
		}

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

		public static O GetRandom<O>(this IList<O> os)
		{
			return os.Count == 0 ? default(O) : os[Random(0, os.Count)];
		}

		/// <summary>
		/// Checks whether the given mail-address is valid.
		/// </summary>
		public static bool IsValidEMailAddress(string mail)
		{
			return EmailSyntaxValidator.Valid(mail, false);
		}

		#region Types

		public static bool IsStatic(this Type type)
		{
			return (type.Attributes & (TypeAttributes.Abstract | TypeAttributes.Sealed)) ==
				   (TypeAttributes.Abstract | TypeAttributes.Sealed);
		}

		/// <summary>
		/// When overridden in a derived class, returns an array of custom attributes identified by System.Type.
		/// </summary>
		/// <typeparam name="T">The type of attribute to search for. Only attributes that are assignable to this type are returned.</typeparam>
		/// <param name="methodInfo"></param>
		/// <returns>An array of custom attributes applied to this member, or an array with zero (0) elements if no attributes have been applied.</returns>
		public static T[] GetCustomAttributes<T>(this MemberInfo methodInfo) where T : Attribute
		{
			return methodInfo.GetCustomAttributes(typeof(T), false) as T[];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arrType"></param>
		/// <returns></returns>
		public static Type GetArrUnderlyingType(Type arrType)
		{
			var name = arrType.FullName;
			var index = name.IndexOf('[');
			if (index > -1)
			{
				name = name.Substring(0, index);
				var nonArrType = arrType.Assembly.GetType(name);
				return nonArrType;
			}
			return null;
		}

		/// <summary>
		/// One second has 10 million system ticks (DateTime.Ticks etc)
		/// </summary>
		private const string DefaultNameSpace = "WCell.Constants.";

		public static Type GetType(string typeName)
		{
			var type = Type.GetType(typeName);
			if (type == null)
			{
				if (!TypeMap.TryGetValue(typeName, out type) &&
					!TypeMap.TryGetValue(DefaultNameSpace + typeName, out type))
				{
					throw new Exception("Invalid Type specified: " + typeName);
				}
			}
			return type;
		}

		/// <summary>
		/// Gets all assemblies that match the given fully qualified name without version checks etc.
		/// </summary>
		/// <param name="asmName"></param>
		/// <returns></returns>
		public static IEnumerable<Assembly> GetMatchingAssemblies(string asmName)
		{
			var parts = asmName.Split(',');
			if (parts.Length > 0)
			{
				asmName = parts[0];
			}
			return AppDomain.CurrentDomain.GetAssemblies().Where(asm =>
																	 {
																		 var matchName = asm.GetName();
																		 return matchName.Name == asmName;
																	 });
		}

		public static object ChangeType(object obj, Type type)
		{
			return ChangeType(obj, type, false);
		}

		public static object ChangeType(object obj, Type type, bool underlyingType)
		{
			if (type.IsEnum)
			{
				//obj = Enum.Parse(type, obj.ToString());
				var uType = Enum.GetUnderlyingType(type);
				if (!underlyingType)
				{
					obj = Enum.ToObject(type, obj);
				}
				else if (uType != obj.GetType())
				{
					obj = Convert.ChangeType(obj, uType);
				}
				return obj;
			}
			else
			{
				// try to find a ctor
				var ctor = type.GetConstructor(new[] { obj.GetType() });
				if (ctor == null)
				{
					try
					{
						return Convert.ChangeType(obj, type);
					}
					catch (Exception e)
					{
						throw new InvalidOperationException(string.Format(
							"Could not convert \"{0}\" from {1} to {2} - {2} has no public ctor with one argument of type \"{1}\".",
							obj, obj.GetType(), type), e);
					}
				}
				else
				{
					return ctor.Invoke(new[] { obj });
				}
			}
		}

		#endregion

		#region Files & Directories

		/// <summary>
		/// Writes the content of all files in the given directory to the given output file
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="outputFile"></param>
		public static void MergeFiles(string directory, string outputFile)
		{
			MergeFiles(Directory.GetFiles(directory), outputFile);
		}

		public static void MergeFiles(string[] inputFiles, string outputFile)
		{
			using (var output = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
			{
				using (var strOutput = new StreamWriter(output))
				{
					foreach (var file in inputFiles)
					{
						var content = File.ReadAllBytes(file);

						strOutput.WriteLine();
						strOutput.WriteLine("# " + file);
						strOutput.WriteLine();
						strOutput.Flush();
						output.Write(content, 0, content.Length);
						output.Flush();
					}
				}
			}
		}

		public static DirectoryInfo GetDirectory(this FileSystemInfo file)
		{
			if (file is DirectoryInfo)
			{
				return ((DirectoryInfo)file).Parent;
			}
			if (file is FileInfo)
			{
				return ((FileInfo)file).Directory;
			}
			return null;
		}

		public static void MKDirs(this FileInfo file)
		{
			MKDirs(file.GetDirectory());
		}

		public static void MKDirs(this DirectoryInfo dir)
		{
			if (!dir.Exists)
			{
				var parent = dir.Parent;
				if (parent != null && !parent.Exists)
				{
					MKDirs(dir.Parent);
				}
				dir.Create();
			}
		}

		/// <summary>
		/// Returns up to the n first lines from the given file.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="n"></param>
		/// <param name="ignoreEmpty"></param>
		/// <returns></returns>
		public static string[] ReadLines(string fileName, int n, bool ignoreEmpty)
		{
			var lines = new string[n];
			using (var reader = new StreamReader(fileName))
			{
				for (var i = 0; i < n; i++)
				{
					if (reader.EndOfStream)
					{
						break;
					}
					lines[i] = reader.ReadLine();
					if (ignoreEmpty && lines[i].Length == 0)
					{
						i--;
					}
				}
			}
			return lines;
		}

		#endregion

		public static string GetStringRepresentation(object val)
		{
			if (val is string)
			{
				return (string)val;
			}
			if (val is ICollection)
			{
				return ((ICollection)val).ToStringCol(", ");
			}
			if (val is IEnumerable)
			{
				return ((IEnumerable)val).ToString(", ");
			}
			if (val is TimeSpan)
			{
				return ((TimeSpan)val).Format();
			}
			return val.ToString();
		}

		public static bool ContainsIgnoreCase(this string str, string part)
		{
			return str.IndexOf(part, StringComparison.InvariantCultureIgnoreCase) > -1;
		}

		private static readonly Random rnd = new Random();

		public static void Shuffle<T>(ICollection<T> col)
		{
			var arr = col.ToArray();
			var b = new byte[arr.Length];
			rnd.NextBytes(b);
			Array.Sort(b, arr);
			col.Clear();
			for (var i = 0; i < arr.Length; i++)
			{
				var item = arr[i];
				col.Add(item);
			}
		}
	}

	public class SingleEnumerator<T> : IEnumerator<T>
		where T : class
	{
		private T m_Current;

		public SingleEnumerator(T element)
		{
			Current = element;
		}

		public void Dispose()
		{
			Current = null;
		}

		public bool MoveNext()
		{
			return Current != null;
		}

		public void Reset()
		{
			throw new System.NotImplementedException();
		}

		public T Current
		{
			get
			{
				var current = m_Current;
				m_Current = null;
				return current;
			}
			private set { m_Current = value; }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}
}