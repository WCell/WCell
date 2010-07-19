using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.Lang
{
	public class TranslatedItem<K>
		where K : IConvertible
	{
		[XmlAttribute]
		public K Key;

		[XmlElement]
		public string Value;
	}
}