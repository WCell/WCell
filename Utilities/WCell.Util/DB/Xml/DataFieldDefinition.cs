using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using WCell.Util.Data;

namespace WCell.Util.DB.Xml
{
	public abstract class DataFieldDefinition : IDataFieldDefinition
	{
		private string m_Name;

		/// <summary>
		/// The name of the DataField it belongs to
		/// </summary>
		[XmlAttribute]
		public string Name
		{
			get
			{
				EnsureName();
				return m_Name;
			}
			set { m_Name = value; }
		}

		public void EnsureName()
		{
			if (string.IsNullOrEmpty(m_Name))
			{
				throw new DataHolderException("DataHolder-definition contained empty field-definitions without Name - Name is required.");
			}
		}

		public abstract DataFieldType DataFieldType { get; }

		public override string ToString()
		{
			return Name;
		}
	}
}