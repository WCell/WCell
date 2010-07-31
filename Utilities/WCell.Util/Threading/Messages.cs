/*************************************************************************
 *
 *   file			: Message.cs
 *   author			: Thomas "zeroflag" Kraemer
 *   author email	: zeroflag@zeroflag.de
 *   copyright		: (C) 2006-2008  Thomas "zeroflag" Kraemer
 *   last changed	: $LastChangedDate: 2008-11-25 18:16:45 +0800 (Tue, 25 Nov 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 686 $
 *
 *   This library is free software; you can redistribute it and/or
 *   modify it under the terms of the GNU Lesser General Public
 *   License as published by the Free Software Foundation; either
 *   version 2.1 of the License, or (at your option) any later version.
 *   
 *   Modifications by Toby "tobz" Lawrence for WCell.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Util.Threading
{
	/// <summary>
	/// Defines the interface of a message.
	/// </summary>
	public interface IMessage
	{
		/// <summary>
		/// Executes the message.
		/// </summary>
		void Execute();
	}

	/// <summary>
	/// Defines a message with no input parameters.
	/// </summary>
	public class Message : IMessage
	{
		//private static LockfreeQueue<Message> msgPool;

		/// <summary>
		/// Returns a recycled or new Message object with the given callback.
		/// TODO: Object recycling
		/// </summary>
		public static Message Obtain(Action callback)
		{
			return new Message(callback);
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Message()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public Message(Action callback)
		{
			Callback = callback;
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

		public static implicit operator Message(Action dele)
		{
			return new Message(dele);
		}

		public override string ToString()
		{
			return Callback.ToString();
		}
	}


	/// <summary>
	/// Rather performance-hungry message to ensure that a task
	/// executed before continuing
	/// </summary>
	public class WaitMessage : Message
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
		/// Waits until this Message executed.
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

	#region Message1
	/// <summary>
	/// Defines a message with one input parameter.
	/// </summary>
	/// <typeparam name="T1">the type of the first input parameter</typeparam>
	public class Message1<T1> : IMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public Message1()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public Message1(Action<T1> callback)
		{
			Callback = callback;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameter.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		public Message1(T1 param1, Action<T1> callback)
		{
			Callback = callback;
			Parameter1 = param1;
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

		public static explicit operator Message1<T1>(Action<T1> dele)
		{
			return new Message1<T1>(dele);
		}
	}
	#endregion

	#region Message2
	public class Message2<T1, T2> : IMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public Message2()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public Message2(Action<T1, T2> callback)
		{
			Callback = callback;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		public Message2(T1 param1, T2 param2, Action<T1, T2> callback) {
			Callback = callback;
			Parameter1 = param1;
			Parameter2 = param2;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		public Message2(T1 param1, T2 param2)
		{
			Parameter1 = param1;
			Parameter2 = param2;
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

		public static explicit operator Message2<T1, T2>(Action<T1, T2> dele)
		{
			return new Message2<T1, T2>(dele);
		}
	}
	#endregion

	#region Message3
	public class Message3<T1, T2, T3> : IMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public Message3()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public Message3(Action<T1, T2, T3> callback)
		{
			Callback = callback;
		}

		/// <summary>
		/// Constructs a message with the specific callback and input parameters.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		/// <param name="param1">the first input parameter</param>
		/// <param name="param2">the second input parameter</param>
		/// <param name="param3">the third input parameter</param>
		public Message3(T1 param1, T2 param2, T3 param3, Action<T1, T2, T3> callback)
		{
			Callback = callback;
			Parameter1 = param1;
			Parameter2 = param2;
			Parameter3 = param3;
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

		public static explicit operator Message3<T1, T2, T3>(Action<T1, T2, T3> dele)
		{
			return new Message3<T1, T2, T3>(dele);
		}
	}
	#endregion

	#region Message4
	public class Message4<T1, T2, T3, T4> : IMessage
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public Message4()
		{
		}

		/// <summary>
		/// Constructs a message with the specific callback.
		/// </summary>
		/// <param name="callback">the callback to invoke when the message is executed</param>
		public Message4(Action<T1, T2, T3, T4> callback)
		{
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
		public Message4(Action<T1, T2, T3, T4> callback, T1 param1, T2 param2, T3 param3, T4 param4)
		{
			Callback = callback;
			Parameter1 = param1;
			Parameter2 = param2;
			Parameter3 = param3;
			Parameter4 = param4;
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

		public static explicit operator Message4<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
		{
			return new Message4<T1, T2, T3, T4>(callback);
		}
	}
	#endregion
}