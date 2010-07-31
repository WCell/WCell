using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCell.RealmServer.Spells
{
	public class RuneCostEntry
	{
		public uint Id;
		public uint BloodCost;
		public uint FrostCost;
		public uint UnholyCost;
		public uint PowerGain;

		public override string ToString()
		{
			return string.Format("{0}, Costs: {1}, {2}, {3}, Gain: {4}", Id, BloodCost, FrostCost, UnholyCost, PowerGain);
		}
	}
}
