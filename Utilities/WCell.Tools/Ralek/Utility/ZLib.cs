/*************************************************************************
 *
 *   file		: ZLib.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-07 07:58:12 +0100 (lø, 07 mar 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 784 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace WoWExtractor
{
	public static class ZLib
	{
		public static void IOCompress(byte[] input, int len, byte[] output, out int deflatedLength)
		{
			Deflater item = new Deflater();
			item.SetInput(input, 0, len);
			item.Finish();
			deflatedLength = item.Deflate(output, 0, output.Length);
		}

		public static void IODecompress(byte[] input, int len, byte[] output, out int inflatedLength)
		{
			Inflater item = new Inflater();
			item.SetInput(input, 0, len);
			inflatedLength = item.Inflate(output, 0, output.Length);
		}

		public static int TargetCompress(IntPtr input, int offset, int len, IntPtr output, int bufferLength)
		{
			int destLength = bufferLength;
			NativeMethods.compress2(output, ref destLength, input, len, ZLibCompressionLevel.Z_DEFAULT_COMPRESSION);
			return destLength;
		}

		public static int TargetCompress(byte[] input, int offset, int len, byte[] output, int bufferLength)
		{
			int destLength = bufferLength;
			NativeMethods.compress2(output, ref destLength, input, len, ZLibCompressionLevel.Z_DEFAULT_COMPRESSION);
			return destLength;
		}

		public enum ZLibCompressionLevel
		{
			Z_DEFAULT_COMPRESSION = -1,
			Z_NO_COMPRESSION = 0,
			Z_BEST_SPEED = 1,
			Z_BEST_COMPRESSION = 9,
		}

		public enum ZLibError
		{
			Z_VERSION_ERROR = -6,
			Z_BUF_ERROR = -5,
			Z_MEM_ERROR = -4,
			Z_DATA_ERROR = -3,
			Z_STREAM_ERROR = -2,
			Z_ERRNO = -1,
			Z_OK = 0,
			Z_STREAM_END = 1,
			Z_NEED_DICT = 2,
		}

		private static class NativeMethods
		{
			[SuppressUnmanagedCodeSecurity, DllImport("zlib")]
			public static extern ZLibError compress(byte[] dest, ref int destLength, byte[] source, int sourceLength);

			[SuppressUnmanagedCodeSecurity, DllImport("zlib")]
			public static extern ZLibError compress2(byte[] dest, ref int destLength, byte[] source, int sourceLength,
			                                         ZLibCompressionLevel level);

			[SuppressUnmanagedCodeSecurity, DllImport("zlib")]
			public static extern ZLibError compress2(byte[] dest, ref int destLength, IntPtr source, int sourceLength,
			                                         ZLibCompressionLevel level);

			[SuppressUnmanagedCodeSecurity, DllImport("zlib")]
			public static extern ZLibError compress2(IntPtr dest, ref int destLength, IntPtr source, int sourceLength,
			                                         ZLibCompressionLevel level);

			[SuppressUnmanagedCodeSecurity, DllImport("zlib")]
			public static extern ZLibError uncompress(byte[] dest, ref int destLen, byte[] source, int sourceLen);

			[SuppressUnmanagedCodeSecurity, DllImport("zlib")]
			public static extern string zlibVersion();
		}
	}
}
