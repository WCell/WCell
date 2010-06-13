using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.Core.Network;
using WCell.PacketAnalysis.Logs;
using WCell.Util;

namespace WCell.PacketAnalysis.Updates
{
	public class ParsedUpdatePacket : IParsedPacket, IDisposable
	{
		public static byte[] GetBytes(PacketIn updatePacket)
		{
			var bytes = updatePacket.ReadBytes(updatePacket.Length - updatePacket.HeaderSize);
			if (updatePacket.PacketId.RawId == (uint)RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT)
			{
				bytes = UpdateFieldsUtil.Decompress(bytes);
			}
			else if (updatePacket.PacketId.RawId != (uint)RealmServerOpCode.SMSG_UPDATE_OBJECT)
			{
				throw new Exception("Given Packet " + updatePacket + " is not an Update Packet!");
			}
			return bytes;
		}

		UpdateBlock[] m_blocks;
		public readonly byte[] Bytes;
		public readonly bool SingleBlock;

		public DateTime TimeStamp;

		internal uint index;

		public ParsedUpdatePacket(ParsablePacketInfo info)
			: this(GetBytes(info.Packet), false)
		{
			TimeStamp = info.Timestamp;
		}

		public ParsedUpdatePacket(PacketIn packet)
			: this(GetBytes(packet), false)
		{
		}

		public ParsedUpdatePacket(string line, bool hexaDecimal, bool singleBlock) :
			this(UpdateFieldsUtil.ParseBytes(line, hexaDecimal), singleBlock)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="singleBlock">whether the given bytes represent a single UpdateBlock or a complete Packet</param>
		public ParsedUpdatePacket(byte[] bytes, bool singleBlock) :
			this(DateTime.MinValue, bytes, singleBlock)
		{
		}

		public ParsedUpdatePacket(DateTime timeStamp, byte[] bytes, bool singleBlock)
		{
			TimeStamp = timeStamp;
			Bytes = bytes;
			SingleBlock = singleBlock;


			if (SingleBlock)
			{
				m_blocks = new UpdateBlock[1];
			}
			else
			{
				var count = Bytes.GetUInt32(0);
				m_blocks = new UpdateBlock[count];
				index = 4;
			}

			for (int i = 0; i < m_blocks.Length; i++)
			{
			    m_blocks[i] = new UpdateBlock(this, i);
			}
		}

		public UpdateBlock[] Blocks
		{
			get
			{
				return m_blocks;
			}
		}

		public uint Index
		{
			get
			{
				return index;
			}
		}

		/// <summary>
		/// Returns all blocks of the given type
		/// </summary>
		public IEnumerable<UpdateBlock> GetBlocks(UpdateType type)
		{
			foreach (var block in Blocks)
			{
				if (block.Type == type)
				{
					yield return block;
				}
			}
		}

		public void GetBlocks(UpdateType type, ICollection<UpdateBlock> blocks)
		{
			foreach (var block in Blocks)
			{
				if (block.Type == type)
				{
					blocks.Add(block);
				}
			}
		}

		public void GetBlocks(EntityId id, UpdateType type, ICollection<UpdateBlock> blocks)
		{
			foreach (var block in Blocks)
			{
				if (block.EntityId == id && block.Type == type)
				{
					blocks.Add(block);
				}
			}
		}

		public int RemainingLength
		{
			get
			{
				return Bytes.Length - (int)index;
			}
		}

		public void Dump(IndentTextWriter writer)
		{
			Dump("", writer);
		}

		public void Dump(string indent, IndentTextWriter writer)
		{
			writer.WriteLine(indent + PacketParser.TimeStampcreator(TimeStamp) + " Update Packet:");
			writer.IndentLevel++;
			foreach (var block in m_blocks)
			{
				block.Dump(indent, writer);
				block.Dispose();
				//writer.WriteLine(indent + "#########################################");
				//writer.WriteLine();
			}
			writer.IndentLevel--;
		}

		/*public static void Dump(byte[] packetContent, bool isSingleBlock, IndentTextWriter writer)
		{
			Dump(packetContent, isSingleBlock, writer, false);
		}

		public static void Dump(PacketIn packet, IndentTextWriter writer)
		{
			Dump(GetBytes(packet), false, writer, false);
		}

		public static void Dump(byte[] packetContent, bool isSingleBlock, IndentTextWriter writer, bool isCompressed)
		{
			Dump(DateTime.Now, packetContent, isSingleBlock, writer, isCompressed);
		}*/

		public static void Dump(DateTime timeStamp, byte[] packetContent, bool isSingleBlock, IndentTextWriter writer, bool isCompressed)
		{
			if (isCompressed)
			{
				packetContent = UpdateFieldsUtil.Decompress(packetContent);
			}
			var packet = new ParsedUpdatePacket(packetContent, isSingleBlock);
			writer.WriteLine("");
			packet.Dump("", writer);
		}

		public static ParsedUpdatePacket Create(DateTime time, byte[] packetContent, bool isSingleBlock, bool isCompressed)
		{
			if (isCompressed)
			{
				packetContent = UpdateFieldsUtil.Decompress(packetContent);
			}
			return new ParsedUpdatePacket(time, packetContent, isSingleBlock);
		}

		#region IDisposable Members

		/// <summary>
		/// Removes circular references
		/// </summary>
		public void Dispose()
		{
			foreach (var block in m_blocks)
			{
				block.Dispose();
			}
			m_blocks = null;
		}

		#endregion
	}
}
