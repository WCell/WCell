using System;
using System.IO;
using NLog;
using WCell.Constants;
using WCell.Core;
using WCell.RealmServer.Network;

namespace WCell.RealmServer.Handlers
{
	public class ClientAddOn
	{
		public string Name;
		public uint AddOnCRC;
		public uint ExtraCRC;
		public byte HasSignature;
	}

	public static class ClientAddonHandler
	{
		static Logger s_log = LogManager.GetCurrentClassLogger();

		const uint BlizzardAddOnCRC = 0x4C1C776D;

		private static readonly byte[] BlizzardPublicKey = {
	                                                           0xC3, 0x5B, 0x50, 0x84, 0xB9, 0x3E, 0x32, 0x42, 0x8C, 0xD0,
	                                                           0xC7, 0x48, 0xFA, 0x0E, 0x5D, 0x54, 0x5A, 0xA3, 0x0E, 0x14,
	                                                           0xBA, 0x9E, 0x0D, 0xB9, 0x5D, 0x8B, 0xEE, 0xB6, 0x84, 0x93,
	                                                           0x45, 0x75, 0xFF, 0x31, 0xFE, 0x2F, 0x64, 0x3F, 0x3D, 0x6D,
	                                                           0x07, 0xD9, 0x44, 0x9B, 0x40, 0x85, 0x59, 0x34, 0x4E, 0x10,
	                                                           0xE1, 0xE7, 0x43, 0x69, 0xEF, 0x7C, 0x16, 0xFC, 0xB4, 0xED,
	                                                           0x1B, 0x95, 0x28, 0xA8, 0x23, 0x76, 0x51, 0x31, 0x57, 0x30,
	                                                           0x2B, 0x79, 0x08, 0x50, 0x10, 0x1C,
	                                                           0x4A, 0x1A, 0x2C, 0xC8, 0x8B, 0x8F, 0x05, 0x2D, 0x22, 0x3D,
	                                                           0xDB, 0x5A, 0x24, 0x7A, 0x0F, 0x13, 0x50, 0x37, 0x8F, 0x5A,
	                                                           0xCC, 0x9E, 0x04, 0x44, 0x0E, 0x87, 0x01, 0xD4, 0xA3, 0x15,
	                                                           0x94, 0x16, 0x34, 0xC6, 0xC2, 0xC3, 0xFB, 0x49, 0xFE, 0xE1,
	                                                           0xF9, 0xDA, 0x8C, 0x50, 0x3C, 0xBE, 0x2C, 0xBB, 0x57, 0xED,
	                                                           0x46, 0xB9, 0xAD, 0x8B, 0xC6, 0xDF, 0x0E, 0xD6, 0x0F, 0xBE,
	                                                           0x80, 0xB3, 0x8B, 0x1E, 0x77, 0xCF, 0xAD, 0x22, 0xCF, 0xB7,
	                                                           0x4B, 0xCF, 0xFB, 0xF0, 0x6B, 0x11,
	                                                           0x45, 0x2D, 0x7A, 0x81, 0x18, 0xF2, 0x92, 0x7E, 0x98, 0x56,
	                                                           0x5D, 0x5E, 0x69, 0x72, 0x0A, 0x0D, 0x03, 0x0A, 0x85, 0xA2,
	                                                           0x85, 0x9C, 0xCB, 0xFB, 0x56, 0x6E, 0x8F, 0x44, 0xBB, 0x8F,
	                                                           0x02, 0x22, 0x68, 0x63, 0x97, 0xBC, 0x85, 0xBA, 0xA8, 0xF7,
	                                                           0xB5, 0x40, 0x68, 0x3C, 0x77, 0x86, 0x6F, 0x4B, 0xD7, 0x88,
	                                                           0xCA, 0x8A, 0xD7, 0xCE, 0x36, 0xF0, 0x45, 0x6E, 0xD5, 0x64,
	                                                           0x79, 0x0F, 0x17, 0xFC, 0x64, 0xDD, 0x10, 0x6F, 0xF3, 0xF5,
	                                                           0xE0, 0xA6, 0xC3, 0xFB, 0x1B, 0x8C,
	                                                           0x29, 0xEF, 0x8E, 0xE5, 0x34, 0xCB, 0xD1, 0x2A, 0xCE, 0x79,
	                                                           0xC3, 0x9A, 0x0D, 0x36, 0xEA, 0x01, 0xE0, 0xAA, 0x91, 0x20,
	                                                           0x54, 0xF0, 0x72, 0xD8, 0x1E, 0xC7, 0x89, 0xD2
	                                                       };

		public static void SendAddOnInfoPacket(IRealmClient client)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ADDON_INFO))
			{
				//Fix for reading past the end of stream
				//Due to fake clients!
				if (client.Addons.Length > 0)
				{
					int unk;
					using (var binReader = new BinaryReader(new MemoryStream(client.Addons)))
					{
						var addonCount = binReader.ReadInt32();
						for (var i = 0; i < addonCount; i++)
						{
							var addon = ReadAddOn(binReader);
							WriteAddOnInfo(packet, addon);
						}

						unk = binReader.ReadInt32();
					}
					Console.WriteLine("CMSG ADDON Unk: " + unk);
				}

				

				const int count = 0;
				packet.Write(count);

				for (int i = 0; i < count; i++)
				{
					packet.Write(0);
					packet.Write(new byte[16]);
					packet.Write(new byte[16]);
					packet.Write(0);
					packet.Write(0);
				}

				client.Send(packet);
			}

			client.Addons = null;
		}

		private static ClientAddOn ReadAddOn(BinaryReader binReader)
		{
			var name = binReader.ReadCString();
			if (binReader.BaseStream.Position + 9 > binReader.BaseStream.Length)
			{
				return new ClientAddOn {Name = name};
			}

			var addon = new ClientAddOn
							{
								Name = name,
								HasSignature = binReader.ReadByte(),
								AddOnCRC = binReader.ReadUInt32(),
								ExtraCRC = binReader.ReadUInt32()
							};
			//Console.WriteLine("AddOn: {0} - {1} - {2} - {3}", addon.Name, addon.HasSignature, addon.AddOnCRC, addon.ExtraCRC);
			return addon;
		}

		// TODO: go back and add addon filtering

		enum AddOnType
		{
			Enabled = 1,
			Blizzard = 2,
		}
		private static void WriteAddOnInfo(RealmPacketOut packet, ClientAddOn addOn)
		{
			packet.Write((byte)AddOnType.Blizzard);
			packet.Write(true);

			// If the CRC32 of the addon's modulus doesnt match the CRC32 of the official blizzard public key
			// We could support 
			bool hasDifferentPublicKey = (addOn.AddOnCRC != BlizzardAddOnCRC);
			packet.Write(hasDifferentPublicKey);
			if (hasDifferentPublicKey)
			{
				// This is actually the modulus used in the SSignature verification
				packet.Write(BlizzardPublicKey);
			}
			packet.Write(0);


			packet.Write(false);

		}
	}
}