using System.Net;
using System.Net.Sockets;

namespace Cell.Core
{
	public static class ByteArrHelper
	{
		/// <summary>
        /// Sets the given bytes in the given array at the given index
		/// </summary>
		public static void SetBytes(this byte[] arr, uint index, byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				arr[index + i] = bytes[i];
			}
		}

		/// <summary>
		/// Sets the ushort in opposite byte-order in the given array at the given index
        /// </summary>
		public static void SetUShortBE(this byte[] arr, uint index, ushort val)
		{
			arr[index] = (byte)((val & 0xFF00) >> 8);
			arr[index + 1] = (byte)(val & 0x00FF);
		}

		#region Get

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

		public static bool AreAllZero(this byte[] data)
		{
			foreach (var b in data)
			{
				if (b != 0)
				{
					return false;
				}
			}
			return true;
        }

        #region IP addresses

        public static bool IsIPV4(this IPAddress addr)
        {
            return addr.AddressFamily == AddressFamily.InterNetwork;
        }

        public static bool IsIPV6(this IPAddress addr)
        {
            return addr.AddressFamily == AddressFamily.InterNetworkV6;
        }

        public static int GetLength(this IPAddress addr)
        {
            return addr.GetAddressBytes().Length;
        }

        #endregion
    }
}