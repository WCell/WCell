using System;
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
		/// Whether this ContextHandler currently belongs to the calling Thread
		/// </summary>
		bool IsInContext { get; }

		void AddMessage(IMessage message);

		void AddMessage(Action action);

		/// <summary>
		/// Executes action instantly, if in context.
		/// Enqueues a Message to execute it later, if not in context.
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