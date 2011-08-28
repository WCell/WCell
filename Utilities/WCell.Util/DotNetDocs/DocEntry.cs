using System.Xml.Serialization;

namespace WCell.Util.DotNetDocs
{
	/// <summary>
	/// 
	/// </summary>
	public class DocEntry
	{
		private string m_FullName;
		private string m_Name;
		private MemberType m_type;

		public MemberType MemberType
		{
			get
			{
				return m_type;
			}
		}

		[XmlElement("name")]
		public string FullName
		{
			get
			{
				return m_FullName;
			}
			set
			{
				m_FullName = value;
				m_type = DotNetDocumentation.GetMemberType(m_FullName[0]);
				var lastIndex = m_FullName.IndexOf('(');
				// TODO: Method-parameters
				if (lastIndex < 0)
				{
					lastIndex = m_FullName.Length;
				}
				m_Name = m_FullName.Substring(2, lastIndex-1);
			}
		}

		[XmlElement("summary")]
		public string Summary
		{
			get;
			set;
		}

		[XmlElement("remarks")]
		public string Remarks
		{
			get;
			set;
		}

		[XmlElement("returns")]
		public string Returns
		{
			get;
			set;
		}

		[XmlElement("value")]
		public string Value
		{
			get;
			set;
		}

		[XmlElement("exceptions")]
		public string[] Exceptions
		{
			get;
			set;
		}

		[XmlElement("see")]
		public string[] See
		{
			get;
			set;
		}

		[XmlElement("seealso")]
		public string[] SeeAlso
		{
			get;
			set;
		}
	}
}