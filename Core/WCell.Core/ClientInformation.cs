/*************************************************************************
 *
 *   file		: SystemInformation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-07-27 16:18:17 +0800 (Sun, 27 Jul 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 582 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using Cell.Core;
using WCell.Constants;
using WCell.Core.Network;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WCell.Core
{
	/// <summary>
	/// Describes basic system information about a client, 
	/// including architecture, OS, and locale.
	/// </summary>
	[Serializable]
	public class ClientInformation
	{
		private OperatingSystem m_operatingSys;
		private ProcessorArchitecture m_architecture;
		private ClientLocale m_Locale;

		public ClientInformation()
		{
			m_operatingSys = OperatingSystem.Win;
			m_architecture = ProcessorArchitecture.x86;
			Locale = ClientLocale.English;
			TimeZone = 0x258;
			IPAddress = new XmlIPAddress(System.Net.IPAddress.Loopback);
		}

		private ClientInformation(PacketIn packet)
		{
			packet.SkipBytes(1);	// 0

			try
			{
				Version = new ClientVersion(packet.ReadBytes(5));
				Architecture = packet.ReadReversedString();
				OS = packet.ReadReversedString();

				var localeStr = packet.ReadReversedPascalString(4);
				if (!ClientLocales.Lookup(localeStr, out m_Locale))
				{
					m_Locale = WCellDef.DefaultLocale;
				}

				TimeZone = BitConverter.ToUInt32(packet.ReadBytes(4), 0);
				IPAddress = new XmlIPAddress(packet.ReadBytes(4));
			}
			catch
			{
			}
		}

		/// <summary>
		/// Possible operating systems of the client
		/// </summary>
		public enum OperatingSystem
		{
			/// <summary>
			/// Mac OSX
			/// </summary>
			OSX,
			/// <summary>
			/// Any supported version of Windows
			/// </summary>
			Win
		}

		/// <summary>
		/// Possible CPU architectures of the client
		/// </summary>
		public enum ProcessorArchitecture
		{
			/// <summary>
			/// x86 architecture (AMD, Intel, Via)
			/// </summary>
			x86,
			/// <summary>
			/// PowerPC architecture (all pre-2006Q1 Apple computers)
			/// </summary>
			PPC
		}

		/// <summary>
		/// The game client version of the client.
		/// </summary>
		public ClientVersion Version
		{
			get;
			set;
		}

		/// <summary>
		/// The operating system of the client.
		/// </summary>
		public string OS
		{
			get { return Enum.GetName(typeof(OperatingSystem), m_operatingSys); }
			set
			{
				try
				{
					m_operatingSys = (OperatingSystem)Enum.Parse(typeof(OperatingSystem), value);
				}
				catch (ArgumentException)
				{
				}
			}
		}

		/// <summary>
		/// The CPU architecture of the client.
		/// </summary>
		public string Architecture
		{
			get { return Enum.GetName(typeof(ProcessorArchitecture), m_architecture); }
			set
			{
				try
				{
					m_architecture = (ProcessorArchitecture)Enum.Parse(typeof(ProcessorArchitecture), value);
				}
				catch (ArgumentException)
				{
				}
			}
		}

		/// <summary>
		/// The location and native language of the client.
		/// </summary>
		public ClientLocale Locale
		{
			get { return m_Locale; }
			set { m_Locale = value; }
		}

		/// <summary>
		/// The timezone of the client.
		/// </summary>
		public uint TimeZone
		{
			get;
			set;
		}

		/// <summary>
		/// The IP address of the client.
		/// Not really trustworthy.
		/// </summary>
		/// <remarks>This is serializable.</remarks>
		public XmlIPAddress IPAddress
		{
			get;
			set;
		}

		/// <summary>
		/// Generates a system information objet from the given packet.
		/// </summary>
		/// <param name="inPacket">contains the system information in a raw, serialized format</param>
		public static ClientInformation ReadFromPacket(PacketIn packet)
		{
			var info = new ClientInformation(packet);
			return info;
		}

		/// <summary>
		/// Serializes a <see cref="ClientInformation" /> object to a binary representation.
		/// </summary>
		/// <param name="clientInfo">the client information object</param>
		/// <returns>the binary representation of the <see cref="ClientInformation" /> object</returns>
		public static byte[] Serialize(ClientInformation clientInfo)
		{
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter bFormatter = new BinaryFormatter();

			bFormatter.Serialize(memStream, clientInfo);

			return memStream.ToArray();
		}

		/// <summary>
		/// Deserializes a <see cref="ClientInformation" /> object from its binary representation.
		/// </summary>
		/// <param name="rawInfoData">the binary data for the <see cref="ClientInformation" /> object</param>
		/// <returns>a <see cref="ClientInformation" /> object</returns>
		public static ClientInformation Deserialize(byte[] rawInfoData)
		{
			MemoryStream memStream = new MemoryStream(rawInfoData);
			BinaryFormatter bFormatter = new BinaryFormatter();

			ClientInformation sInfo = (ClientInformation)bFormatter.Deserialize(memStream);

			return sInfo;
		}
	}
}