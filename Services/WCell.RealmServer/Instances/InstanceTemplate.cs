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
		private readonly RegionTemplate m_RegionTemplate;
		public InstanceCreator Creator;

		public InstanceTemplate(RegionTemplate template)
		{
			m_RegionTemplate = template;
		}

		public RegionTemplate RegionTemplate
		{
			get { return m_RegionTemplate; }
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