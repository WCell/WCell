using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cell.Core;
using WCell.Core;
using WCell.Constants;

namespace WCell.PacketAnalysis.Updates
{
	public static class UpdateFieldsUtil
	{
		public static byte[] Decompress(byte[] bytes)
		{
			var uncompressedSize = bytes.GetUInt32(0);

			byte[] compdata = new byte[bytes.Length - 4];

			Array.Copy(bytes, 4, compdata, 0, compdata.Length);

			byte[] output = new byte[uncompressedSize];

			Compression.DecompressZLib(compdata, output);

			return output;
		}

		public static byte[] ParseBytes(string s, bool hexaDecimal)
		{
			var byteStrs = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			return ParseBytes(byteStrs, hexaDecimal);
		}

		public static byte[] ParseBytes(string[] s, bool hexaDecimal)
		{
			var bytes = new byte[s.Length];
			for (int i = 0; i < bytes.Length; i++)
			{
				bytes[i] = ParseByte(s[i], hexaDecimal);
			}
			return bytes;
		}

		public static byte ParseByte(string s, bool hexaDecimal)
		{
			byte b;
			if (hexaDecimal)
			{
				byte.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out b);
			}
			else
			{
				byte.TryParse(s, out b);
			}
			return b;
		}

		internal static class HexStringConverter
		{
			public static byte[] ToByteArray(string hexString)
			{
				if (string.IsNullOrEmpty(hexString))
					return new byte[0];

				int length = hexString.Length;
				byte[] bytes = new byte[length / 2];

				for (int i = 0; i < length; i += 2)
				{
					bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
				}
				return bytes;
			}
		}
	}
}
