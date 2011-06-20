using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WCell.Util.Variables
{
	public interface IVariableDefinition
	{
		string Name
		{
			get;
		}

		bool IsReadOnly
		{
			get;
		}

		bool IsFileOnly
		{
			get;
		}

		string TypeName
		{
			get;
		}

		Object Value
		{
			get;
			set;
		}
	}

	public class VariableDefinition
	{
		public static readonly VariableDefinition[] EmptyArray = new VariableDefinition[0];

		public VariableDefinition()
		{
			
		}

		public VariableDefinition(string name)
		{
			Name = name;
		}

		[XmlAttribute]
		public string Name
		{
			get;
			set;
		}

		[XmlAttribute("Value")]
		public string StringValue
		{
			get;
			set;
		}

		/// <summary>
		/// Copies the actual Value 
		/// </summary>
		public object Eval(Type type)
		{
			if (Name == null)
			{
				throw new Exception("Variable's Name was not set - Value: " + StringValue);
			}
			if (StringValue == null)
			{
				throw new Exception("Variable's StringValue was not set - Name: " + Name + "");
			}
			object obj = null;
			if (!Utility.Parse(StringValue, type, ref obj))
			{
				throw new Exception(string.Format("Unable to parse Variable Value \"{0}\" as Type \"{1}\"", StringValue, type.Name));
			}
			return obj;
		}

		public override string ToString()
		{
			return Name + " (Value: " + StringValue + ")";
		}
	}
}