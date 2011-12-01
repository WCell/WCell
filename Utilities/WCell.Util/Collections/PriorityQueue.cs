/*************************************************************************
 *
 *   file			: PriorityQueue.cs
 *   copyright		: (C) 2004 Julian M Bucknall 
 *   last changed	: $LastChangedDate: 2009-02-16 05:30:51 +0100 (ma, 16 feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
 *
 *   Written by/rights held by Julian M Bucknall (boyet.com)
 *   http://www.boyet.com/Articles/LockfreeStack.html
 *   http://www.boyet.com/Articles/LockfreeQueue.html
 *   
 *   Modified by WCell
 *
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace WCell.Util.Collections
{
	[Serializable]
	public struct HeapEntry<T>
	{
		private T _item;
		private IComparable _priority;

		public HeapEntry(T item, IComparable priority)
		{
			_item = item;
			_priority = priority;
		}

		public T Item
		{
			get { return _item; }
		}

		public IComparable Priority
		{
			get { return _priority; }
		}

		public void Clear()
		{
			_item = default(T);
			_priority = null;
		}
	}

	[Serializable]
	public class PriorityQueue<T> : IEnumerable<T>, ISerializable
	{
		private int _count;
		private int _capacity;
		private int _version;
		private HeapEntry<T>[] _heap;

		private const string CAPACITY_NAME = "capacity";
		private const string COUNT_NAME = "count";
		private const string HEAP_NAME = "heap";

		public PriorityQueue()
		{
			_capacity = 15; // 15 is equal to 4 complete levels
			_heap = new HeapEntry<T>[_capacity];
		}

		protected PriorityQueue(SerializationInfo info, StreamingContext context)
		{
			_capacity = info.GetInt32(CAPACITY_NAME);
			_count = info.GetInt32(COUNT_NAME);

			var heapCopy = (HeapEntry<T>[])info.GetValue(HEAP_NAME, typeof(HeapEntry<T>[]));
			_heap = new HeapEntry<T>[_capacity];
			Array.Copy(heapCopy, 0, _heap, 0, _count);

			_version = 0;
		}

		public T Dequeue()
		{
			if (_count == 0)
				throw new InvalidOperationException();

			T result = _heap[0].Item;
			_count--;

			TrickleDown(0, _heap[_count]);

			_heap[_count].Clear();
			_version++;

			return result;
		}

		public void Enqueue(T item, IComparable priority)
		{
			if (priority == null)
				throw new ArgumentNullException("priority");

			if (_count == _capacity)
			{
				GrowHeap();
			}

			_count++;
			BubbleUp(_count - 1, new HeapEntry<T>(item, priority));

			_version++;
		}

		private void BubbleUp(int index, HeapEntry<T> he)
		{
			int parent = GetParent(index);
			// note: (index > 0) means there is a parent

			while ((index > 0) &&
				  (_heap[parent].Priority.CompareTo(he.Priority) < 0))
			{
				_heap[index] = _heap[parent];
				index = parent;
				parent = GetParent(index);
			}

			_heap[index] = he;
		}

		private int GetLeftChild(int index)
		{
			return (index * 2) + 1;
		}

		private int GetParent(int index)
		{
			return (index - 1) / 2;
		}

		private void GrowHeap()
		{
			_capacity = (_capacity * 2) + 1;
			var newHeap = new HeapEntry<T>[_capacity];

			Array.Copy(_heap, 0, newHeap, 0, _count);

			_heap = newHeap;
		}

		private void TrickleDown(int index, HeapEntry<T> he)
		{
			int child = GetLeftChild(index);
			while (child < _count)
			{
				if (((child + 1) < _count) &&
					(_heap[child].Priority.CompareTo(_heap[child + 1].Priority) < 0))
				{
					child++;
				}
				_heap[index] = _heap[child];
				index = child;
				child = GetLeftChild(index);
			}
			BubbleUp(index, he);
		}

		#region IEnumerable implementation
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new PriorityQueueEnumerator(this);
		}
		#endregion

		#region IEnumerable<T> implementation
		public IEnumerator<T> GetEnumerator()
		{
			return new PriorityQueueEnumerator(this);
		}
		#endregion

		#region ICollection implementation
		public int Count
		{
			get { return _count; }
		}

		public void CopyTo(Array array, int index)
		{
			Array.Copy(_heap, 0, array, index, _count);
		}

		public object SyncRoot
		{
			get { return this; }
		}

		public bool IsSynchronized
		{
			get { return false; }
		}
		#endregion

		#region ISerializable implementation
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(CAPACITY_NAME, _capacity);
			info.AddValue(COUNT_NAME, _count);

			var heapCopy = new HeapEntry<T>[_count];
			Array.Copy(_heap, 0, heapCopy, 0, _count);

			info.AddValue(HEAP_NAME, heapCopy, typeof(HeapEntry<T>[]));
		}
		#endregion

		#region Priority Queue enumerator
		[Serializable]
		private class PriorityQueueEnumerator : IEnumerator<T>
		{
			private int _index;
			private PriorityQueue<T> _pq;
			private int _version;

			public PriorityQueueEnumerator(PriorityQueue<T> pq)
			{
				_pq = pq;
				Reset();
			}

			private void CheckVersion()
			{
				if (_version != _pq._version)
					throw new InvalidOperationException();
			}

			#region IEnumerator Members

			public void Reset()
			{
				_index = -1;
				_version = _pq._version;
			}

			public T Current
			{
				get
				{
					CheckVersion();

					return _pq._heap[_index].Item;
				}
			}

			public bool MoveNext()
			{
				CheckVersion();

				if (_index + 1 == _pq._count)
					return false;

				_index++;

				return true;
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				_pq = null;
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					CheckVersion();

					return _pq._heap[_index].Item;
				}
			}

			#endregion
		}
		#endregion

	}
}