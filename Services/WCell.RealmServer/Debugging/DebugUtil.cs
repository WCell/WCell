using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.Intercommunication.DataTypes;
using WCell.PacketAnalysis;
using WCell.PacketAnalysis.Logs;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Variables;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Debugging
{
	/// <summary>
	/// Static helper class for advanced Debugging
	/// </summary>
	public static class DebugUtil
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		[Variable("DumpDirectory")]
		public static string DumpDirName = "./Dumps/";

		/// <summary>
		/// Dump files will be overwritten when they already exceed this length
		/// </summary>
		public static int MaxDumpFileSize = 1024*1024;

		public static string[] IgnoredOpcodeStrings = new[] { 
			// ignore movement packets
			"CMSG_PING", 
			"SMSG_PONG",
			"SMSG_POWER_UPDATE", 
			"CMSG_SET_ACTIVE_VOICE_CHANNEL",
			"_MOVE",
			"WORLD_STATE_UI_TIMER_UPDATE"
		};

		/// <summary>
		/// Where to dump packets to (temporarily)
		/// </summary>
		static DirectoryInfo DumpDir
		{
			get { return new DirectoryInfo(DumpDirName); }
		}

		private static bool dumps = true;

		/// <summary>
		/// whether packets are currently dumped to files.
		/// </summary>
		[Variable("EnablePacketDump")]
		public static bool Dumps
		{
			get { return dumps; }
			set
			{
				//if (dumps != value)
				{
					dumps = value;
				}
			}
		}

		/// <summary>
		/// Where to load Packet definitions from
		/// </summary>
		public static DirectoryInfo DefinitionDir;

		static IndentTextWriter m_defaultWriter;
		static Dictionary<string, IndentTextWriter> m_packetWriters;

		static bool m_initialized;


		[Initialization(InitializationPass.Tenth, "Initializing Debug Tools")]
		public static void Init(InitMgr mgr)
		{
			if (!m_initialized)
			{
				DefinitionDir = new DirectoryInfo(Path.Combine(RealmServerConfiguration.ContentDir, "Packets"));

				var paAsm = typeof(PacketParser).Assembly;
				if (mgr == null)
				{
					InitMgr.Initialize(paAsm);
				}
				else
				{
					mgr.AddStepsOfAsm(paAsm);
				}

				m_packetWriters = new Dictionary<string, IndentTextWriter>();

				LoadDefinitions();

				m_initialized = true;
				DumpDir.Create();

				//LoginHandler.ClientDisconnected += OnDisconnect;
			}
		}

		public static void Init()
		{
			Init(null);
		}

		/// <summary>
		/// Gets the Misc writer
		/// </summary>
		public static IndentTextWriter DefaultWriter
		{
			get
			{
				lock (DumpDir)
				{
					if (m_defaultWriter == null)
					{
						var file = Path.Combine(DumpDir.FullName, "_default.txt");
						m_defaultWriter = new IndentTextWriter(new StreamWriter(file))
						{
							AutoFlush = true
						};
					}
				}
				return m_defaultWriter;
			}
		}

		/// <summary>
		/// Loads all Packet definitions from XML
		/// </summary>
		public static void LoadDefinitions()
		{
			if (DefinitionDir == null)
			{
				DefinitionDir = new DirectoryInfo(Path.Combine(RealmServerConfiguration.ContentDir, "Packets"));
			}
			PacketAnalyzer.LoadDefinitions(DefinitionDir);
		}

		/// <summary>
		/// Writes a line into the debug log of the given Account.
		/// </summary>
		public static void Log(RealmAccount acc, string msg, params object[] args)
		{
			GetTextWriter(acc).WriteLine(string.Format(msg, args));
		}

		public static IndentTextWriter GetTextWriter(RealmAccount account)
		{
			IndentTextWriter writer;
			if (account == null)
			{
				writer = DefaultWriter;
			}
			else if (!m_packetWriters.TryGetValue(account.Name, out writer))
			{
				lock (m_packetWriters)
				{
					// check if the writer was added after the lock was released
					if (!m_packetWriters.TryGetValue(account.Name, out writer))
					{
						try
						{
							var file = Path.Combine(DumpDir.FullName, account.Name + ".txt");
							var fileInfo = new FileInfo(file);
							var append = fileInfo.Exists ? fileInfo.Length < MaxDumpFileSize : false;
							writer = new IndentTextWriter(new StreamWriter(file, append))
							{
								AutoFlush = true
							};
							m_packetWriters.Add(account.Name, writer);
						}
						catch (Exception e)
						{
							LogUtil.WarnException("Writing to Default writer - TextWriter for Account {0} could not be created: {1}", account, e);
							return DefaultWriter;
						}
					}
				}
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

		public static void DumpPacketOut(RealmAccount acc, byte[] packetOut, int offset, int length, PacketSender sender)
		{
			if (Dumps)
			{
				var writer = GetTextWriter(acc);
				using (var packet = DisposableRealmPacketIn.CreateFromOutPacket(packetOut, offset, length))
				{
					if (CanDump(packet.PacketId))
					{
						DumpPacket(packet, sender, false, writer);
					}
				}
			}
		}

		public static void DumpPacketOut(RealmAccount acc, RealmPacketOut packetOut, PacketSender sender)
		{
			if (CanDump(packetOut.PacketId))
			{
				var writer = GetTextWriter(acc);
				using (var packet = DisposableRealmPacketIn.CreateFromOutPacket(packetOut))
				{
					DumpPacket(packet, sender, false, writer);
				}
			}
		}

		private static bool CanDump(PacketId id)
		{
			return Dumps &&
				   IgnoredOpcodeStrings.Where(filter =>
					   id.ToString().IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1).Count() == 0;
		}

		public static void DumpPacket(RealmAccount acc, RealmPacketIn packet, PacketSender sender)
		{
			if (CanDump(packet.PacketId))
			{
				var writer = GetTextWriter(acc);
				DumpPacket(packet, sender, true, writer);
			}
		}

		public static void DumpPacket(RealmAccount acc, RealmPacketIn packet, bool copy, PacketSender sender)
		{
			if (CanDump(packet.PacketId))
			{
				var writer = GetTextWriter(acc);
				DumpPacket(packet, sender, copy, writer);
			}
		}

		public static void DumpPacket(RealmPacketIn packet, PacketSender sender, bool copy, IndentTextWriter writer)
		{
			lock (writer)
			{
				try
				{
					if (copy)
					{
						using (var pkt = packet.Copy())
						{
							PacketAnalyzer.Dump(new ParsablePacketInfo(pkt, sender, DateTime.Now), writer);
						}
					}
					else
					{
						PacketAnalyzer.Dump(new ParsablePacketInfo(packet, sender, DateTime.Now), writer);
					}
				}
				catch (Exception e)
				{
					LogUtil.ErrorException(e, "Unable to parse/render packet " + packet);

					writer.IndentLevel = 0;
					writer.Write(packet.ToHexDump());
				}
			}
		}

		private static void RemoveWriter(IAccount acc)
		{
			lock (m_packetWriters)
			{
				IndentTextWriter writer;
				if (m_packetWriters.TryGetValue(acc.Name, out writer))
				{
					writer.Close();
					m_packetWriters.Remove(acc.Name);
				}
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
		//    m_updatePacketWriter.Write(Utility.ToHex(new PacketId(), block));
		//    m_updatePacketWriter.Flush();
		//}
	}
}