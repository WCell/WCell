using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WCell.Util.Xml;
using WCell.Util.NLog;

namespace WCell.Util.Strings
{
	public class StringNode<V> : IXmlSerializable
		where V : class, IXmlSerializable
	{
		internal readonly IDictionary<string, StringNode<V>> Children = new SortedList<string, StringNode<V>>();

		protected string m_Name;
		protected V m_Value;
		protected StringTree<V> m_tree;
		protected StringNode<V> m_Parent;
		protected int m_depth;
		protected string m_indent;

		protected internal StringNode(StringTree<V> tree)
		{
			m_tree = tree;
		}

		protected internal StringNode(StringTree<V> tree, string key, V value) :
			this(tree)
		{
			m_Name = key;
			m_Value = value;
		}

		public string Name
		{
			get { return m_Name; }
		}

		public string FullName
		{
			get
			{
				if (m_Parent == null)
				{
					return m_Name;
				}
				if (!string.IsNullOrEmpty(m_Parent.Name))
				{
					return m_Parent.FullName + m_tree.Seperators[0] + m_Name;
				}
				return m_Name;
			}
		}

		public V Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		public int ChildCount
		{
			get { return Children.Count; }
		}

		public StringNode<V> Parent
		{
			get { return m_Parent; }
		}

		public string GetQualifiedName(string name)
		{
			return (FullName != null ? FullName + m_tree.Seperators[0] : "") + name;
		}

		#region Lookup Children
		public StringNode<V> GetChild(string key)
		{
			StringNode<V> child;
			Children.TryGetValue(key, out child);
			return child;
		}

		public StringNode<V> FindChild(string keyChain)
		{
			var keys = keyChain.Split(m_tree.Seperators, StringSplitOptions.RemoveEmptyEntries);
			return FindChild(keys, 0);
		}

		public StringNode<V> FindChild(string[] keyChain)
		{
			return FindChild(keyChain, 0);
		}

		public StringNode<V> FindChild(string[] keyChain, int index)
		{
			if (index >= keyChain.Length)
			{
				return this;
			}

			var child = GetChild(keyChain[index]);
			if (child != null)
			{
				return child.FindChild(keyChain, index + 1);
			}
			return null;
		}

		public StringNode<V> GetOrCreate(string key)
		{
			return GetChild(key) ?? AddChild(key, default(V));
		}
		#endregion


		#region Values
		public V GetValue(string key)
		{
			var child = GetChild(key);
			return child != null ? child.Value : default(V);
		}

		public V FindValue(string keyChain)
		{
			var child = FindChild(keyChain);
			return child != null ? child.Value : default(V);
		}

		public V FindValue(string[] keyChain)
		{
			var child = FindChild(keyChain);
			return child != null ? child.Value : default(V);
		}

		public V FindValue(string[] keyChain, int index)
		{
			var child = FindChild(keyChain, index);
			return child != null ? child.Value : default(V);
		}
		#endregion


		#region Add/Remove Children
		public StringNode<V> AddChild(string key, V value)
		{
			var child = new StringNode<V>(m_tree, key, value);
			AddChild(child);
			return child;
		}

		public StringNode<V> AddChildInChain(string keyChain, V value)
		{
			var keys = keyChain.Split(m_tree.Seperators, StringSplitOptions.RemoveEmptyEntries);
			return AddChildInChain(keys, value);
		}

		public StringNode<V> AddChildInChain(string[] keyChain, V value)
		{
			var node = this;

			for (var i = 0; i < keyChain.Length; i++)
			{
				var key = keyChain[i];
				node = node.GetOrCreate(key);
				if (i == keyChain.Length - 1)
				{
					// last key in chain is the actual node name
					node.Value = value;
				}
			}
			return node;
		}

		public void AddChild(StringNode<V> child)
		{
			Children.Add(child.Name, child);
			child.m_Parent = this;
			child.m_depth = m_depth + 1;
			child.m_indent = m_indent + "\t";
		}

		public void Remove()
		{
			if (m_Parent == null)
			{
				throw new InvalidOperationException("Cannot remove the Root of a Tree.");
			}
			m_Parent.Children.Remove(m_Name);
			m_Parent = null;
			m_depth = 0;
		}

		/// <summary>
		/// Removes and returns the direct Child with the given key
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public StringNode<V> RemoveChild(string key)
		{
			var child = GetChild(key);
			if (child != null)
			{
				child.Remove();
			}
			return child;
		}

		/// <summary>
		/// Removes and returns the direct Child with the given key
		/// </summary>
		/// <returns></returns>
		public StringNode<V> FindAndRemoveChild(string keyChain)
		{
			var child = FindChild(keyChain);
			if (child != null)
			{
				child.Remove();
			}
			return child;
		}

		/// <summary>
		/// Removes and returns the direct Child with the given key
		/// </summary>
		/// <returns></returns>
		public StringNode<V> FindAndRemoveChild(string[] keyChain)
		{
			var child = FindChild(keyChain);
			if (child != null)
			{
				child.Remove();
			}
			return child;
		}

		/// <summary>
		/// Removes and returns the direct Child with the given key
		/// </summary>
		/// <returns></returns>
		public StringNode<V> FindAndRemoveChild(string[] keyChain, int index)
		{
			var child = FindChild(keyChain, index);
			if (child != null)
			{
				child.Remove();
			}
			return child;
		}
		#endregion

		#region XML Serializing
		public void ReadXml(XmlReader reader)
		{
			if (Value != null)
			{
				try
				{
					Value.ReadXml(reader);
				}
				catch (Exception e)
				{
					var msg = string.Format("Failed to parse Node: {0}", FullName);
					//m_tree.OnError(msg);
					LogUtil.ErrorException(e, msg);
				}
			}

			if (Children.Count > 0)
			{
				var used = new List<StringNode<V>>();
				for (var i = 0; i < Children.Count; )
				{
					reader.Read();
					if (reader.ReadState == ReadState.EndOfFile)
					{
						throw new Exception("Unexpected EOF in Config.");
					}
					reader.SkipEmptyNodes();

					if (reader.NodeType == XmlNodeType.EndElement)
					{
						break;
					}
				    if (reader.NodeType == XmlNodeType.Element)
				    {
				        var child = GetChild(reader.Name);
				        if (child == null)
				        {
				            OnInvalidNode(reader);
				        }
				        else
				        {
				            i++;
				            used.Add(child);
				            if (!reader.IsEmptyElement)
				            {
				                child.ReadXml(reader);
				            }
				            else
				            {
				                reader.SkipEmptyNodes();
				            }
				        }
				    }
				}

				var unused = Children.Values.Except(used);
				if (unused.Count() > 0)
				{
					m_tree.OnError("Found {0} missing Node(s): {1}",
						unused.Count(), 
						unused.ToString(", ", node => node.FullName));
				}
			}
			reader.SkipEmptyNodes();

			if (reader.IsEmptyElement)
			{
				reader.Read();
			}

			reader.SkipEmptyNodes();
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				OnInvalidNode(reader);
				reader.SkipEmptyNodes();
			}

			reader.ReadEndElement();
		}

		void OnInvalidNode(XmlReader reader)
		{
			m_tree.OnError("Found invalid Node \"{0}\"", GetQualifiedName(reader.Name));


			if (reader.IsEmptyElement)
			{
				return;
			}

			var endCount = 1;
			do
			{
				reader.Read();
				if (reader.ReadState == ReadState.EndOfFile)
				{
					throw new Exception("Unexpected EOF in Config.");
				}
				if (reader.NodeType == XmlNodeType.EndElement)
				{
					reader.ReadEndElement();
					endCount--;
				}
				else if (reader.NodeType == XmlNodeType.Element &&
					!reader.IsEmptyElement)
				{
					endCount++;
				}
			} while (endCount > 0);

			//reader.SkipEmptyNodes();
		}

		public void WriteXml(XmlWriter writer)
		{
			if (Value != null)
			{
				Value.WriteXml(writer);
			}

			if (Children.Count > 0)
			{
				foreach (var node in Children.Values)
				{
					//writer.WriteWhitespace("\n" + node.m_indent);
					writer.WriteStartElement(node.Name);
					node.WriteXml(writer);
					writer.WriteEndElement();
				}
				//writer.WriteWhitespace("\n" + m_indent);
			}
		}

		public XmlSchema GetSchema()
		{
			throw new System.NotImplementedException();
		}
		#endregion

		public override string ToString()
		{
			return FullName;
		}
	}
}
