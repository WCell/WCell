using System;
using WCell.Util;

namespace WCell.Core
{
	public class ModifyableArraySegment<T>
	{
		public T[] Array;
		public int Index;
		public int End;

		public ModifyableArraySegment(T[] arr)
		{
			Array = arr;
			Index = 0;
			End = Array.Length - 1;
		}

		public ModifyableArraySegment(T[] arr, int index, int end)
		{
			if (index >= arr.Length || index < 0)
			{
				throw new ArgumentException("index out of bounds");
			}
			if (end >= arr.Length || end < 0)
			{
				throw new ArgumentException("end out of bounds");
			}
			if (end < index)
			{
				throw new ArgumentException("end < index");
			}

			Array = arr;
			Index = index;
			End = end;
		}

		public int Length
		{
			get
			{
				return End - Index + 1;
			}
		}

		public T Current
		{
			get
			{
				return Array[Index];
			}
		}

		public T this[uint index]
		{
			get
			{
				return Array.Get(index);
			}
			set
			{
				Array[index] = value;
			}
		}

		public void Reset()
		{
			Index = 0;
		}

		/// <summary>
		/// Returns the current value and moves to next (much like an iterator)
		/// </summary>
		public T GetCurrentMoveNext()
		{
			return Array[Index++];
		}

		/// <summary>
		/// Sets the current value and moves to next
		/// </summary>
		public void SetCurrentMoveNext(T val)
		{
			ArrayUtil.Set(ref Array, (uint)Index++, val);
		}

		public static implicit operator ModifyableArraySegment<T>(Array arr)
		{
			return new ModifyableArraySegment<T>((T[])arr);
		}
	}
}
