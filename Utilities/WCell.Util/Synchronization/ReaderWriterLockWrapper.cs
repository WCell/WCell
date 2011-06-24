using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WCell.Util
{
	public class ReaderWriterLockWrapper
	{
		readonly ReaderWriterLockSlim lck;
		private readonly ReaderWrapper reader;
		private readonly WriterWrapper writer;

		public ReaderWriterLockWrapper()
		{
			lck = new ReaderWriterLockSlim();
			reader = new ReaderWrapper(lck);
			writer = new WriterWrapper(lck);
		}

		public IDisposable EnterReadLock()
		{
			lck.EnterReadLock();
			return reader;
		}

		public IDisposable EnterWriteLock()
		{
			lck.EnterWriteLock();
			return writer;
		}
		
		struct ReaderWrapper : IDisposable
		{
			readonly ReaderWriterLockSlim lck;

			public ReaderWrapper(ReaderWriterLockSlim lck)
			{
				this.lck = lck;
			}

			public void Dispose()
			{
				lck.ExitReadLock();
			}
		}

		struct WriterWrapper : IDisposable
		{
			readonly ReaderWriterLockSlim lck;

			public WriterWrapper(ReaderWriterLockSlim lck)
			{
				this.lck = lck;
			}

			public void Dispose()
			{
				lck.ExitWriteLock();
			}
		}
	}
}
