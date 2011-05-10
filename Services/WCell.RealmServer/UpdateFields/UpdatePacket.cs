/*************************************************************************
 *
 *   file		: UpdatePacket.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-01-02 17:46:41 +0100 (lø, 02 jan 2010) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1166 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.IO;
using Cell.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;
using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Global;
using WCell.RealmServer.Network;
using NLog;

namespace WCell.RealmServer.UpdateFields
{
	/// <summary>
	/// TODO: Create fully customizable UpdatePacket class
	/// </summary>
	public class UpdatePacket : RealmPacketOut
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public const int DefaultCapacity = 1024;

		public UpdatePacket()
			: base(RealmServerOpCode.SMSG_UPDATE_OBJECT)
		{
			Position = FullUpdatePacketHeaderSize + 2;
		}

		public UpdatePacket(int maxContentLength)
			: base(RealmServerOpCode.SMSG_UPDATE_OBJECT, maxContentLength + FullUpdatePacketHeaderSize)
		{
			Position = FullUpdatePacketHeaderSize + 2;
		}

		/// <summary>
		/// Sends packet (might be compressed)
		/// </summary>
		/// <returns></returns>
		public void SendTo(IRealmClient client)
		{
			if (TotalLength <= WCellConstants.MAX_UNCOMPRESSED_UPDATE_PACKET)
			{
				client.Send(GetFinalizedPacket());
			}
			else
			{
				var segment = ((SegmentStream)BaseStream).Segment;
				var inputOffset = HeaderSize;

				var length = ContentLength;
				if (length > 0x7FFF)
				{
					log.Warn("Sent UpdatePacket with Length {0} to {1} in {2}", length, client,
						client.ActiveCharacter.Zone as IWorldSpace ?? client.ActiveCharacter.Map);
				}

				var maxOutputLength = length + FullUpdatePacketHeaderSize;

				var outSegment = BufferManager.GetSegment(maxOutputLength);

				var deflater = new Deflater(RealmServerConfiguration.CompressionLevel);
				deflater.SetInput(segment.Buffer.Array, segment.Offset + inputOffset, length);
				deflater.Finish();
				int deflatedLength = deflater.Deflate(outSegment.Buffer.Array,
					outSegment.Offset + FullUpdatePacketHeaderSize, length);

				var totalLength = deflatedLength + FullUpdatePacketHeaderSize;

				if (totalLength > MaxPacketSize)
				{
					throw new Exception("Compressed Update packet exceeded max length: " + totalLength);
				}
				SendPacket(client, outSegment, totalLength, length);

				outSegment.DecrementUsage();
			}
		}

		public void Reset()
		{
			Position = 2;
			Write((ushort)m_id.RawId);
			Zero(5);
		}

		static void SendPacket(IRealmClient client, BufferSegment outputBuffer, int totalLength, int actualLength)
		{
			var offset = (uint)outputBuffer.Offset;
			outputBuffer.Buffer.Array.SetUShortBE(offset, (ushort)(totalLength - 2));
			outputBuffer.Buffer.Array.SetBytes(offset + 2,
				BitConverter.GetBytes((ushort)RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT));

			// original length
			outputBuffer.Buffer.Array.SetBytes(offset + HEADER_SIZE, BitConverter.GetBytes((uint)actualLength));

			client.Send(outputBuffer, totalLength);
		}
	}
}