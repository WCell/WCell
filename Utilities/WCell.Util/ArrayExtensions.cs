using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Util
{
	public static class ArrayUtil
	{
		/// <summary>
		/// At least ensure a size of highestIndex * this, if index is not valid within an array.
		/// </summary>
		public const float LoadConstant = 1.5f;

		/// <summary>
		/// Ensures that the given array has at least the given size and resizes if its too small
		/// </summary>
		public static void EnsureSize<T>(ref T[] arr, int size)
		{
			if (arr.Length < size)
			{
				Array.Resize(ref arr, size);
			}
		}

		/// <summary>
		/// Returns the entry in this array at the given index, or null if the index is out of bounds
		/// </summary>
		public static T Get<T>(this T[] arr, int index)
		{
			if (index >= arr.Length || index < 0)
			{
				return default(T);
			}
			return arr[index];
		}

		/// <summary>
		/// Returns the entry in this array at the given index, or null if the index is out of bounds
		/// </summary>
		public static T Get<T>(this T[] arr, uint index)
		{
			if (index >= arr.Length)
			{
				return default(T);
			}
			return arr[index];
		}

		/// <summary>
		/// Returns arr[index] or, if index is out of bounds, arr.Last()
		/// </summary>
		public static T GetMax<T>(this T[] arr, uint index)
		{
			if (index >= arr.Length)
			{
				index = (uint) (arr.Length - 1);
			}
			return arr[index];
		}

		/// <summary>
		/// Cuts away everything after and including the first null
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="arr"></param>
		public static void Trunc<T>(ref T[] arr)
			where T : class
		{
			var lastIndex = arr.Length - 1;
			for (var i = 0; i <= lastIndex; i++)
			{
				if (arr[i] != null && i != lastIndex)
				{
					Array.Resize(ref arr, i);
					break;
				}
			}
		}

		public static void TruncVals<T>(ref T[] arr)
			where T : struct
		{
			var lastIndex = arr.Length - 1;
			for (var i = 0; i <= lastIndex; i++)
			{
				if (arr[i].Equals(default(T)) && i != lastIndex)
				{
					Array.Resize(ref arr, i);
					break;
				}
			}
		}

		/// <summary>
		/// Cuts away all null values
		/// </summary>
		public static void Prune<T>(ref T[] arr) where T : class
		{
			var list = new List<T>(arr.Length);
			foreach (var obj in arr)
			{
				if (obj != null)
				{
					list.Add(obj);
				}
			}
			arr = list.ToArray();
		}

		/// <summary>
		/// Cuts away all null values
		/// </summary>
		public static void PruneStrings(ref string[] arr)
		{
			var list = new List<string>(arr.Length);
			foreach (var obj in arr)
			{
				if (!string.IsNullOrEmpty(obj))
				{
					list.Add(obj);
				}
			}
			arr = list.ToArray();
		}

		/// <summary>
		/// Cuts away all null values
		/// </summary>
		public static void PruneVals<T>(ref T[] arr) where T : struct
		{
			var list = new List<T>(arr.Length);
			foreach (var obj in arr)
			{
				if (!obj.Equals(default(T)))
				{
					list.Add(obj);
				}
			}
			arr = list.ToArray();
		}

		public static void Set<T>(ref T[] arr, uint index, T val)
		{
			if (index >= arr.Length)
			{
				EnsureSize(ref arr, (int)(index * LoadConstant) + 1);
			}
			arr[index] = val;
		}

		public static void Set<T>(ref T[] arr, uint index, T val, int maxSize)
		{
			if (index >= arr.Length)
			{
				EnsureSize(ref arr, maxSize);
			}
			arr[index] = val;
		}

		public static List<T> GetOrCreate<T>(ref List<T>[] arr, uint index)
		{
			if (index >= arr.Length)
			{
				EnsureSize(ref arr, (int)(index * LoadConstant) + 1);
			}

			var list = arr[index];
			if (list == null)
			{
				return arr[index] = new List<T>();
			}
			return list;
		}

		/// <summary>
		/// Adds the given value to the first slot that is not occupied in the given array
		/// </summary>
		/// <returns>The index at which it was added</returns>
		public static uint Add<T>(ref T[] arr, T val)
		{
			var index = arr.GetFreeIndex();

			// no free space found: Make more space
			if (index >= arr.Length)
			{
				EnsureSize(ref arr, (int)(index * LoadConstant) + 1);
			}
			arr[index] = val;
			return index;
		}

		/// <summary>
		/// Appends the given values to the end of arr
		/// </summary>
		/// <returns>The index at which it was added</returns>
		public static void Concat<T>(ref T[] arr, T[] values)
		{
			var oldLen = arr.Length;
			Array.Resize(ref arr, oldLen + values.Length);
			Array.Copy(values, 0, arr, oldLen, values.Length);
		}

		/// <summary>
		/// Adds the given value to the first slot that is not occupied in the given array
		/// </summary>
		/// <returns>The index at which it was added</returns>
		public static uint AddOnlyOne<T>(ref T[] arr, T val)
		{
			var index = arr.GetFreeIndex();

			// no free space found: Make more space
			if (index >= arr.Length)
			{
				EnsureSize(ref arr, (int)index + 1);
			}
			arr[index] = val;
			return index;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>The index at which it was added</returns>
		public static int AddElement<T>(this T[] arr, T val)
		{
			var index = arr.GetFreeIndex();
			if (index < arr.Length)
			{
				arr[index] = val;
			}
			return -1;
		}

		public static bool ContainsIndex<T>(this T[] arr, uint index)
			where T : class
		{
			return index < arr.Length && arr[index] != null;
		}

		public static uint GetFreeIndex<T>(this T[] arr)
		{
			uint i = 0;
			for (; i < arr.Length; i++)
			{
				if (arr[i] == null || arr[i].Equals(default(T)))
				{
					return i;
				}
			}
			return i;
		}

		/// <summary>
		/// Believe it or not, .NET has no such method by default.
		/// Array.Equals is not overridden properly.
		/// </summary>
		public static bool EqualValues<T>(this T[] arr, T[] arr2)
			where T : struct
		{
			for (var i = 0; i < arr.Length; i++)
			{
				if (!arr[i].Equals(arr2[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static uint GetFreeValueIndex<T>(this T[] arr)
			where T : struct
		{
			uint i = 0;
			for (; i < arr.Length; i++)
			{
				if (arr[i].Equals(default(T)))
				{
					return i;
				}
			}
			return i + 1;
		}

		/// <summary>
		/// Removes all empty entries from an array
		/// </summary>
		public static T GetWhere<T>(this T[] arr, Func<T, bool> predicate)
		{
			for (var i = 0; i < arr.Length; i++)
			{
				var item = arr[i];
				if (predicate(item))
				{
					return item;
				}
			}
			return default(T);
		}

		public static void SetValue(Array arr, int index, object value)
		{
			var type = arr.GetType().GetElementType();
			if (type.IsEnum && type != value.GetType())
			{
				//value = Convert.ChangeType(value, type);
				value = Enum.Parse(type, value.ToString());
			}
			arr.SetValue(value, index);
		}

		// TODO: Shuffle
		public static void Shuffle<T>(this T[] arr)
		{
			//var randomIndices = new int[arr.Length];
			//for (int i = 1; i <= randomIndices.Length; i++)
			//{
			//    int index;
			//    while (randomIndices[index = Utility.Random(0, randomIndices.Length)] != 0)
			//    {
			//    }
			//    randomIndices[index] = i;
			//}
			//for (int i = 0; i < randomIndices.Length; i++)
			//{
			//    arr[i] 
			//}
		}
	}
}