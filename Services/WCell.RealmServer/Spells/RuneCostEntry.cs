using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells
{
	public class RuneCostEntry
	{
		public uint Id;
		public int BloodCost;
		public int UnholyCost;
		public int FrostCost;
		public int RunicPowerGain;

		public bool CostsRunes
		{
			get { return BloodCost > 0 || UnholyCost > 0 || FrostCost > 0;}
		}

		public override string ToString()
		{
			return string.Format("{0}, Costs: {1}, {2}, {3}, Gain: {4}", Id, BloodCost, UnholyCost, FrostCost, RunicPowerGain);
		}
	}
}
