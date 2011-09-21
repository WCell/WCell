/*************************************************************************
 *
 *   file		: AuthClient.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-28 09:34:05 +0100 (lø, 28 mar 2009) $
 
 *   revision		: $Rev: 826 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using Cell.Core;
using WCell.AuthServer.Accounts;
using WCell.Core;
using WCell.Core.Cryptography;

namespace WCell.AuthServer.Network
{
    /// <summary>
    /// The interface for any kind of IAuthClient (can be used to create fake-IAuthClients)
    /// </summary>
    public interface IAuthClient : IClient
    {
        /// <summary>
        /// The server this client is connected to.
        /// </summary>
        new AuthenticationServer Server { get; }

        /// <summary>
        /// The system information for this client.
        /// </summary>
        ClientInformation Info { get; set; }

        /// <summary>
        /// The current user.
        /// </summary>
		string AccountName { get; set; }

		/// <summary>
		/// The authenticator instance for this client.
		/// </summary>
		Account Account { get; set; }

		/// <summary>
		/// The authenticator instance for this client.
		/// </summary>
		Authenticator Authenticator { get; set; }

        /// <summary>
        /// Sends a packet to the client.
        /// </summary>
        /// <param name="packet">the packet to send</param>
        void Send(AuthPacketOut packet);
    }
}