using System;
using System.Diagnostics;
using System.IO;

namespace Cell.Core
{
	/// <summary>
	/// Similar to MemoryStream, but with an underlying BufferSegment.
	/// Will automatically free the old and get a new segment if its length was exceeded.
	/// </summary>
	public class SegmentStream : Stream
	{
		private int m_Position;
		private BufferSegment _segment;
		private int __length;
		private int _length
		{
			get { return __length; }
			set { __length = value; }
		}

		public SegmentStream(BufferSegment segment)
		{
			_segment = segment;
			m_Position = _segment.Offset;
		}

		public BufferSegment Segment
		{
			get { return _segment; }
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					m_Position = (int)offset;
					break;
				case SeekOrigin.Current:
					m_Position += (int)offset;
					break;
				case SeekOrigin.End:
					m_Position = _segment.Offset + _segment.Length - (int)offset;
					break;
			}
			if (m_Position > _segment.Length)
			{
				m_Position = _segment.Length;
			}
			return m_Position;
		}

		public override void SetLength(long value)
		{
			_length = (int)value;
			if (m_Position > _length)
			{
				m_Position = _length + _segment.Offset;
			}
			if (_length > _segment.Length)
			{
				EnsureCapacity(_length);
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			count = Math.Min(count, _segment.Offset + _segment.Length - m_Position);
			Buffer.BlockCopy(_segment.Buffer.Array, m_Position, buffer, offset, count);
			m_Position += count;
			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (m_Position + count >= _segment.Offset + _segment.Length)
			{
				EnsureCapacity(m_Position - _segment.Offset + count);
			}
			Buffer.BlockCopy(buffer, offset, _segment.Buffer.Array, m_Position, count);
			m_Position += count;
			_length = Math.Max(_length, m_Position - _segment.Offset);
#if DEBUG
			//CheckOpcode(count);
#endif
		}

		public override int ReadByte()
		{
			return _segment.Buffer.Array[m_Position++];
		}

		public override void WriteByte(byte value)
		{
			if (m_Position + 1 >= _segment.Offset + _segment.Length)
			{
				EnsureCapacity(m_Position - _segment.Offset + 1);
			}
			_segment.Buffer.Array[m_Position++] = value;
			_length = Math.Max(_length, m_Position - _segment.Offset);
#if DEBUG
			//CheckOpcode(1);
#endif
		}

		void CheckOpcode(int count)
		{
			//var opcode = (RealmServerOpCode)(_segment.Buffer.Array[_segment.Offset + 2] | (_segment.Buffer.Array[_segment.Offset + 3] << 8));
			//if (!Enum.IsDefined(typeof(RealmServerOpCode), opcode))
			//{
			//    string extra;
			//    if (count > 0)
			//    {
			//        extra = string.Format(" when writing bytes {0} to {1}", Position - count, count);
			//    }  
			//    else
			//    {
			//        extra = " when writing no bytes at all.";
			//    }

			//    lock (Console.Out)
			//    {
			//        Console.WriteLine("Invalid opcode {0} in stream" + extra, opcode);
			//    }
			//}
		}

		private void EnsureCapacity(int size)
		{
			// return the old segment and get a new, bigger one
			var newSegment = BufferManager.GetSegment(size);
			_segment.CopyTo(newSegment, _length);
			m_Position = m_Position - _segment.Offset + newSegment.Offset;

			_segment.DecrementUsage();
			_segment = newSegment;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return true; }
		}

		public override long Length
		{
			get { return _length; }
		}

		public override long Position
		{
			get { return m_Position - _segment.Offset; }
			set { m_Position = (int)value + _segment.Offset; }
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_segment.DecrementUsage();
		}
	}
}