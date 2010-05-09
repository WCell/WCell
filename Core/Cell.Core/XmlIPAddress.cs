/*************************************************************************
 *
 *   file		: XmlIPAddress.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-18 09:09:50 +0100 (on, 18 feb 2009) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 766 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Net;
using System.Xml.Serialization;

namespace Cell.Core
{
    /// <summary>
    /// This class provides a wrapper for <see cref="System.Net.IPAddress"/> that can be serialized with XML.
    /// </summary>
    /// <seealso cref="XmlConfig"/>
    /// <seealso cref="System.Xml.Serialization"/>
    /// <seealso cref="System.Net.IPAddress"/>
    [Serializable]
    public class XmlIPAddress
    {
        /// <summary>
        /// The <see cref="IPAddress"/>.
        /// </summary>
        private IPAddress _ipAddress = new IPAddress(0x0100007f); //127.0.0.1

        /// <summary>
        /// Gets/Sets a string representation of a <see cref="System.Net.IPAddress"/>.
        /// </summary>
        public string Address
        {
            get { return _ipAddress.ToString(); }
            set
            {
                IPAddress buf;
                if (IPAddress.TryParse(value, out buf))
                {
                    _ipAddress = buf;
                }
            }
        }

        /// <summary>
        /// Gets/Sets the internal <see cref="System.Net.IPAddress"/>.
        /// </summary>
        [XmlIgnore]
        public IPAddress IPAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="XmlIPAddress"/> class.
        /// </summary>
        public XmlIPAddress()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlIPAddress"/> class with the address specified as a <see cref="System.Byte"/> array.
        /// </summary>
        /// <param name="address">The byte array value of the IP address.</param>
        /// <exception cref="System.ArgumentNullException">address is null.</exception>
        public XmlIPAddress(byte[] address)
        {
            _ipAddress = new IPAddress(address);
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="XmlIPAddress"/> class with the specified address and scope.
        /// </summary>
        /// <param name="address">The byte array value of the IP address.</param>
        /// <param name="scopeid">The long value of the scope identifier.</param>
        /// <exception cref="System.ArgumentNullException">address is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// scopeid &lt; 0 or
        /// scopeid > 0x00000000FFFFFFF
        /// </exception>
        public XmlIPAddress(byte[] address, long scopeId)
        {
            _ipAddress = new IPAddress(address, scopeId);
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="XmlIPAddress"/> class with the address specified as a <see cref="System.Int64"/>.
        /// </summary>
        /// <param name="newAddress">The long value of the IP address.</param>
		/// <remarks>For example, the value 0x2414188f in big endian format would be the IP address "143.24.20.36".+ </remarks>
        public XmlIPAddress(long newAddress)
        {
            _ipAddress = new IPAddress(newAddress);
        }

        /// <summary>
        /// Initializes a new instace of the <see cref="XmlIPAddress"/> class with the address specified as a <see cref="System.Net.IPAddress"/>.
        /// </summary>
        /// <param name="newAddress">The new <see cref="IPAddress"/>.</param>
        public XmlIPAddress(IPAddress newAddress)
        {
            _ipAddress = newAddress;
        }

        /// <summary>
        /// Converts the <see cref="XmlIPAddress"/> into a string.
        /// </summary>
        /// <returns>A string representation of the internal <see cref="IPAddress"/>.</returns>
        public override string ToString()
        {
            return _ipAddress.ToString();
        }

        /// <summary>
        /// Gets a hash code for the object.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return _ipAddress.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            XmlIPAddress xmlAddress = obj as XmlIPAddress;
            if (xmlAddress != null)
            {
                return xmlAddress.IPAddress.Equals(IPAddress);
            }

            return false;
        }
    }
}