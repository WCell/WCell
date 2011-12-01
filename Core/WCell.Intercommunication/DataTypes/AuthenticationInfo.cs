/*************************************************************************
 *
 *   file		: AuthenticationInfo.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-06-17 19:24:35 +0200 (on, 17 jun 2009) $
 
 *   revision		: $Rev: 993 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Runtime.Serialization;

namespace WCell.Intercommunication.DataTypes
{
    /// <summary>
    /// Holds authentication information
    /// </summary>
    [DataContract]
    public class AuthenticationInfo
    {
        /// <summary>
        /// Session key used for the session
        /// </summary>
        [DataMember]
        public byte[] SessionKey;

        /// <summary>
        /// Salt used for the session
        /// </summary>
        [DataMember]
        public byte[] Salt;

        /// <summary>
        /// Verifier used for the session
        /// </summary>
        [DataMember]
        public byte[] Verifier;

        /// <summary>
        /// System information of the client
        /// </summary>
        [DataMember]
        public byte[] SystemInformation;
    }
}