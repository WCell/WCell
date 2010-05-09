using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Remoting;

namespace WCell.Util
{
	public class IndentTextWriter : TextWriter
	{
		TextWriter m_writer;
		string indent;
		char m_indentChar;
		bool m_autoFlush;
		bool m_lastWasNewLine;

		public IndentTextWriter(string file)
			: this(file, '\t')
		{ }

		public IndentTextWriter(string file, char indentChar)
		{
			m_writer = new StreamWriter(file);
			IndentLevel = 0;
			m_indentChar = indentChar;
		}

		public IndentTextWriter(TextWriter writer)
			: this(writer, '\t')
		{ }

		public IndentTextWriter(TextWriter writer, char indentChar)
		{
			m_writer = writer;
			IndentLevel = 0;
			m_indentChar = indentChar;
		}

		#region Properties
		public int IndentLevel
		{
			get
			{
				return indent.Length;
			}
			set
			{
				indent = "";
				for (int i = 0; i < value; i++)
				{
					indent += m_indentChar;
				}
			}
		}

		public char IndentChar
		{
			get { return m_indentChar; }
			set
			{
				m_indentChar = value;
				IndentLevel = IndentLevel;
			}
		}

		public string Indent
		{
			get
			{
				return indent;
			}
		}

		public bool LastWasNewLine
		{
			get
			{
				return m_lastWasNewLine;
			}
			set
			{
				m_lastWasNewLine = value;
			}
		}

		public override Encoding Encoding
		{
			get { return m_writer.Encoding; }
		}

		public override IFormatProvider FormatProvider
		{
			get
			{
				return m_writer.FormatProvider;
			}
		}

		public override string NewLine
		{
			get
			{
				return m_writer.NewLine;
			}
			set
			{
				m_writer.NewLine = value;
			}
		}

		public bool AutoFlush
		{
			get
			{
				return m_autoFlush;
			}
			set
			{
				m_autoFlush = value;
			}
		}
		#endregion

		#region Write
		public override void Write(bool value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(char value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(char[] buffer)
		{
			m_writer.Write(buffer);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(char[] buffer, int index, int count)
		{
			m_writer.Write(buffer, index, count);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(decimal value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(double value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(float value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(int value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(long value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(object value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(string format, object arg0)
		{
			m_writer.Write(format, arg0);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(string format, object arg0, object arg1)
		{
			m_writer.Write(format, arg0, arg1);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(string format, object arg0, object arg1, object arg2)
		{
			m_writer.Write(format, arg0, arg1, arg2);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(string format, params object[] arg)
		{
			m_writer.Write(format, arg);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(string value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(uint value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}

		public override void Write(ulong value)
		{
			m_writer.Write(value);
			m_lastWasNewLine = false;
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
		}
		#endregion

		#region WriteLine
		public override void WriteLine()
		{
			m_writer.WriteLine();
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(bool value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(char value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(char[] buffer)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(buffer);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(char[] buffer, int index, int count)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(buffer, index, count);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(decimal value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(double value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(float value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(int value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(long value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(object value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(string format, object arg0)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(format, arg0);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(string format, object arg0, object arg1)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(format, arg0, arg1);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(string format, object arg0, object arg1, object arg2)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(format, arg0, arg1, arg2);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(string format, params object[] arg)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(format, arg);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public void WriteLineNotNull<T>(T obj, string format, params object[] args) where T : class
		{
			if (obj != null)
			{
				WriteLine(format, args);
			}
		}

		public void WriteLineNotDefault<T>(T obj, string format, params object[] args) where T : struct
		{
			if (!obj.Equals(default(T)))
			{
				WriteLine(format, args);
			}
		}

		public override void WriteLine(string value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(uint value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}

		public override void WriteLine(ulong value)
		{
			if (m_lastWasNewLine)
			{
				m_writer.Write(indent);
			}
			m_writer.WriteLine(value);
			if (m_autoFlush)
			{
				m_writer.Flush();
			}
			m_lastWasNewLine = true;
		}
		#endregion


		#region Other
		public override ObjRef CreateObjRef(Type requestedType)
		{
			return m_writer.CreateObjRef(requestedType);
		}

		public override bool Equals(object obj)
		{
			return m_writer.Equals(obj);
		}

		public override void Close()
		{
			m_writer.Close();
		}

		public override void Flush()
		{
			m_writer.Flush();
		}

		public override int GetHashCode()
		{
			return m_writer.GetHashCode();
		}

		public override object InitializeLifetimeService()
		{
			return m_writer.InitializeLifetimeService();
		}

		public override string ToString()
		{
			return m_writer.ToString();
		}

		protected override void Dispose(bool disposing)
		{
			m_writer.Dispose();
		}
		#endregion
	}
}