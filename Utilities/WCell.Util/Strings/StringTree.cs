using System;
using System.Xml.Serialization;

namespace WCell.Util.Strings
{
	public class StringTree<V> : StringNode<V>
		where V : class, IXmlSerializable
	{
		private char[] m_Seperators;
		private Action<string> m_onError;

		public StringTree(Action<string> onError)
			: this(onError, '.')
		{
		}

		public StringTree(Action<string> onError, params char[] seperators)
			: this(onError, "\t", seperators)
		{
		}

		public StringTree(Action<string> onError, string indent, params char[] seperators)
			: base(null)
		{
			m_tree = this;
			m_Seperators = seperators;
			m_onError = onError;
			m_depth = 0;
			m_indent = indent;
		}

		public void OnError(string msg, params object[] args)
		{
			m_onError(string.Format(msg, args));
		}

		public Action<string> OnErrorHandler
		{
			get
			{
				return m_onError;
			}
		}

		public char[] Seperators
		{
			get { return m_Seperators; }
		}
	}
}
