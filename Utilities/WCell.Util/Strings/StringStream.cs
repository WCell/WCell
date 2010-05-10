/*************************************************************************
 *
 *   file		: StringStream.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-10 11:07:44 +0800 (Mon, 10 Mar 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 191 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;

namespace WCell.Util.Strings
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
		readonly string str;
		int pos;

		public StringStream(string s)
			: this(s, 0)
		{
		}

		public StringStream(string s, int initialPos)
		{
			str = s;
			pos = initialPos;
		}

		public StringStream(StringStream stream)
			: this(stream.str, stream.pos)
		{
		}

		/// <summary>
		/// Indicates whether we did not reach the end yet.
		/// </summary>
		public bool HasNext
		{
			get
			{
				return pos < str.Length;
			}
		}

		/// <summary>
		/// The position within the initial string.
		/// </summary>
		public int Position
		{
			get
			{
				return pos;
			}
			set
			{
				pos = value;
			}
		}

		/// <summary>
		/// The remaining length (from the current position until the end).
		/// </summary>
		public int Length
		{
			get
			{
				return str.Length - pos;
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
				return str.Substring(pos, Length);
			}
		}

		/// <summary>
		/// The wrapped string.
		/// </summary>
		public string String
		{
			get
			{
				return str;
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
		/// Resets the position to the beginning.
		/// </summary>
		public void Reset()
		{
			pos = 0;
		}

		/// <summary>
		/// Increases the position by the given count.
		/// </summary>
		public void Skip(int charCount)
		{
			pos += charCount;
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
		/// <param name="separator">What the next word should be seperated by.</param>
		public long NextLong(long defaultVal, string separator)
		{
			try
			{
				return long.Parse(NextWord(separator));
			}
			catch
			{
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
		/// <param name="separator">What the next word should be seperated by.</param>
		public int NextInt(int defaultVal, string separator)
		{
			int result;
			if (int.TryParse(NextWord(separator), out result))
			{
				return result;
			}
			return defaultVal;
		}

		/// <returns><code>NextUInt(-1, \" \")</code></returns>
		public uint NextUInt()
		{
			return NextUInt(0, " ");
		}

		/// <returns><code>NextUInt(defaultVal, \" \")</code></returns>
		public uint NextUInt(uint defaultVal)
		{
			return NextUInt(defaultVal, " ");
		}

		/// <returns>The next word as uint.</returns>
		/// <param name="defaultVal">What should be returned if the next word cannot be converted into an int.</param>
		/// <param name="separator">What the next word should be seperated by.</param>
		public uint NextUInt(uint defaultVal, string separator)
		{
			uint result;
			if (uint.TryParse(NextWord(separator), out result))
			{
				return result;
			}
			return defaultVal;
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
		/// <param name="separator">What the next word should be seperated by.</param>
		public float NextFloat(float defaultVal, string separator)
		{
			float result;
			if (float.TryParse(NextWord(separator), out result))
			{
				return result;
			}
			return defaultVal;
		}

		public char NextChar()
		{
			if (!HasNext)
			{
				return '\0';
			}
			return str[pos++];
		}

		public bool NextBool()
		{
			return NextBool(" ");
		}

		public bool NextBool(bool dflt)
		{
			if (HasNext)
			{
				return NextBool(" ");
			}
			return dflt;
		}

		public bool NextBool(string separator)
		{
			var word = NextWord(separator);
			return GetBool(word);
		}

		public static bool GetBool(string word)
		{
			if (word.Equals("1") ||
				word.StartsWith("y", StringComparison.InvariantCultureIgnoreCase) ||
				word.Equals("true", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Calls <code>NextEnum(" ")</code>.
		/// </summary>
		public T NextEnum<T>(T defaultVal)
		{
			if (!HasNext)
			{
				return defaultVal;
			}
			return NextEnum(" ", defaultVal);
		}

		public T NextEnum<T>(string separator)
		{
			return NextEnum((T)Enum.GetValues(typeof(T)).GetValue(0));
		}

		public T NextEnum<T>(string separator, T defaultVal)
		{
			//return (T)Enum.Parse(typeof(T), NextWord(separator), true);
			T value;
			var word = NextWord(separator);
			//var vals = word.Split('|');
			//if (vals.Length > 1)
			//{
			//    var result = 0l;
			//    foreach (var val in vals)
			//    {
			//        if (!EnumUtil.TryParse(val, out value))
			//        {
			//            return defaultVal;
			//        }
			//        result |= (long)Convert.ChangeType(value, typeof(long));
			//    }
			//    return (T)(object)result;
			//}
			//else 
			if (!EnumUtil.TryParse(word, out value))
			{
				return defaultVal;
			}
			return value;
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
			int length = str.Length;
			if (pos >= length)
				return "";

			int x;
			while ((x = str.IndexOf(separator, pos)) == pos)
			{
				pos += separator.Length;
			}

			if (x < 0)
			{
				if (pos == length)
					return "";
				x = length;
			}
			var word = str.Substring(pos, x - pos);

			pos = x + separator.Length;
			if (pos > length)
				pos = length;

			return word;
		}

		/// <returns><code>NextWords(count, \" \")</code></returns>
		public string NextWords(int count)
		{
			return NextWords(count, " ");
		}

		/// <summary>
		/// -[smhdw] [seconds] [minutes] [hours] [days] [weeks]
		/// </summary>
		/// <returns></returns>
		public TimeSpan? NextTimeSpan()
		{
			var mod = NextModifiers();
			var seconds = 0;
			var minutes = 0;
			var hours = 0;
			var days = 0;

			if (mod.Contains("s"))
			{
				seconds = NextInt(0);
			}

			if (mod.Contains("m"))
			{
				minutes = NextInt(0);
			}

			if (mod.Contains("h"))
			{
				hours = NextInt(0);
			}

			if (mod.Contains("d"))
			{
				days = NextInt(0);
			}

			if (mod.Contains("w"))
			{
				days += NextInt(0) * 7;
			}

			if (seconds > 0 || minutes > 0 || hours > 0 || days > 0)
			{
				return new TimeSpan(days, hours, minutes, seconds, 0);
			}
			return null;
		}

		/// <returns>The next <code>count</code> word seperated by <code>seperator</code> as a string.</returns>
		public string NextWords(int count, string separator)
		{
			string result = "";
			for (int i = 0; i < count && HasNext; i++)
			{
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
			var words = new string[count];
			for (int i = 0; i < count && HasNext; i++)
			{
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
			var words = new List<string>();
			while (HasNext)
			{
				words.Add(NextWord(separator));
			}
			return words.ToArray();
		}

		//public long NextEvalExpr()
		//{
		//    return NextEvalExpr(" ");
		//}

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
			while (HasNext)
			{
				int i = 0;
				for (; i < rs.Length; i++)
				{
					if (str[pos + i] != rs[i])
					{
						return;
					}
				}
				pos += i;
			}
		}

		/// <summary>
		/// Ignores all directly following characters that are equal to <code>c</code>.
		/// </summary>
		public void Consume(char c)
		{
			while (HasNext && str[pos] == c)
				pos++;
		}

		/// <summary>
		/// Ignores a maximum of <code>amount</code> characters that are equal to <code>c</code>.
		/// </summary>
		public void Consume(char c, int amount)
		{
			for (int i = 0; i < amount && HasNext && str[pos] == c; i++)
				pos++;
		}

		/// <summary>
		/// Consumes the next character, if it equals <code>c</code>.
		/// </summary>
		/// <returns>whether the character was equal to <code>c</code> (and thus has been deleted)</returns>
		public bool ConsumeNext(char c)
		{
			if (HasNext && str[pos] == c)
			{
				pos++;
				return true;
			}
			return false;
		}

		/// <returns>whether or not the remainder contains the given string.</returns>
		public bool Contains(string s)
		{
			return s.IndexOf(s, pos) > -1;
		}

		/// <returns>whether or not the remainder contains the given char.</returns>
		public bool Contains(char c)
		{
			return str.IndexOf(c, pos) > -1;
		}

		/// <summary>
		/// Reads the next word as string of modifiers. 
		/// Modifiers are a string (usually representing a set of different modifiers per char), preceeded by a -.
		/// </summary>
		/// <remarks>Doesn't do anything if the next word does not start with a -.</remarks>
		/// <returns>The set of flags without the - or "" if none found</returns>
		public string NextModifiers()
		{
			var i = pos;
			var word = NextWord();
			if (word.StartsWith("-") && word.Length > 1)
			{
				return word.Substring(1);
			}
			pos = i;
			return "";
		}

		public StringStream CloneStream()
		{
			return (StringStream)Clone();
		}

		public Object Clone()
		{
			return new StringStream(str, pos);
		}

		public override string ToString()
		{
			return Remainder.Trim();
		}
	}
}