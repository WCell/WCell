using System;

namespace WCell.Core.Initialization
{
	public class InitializationDependency
	{
		private string m_Name;
		private Type m_DependentType;

		public InitializationDependency(DependentInitializationAttribute attr)
		{
			m_Name = attr.Name;
			m_DependentType = attr.DependentType;
		}

		public string Name
		{
			get { return m_Name; }
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