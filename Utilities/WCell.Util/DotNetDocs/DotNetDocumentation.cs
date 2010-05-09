using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WCell.Util.DotNetDocs;

namespace WCell.Util.DotNetDocs
{
	[XmlRoot("doc")]
	public class DotNetDocumentation : XmlConfig<DotNetDocumentation>
	{
		private static readonly Dictionary<char, MemberType> TypeMap = new Dictionary<char,MemberType>();

		static DotNetDocumentation()
		{
			TypeMap.Add('T', MemberType.Type);
			TypeMap.Add('F', MemberType.Field);
			TypeMap.Add('P', MemberType.Property);
			TypeMap.Add('M', MemberType.Method);
			TypeMap.Add('E', MemberType.Event);
		}

		public static MemberType GetMemberType(char shortcut)
		{
			MemberType type;
			if (!TypeMap.TryGetValue(shortcut, out type))
			{
				throw new Exception("Undefined Type-shortcut: " + shortcut);
			}
			return type;
		}

		protected override void OnLoad()
		{

		}

		[XmlElement("assembly")]
		public string Assembly
		{
			get;
			set;
		}

		[XmlArray("members")]
		[XmlElement("member")]
		public DocEntry[] Members
		{
			get;
			set;
		}
	}
}