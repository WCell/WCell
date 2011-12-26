/*************************************************************************
 *
 *   file		: ClientInformation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
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
using WCell.Core.Localization;
using WCell.Core.Network;
using OperatingSystem = WCell.Constants.OperatingSystem;

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
                    Log.Warn(WCell_Core.Auth_Logon_with_invalid_length, claimedRemainingLength, packet.RemainingLength);
                }

                var clientInstallationType = packet.ReadFourCC();
                _clientInstallationType = ClientTypeUtility.Lookup(clientInstallationType);

                Version = new ClientVersion(packet.ReadBytes(5));
                Architecture = packet.ReadFourCC().TrimEnd('\0');
                OS = packet.ReadFourCC().TrimEnd('\0');

                var locale = packet.ReadFourCC();
                _locale = ClientLocaleUtility.Lookup(locale);

                TimeZone = BitConverter.ToUInt32(packet.ReadBytes(4), 0);
                IPAddress = new XmlIPAddress(packet.ReadBytes(4));

                Log.Info(WCell_Core.ClientInformationFourCCs, ProtocolVersion, ClientInstallationType, Version, Architecture, OS, Locale, TimeZone, IPAddress);
            }
            catch
            {
            }
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
		    byte[] memStreamArray;
            using (var memStream = new MemoryStream())
            {
                var bFormatter = new BinaryFormatter();

                bFormatter.Serialize(memStream, clientInfo);
                memStreamArray = memStream.ToArray();
            }
		    return memStreamArray;
		}

		/// <summary>
		/// Deserializes a <see cref="ClientInformation" /> object from its binary representation.
		/// </summary>
		/// <param name="rawInfoData">the binary data for the <see cref="ClientInformation" /> object</param>
		/// <returns>a <see cref="ClientInformation" /> object</returns>
		public static ClientInformation Deserialize(byte[] rawInfoData)
		{
		    ClientInformation sInfo;
            using (var memStream = new MemoryStream(rawInfoData))
            {
                var bFormatter = new BinaryFormatter();

                sInfo = (ClientInformation) bFormatter.Deserialize(memStream);
            }

		    return sInfo;
		}
	}
}