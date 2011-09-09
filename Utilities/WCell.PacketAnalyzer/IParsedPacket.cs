using WCell.Util;

namespace WCell.PacketAnalysis
{
	public interface IParsedPacket
	{
		void Dump(IndentTextWriter writer);
	}
}