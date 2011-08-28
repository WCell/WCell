/*************************************************************************
 *
 *   file		: SynchronizedList.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-04-05 02:29:47 +0200 (sø, 05 apr 2009) $
 
 *   revision		: $Rev: 864 $
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

namespace WCell.Util.Collections
{
	/// <summary>
	/// Not actually synchronized.
	/// It's especially missing a synchronized enumerator.
	/// </summary>
	public class SynchronizedList<T> : List<T>
	{
		private readonly object _syncLock = new object();

		public SynchronizedList() : base() { }
		public SynchronizedList(int capacity) : base(capacity) { }
		public SynchronizedList(IEnumerable<T> collection) : base(collection) { }

		public new T this[int index]
		{
			get
			{
				if (index > Count)
					throw new ArgumentOutOfRangeException("index");

				T result;

				Monitor.Enter(_syncLock);

				try
				{
					result = base[index];
				}
				finally
				{
					Monitor.Exit(_syncLock);
				}

				return result;
			}
			set
			{
				if (index > Count)
					throw new ArgumentOutOfRangeException("index");

				Monitor.Enter(_syncLock);

				try
				{
					base[index] = value;
				}
				finally
				{
					Monitor.Exit(_syncLock);
				}
			}
		}

		public new void Add(T value)
		{
			Monitor.Enter(_syncLock);

			try
			{
				base.Add(value);
			}
			finally
			{
				Monitor.Exit(_syncLock);
			}
		}

		public new bool Remove(T value)
		{
			Monitor.Enter(_syncLock);

			try
			{
				return base.Remove(value);
			}
			finally
			{
				Monitor.Exit(_syncLock);
			}
		}

		public new void RemoveAt(int index)
		{
			if (index > Count)
				throw new ArgumentOutOfRangeException("index");

			Monitor.Enter(_syncLock);

			try
			{
				base.RemoveAt(index);
			}
			finally
			{
				Monitor.Exit(_syncLock);
			}
		}

		protected void RemoveUnlocked(int index)
		{
			base.RemoveAt(index);
		}

		public new void Clear()
		{
			Monitor.Enter(_syncLock);

			try
			{
				base.Clear();
			}
			finally
			{
				Monitor.Exit(_syncLock);
			}
		}

		public new bool Contains(T item)
		{
			Monitor.Enter(_syncLock);

			try
			{
				return base.Contains(item);
			}
			finally
			{
				Monitor.Exit(_syncLock);
			}
		}

		public void EnterLock()
		{
			Monitor.Enter(_syncLock);
		}

		public void ExitLock()
		{
			Monitor.Exit(_syncLock);
		}
	}
}