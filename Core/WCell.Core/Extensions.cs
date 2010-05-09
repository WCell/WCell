using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using WCell.Util.Graphics;

namespace WCell.Core
{
	public static class Extensions
	{
		/// <summary>
		/// Reads a C-style null-terminated string from the current stream.
		/// </summary>
		/// <param name="binReader">the extended <see cref="BinaryReader" /> instance</param>
		/// <returns>the string being reader</returns>
		public static string ReadCString(this BinaryReader binReader)
		{
			// TODO: Support all encodings

			var sb = new StringBuilder();
			byte c;

			while (binReader.BaseStream.Position < binReader.BaseStream.Length && (c = binReader.ReadByte()) != 0)
			{
				sb.Append((char)c);
			}

			return sb.ToString();
		}

		public static IPAddress ToIPAddress(this string szValue)
		{
			IPAddress addr;

			IPAddress.TryParse(szValue, out addr);

			return addr ?? IPAddress.Any;
		}

		public static string GetHexCode(this Color color)
		{
			return color.R.ToString("X2") + color.B.ToString("X2") + color.G.ToString("X2");
		}

		public static bool IsBetween(this int value, int min, int max)
		{
			return value >= min && value <= max;
		}

		public static bool IsBetween(this uint value, int min, int max)
		{
			return value >= min && value <= max;
		}
	}
}
