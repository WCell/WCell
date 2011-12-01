using System.Text;
using WCell.Core.Network;

namespace WCell.Core
{
	public static class WCellUtil
	{
		/// <summary>
		/// Dumps the array to string form, using hexadecimal as the formatter
		/// </summary>
		/// <returns>hexadecimal representation of the data parsed</returns>
		public static string ToHex(PacketId packetId, byte[] arr, int start, int count)
		{
			var hexDump = new StringBuilder();

			hexDump.Append('\n');
			hexDump.Append("{SERVER} " + string.Format("Packet: ({0}) {1} PacketSize = {2}\n" +
													   "|------------------------------------------------|----------------|\n" +
													   "|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F |0123456789ABCDEF|\n" +
													   "|------------------------------------------------|----------------|\n",
													   "0x" + ((ushort)packetId.RawId).ToString("X4"), packetId, count));

			int end = start + count;
			for (int i = start; i < end; i += 16)
			{
				var text = new StringBuilder();
				var hex = new StringBuilder();
				hex.Append("|");

				for (int j = 0; j < 16; j++)
				{
					if (j + i < end)
					{
						byte val = arr[j + i];
						hex.Append(arr[j + i].ToString("X2"));
						hex.Append(" ");
						if (val >= 32 && val <= 127)
						{
							text.Append((char)val);
						}
						else
						{
							text.Append(".");
						}
					}
					else
					{
						hex.Append("   ");
						text.Append(" ");
					}
				}
				hex.Append("|");
				hex.Append(text + "|");
				hex.Append('\n');
				hexDump.Append(hex.ToString());
			}

			hexDump.Append("-------------------------------------------------------------------");

			return hexDump.ToString();
		}

		public static string FormatBytes(float num)
		{
			string numPrefix = "B";

			if (num >= 1024f)
			{
				num /= 1024.0f; numPrefix = "kb";
			}
			if (num >= 1024f)
			{
				num /= 1024.0f; numPrefix = "MB";
			}
			if (num >= 1024f)
			{
				num /= 1024.0f; numPrefix = "GB";
			}

			return string.Format("{0,6:f}{1}", num, numPrefix);
		}

		public static string FormatBytes(double num)
		{
			string numPrefix = "B";

			if (num >= 1024.0)
			{
				num /= 1024.0; numPrefix = "kb";
			}
			if (num >= 1024.0)
			{
				num /= 1024.0; numPrefix = "MB";
			}
			if (num >= 1024.0)
			{
				num /= 1024.0; numPrefix = "GB";
			}

			return string.Format("{0,6:f}{1}", num, numPrefix);
		}
	}
}