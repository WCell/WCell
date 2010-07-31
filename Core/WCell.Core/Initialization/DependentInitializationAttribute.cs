using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Initialization
{
	public class DependentInitializationInfo
	{
		private string m_Name;
		private bool m_IsRequired;
		private Type m_DependentType;

		public DependentInitializationInfo(DependentInitializationAttribute attr)
		{
			m_Name = attr.Name;
			m_IsRequired = attr.IsRequired;
			m_DependentType = attr.DependentType;
		}

		public string Name
		{
			get { return m_Name; }
		}

		public bool IsRequired
		{
			get { return m_IsRequired; }
		}

		public Type DependentType
		{
			get { return m_DependentType; }
		}

		public GlobalMgrInfo DependentMgr
		{
			get;
			internal set;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class DependentInitializationAttribute : Attribute
	{
		public string Name { get; set; }
		public bool IsRequired { get; set; }

		public DependentInitializationAttribute(Type dependentType)
			: this(dependentType, "")
		{
		}

		public DependentInitializationAttribute(Type dependentType, string name)
		{
			DependentType = dependentType;
			Name = name;
		}

		public Type DependentType
		{
			get;
			set;
		}
	}
}