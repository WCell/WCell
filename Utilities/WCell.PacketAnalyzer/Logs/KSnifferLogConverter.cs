using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Core.Network;
using WCell.PacketAnalysis.Updates;
using WCell.Util;

namespace WCell.PacketAnalysis.Logs
{

	public class KSnifferLogConverter
	{
		protected static Logger log = LogManager.GetCurrentClassLogger();

		public static void ExtractSingleLine(string logFile, params LogHandler[] handlers)
		{
			try
			{
				var lines = File.ReadAllLines(logFile);
				Extract(lines, true, handlers);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Failed to parse log-file \"{0}\"", logFile), e);
			}
		}

		public static void ExtractMultiLine(string logFile, params LogHandler[] handlers)
		{
			try
			{
				var lines = File.ReadAllLines(logFile);
				Extract(lines, false, handlers);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Failed to parse log-file \"{0}\"", logFile), e);
			}
		}

		public static void ExtractSingleLine(string[] lines, params LogHandler[] handlers)
		{
			Extract(lines, true, handlers);
		}

		public static void ExtractMultiLine(string[] lines, params LogHandler[] handlers)
		{
			Extract(lines, false, handlers);
		}

		public static void Extract(string[] lines, bool line, Action<ParsablePacketInfo> parser, OpCodeValidator validator)
		{
			Extract(lines, line, new LogHandler(validator, parser));
		}

		/// <summary>
		/// Extracts all Packets out of the given logged and default-formatted lines
		/// </summary>
		/// <param name="singleLinePackets">Whether the packet content is one single line (or false in case of the fancy ksniffer format)</param>
		public static void Extract(string[] lines, bool singleLinePackets, params LogHandler[] handlers)
		{
			var lineNo = -1;
			var opCode = (RealmServerOpCode)uint.MaxValue;
			var sender = PacketSender.Any;
			var timeStrLen = "TimeStamp".Length;
			for (lineNo = 0; lineNo < lines.Length; lineNo++)
			{
				try
				{
					var timestamp = DateTime.Now;
					var line = lines[lineNo];

					if (line.Length == 0 || (singleLinePackets && !line.StartsWith("{")))
					{
						continue;
					}

					// find sender
					if (line.IndexOf("SERVER", StringComparison.InvariantCultureIgnoreCase) > -1)
					{
						sender = PacketSender.Server;
					}
					else
					{
						sender = PacketSender.Client;
					}

					// find opcode and timestamp
					var match = Regex.Match(line, @"\(0x(.{4})\)");
					if (match.Success)
					{
						var timestampIndex = line.IndexOf("TimeStamp", StringComparison.InvariantCultureIgnoreCase) + timeStrLen;
						if (timestampIndex >= 0)
						{
							uint x;
							while (!uint.TryParse(line[timestampIndex].ToString(), out x))
							{
								timestampIndex++;
							}
							var timestampStr = line.Substring(timestampIndex).Trim();
							long seconds;
							if (long.TryParse(timestampStr, out seconds))
							{
								timestamp = Utility.GetUTCTimeMillis(seconds);
							}
						}
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

					lineNo++; // one line for the packet-header

					var opcodeHandler = handlers.Where(handler => handler.Validator(opCode)).FirstOrDefault();
					var buildPacket = opcodeHandler != null;

					var sb = new StringBuilder();
					if (singleLinePackets)
					{
						if (buildPacket)
						{
							sb.Append(lines[lineNo]);
						}
						lineNo++;
					}
					else
					{
						// skip the column count
						while (string.IsNullOrEmpty(line = lines[lineNo]) ||
								line.StartsWith("|00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F") ||
								line.StartsWith("|--------------------------------"))
						{
							lineNo++;
						}

						int start, end;
						while (((line.Length > 5 && line[start = 4] == ':') || (start = line.IndexOf('|')) >= 0) &&
							   (end = line.IndexOf('|', start += 1)) > 0)
						{
							++lineNo;
							if (buildPacket)
							{
								end -= 1;

								var str = line.Substring(start, end - start);

								var fillerStart = str.IndexOf("--");
								if (fillerStart >= 0)
								{
									str = str.Substring(0, fillerStart - 1);
								}

								sb.Append(str + " ");
								while ((line = lines[lineNo]).Length == 0)	// skip empty lines
								{
									++lineNo;
								}	
							}
						}
					}

					if (buildPacket)
					{
						if (!Enum.IsDefined(typeof(RealmServerOpCode), opCode))
						{
							log.Warn("Packet at line #{0} had undefined Opcode: " + opCode, lineNo);
							continue;
						}

						var bytes = UpdateFieldsUtil.ParseBytes(sb.ToString(), true);
						var packet = DisposableRealmPacketIn.Create(opCode, bytes);
						if (packet != null)
						{
							if (packet.PacketId == RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT && packet.Length < 20)
							{
								throw new Exception("Format error - Did you specify singlePackets although its not single-line packets?");
							}

							opcodeHandler.PacketParser(new ParsablePacketInfo(packet, sender, timestamp));
						}
					}
				}
				catch (Exception e)
				{
					LogUtil.ErrorException(e, "Error in KSniffer-log at line {0} ({1})", lineNo, opCode);
				}
			}
		}


		/// <summary>
		/// Renders the given log file to the given output.
		/// </summary>
		/// <param name="outputFile">The location of the file that the result should be written to</param>
		public static void ConvertLog(string logFile, string outputFile)
		{
			ConvertLog(logFile, outputFile, false);
		}

		public static void ConvertLog(string logFile, string outputFile, bool singleLine)
		{
			new FileInfo(outputFile).MKDirs();

			using (var stream = new StreamWriter(outputFile, false))
			{
				ConvertLog(logFile, stream, singleLine);
			}
		}

		public static void ConvertLog(string logFile, TextWriter output)
		{
			ConvertLog(logFile, output, false);
		}

		/// <summary>
		/// Renders the given log file to the given output.
		/// </summary>
		/// <param name="output">A StreamWriter or Console.Out etc</param>
		public static void ConvertLog(string logFile, TextWriter output, bool singleLine)
		{
			var writer = new IndentTextWriter(output);
			try
			{
				var lines = File.ReadAllLines(logFile);
				Extract(lines, singleLine, info => LogConverter.ParsePacket(info, writer),
						LogConverter.DefaultValidator);
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("Failed to parse log-file \"{0}\"", logFile), e);
			}
		}
	}
}