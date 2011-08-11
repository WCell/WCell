using System;
using System.Xml.Serialization;

namespace WCell.Util.Strings
{
	public class StringTree<V> : StringNode<V>
		where V : class, IXmlSerializable
	{
		private char[] m_Seperators;
		private Action<string> errorHandler;

		public StringTree(Action<string> errorHandler)
			: this(errorHandler, '.')
		{
		}

		public StringTree(Action<string> errorHandler, params char[] seperators)
			: this(errorHandler, "\t", seperators)
		{
		}

		public StringTree(Action<string> errorHandler, string indent, params char[] seperators)
			: base(null)
		{
			m_tree = this;
			m_Seperators = seperators;
			this.errorHandler = errorHandler;
			m_depth = 0;
			m_indent = indent;
		}

		public void OnError(string msg, params object[] args)
		{
			errorHandler(string.Format(msg, args));
		}

		public Action<string> ErrorHandler
		{
			get{return errorHandler;}
			set { errorHandler = value;}
		}

		public char[] Seperators
		{
			get { return m_Seperators; }
		}
	}
}