using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WCell.Core.Network
{
	public static class IOExtensions
	{
		public static int WritePackedUInt64(this BinaryWriter binWriter, ulong number)
		{
			var buffer = BitConverter.GetBytes(number);

			byte mask = 0;
			var startPos = binWriter.BaseStream.Position;

			binWriter.Write(mask);

			for (var i = 0; i < 8; i++)
			{
				if (buffer[i] != 0)
				{
					mask |= (byte)(1 << i);
					binWriter.Write(buffer[i]);
				}
			}

			var endPos = binWriter.BaseStream.Position;

			binWriter.BaseStream.Position = startPos;
			binWriter.Write(mask);
			binWriter.BaseStream.Position = endPos;

			return (int)(endPos - startPos);
		}
	}
}
