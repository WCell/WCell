using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util.Threading;
using NLog;
using Cell.Core;

namespace WCell.Core.Database
{
	/// <summary>
	/// Interface for asynchronous query procs.
	/// </summary>
	public abstract class AsyncQuery : IMessage, IExecutionObject
	{
		protected Logger s_log = LogManager.GetCurrentClassLogger();

		public abstract void Execute();
	}
}
