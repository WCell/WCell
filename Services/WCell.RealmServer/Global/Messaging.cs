using System;
using System.Threading;

namespace WCell.RealmServer.Global
{
	public enum RealmMessageBoundary
	{
		Global,
		Mapal
	}

	/// <summary>
	/// Defines the interface of a message.
	/// </summary>
	public interface IRealmMessage
	{
		/// <summary>
		/// Indicates where the message is valid.
		/// Mapal messages must be disposed when object is moved to a different Map.
		/// </summary>
		RealmMessageBoundary Boundary
		{
			get;
		}

		/// <summary>
		/// Executes the message.
		/// </summary>
		void Execute();
	}

	/// <summary>
	/// Defines a message with no input parameters.
	/// </summary>
	public class RealmMessage : IRealmMessage
	{
		//private static LockfreeQueue<RealmMessage> msgPool;

		/// <summary>
		/// Returns a recycled or new RealmMessage object with the given callback.
		/// TODO: Object recycling
		/// </summary>
		public static RealmMessage Obtain(Action callback)
		{
			return new RealmMessage(callback);
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RealmMessage()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage(Action callback)
			: this(callback, RealmMessageBoundary.Global)
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage(Action callback, RealmMessageBoundary boundary)
		{
			Callback = callback;
			Boundary = boundary;
		}

		public RealmMessageBoundary Boundary
		{
			get;
			private set;
		}

		/// <summary>
		/// The callback that is called when the message is executed.
		/// </summary>
		public Action Callback
		{
			get;
			private set;
		}

		/// <summary>
		/// Executes the message, calling any callbacks that are bound.
		/// </summary>
		public virtual void Execute()
		{
			Action cb = Callback;
			if (cb != null)
			{
				cb();
			}
		}

		public static implicit operator RealmMessage(Action dele)
		{
			return new RealmMessage(dele);
		}
	}


	/// <summary>
	/// Rather performance-hungry message to ensure that a task
	/// executed before continuing
	/// </summary>
	public class WaitRealmMessage : RealmMessage
	{
		bool m_executed;

		public override void Execute()
		{
			try
			{
				base.Execute();
			}
			finally
			{
				lock (this)
				{
					m_executed = true;
					Monitor.PulseAll(this);
				}
			}
		}

		/// <summary>
		/// Waits until this RealmMessage executed.
		/// </summary>
		public void Wait()
		{
			if (!m_executed)
			{
				lock (this)
				{
					Monitor.Wait(this);
				}
			}
		}
	}

	#region RealmMessage1
	/// <summary>
	/// Defines a message with one input parameter.
	/// </summary>
	/// <typeparam name="T1">the type of the first input parameter</typeparam>
	public class RealmMessage1<T1> : IRealmMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public RealmMessage1()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage1(Action<T1> callback)
		{
			Callback = callback;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameter.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		public RealmMessage1(T1 param1, Action<T1> callback)
			: this(param1, callback, RealmMessageBoundary.Global)
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameter.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		public RealmMessage1(T1 param1, Action<T1> callback, RealmMessageBoundary boundary)
		{
			Callback = callback;
			Boundary = boundary;
			Parameter1 = param1;
		}

		public RealmMessageBoundary Boundary
		{
			get;
			private set;
		}

		/// <summary>
		/// The callback that is called when the message is executed.
		/// </summary>
		public Action<T1> Callback
		{
			get;
			set;
		}

		/// <summary>
		/// The first input parameter.
		/// </summary>
		public T1 Parameter1
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the message, calling any callbacks that are bound, passing the given input parameters.
		/// </summary>
		public virtual void Execute()
		{
			Action<T1> cb = Callback;
			if (cb != null)
			{
				cb(Parameter1);
			}
		}

		public static explicit operator RealmMessage1<T1>(Action<T1> dele)
		{
			return new RealmMessage1<T1>(dele);
		}
	}
	#endregion

	#region RealmMessage2
	public class RealmMessage2<T1, T2> : IRealmMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public RealmMessage2()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage2(Action<T1, T2> callback)
			: this(callback, RealmMessageBoundary.Global)
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage2(Action<T1, T2> callback, RealmMessageBoundary boundary)
		{
			Callback = callback;
			Boundary = boundary;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		public RealmMessage2(T1 param1, T2 param2, Action<T1, T2> callback)
			: this(param1, param2, callback, RealmMessageBoundary.Global)
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		public RealmMessage2(T1 param1, T2 param2, Action<T1, T2> callback, RealmMessageBoundary boundary)
		{
			Callback = callback;
			Boundary = boundary;
			Parameter1 = param1;
			Parameter2 = param2;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		public RealmMessage2(T1 param1, T2 param2)
		{
			Parameter1 = param1;
			Parameter2 = param2;
		}

		public RealmMessageBoundary Boundary
		{
			get;
			private set;
		}

		/// <summary>
		/// The callback that is called when the message is executed.
		/// </summary>
		public Action<T1, T2> Callback
		{
			get;
			set;
		}

		/// <summary>
		/// The first input parameter.
		/// </summary>
		public T1 Parameter1
		{
			get;
			set;
		}

		/// <summary>
		/// The second input parameter.
		/// </summary>
		public T2 Parameter2
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the message, calling any callbacks that are bound, passing the given input parameters.
		/// </summary>
		public virtual void Execute()
		{
			Action<T1, T2> cb = Callback;
			if (cb != null)
			{
				cb(Parameter1, Parameter2);
			}
		}

		public static explicit operator RealmMessage2<T1, T2>(Action<T1, T2> dele)
		{
			return new RealmMessage2<T1, T2>(dele);
		}
	}
	#endregion

	#region RealmMessage3
	public class RealmMessage3<T1, T2, T3> : IRealmMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public RealmMessage3()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage3(Action<T1, T2, T3> callback)
			: this(callback, RealmMessageBoundary.Global)
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage3(Action<T1, T2, T3> callback, RealmMessageBoundary boundary)
		{
			Boundary = boundary;
			Callback = callback;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		/// <param name="param3">the third input parameter</param>
		public RealmMessage3(T1 param1, T2 param2, T3 param3, Action<T1, T2, T3> callback, RealmMessageBoundary boundary)
		{
			Boundary = boundary;
			Callback = callback;
			Parameter1 = param1;
			Parameter2 = param2;
			Parameter3 = param3;
		}

		public RealmMessageBoundary Boundary
		{
			get;
			private set;
		}

		/// <summary>
		/// The callback that is called when the message is executed.
		/// </summary>
		public Action<T1, T2, T3> Callback
		{
			get;
			set;
		}

		/// <summary>
		/// The first input parameter.
		/// </summary>
		public T1 Parameter1
		{
			get;
			set;
		}

		/// <summary>
		/// The second input parameter.
		/// </summary>
		public T2 Parameter2
		{
			get;
			set;
		}

		/// <summary>
		/// The third input parameter.
		/// </summary>
		public T3 Parameter3
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the message, calling any callbacks that are bound, passing the given input parameters.
		/// </summary>
		public virtual void Execute()
		{
			Action<T1, T2, T3> cb = Callback;
			if (cb != null)
			{
				cb(Parameter1, Parameter2, Parameter3);
			}
		}

		public static explicit operator RealmMessage3<T1, T2, T3>(Action<T1, T2, T3> dele)
		{
			return new RealmMessage3<T1, T2, T3>(dele);
		}
	}
	#endregion

	#region RealmMessage4
	public class RealmMessage4<T1, T2, T3, T4> : IRealmMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public RealmMessage4()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage4(Action<T1, T2, T3, T4> callback)
			: this(callback, RealmMessageBoundary.Global)
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public RealmMessage4(Action<T1, T2, T3, T4> callback, RealmMessageBoundary boundary)
		{
			Boundary = boundary;
			Callback = callback;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		/// <param name="param3">the third input parameter</param>
		/// <param name="param4">the fourth input parameter</param>
		public RealmMessage4(Action<T1, T2, T3, T4> callback, T1 param1, T2 param2, T3 param3, T4 param4, RealmMessageBoundary boundary)
		{
			Boundary = boundary;
			Callback = callback;
			Parameter1 = param1;
			Parameter2 = param2;
			Parameter3 = param3;
			Parameter4 = param4;
		}

		public RealmMessageBoundary Boundary
		{
			get;
			private set;
		}

		/// <summary>
		/// The callback that is called when the message is executed.
		/// </summary>
		public Action<T1, T2, T3, T4> Callback
		{
			get;
			set;
		}

		/// <summary>
		/// The first input parameter.
		/// </summary>
		public T1 Parameter1
		{
			get;
			set;
		}

		/// <summary>
		/// The second input parameter.
		/// </summary>
		public T2 Parameter2
		{
			get;
			set;
		}

		/// <summary>
		/// The third input parameter.
		/// </summary>
		public T3 Parameter3
		{
			get;
			set;
		}

		/// <summary>
		/// The fourth input parameter.
		/// </summary>
		public T4 Parameter4
		{
			get;
			set;
		}

		/// <summary>
		/// Executes the message, calling any callbacks that are bound, passing the given input parameters.
		/// </summary>
		public virtual void Execute()
		{
			Action<T1, T2, T3, T4> cb = Callback;
			if (cb != null)
			{
				cb(Parameter1, Parameter2, Parameter3, Parameter4);
			}
		}

		public static explicit operator RealmMessage4<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
		{
			return new RealmMessage4<T1, T2, T3, T4>(callback);
		}
	}
	#endregion
}