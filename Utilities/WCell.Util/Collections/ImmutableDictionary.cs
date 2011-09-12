/*************************************************************************
 *
 *   file		: ImmutableDictionary.cs
 *   copyright		: (C) 2008 Wilco Bauwer
 *   last changed	: $LastChangedDate: 2009-09-20 22:05:05 +0200 (sø, 20 sep 2009) $
 
 *   revision		: $Rev: 1110 $
 *
 *   Written by/rights held by Wilco Bauwer (wilcob.com)
 *   Modified by WCell
 *   
 *************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace WCell.Util.Collections
{
	/// <summary>
	/// Represents a per-thread immutable collection of keys and values.
	/// </summary>
	/// <remarks>
	/// This class is immutable in the sense of a string; you never change the
	/// collection itself, but rather you create a copy of it, and modify the copy,
	/// which then replaces the old dictionary.  All updates are atomic with the use 
	/// of the <see cref="Interlocked" /> class.
	/// </remarks>
	/// <typeparam name="TKey">the type of the keys in the dictionary</typeparam>
	/// <typeparam name="TValue">the type of the values in the dictionary</typeparam>
	public class ImmutableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private BaseImmutableDictionary<TKey, TValue> _headDictionary;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ImmutableDictionary()
		{
			_headDictionary = new BaseImmutableDictionary<TKey, TValue>();
		}

		/// <summary>
		/// Creates a dictionary with the specified initial capacity.
		/// </summary>
		/// <param name="capacity">the starting capacity of the dictionary</param>
		public ImmutableDictionary(int capacity)
		{
			_headDictionary = new BaseImmutableDictionary<TKey, TValue>(capacity);
		}

		/// <summary>
		/// The number of key/value pairs in the dictionary.
		/// </summary>
		public int Count
		{
			get
			{
				return _headDictionary.Count;
			}
		}

		/// <summary>
		/// Whether or not this dictionary is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Adds the specified key and value to the dictionary.
		/// </summary>
		/// <param name="key">the key of the element to add</param>
		/// <param name="value">the value of the element to add</param>
		public virtual void Add(TKey key, TValue value)
		{
			PromoteChanges(headDictionary => headDictionary.Add(key, value));
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
		{
			PromoteChanges(headDictionary => headDictionary.Add(pair.Key, pair.Value));
		}

		/// <summary>
		/// Removes the value with the specified key from the dictionary.
		/// </summary>
		/// <param name="key">the key of the element to remove</param>
		/// <returns>true if the element is found and removed; false if the key does not exist</returns>
		public virtual bool Remove(TKey key)
		{
			if (ContainsKey(key))
			{
				PromoteChanges(headDictionary => headDictionary.Remove(key));
				return true;
			}

			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair)
		{
			if (ContainsKey(pair.Key))
			{
				PromoteChanges(headDictionary => headDictionary.Remove(pair.Key));
				return true;
			}

			return false;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
		{
			return false;
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
		{
		}

		/// <summary>
		/// Removes all keys and values from the dictionary.
		/// </summary>
		public virtual void Clear()
		{
			PromoteChanges(headDictionary => headDictionary.Clear());
		}

		/// <summary>
		/// Determines whether or not the key is contained in the dictionary.
		/// </summary>
		/// <param name="key">the key to locate in the dictionary</param>
		/// <returns>true if the dictionary contains an element with the specified key; false otherwise</returns>
		public bool ContainsKey(TKey key)
		{
			return _headDictionary.ContainsKey(key);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the dictionary
		/// </summary>
		/// <returns>an enumerator structure for the list</returns>
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _headDictionary.GetEnumerator();
		}

		/// <summary>
		/// Gets a collection containing all the keys in the dictionary.
		/// </summary>
		public ICollection<TKey> Keys
		{
			get
			{
				return _headDictionary.Keys;
			}
		}

		/// <summary>
		/// Gets a collection containing all the values in the dictionary.
		/// </summary>
		public ICollection<TValue> Values
		{
			get
			{
				return _headDictionary.Values;
			}
		}

		private void PromoteChanges(Func<BaseImmutableDictionary<TKey, TValue>, BaseImmutableDictionary<TKey, TValue>> candidateBuilder)
		{
			BaseImmutableDictionary<TKey, TValue> dictionary;
			BaseImmutableDictionary<TKey, TValue> dictionary2;

			do
			{
				dictionary = _headDictionary;
				dictionary2 = candidateBuilder(_headDictionary);
			}
			while (Interlocked.CompareExchange(ref _headDictionary, dictionary2, dictionary) != dictionary);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">the key of the value to get</param>
		/// <param name="value">
		/// when the method returns, contains the value associated with the key, 
		/// if the key is found; otherwise it is the default value for the type 
		/// of the value parameter
		/// </param>
		/// <returns>true if the dictionary contains an element with the specified key; false otherwise</returns>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _headDictionary.TryGetValue(key, out value);
		}

		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		/// <param name="key">the key of the value to get or set</param>
		/// <returns>
		/// the value associated with the specified key; if the key does not exist, 
		/// a get operation will throw a KeyNotFoundException, and a set operation 
		/// creates a new element with the key
		/// </returns>
		public virtual TValue this[TKey key]
		{
			get
			{
				return _headDictionary[key];
			}
			set
			{
				PromoteChanges(headDictionary => headDictionary.SetValue(key, value));
			}
		}
	}
}