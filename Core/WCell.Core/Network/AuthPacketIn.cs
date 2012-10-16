/*************************************************************************
 *
 *   file		: AuthPacketIn.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-04-28 12:55:13 +0800 (Mon, 28 Apr 2008) $
 *   last author	: $LastChangedBy: tobz $
 *   revision		: $Rev: 301 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using Cell.Core;
using WCell.Constants;
using WCell.Core.Network;

namespace WCell.AuthServer
{
    /// <summary>
    /// Represents an inbound packet from the client.
    /// </summary>
    public sealed class AuthPacketIn : PacketIn
    {
        #region Properties

        /// <summary>
        /// Constant indicating this <c>AuthPacketIn</c> header size.
        /// </summary>
        private const int _headerSize = 1;

        /// <summary>
        /// The <c>AuthPacketOut</c> header size.
        /// </summary>
        public override int HeaderSize
        {
            get { return _headerSize; }
        }

        #endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="length">the length of bytes to read</param>
		public AuthPacketIn(BufferSegment segment, int length)
			: base(segment, 0, length)
		{
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="offset">the zero-based index to read from</param>
		/// <param name="length">the length of bytes to read</param>
		public AuthPacketIn(BufferSegment segment, int offset, int length)
			: base(segment, offset, length)
		{
			_packetID = (AuthServerOpCode)ReadByte();
		}

        #endregion
    }
}