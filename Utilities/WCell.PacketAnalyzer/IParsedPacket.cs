using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Util;

namespace WCell.PacketAnalysis
{
	public interface IParsedPacket
	{
		void Dump(IndentTextWriter writer);
	}
}