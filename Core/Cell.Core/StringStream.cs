/*************************************************************************
 *
 *   file		: StringStream.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-05-19 08:49:10 +0200 (ma, 19 maj 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 368 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Wraps a string for convinient string parsing.
	/// It is using an internal position for the given string so you can read
	/// continuesly the next part.
	/// 
	/// TODO: Make it an actual stream
	/// </summary>
	public class StringStream : ICloneable
	{
		private string _str;
		private int _pos;

		public StringStream(string s)
			: this(s, 0)
		{
		}

		public StringStream(string s, int initialPos)
		{
			this._str = s;
			_pos = initialPos;
		}

		public StringStream(StringStream stream)
			: this(stream._str, stream._pos)
		{
		}

		/// <summary>
		/// Indicates wether we did not reach the end yet.
		/// </summary>
		public bool HasNext
		{
			get
			{
				return _pos < _str.Length;
			}
		}

		/// <summary>
		/// The position within the initial string.
		/// </summary>
		public int Position
		{
			get
			{
				return _pos;
			}
		}

		/// <summary>
		/// The remaining length (from the current position until the end).
		/// </summary>
		public int Length
		{
			get
			{
				return _str.Length - _pos;
			}
		}

		/// <summary>
		/// The remaining string (from the current position until the end).
		/// </summary>
		public string Remainder
		{
			get
			{
				if (!HasNext)
					return "";
				return _str.Substring(_pos, Length);
			}
		}

		/// <summary>
		/// Resets the position to the beginning.
		/// </summary>
		public void Reset()
		{
			_pos = 0;
		}

		/// <summary>
		/// The wrapped string.
		/// </summary>
		public string String
		{
			get
			{
				return _str;
			}
		}

		/// <summary>
		/// [Not implemented]
		/// </summary>
		public string this[int index]
		{
			get
			{

				return "";
			}
		}

		/// <summary>
		/// Increases the position by the given count.
		/// </summary>
		public void Ignore(int charCount)
		{
			_pos += charCount;
		}

		/// <returns><code>NextLong(-1, \" \")</code></returns>
		public long NextLong()
		{
			return NextLong(-1, " ");
		}

		/// <returns><code>NextLong(defaultVal, \" \")</code></returns>
		public long NextLong(long defaultVal)
		{
			return NextLong(defaultVal, " ");
		}

		/// <returns>The next word as long.</returns>
		/// <param name="defaultVal">What should be returned if the next word cannot be converted into a long.</param>
		/// <param name="seperator">What the next word should be seperated by.</param>
		public long NextLong(long defaultVal, string separator)
		{
			try {
				return long.Parse(NextWord(separator));
			}
			catch {
				return defaultVal;
			}
		}

		/// <returns><code>NextInt(-1, \" \")</code></returns>
		public int NextInt()
		{
			return NextInt(-1, " ");
		}

		/// <returns><code>NextInt(defaultVal, \" \")</code></returns>
		public int NextInt(int defaultVal)
		{
			return NextInt(defaultVal, " ");
		}

		/// <returns>The next word as int.</returns>
		/// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
		/// <param name="seperator">What the next word should be seperated by.</param>
		public int NextInt(int defaultVal, string separator)
		{
			try {
				return int.Parse(NextWord(separator));
			}
			catch {
				return defaultVal;
			}
		}

		/// <returns><code>NextUInt(-1, \" \")</code></returns>
		[CLSCompliant(false)]
		public uint NextUInt()
		{
			return NextUInt(0, " ");
		}

		/// <returns><code>NextUInt(defaultVal, \" \")</code></returns>
		[CLSCompliant(false)]
		public uint NextUInt(uint defaultVal)
		{
			return NextUInt(defaultVal, " ");
		}

		/// <returns>The next word as uint.</returns>
		/// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
		/// <param name="seperator">What the next word should be seperated by.</param>
		[CLSCompliant(false)]
		public uint NextUInt(uint defaultVal, string separator)
		{
			try {
				return uint.Parse(NextWord(separator));
			}
			catch {
				return defaultVal;
			}
		}

		/// <returns><code>NextInt(-1, \" \")</code></returns>
		public float NextFloat()
		{
			return NextFloat(0.0f, " ");
		}

		/// <returns><code>NextInt(defaultVal, \" \")</code></returns>
		public float NextFloat(float defaultVal)
		{
			return NextFloat(defaultVal, " ");
		}

		/// <returns>The next word as int.</returns>
		/// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
		/// <param name="seperator">What the next word should be seperated by.</param>
		public float NextFloat(float defaultVal, string separator)
		{
			try {
				return float.Parse(NextWord(separator));
			}
			catch {
				return defaultVal;
			}
		}

		public bool NextBool()
		{
			return NextBool(" ");
		}

		public bool NextBool(string separator)
		{
			try {
				string word = NextWord(separator);
				if (word.Equals("0")) {
					return false;
				}
				else if (word.Equals("1")) {
					return true;
				}
				return bool.Parse(word);
			}
			catch {
				return false;
			}
		}

		/// <summary>
		/// Calls <code>NextEnum(" ")</code>.
		/// </summary>
		public T NextEnum<T>()
		{
			return NextEnum<T>(" ");
		}

		public T NextEnum<T>(string separator)
		{
			return (T)Enum.Parse(typeof(T), NextWord(separator), true);
		}

		/// <summary>
		/// Calls <code>NextWord(" ")</code>.
		/// </summary>
		public string NextWord()
		{
			return NextWord(" ");
		}

		/// <summary>
		/// Moves the position behind the next word in the string, seperated by <code>seperator</code> and returns the word.
		/// </summary>
		public string NextWord(string separator)
		{
			int length = _str.Length;
			if (_pos >= length)
				return "";

			int x;
			while ((x = _str.IndexOf(separator, _pos)) == 0) {
				_pos += separator.Length;
			}

			string word;
			if (x < 0) {
				if (_pos == length)
					return "";
				else
					x = length;
			}
			word = _str.Substring(_pos, x - _pos);

			_pos = x + separator.Length;
			if (_pos > length)
				_pos = length;

			return word;
		}

		/// <returns><code>NextWords(count, \" \")</code></returns>
		public string NextWords(int count)
		{
			return NextWords(count, " ");
		}

		/// <returns>The next <code>count</code> word seperated by <code>seperator</code> as a string.</returns>
		public string NextWords(int count, string separator)
		{
			string result = "";
			for (int i = 0; i < count && HasNext; i++) {
				if (i > 0)
					result += separator;
				result += NextWord(separator);
			}
			return result;
		}

		/// <returns><code>NextWordsArray(count, " ")</code></returns>
		public string[] NextWordsArray(int count)
		{
			return NextWordsArray(count, " ");
		}

		/// <returns>The next <code>count</code> word seperated by <code>seperator</code> as an array of strings.</returns>
		public string[] NextWordsArray(int count, string separator)
		{
			string[] words = new string[count];
			for (int i = 0; i < count && HasNext; i++) {
				words[i] = NextWord(separator);
			}
			return words;
		}

		/// <summary>
		/// Calls <code>RemainingWords(" ")</code>
		/// </summary>
		public string[] RemainingWords()
		{
			return RemainingWords(" ");
		}

		public string[] RemainingWords(string separator)
		{
			List<string> words = new List<string>();
			while (HasNext) {
				words.Add(NextWord(separator));
			}
			return words.ToArray();
		}

		/// <returns><code>Consume(' ')</code></returns>
		public void ConsumeSpace()
		{
			Consume(' ');
		}

		/// <summary>
		/// Calls <code>SkipWord(" ")</code>.
		/// </summary>
		public void SkipWord()
		{
			SkipWord(" ");
		}

		/// <summary>
		/// Skips the next word, seperated by the given seperator.
		/// </summary>
		public void SkipWord(string separator)
		{
			SkipWords(1, separator);
		}

		/// <summary>
		/// Calls <code>SkipWords(count, " ")</code>.
		/// </summary>
		/// <param name="count">The amount of words to be skipped.</param>
		public void SkipWords(int count)
		{
			SkipWords(count, " ");
		}

		/// <summary>
		/// Skips <code>count</code> words, seperated by the given seperator.
		/// </summary>
		/// <param name="count">The amount of words to be skipped.</param>
		public void SkipWords(int count, string separator)
		{
			NextWords(count, separator);
		}

		/// <summary>
		/// Consume a whole string, as often as it occurs.
		/// </summary>
		public void Consume(string rs)
		{
			while (HasNext) {
				int i = 0;
				for (; i < rs.Length; i++) {
					if (_str[_pos + i] != rs[i]) {
						return;
					}
				}
				_pos += i;
			}
		}

		/// <summary>
		/// Ignores all directly following characters that are equal to <code>c</code>.
		/// </summary>
		public void Consume(char c)
		{
			while (HasNext && _str[_pos] == c)
				_pos++;
		}

		/// <summary>
		/// Ignores a maximum of <code>amount</code> characters that are equal to <code>c</code>.
		/// </summary>
		public void Consume(char c, int amount)
		{
			for (int i = 0; i < amount && HasNext && _str[_pos] == c; i++)
				_pos++;
		}

		/// <summary>
		/// Consumes the next character, if it equals <code>c</code>.
		/// </summary>
		/// <returns>Wether the character was equal to <code>c</code> (and thus has been deleted)</returns>
		public bool ConsumeNext(char c)
		{
			if (HasNext && _str[_pos] == c) {
				_pos++;
				return true;
			}
			return false;
		}

		/// <returns>Wether or not the remainder contains the given string.</returns>
		public bool Contains(string s)
		{
            return s.IndexOf(s, _pos) > -1;
		}

		/// <returns>Wether or not the remainder contains the given char.</returns>
		public bool Contains(char c)
		{
            return _str.IndexOf(c, _pos) > -1;
		}

		public override string ToString()
		{
			return Remainder.Trim();
		}

		public StringStream CloneStream()
		{
			return Clone() as StringStream;
		}

		public Object Clone()
		{
			StringStream ss = new StringStream(_str);
			ss._pos = _pos;
			return ss;
		}
	}
}
