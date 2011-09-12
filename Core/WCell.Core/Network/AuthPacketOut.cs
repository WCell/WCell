/*************************************************************************
 *
 *   file		: AuthPacketOut.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-10 07:29:20 +0800 (Mon, 10 Mar 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 184 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using WCell.Constants;
using WCell.Core.Network;

namespace WCell.AuthServer
{
    /// <summary>
    /// Represents an outbound packet going to the client.
    /// </summary>
    public class AuthPacketOut : PacketOut
    {
        /// <summary>
        /// Constant indicating this <c>AuthPacketOut</c> header size.
        /// </summary>
        private const int HEADER_SIZE = 2;

        /// <summary>
        /// The <c>AuthPacketOut</c> header size.
        /// </summary>
        public override int HeaderSize
        {
            get { return HEADER_SIZE; }
        }

        /// <summary>
        /// The op-code of this packet.
        /// </summary>
        public AuthServerOpCode OpCode
        {
            get { return (AuthServerOpCode)PacketId.RawId; }
        }

        ///// <summary>
        ///// Constructor for authentication error packets.
        ///// </summary>
        ///// <param name="packetOpcode">the opcode of the packet</param>
        ///// <param name="error">the error of the packet</param>
        //public AuthPacketOut(AuthServerOpCode packetOpcode, AccountStatus error)
        //    : base(new PacketId(packetOpcode))
        //{
        //    base.WriteByte((byte)packetOpcode);
        //    base.WriteByte((byte)error);
        //}

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="packetOpcode">the opcode of the packet</param>
        public AuthPacketOut(AuthServerOpCode packetOpcode)
            : base(new PacketId(packetOpcode))
        {
            base.WriteByte((byte)packetOpcode);
        }
    }
}