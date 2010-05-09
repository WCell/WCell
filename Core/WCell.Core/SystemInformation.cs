/*************************************************************************
 *
 *   file		: SystemInformation.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-07-27 10:18:17 +0200 (sø, 27 jul 2008) $
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
    public class SystemInformation
    {
        private OperatingSystem m_Operating;
        private ProcessorArch m_architecture;

        public SystemInformation()
        {
            m_Operating = OperatingSystem.Win;
            m_architecture = ProcessorArch.x86;
            Locale = "enUS";
            TimeZone = 0x258;
            IPAddress = new XmlIPAddress(System.Net.IPAddress.Loopback);
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
        public enum ProcessorArch
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
            get { return Enum.GetName(typeof(OperatingSystem), m_Operating); }
            set
            {
                try
                {
                    m_Operating = (OperatingSystem)Enum.Parse(typeof(OperatingSystem), value);
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
            set
            {
                try
                {
                    m_architecture = (ProcessorArch)Enum.Parse(typeof(ProcessorArch), value);
                }
                catch (ArgumentException)
                {
                }
            }
            get { return Enum.GetName(typeof(ProcessorArch), m_architecture); }
        }

        /// <summary>
        /// The location and native language of the client.
        /// </summary>
        public string Locale
        {
            get;
            set;
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
        /// </summary>
        /// <remarks>This is serializable</remarks>
        public XmlIPAddress IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Generates a system information objet from the given packet.
        /// </summary>
        /// <param name="inPacket">contains the system information in a raw, serialized format</param>
        public static SystemInformation ReadFromPacket(PacketIn inPacket)
        {
            SystemInformation info = new SystemInformation();
        	inPacket.SkipBytes(1);	// 0

            try
            {
                info.Version = new ClientVersion(inPacket.ReadBytes(5));
                info.Architecture = inPacket.ReadReversedString();
                info.OS = inPacket.ReadReversedString();
                info.Locale = inPacket.ReadReversedPascalString(4);
                info.TimeZone = BitConverter.ToUInt32(inPacket.ReadBytes(4), 0);
                info.IPAddress = new XmlIPAddress(inPacket.ReadBytes(4));
            }
            catch
            {
            }

            return info;
        }

        /// <summary>
        /// Serializes a SystemInformation object.
        /// </summary>
        /// <param name="info">the SystemInformation object</param>
        /// <returns>a serialized representation of the info object</returns>
        public static byte[] Serialize(SystemInformation info)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter bFormatter = new BinaryFormatter();

            bFormatter.Serialize(memStream, info);

            return memStream.ToArray();
        }

        /// <summary>
        /// Deserializes a SystemInformation object.
        /// </summary>
        /// <param name="serializedInfo">the serialized SystemInformtion object</param>
        /// <returns>a deseralized SystemInformation object</returns>
        public static SystemInformation Deserialize(byte[] serializedInfo)
        {
            MemoryStream memStream = new MemoryStream(serializedInfo);
            BinaryFormatter bFormatter = new BinaryFormatter();

            SystemInformation sInfo = (SystemInformation)bFormatter.Deserialize(memStream);

            return sInfo;
        }
    }
}