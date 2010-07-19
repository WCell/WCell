using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Global;

namespace WCell.RealmServer.Instances
{
	public delegate BaseInstance InstanceCreator();

	public class InstanceTemplate
	{
		private readonly RegionInfo m_regionInfo;
		public InstanceCreator Creator;

		public InstanceTemplate(RegionInfo info)
		{
			m_regionInfo = info;
		}

		public RegionInfo RegionInfo
		{
			get { return m_regionInfo; }
		}

		internal BaseInstance Create()
		{
			if (Creator != null)
			{
				return Creator();
			}
			return null;
		}
	}
}