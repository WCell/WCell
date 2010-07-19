using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.AuthServer.Firewall
{
	public struct IPRange
	{
		public IPRange(long? min, long? max)
		{
			if (min == null)
			{
				Min = max.Value;
			}
			else
			{
				Min = min.Value;
			}

			if (max != null)
			{
				Max = max.Value;
			}
			else
			{
				Max = min.Value; ;
			}
		}

		public long Min;
		public long Max;
	}
}