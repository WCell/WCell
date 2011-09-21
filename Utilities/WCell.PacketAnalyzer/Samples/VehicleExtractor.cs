using WCell.Constants.Updates;
using WCell.Core;
using WCell.PacketAnalysis.Logs;
using WCell.PacketAnalysis.Updates;
using WCell.Util;

namespace WCell.PacketAnalysis.Samples
{
	/// <summary>
	/// Extracts vehicle-information from Update packets
	/// </summary>
	public static class VehicleExtractor
	{
		private static IndentTextWriter writer;

		public static void Extract(string dir, string outFile)
		{
			using (writer = new IndentTextWriter(outFile))
			{
				GenericLogParser.ParseDir(SniffitztLogConverter.Extract,
					dir,
					ExtractVehicleIds);
			}
		}

		private static void ExtractVehicleIds(ParsedUpdatePacket packet)
		{
			foreach (var block in packet.Blocks)
			{
				if (block.Type == UpdateType.Create)
				{
					var movement = block.Movement;
					if ((movement.UpdateFlags & UpdateFlags.Vehicle) != 0)
					{
						var isChar = block.EntityId.High == HighId.Player;
						string suffix;
						if (isChar)
						{
							suffix = "";
						}
						else
						{
							suffix = string.Format(" {0} ({1})", block.EntityId.Entry, (int)block.EntityId.Entry);
						}

						writer.WriteLine("{0}{1}", isChar ? "Player" : "NPC", suffix);
						writer.IndentLevel++;
						writer.WriteLine("VehicleId: " + movement.VehicleId);
						writer.WriteLine("VehicleAimAdjustment: " + movement.VehicleAimAdjustment);
						writer.WriteLine("Hoverheight: " + block.GetFloat(UnitFields.HOVERHEIGHT));
						writer.IndentLevel--;
					}
				}
			}
		}
	}
}