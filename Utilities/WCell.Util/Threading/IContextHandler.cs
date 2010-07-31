using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Util.Threading
{
	/// <summary>
	/// A ContextHandlers usually represents a set of instructions to be performed by one Thread at a time and allows
	/// Messages to be dispatched.
	/// </summary>
	public interface IContextHandler
	{
		/// <summary>
		/// Whether the currently executing Thread belongs to this ContextHandler
		/// </summary>
		bool IsInContext { get; }

		void AddMessage(IMessage message);

		void AddMessage(Action action);

		/// <summary>
		/// Executes action now or enqueues a Message, depending on the current Context
		/// </summary>
		bool ExecuteInContext(Action action);

		void EnsureContext();
	}

	public static class _ContextUtil
	{
		/// <summary>
		/// Lets the given ContextHandler wait one Tick. Does nothing if within the given Handler's Context.
		/// </summary>
		/// <param name="contextHandler"></param>
		public static void WaitOne(this IContextHandler contextHandler)
		{
			var obj = new Object();

			if (!contextHandler.IsInContext)
			{
				lock (obj)
				{
					contextHandler.AddMessage(() =>
					{
						lock (obj)
						{
							Monitor.PulseAll(obj);
						}
					});

					Monitor.Wait(obj);
				}
			}
		}
	}
}