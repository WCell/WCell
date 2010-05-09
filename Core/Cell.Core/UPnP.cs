///*************************************************************************
// *
// *   file		: UPnP.cs
// *   copyright		: (C) The WCell Team
// *   email		: info@wcell.org
// *   last changed	: $LastChangedDate: 2009-03-10 23:51:03 +0100 (ti, 10 mar 2009) $
// *   last author	: $LastChangedBy: dominikseifert $
// *   revision		: $Rev: 793 $
// *
// *   This program is free software; you can redistribute it and/or modify
// *   it under the terms of the GNU General Public License as published by
// *   the Free Software Foundation; either version 2 of the License, or
// *   (at your option) any later version.
// *
// *************************************************************************/

//using NATUPNPLib;

//namespace Cell.Core
//{
//    /// <summary>
//    /// Handles NAT negotiations for Cell on Universal Plug-and-Play routers.
//    /// </summary>
//    public static class UPnP
//    {
//        /// <summary>
//        /// Checks to see if a mapping already exists.
//        /// </summary>
//        /// <param name="externalPort">The external port of the mapping.</param>
//        /// <param name="internalAddr">The internal address of the mapping.</param>
//        /// <param name="protocol">The protocol of the mapping.</param>
//        /// <returns>True if the mapping exists, false otherwise.</returns>
//        public static bool CheckMapping(int externalPort, string internalAddr, string protocol)
//        {
//            UPnPNATClass cls = new UPnPNATClass();

//            IStaticPortMappingCollection spmc = cls.StaticPortMappingCollection;

//            foreach (IStaticPortMapping mapping in spmc)
//            {
//                if (mapping.ExternalPort == externalPort
//                    && mapping.InternalClient == internalAddr
//                    && mapping.Protocol == protocol)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        /// <summary>
//        /// Checks to see if a mapping already exists.
//        /// </summary>
//        /// <param name="externalPort">The external port of the mapping.</param>
//        /// <param name="internalAddr">The internal address of the mapping.</param>
//        /// <param name="protocol">The protocol of the mapping.</param>
//        /// <returns>True if the mapping exists, false otherwise.</returns>
//        public static bool CheckMapping(int externalPort, XmlIPAddress internalAddr, string protocol)
//        {
//            UPnPNATClass cls = new UPnPNATClass();

//            IStaticPortMappingCollection spmc = cls.StaticPortMappingCollection;

//            foreach (IStaticPortMapping mapping in spmc)
//            {
//                if (mapping.ExternalPort == externalPort
//                    && mapping.InternalClient == internalAddr.Address
//                    && mapping.Protocol == protocol)
//                {
//                    return true;
//                }
//            }

//            return false;
//        }

//        /// <summary>
//        /// Maps an external port to an internal port, using the specified protocol.
//        /// </summary>
//        /// <param name="externalPort">The external port to map.</param>
//        /// <param name="internalPort">The internal port to map to.</param>
//        /// <param name="protocol">The protocol of the port.</param>
//        /// <param name="internalAddr">The internal address to map to.</param>
//        /// <param name="description">A description of the mapping.</param>
//        public static void NegotiateMapping(int externalPort, int internalPort, string protocol, string internalAddr,
//                                            string description)
//        {
//            UPnPNATClass cls = new UPnPNATClass();

//            IStaticPortMappingCollection spmc = cls.StaticPortMappingCollection;

//            spmc.Add(externalPort, protocol, internalPort, internalAddr, true, "[Cell] Custom - " + description);
//        }

//        /// <summary>
//        /// Maps an external port to an internal port, using the specified protocol.
//        /// </summary>
//        /// <param name="externalPort">The external port to map.</param>
//        /// <param name="internalPort">The internal port to map to.</param>
//        /// <param name="protocol">The protocol of the port.</param>
//        /// <param name="internalAddr">The internal address to map to.</param>
//        /// <param name="description">A description of the mapping.</param>
//        public static void NegotiateMapping(int externalPort, int internalPort, string protocol,
//                                            XmlIPAddress internalAddr, string description)
//        {
//            UPnPNATClass cls = new UPnPNATClass();

//            IStaticPortMappingCollection spmc = cls.StaticPortMappingCollection;

//            spmc.Add(externalPort, protocol, internalPort, internalAddr.Address, true, "[Cell] " + description);
//        }

//        /// <summary>
//        /// Removes all previous NAT mappings on any device made from Cell.
//        /// </summary>
//        public static void RemoveAllMappings()
//        {
//            UPnPNATClass cls = new UPnPNATClass();

//            IStaticPortMappingCollection spmc = cls.StaticPortMappingCollection;

//            foreach (IStaticPortMapping mapping in spmc)
//            {
//                if (mapping.Description.Contains("[Cell]"))
//                {
//                    spmc.Remove(mapping.ExternalPort, mapping.Protocol);
//                }
//            }
//        }
//    }
//}