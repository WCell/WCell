/*************************************************************************
 *
 *   file			: LockfreeStack.cs
 *   copyright		: (C) 2004 Julian M Bucknall 
 *   last changed	: $LastChangedDate: 2008-11-25 11:16:45 +0100 (ti, 25 nov 2008) $
 
 *   revision		: $Rev: 686 $
 *
 *   Written by/rights held by Julian M Bucknall (boyet.com)
 *   http://www.boyet.com/Articles/LockfreeStack.html
 *   
 *   Modified by WCell
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WCell.Util.Collections
{
	/// <summary>
	/// Represents a lock-free, thread-safe, last-in, first-out collection of objects.
	/// </summary>
	/// <typeparam name="T">specifies the type of the elements in the stack</typeparam>
	public class LockFreeStack<T>
	{
		private SingleLinkNode<T> _head;

		/// <summary>
		/// Default constructors.
		/// </summary>
		public LockFreeStack()
		{
			_head = new SingleLinkNode<T>();
		}

		/// <summary>
		/// Inserts an object at the top of the stack.
		/// </summary>
		/// <param name="item">the object to push onto the stack</param>
		public void Push(T item)
		{
			SingleLinkNode<T> newNode = new SingleLinkNode<T>();
			newNode.Item = item;

			do
			{
				newNode.Next = _head.Next;
			} while (Interlocked.CompareExchange<SingleLinkNode<T>>(ref _head.Next, newNode, newNode.Next) != newNode.Next);
		}

		/// <summary>
		/// Removes and returns the object at the top of the stack.
		/// </summary>
		/// <param name="item">
		/// when the method returns, contains the object removed from the top of the stack, 
		/// if the queue is not empty; otherwise it is the default value for the element type
		/// </param>
		/// <returns>
		/// true if an object from removed from the top of the stack 
		/// false if the stack is empty
		/// </returns>
		public bool Pop(out T item)
		{
			SingleLinkNode<T> node;

			do
			{
				node = _head.Next;

				if (node == null)
				{
					item = default(T);
					return false;
				}
			} while (Interlocked.CompareExchange<SingleLinkNode<T>>(ref _head.Next, node.Next, node) != node);

			item = node.Item;

			return true;
		}

		/// <summary>
		/// Removes and returns the object at the top of the stack.
		/// </summary>
		/// <returns>the object that is removed from the top of the stack</returns>
		public T Pop()
		{
			T result;

			if (!Pop(out result))
				throw new InvalidOperationException("the stack is empty");

			return result;
		}
	}

}