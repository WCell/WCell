/*************************************************************************
 *
 *   file		: SystemInformation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-07-27 16:18:17 +0800 (Sun, 27 Jul 2008) $
 
 *   revision		: $Rev: 582 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Cell.Core;
using NLog;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.Core
{
	/// <summary>
	/// Describes basic system information about a client, 
	/// including architecture, OS, and locale.
	/// </summary>
	[Serializable]
	public class ClientInformation
	{
	    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        private ClientType _clientInstallationType;
		private OperatingSystem _operatingSys;
		private ProcessorArchitecture _architecture;
		private ClientLocale _locale;

		public ClientInformation()
		{
		    _clientInstallationType = ClientType.Normal;
			_operatingSys = OperatingSystem.Win;
			_architecture = ProcessorArchitecture.x86;
			Locale = ClientLocale.English;
			TimeZone = 0x258;
			IPAddress = new XmlIPAddress(System.Net.IPAddress.Loopback);
		}

		private ClientInformation(PacketIn packet)
		{
            try
            {
                ProtocolVersion = packet.ReadByte();
                var claimedRemainingLength = packet.ReadUInt16();
                if (packet.RemainingLength != claimedRemainingLength)
                {
                    Log.Warn("Client attempting login sent AUTH_LOGON_CHALLENGE remaining length as {0}, however {1} bytes are remaining", claimedRemainingLength, packet.RemainingLength);
                }

                var clientInstallationType = packet.ReadFourCC();
                ClientTypes.Lookup(clientInstallationType, out _clientInstallationType);

                Version = new ClientVersion(packet.ReadBytes(5));
                Architecture = packet.ReadFourCC().TrimEnd('\0');
                OS = packet.ReadFourCC().TrimEnd('\0');

                var localeStr = packet.ReadFourCC();
                if (!ClientLocales.Lookup(localeStr, out _locale))
                {
                    _locale = WCellConstants.DefaultLocale;
                }

                TimeZone = BitConverter.ToUInt32(packet.ReadBytes(4), 0);
                IPAddress = new XmlIPAddress(packet.ReadBytes(4));

                Log.Info("ProtocolVersion: {0} ClientType: {1} Version: {2} Architecture: {3} OS: {4} Locale: {5} TimeZone: {6} IP: {7}", ProtocolVersion, ClientInstallationType, Version, Architecture, OS, Locale, TimeZone, IPAddress);
            }
            catch
            {
            }
		}

        public enum ClientType : byte
        {
            Test = 0,
            Beta = 1,
            Normal = 2,
            Installing = 3,
            Invalid = 4
        }

        public static class ClientTypes
        {
            public static readonly Dictionary<string, ClientType> TypeMap = 
			new Dictionary<string, ClientType>(StringComparer.InvariantCultureIgnoreCase);

            static ClientTypes()
            {
                TypeMap["WoWT"] = ClientType.Test;
                TypeMap["WoWB"] = ClientType.Beta;
                TypeMap["WoW\0"] = ClientType.Normal;
                TypeMap["WoWI"] = ClientType.Installing;
            }

            public static bool Lookup(string clientInstallationTypeStr, out ClientType clientType)
            {
                if (!TypeMap.TryGetValue(clientInstallationTypeStr.Substring(0, 4), out clientType))
                {
                    clientType = ClientType.Invalid;
                    return false;
                }
                return true;
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
        /// The game client version of the client.
        /// </summary>
        public byte ProtocolVersion
        {
            get;
            set;
        }

        /// <summary>
        /// The type of client that is attempting to connect.
        /// </summary>
        public ClientType ClientInstallationType
        {
            get { return _clientInstallationType; }
            set { _clientInstallationType = value; }
        }

		/// <summary>
		/// The operating system of the client.
		/// </summary>
		public string OS
		{
			get { return Enum.GetName(typeof(OperatingSystem), _operatingSys); }
			set
			{
				try
				{
					_operatingSys = (OperatingSystem)Enum.Parse(typeof(OperatingSystem), value);
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
			get { return Enum.GetName(typeof(ProcessorArchitecture), _architecture); }
			set
			{
				try
				{
					_architecture = (ProcessorArchitecture)Enum.Parse(typeof(ProcessorArchitecture), value);
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
			get { return _locale; }
			set { _locale = value; }
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