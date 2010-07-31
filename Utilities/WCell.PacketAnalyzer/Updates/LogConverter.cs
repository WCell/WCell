using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WCell.Core.Network;
using WCell.RealmServer;
using NLog;
using WCell.Core;
using System.IO;

namespace WCell.PacketAnalyzer.Updates
{
	public class LogConverter
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Extracts all Packets out of the given logged and default-formatted lines
		/// </summary>
		public static List<PinnedRealmPacketIn> Extract(string[] log)
		{
			List<PinnedRealmPacketIn> packets = new List<PinnedRealmPacketIn>();
			for (int i = 0; i < log.Length; i++)
			{
				RealmServerOpCode opCode;
				var line = log[i];
				if (line.StartsWith("{"))
				{
					Match match = Regex.Match(line, @"\(0x(.{4})\)");
					if (match.Success)
					{
						opCode = (RealmServerOpCode)Int32.Parse(match.Groups[1].Value, NumberStyles.HexNumber);
					}
					else
					{
						match = Regex.Match(line, @"\(([^\)]+)\)");
						if (match.Success)
						{
							opCode = (RealmServerOpCode)Enum.Parse(typeof(RealmServerOpCode), match.Groups[1].Value);
						}
						else
						{
							Console.WriteLine("Could not parse Packet Header: " + line);
							continue;
						}
					}

					// skip the column count
					i += 4;
					StringBuilder sb = new StringBuilder();
					while ((line = log[i++]).StartsWith("|") && line.EndsWith("|"))
					{
						int first = line.IndexOf('|') + 1;
						int second = line.IndexOf('|', first) - 1;
						var str = line.Substring(first, second - first);
						//str = str.TrimEnd(' ');
						sb.Append(str + " ");
					}

					var bytes = UpdateFieldsUtil.ParseBytes(sb.ToString(), true);
					var packetBytes = new byte[bytes.Length + RealmPacketIn.HEADER_SIZE];

					var size = bytes.Length + RealmPacketIn.HEADER_SIZE;

					packetBytes[0] = (byte)((size >> 8) & 0xFF);
					packetBytes[1] = (byte)(size & 0xFF);

					packetBytes[2] = (byte)((int)opCode & 0xFF);
					packetBytes[3] = (byte)(((int)opCode >> 8) & 0xFF);

					Array.Copy(bytes, 0, packetBytes, RealmPacketIn.HEADER_SIZE, bytes.Length);

					var packet = PinnedRealmPacketIn.Create(packetBytes);
					packet.Initialize();
					packets.Add(packet);

				}
			}
			return packets;
		}


		/// <summary>
		/// Renders the given log file to the given output.
		/// </summary>
		/// <param name="file">The file from where to read the ksniffer-style logs</param>
		/// <param name="outputFile">The location of the file that the result should be written to</param>
		public static void ConvertLog(string logFile, string outputFile)
		{
			using (var stream = new StreamWriter(outputFile, false))
			{
				ConvertLog(logFile, stream);
			}
		}


		/// <summary>
		/// Renders the given log file to the given output.
		/// </summary>
		/// <param name="file">The file from where to read the ksniffer-style logs</param>
		/// <param name="output">A StreamWriter or Console.Out etc</param>
		public static void ConvertLog(string logFile, TextWriter output)
		{
			// @"F:\coding\C#\WCell\Dumps\Dump_Spells.txt"
			var writer = new IndentTextWriter(output);
			FieldRenderUtil.IsOldEntity = false;
			var log = File.ReadAllLines(logFile);
			var packets = Extract(log);
			foreach (var packet in packets)
			{
				PacketAnalyzer.Render(packet, writer);
				writer.WriteLine();
				packet.Dispose();
			}
		}
	}
}