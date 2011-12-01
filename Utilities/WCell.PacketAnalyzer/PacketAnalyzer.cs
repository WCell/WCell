using System;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Core.Network;
using WCell.PacketAnalysis.Logs;
using WCell.PacketAnalysis.Updates;
using WCell.PacketAnalysis.Xml;
using WCell.Util;

namespace WCell.PacketAnalysis
{
	public static class PacketAnalyzer
	{
		private static Logger log = LogManager.GetCurrentClassLogger();
		#region Static Fields

		static readonly PacketDefinition[][] PacketDefinitions = new PacketDefinition[(uint)ServiceType.Count][];
		private static int s_defCount;

		static PacketAnalyzer()
		{
		}

		#endregion

		/// <summary>
		/// Amount of defined Packets.
		/// </summary>
		public static int DefinitionCount
		{
			get { return s_defCount; }
		}

		public static bool IsInitialized
		{
			get { return s_defCount > 0; }
		}

		// TODO: Fix definition lookup, depending on sender, correctly
		#region Static Methods
		public static bool IsDefined(PacketId packetId, PacketSender sender)
		{
			return GetDefinition(PacketDefinitions[(uint)packetId.Service], packetId.RawId, sender) != null;
		}

		/// <summary>
		/// Registers a new PacketDefinition. Overrides existing Definitions (if any)
		/// </summary>
		public static void RegisterDefintion(PacketDefinition def)
		{
			foreach (var id in def.PacketIds)
			{
				var arr = PacketDefinitions[(uint)id.Service];
				if (def.Sender == PacketSender.Any || def.Sender == PacketSender.Client)
				{
					arr[id.RawId * 2] = def;
				}
				if (def.Sender == PacketSender.Any || def.Sender == PacketSender.Server)
				{
					arr[id.RawId * 2 + 1] = def;
				}
				s_defCount++;
			}
		}

		/// <summary>
		/// Gets the PacketDefinition for the PacketId
		/// </summary>
		public static PacketDefinition GetDefinition(PacketId packetId, PacketSender sender)
		{
			return GetDefinition(PacketDefinitions[(uint)packetId.Service], packetId.RawId, sender);
		}

		/// <summary>
		/// Gets the PacketDefinition for the PacketId
		/// </summary>
		public static PacketDefinition GetDefinition(ServiceType service, DirectedPacketId packetId)
		{
			return GetDefinition(PacketDefinitions[(uint)service], packetId.OpCode, packetId.Sender);
		}

		/// <summary>
		/// Gets the PacketDefinition of the given service for the given opcode
		/// </summary>
		public static PacketDefinition GetDefinition(ServiceType service, uint opcode, PacketSender sender)
		{
			return GetDefinition(service, new DirectedPacketId { Sender = sender, OpCode = opcode });
		}

		static PacketDefinition GetDefinition(PacketDefinition[] arr, uint rawId, PacketSender sender)
		{
#if DEBUG
			if (rawId * 2 >= arr.Length)
			{
				//Debugger.Break();
				log.Error("Invalid Packet definition: " + rawId);
				return arr[0];
			}
#endif
			if (sender == PacketSender.Any || sender == PacketSender.Client)
			{
				return arr[rawId * 2];
			}
			return arr[rawId * 2 + 1];
		}

		/// <summary>
		/// Gets all PacketDefinitions of the given service
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static PacketDefinition[] GetDefinitions(ServiceType type)
		{
			return PacketDefinitions[(uint)type];
		}
		#endregion

		/// <summary>
		/// Renders a single WoW - Packet
		/// </summary>
		public static void Dump(ParsablePacketInfo info, IndentTextWriter writer)
		{
			var packet = info.Packet;
			if (packet.PacketId.IsUpdatePacket)
			{
				ParsedUpdatePacket.Dump(info.Timestamp, packet.ReadBytes(packet.Length - packet.HeaderSize), false, writer,
					packet.PacketId.RawId == (uint)RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT);
			}
			else
			{
				var parser = new PacketParser(info);
				parser.Parse();
				parser.Dump(writer);
			}

			writer.WriteLine();
		}

		public static void LoadDefinitions(string dir)
		{
			LoadDefinitions(new DirectoryInfo(dir));
		}

		public static void LoadDefinitions(DirectoryInfo dir)
		{
			var oldDefs = PacketDefinitions;
			var oldDefCount = s_defCount;

			s_defCount = 0;
			PacketDefinitions[(int)ServiceType.Authentication] = new PacketDefinition[((uint)AuthServerOpCode.Maximum * 2) + 1];
			PacketDefinitions[(int)ServiceType.Realm] = new PacketDefinition[((uint)RealmServerOpCode.Maximum * 2) + 1];

			foreach (var file in dir.GetFileSystemInfos())
			{
				if (file is DirectoryInfo)
				{
					if (!file.Name.StartsWith("_"))
					{
						LoadDefinitions((DirectoryInfo) file);
					}
				}
				else
				{
					if (file.Extension.EndsWith("xml", StringComparison.InvariantCultureIgnoreCase))
					{
						// we found an xml-file
						try
						{
							var defs = XmlPacketDefinitions.LoadDefinitions(file.FullName);
							foreach (var def in defs)
							{
								RegisterDefintion(def);
							}
						}
						catch (Exception e)
						{
							Array.Copy(oldDefs, PacketDefinitions, oldDefs.Length);
							s_defCount = oldDefCount;
							throw new Exception("Error when loading PacketDefinitions from: " + file, e);
						}
					}
				}
			}
		}
	}
}