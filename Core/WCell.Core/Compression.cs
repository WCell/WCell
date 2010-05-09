/*************************************************************************
 *
 *   file		: Compression.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-28 06:55:13 +0200 (ma, 28 apr 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 301 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using ICSharpCode.SharpZipLib.Zip.Compression;

namespace WCell.Core
{
	/// <summary>
	/// Wrapper for ICSharpCode.SharpZipLib.
	/// </summary>
	public static class Compression
	{
		/// <summary>
		/// Performs deflate compression on the given data.
		/// </summary>
		/// <param name="input">the data to compress</param>
		/// <param name="output">the compressed data</param>
		public static void CompressZLib(byte[] input, byte[] output, int compressionLevel, out int deflatedLength)
		{
			Deflater item = new Deflater(compressionLevel);

			item.SetInput(input, 0, input.Length);
			item.Finish();
			deflatedLength = item.Deflate(output, 0, output.Length);
		}

		/// <summary>
		/// Performs inflate decompression on the given data.
		/// </summary>
		/// <param name="input">the data to decompress</param>
		/// <param name="output">the decompressed data</param>
		public static void DecompressZLib(byte[] input, byte[] output)
		{
			Inflater item = new Inflater();

			item.SetInput(input, 0, input.Length);
			item.Inflate(output, 0, output.Length);
		}
	}
}
