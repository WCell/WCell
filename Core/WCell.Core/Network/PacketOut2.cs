using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cell.Core;

namespace WCell.Core.Network
{
	public unsafe abstract class PacketOut2
	{
		private static BufferManager s_bufMgr = new BufferManager(1000, 512);
		private LinkedList<ArraySegment<byte>> m_bufferList;
		private byte* m_buffer;
		private int m_position, m_length;

		public PacketOut2()
		{
			IncreaseCapacity();
		}

		private void IncreaseCapacity()
		{
			var buf = s_bufMgr.CheckOut();

			m_bufferList.AddLast(buf);

			m_length += buf.Count;

			fixed (byte* bufPtr = &buf.Array[buf.Offset])
			{
				m_buffer = bufPtr;
			}
		}

		public void WriteInt(int value)
		{
			if (m_position + 4 <= m_length)
			{
				*(int*)&m_buffer[m_position] = value;
				m_position += 4;
			}
			else
			{
				int bytePos = 0;
				int startLength = m_length - m_position;

				for (int start = 0; start < startLength; start++, bytePos += 8)
				{
					*(&m_buffer[m_position++]) = (byte)(value >> bytePos);
				}

				IncreaseCapacity();

				int endLength = 4 - startLength;

				for (int end = 0; end < endLength; end++, bytePos += 8)
				{
					*(&m_buffer[m_position++]) = (byte)(value >> bytePos);
				}
			}
		}
	}
}
