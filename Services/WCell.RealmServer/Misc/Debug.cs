using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;
using NLog.Targets;
using WCell.Core.Initialization;
using WCell.Core;
using WCell.Core.Network;
using WCell.PacketAnalysis;
using WCell.PacketAnalysis.Updates;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Static helper class for advanced Debugging
	/// </summary>
	public static class DebugHelper
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Where to dump packets to (temporarily)
		/// </summary>
		public static readonly DirectoryInfo DumpDir = new DirectoryInfo("./Dumps/");

		/// <summary>
		/// Where to load Packet definitions from
		/// </summary>
		static DirectoryInfo DefinitionDir;

		static IndentTextWriter m_defaultWriter;
		static Dictionary<string, IndentTextWriter> m_packetWriters;


#if DEBUG
		[Initialization(InitializationPass.Tenth, Name = "Initializing Debug Tools")]
#endif
		public static void Init()
		{
			LoadDefinitions();

			WCell.PacketAnalysis.Program.Main();

			DumpDir.Create();

			m_packetWriters = new Dictionary<string, IndentTextWriter>();

			Character.LoggedOut += (chr) => {
				RemoveWriter(chr.Account);
			};
		}

		/// <summary>
		/// Gets the Misc writer
		/// </summary>
		public static IndentTextWriter DefaultWriter
		{
			get
			{
				if (m_defaultWriter == null)
				{
					var file = Path.Combine(DumpDir.FullName, "_default.txt");
					m_defaultWriter = new IndentTextWriter(new StreamWriter(file));
					m_defaultWriter.AutoFlush = true;
				}
				return m_defaultWriter;
			}
		}

		/// <summary>
		/// Loads all Packet definitions from XML
		/// </summary>
		public static void LoadDefinitions()
		{
			DefinitionDir = new DirectoryInfo(Path.Combine(RealmServer.Instance.Configuration.ContentDir, "Packets"));
			PacketAnalyzer.LoadDefinitions(DefinitionDir);
		}

		public static IndentTextWriter GetTextWriter(Account account)
		{
			IndentTextWriter writer;
			if (account == null)
			{
				writer = DefaultWriter;
			}
			else if (!m_packetWriters.TryGetValue(account.Name, out writer))
			{
				var file = Path.Combine(DumpDir.FullName, account.Name + ".txt");
				writer = new IndentTextWriter(new StreamWriter(file));
				writer.AutoFlush = true;
				m_packetWriters.Add(account.Name, writer);
			}
			return writer;
		}

		/// <summary>
		/// Dumps the content of an Update packet to the console
		/// </summary>
		//public static void DumpUpdatePacket(RealmPacketOut packet)
		//{
		//    try
		//    {
		//        ParsedUpdatePacket.Dump(packet.GetPacketPayload(), false, m_updatePacketWriter);
		//        m_updatePacketWriter.Flush();
		//    }
		//    catch (Exception e)
		//    {
		//        OnException("Failed to parse Update-Packet.", e, packet.GetPacketPayload());
		//    }
		//}

		public static void DumpPacket(byte[] packetOut)
		{
			using (var packet = PinnedRealmPacketIn.CreateFromOutPacket(packetOut))
			{
				DumpPacket(null, packet);
			}
		}

		public static void DumpPacket(Account acc, byte[] packetOut)
		{
			using (var packet = PinnedRealmPacketIn.CreateFromOutPacket(packetOut))
			{
				DumpPacket(acc, packet);
			}
		}

		public static void DumpPacket(Account acc, RealmPacketIn packet)
		{
			var writer = GetTextWriter(acc);
			DumpPacket(packet, true, writer);
		}

		public static void DumpPacket(RealmPacketIn packet, bool copy, IndentTextWriter writer)
		{
			try
			{
				if (copy)
				{
					using (PinnedRealmPacketIn pkt = packet.Copy())
					{
						PacketAnalyzer.Render(pkt, writer);
					}
				}
				else
				{
					PacketAnalyzer.Render(packet, writer);
				}
			}
			catch (Exception e)
			{
				log.ErrorException("Unable to parse/render packet " + packet, e);

				writer.IndentLevel = 0;
				writer.Write(packet.ToHexDump());
			}
		}

		private static void RemoveWriter(Account acc)
		{
			IndentTextWriter writer;
			if (m_packetWriters.TryGetValue(acc.Name, out writer))
			{
				writer.Close();
				m_packetWriters.Remove(acc.Name);
			}
		}

		/// <summary>
		/// Dumps a single Update block to the console
		/// </summary>
		//public static void DumpUpdateBlock(byte[] block)
		//{
		//    try
		//    {
		//        ParsedUpdatePacket.Dump(block, true, m_updatePacketWriter);
		//        m_updatePacketWriter.Flush();
		//    }
		//    catch (Exception e)
		//    {
		//        OnException("Failed to parse Update-Block", e, block);
		//    }
		//}

		//public static void OnException(string msg, Exception e, byte[] block)
		//{
		//    log.ErrorException(msg, e);
		//    m_updatePacketWriter.Write(Utility.ToHexDump(new PacketId(), block));
		//    m_updatePacketWriter.Flush();
		//}
	}
}