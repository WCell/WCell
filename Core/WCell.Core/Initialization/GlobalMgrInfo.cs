using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.Core.Initialization
{
	public class GlobalMgrInfo
	{
		public readonly List<InitializationDependency> Dependencies = new List<InitializationDependency>(5);

		private bool m_isInitialized;

		public GlobalMgrInfo()
		{
		}

		public bool IsInitialized
		{
			get { return m_isInitialized; }
			internal set { m_isInitialized = true; }
		}
	}
}
