/*************************************************************************
 *
 *   file		: ObjectPool.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-04-21 16:29:23 +0200 (on, 21 apr 2010) $
 *   last author	: $LastChangedBy: XTZGZoReX $
 *   revision		: $Rev: 1281 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using WCell.Util.Collections;

#pragma warning disable 0420

namespace Cell.Core
{
	/// <summary>
	/// A structure that contains information about an object pool.
	/// </summary>
	public struct ObjectPoolInfo
	{
		/// <summary>
		/// The number of hard references contained in the pool.
		/// </summary>
		public int HardReferences;

		/// <summary>
		/// The number of weak references contained in the pool.
		/// </summary>
		public int WeakReferences;


		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="weak">The number of weak references in the pool.</param>
		/// <param name="hard">The number of hard references in the pool.</param>
		public ObjectPoolInfo(int weak, int hard)
		{
			HardReferences = hard;
			WeakReferences = weak;
		}
	}

	/// <summary>
	/// This class represents a pool of objects.
	/// </summary>
	public class ObjectPool<T> : IObjectPool where T : class
	{
		private bool m_IsBalanced;

		/// <summary>
		/// A queue of objects in the pool.
		/// </summary>
		private readonly LockfreeQueue<object> _queue = new LockfreeQueue<object>();

		/// <summary>
		/// The minimum # of hard references that must be in the pool.
		/// </summary>
		private volatile int _minSize = 25;

		/// <summary>
		/// The number of hard references in the queue.
		/// </summary>
		private volatile int _hardReferences = 0;

		/// <summary>
		/// The number of hard references in the queue.
		/// </summary>
		private volatile int _obtainedReferenceCount;

		/// <summary>
		/// Function pointer to the allocation function.
		/// </summary>
		private readonly Func<T> _createObj;

		/// <summary>
		/// Gets the number of hard references that are currently in the pool.
		/// </summary>
		public int HardReferenceCount
		{
			get { return _hardReferences; }
		}

		/// <summary>
		/// Gets the minimum size of the pool.
		/// </summary>
		public int MinimumSize
		{
			get { return _minSize; }
			set { _minSize = value; }
		}

		public int AvailableCount
		{
			get { return _queue.Count; }
		}

		public int ObtainedCount
		{
			get { return _obtainedReferenceCount; }
		}

		/// <summary>
		/// Gets information about the object pool.
		/// </summary>
		/// <value>A new <see cref="ObjectPoolInfo"/> object that contains information about the pool.</value>
		public ObjectPoolInfo Info
		{
			get
			{
				ObjectPoolInfo info;
				info.HardReferences = _hardReferences;
				info.WeakReferences = _queue.Count - _hardReferences;

				return info;
			}
		}

		public bool IsBalanced
		{
			get { return m_IsBalanced; }
			set { m_IsBalanced = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="func">Function pointer to the allocation function.</param>
		public ObjectPool(Func<T> func) : this(func, false)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="func">Function pointer to the allocation function.</param>
		public ObjectPool(Func<T> func, bool isBalanced)
		{
			IsBalanced = isBalanced;
			_createObj = func;
		}

		/// <summary>
		/// Adds an object to the queue.
		/// </summary>
		/// <param name="obj">The object to be added.</param>
		/// <remarks>If there are at least <see cref="ObjectPool&lt;T&gt;.MinimumSize"/> hard references in the pool then the object is added as a WeakReference.
		/// A WeakReference allows an object to be collected by the GC if there are no other hard references to it.</remarks>
		public void Recycle(T obj)
		{
			if (obj is IPooledObject)
			{
				((IPooledObject)obj).Cleanup();
			}

			if (_hardReferences >= _minSize)
			{
				_queue.Enqueue(new WeakReference(obj));
			}
			else
			{
				_queue.Enqueue(obj);
				Interlocked.Increment(ref _hardReferences);
			}

			if (m_IsBalanced)
			{
				OnRecycle();
			}
		}

		/// <summary>
		/// Adds an object to the queue.
		/// </summary>
		/// <param name="obj">The object to be added.</param>
		/// <remarks>If there are at least <see cref="ObjectPool&lt;T&gt;.MinimumSize"/> hard references in the pool then the object is added as a WeakReference.
		/// A WeakReference allows an object to be collected by the GC if there are no other hard references to it.</remarks>
		public void Recycle(object obj)
		{
			if (obj is T)
			{
				if (obj is IPooledObject)
				{
					((IPooledObject)obj).Cleanup();
				}

				if (_hardReferences >= _minSize)
				{
					_queue.Enqueue(new WeakReference(obj));
				}
				else
				{
					_queue.Enqueue(obj);
					Interlocked.Increment(ref _hardReferences);
				}

				if (m_IsBalanced)
				{
					OnRecycle();
				}
			}
		}

		private void OnRecycle()
		{
			if (Interlocked.Decrement(ref _obtainedReferenceCount) < 0)
			{
				throw new InvalidOperationException("Objects in Pool have been recycled too often: " + this);
			}
		}

#pragma warning disable 0693
		/// <summary>
		/// Removes an object from the queue.
		/// </summary>
		/// <returns>An object from the queue or a new object if none were in the queue.</returns>

		public T Obtain()
		{
			if (m_IsBalanced)
			{
				Interlocked.Increment(ref _obtainedReferenceCount);
			}

		DequeueObj:
			{
				object obj;
				if (!_queue.TryDequeue(out obj))
				{
					return _createObj();
				}

				if (obj is WeakReference)
				{
				    var robj = ((WeakReference)obj).Target;
                    if (robj != null)
                        return robj as T;

					goto DequeueObj;
				}

				Interlocked.Decrement(ref _hardReferences);
				return obj as T;
			}
		}
#pragma warning restore 0693

		/// <summary>
		/// Removes an object from the queue.
		/// </summary>
		/// <returns>An object from the queue or a new object if none were in the queue.</returns>
		public object ObtainObj()
		{
			object obj;
			if (m_IsBalanced)
			{
				Interlocked.Increment(ref _obtainedReferenceCount);
			}

		DequeueObj:
			{
				if (!_queue.TryDequeue(out obj))
				{
					return _createObj();
				}

				var robj = obj as WeakReference;
				if (robj != null)
				{
				    var robj2 = robj.Target;
                    if (robj2 != null)
                        return robj2;

					goto DequeueObj;
				}
				Interlocked.Decrement(ref _hardReferences);
			}

			return obj;
		}

		public override string ToString()
		{
			return GetType().Name + " for " + typeof(T).FullName;
		}
	}
}

#pragma warning restore 0420