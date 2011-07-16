using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Util.Synchronization
{
	/// <summary>
	/// When used with the "using" statement, does pretty much the same as the lock statement.
	/// But we use this class so we can easily change the implementation, if required.
	/// </summary>
	public class SimpleLockWrapper
	{
		private readonly LockReleaser releaser = new LockReleaser();

		public IDisposable Enter()
		{
			return releaser;
		}

		class LockReleaser : IDisposable
		{
			public void Dispose()
			{
				Monitor.Exit(this);
			}
		}
	}
}
