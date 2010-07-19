using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cell.Core;

namespace WCell.Core.Network
{
    public class PacketHandler<C, P> where C : IClient
    {
		/// <summary>
		/// Creates a packet handler definition.
		/// </summary>
		/// <param name="handler">the handler to wrap</param>
		/// <param name="gamePacket">whether or not this packet is a game packet (after authntication)</param>
		public PacketHandler(Action<C, P> handler, bool gamePacket, bool requiresLogIn)
        {
            Handler = handler;
            IsGamePacket = gamePacket;
			RequiresLogIn = requiresLogIn;
        }

		/// <summary>
		/// The handler delegate.
		/// </summary>
		public Action<C, P> Handler
        {
            get;
            set;
		}

		/// <summary>
		/// Whether a selected Character is required for this kind of Packet
		/// </summary>
		public bool IsGamePacket
		{
			get;
			set;
		}

		/// <summary>
		/// Whether this Packet requires the player
		/// to be logged in with a Character and not being
		/// in the process of logging out.
		/// </summary>
		public bool RequiresLogIn
		{
			get;
			set;
		}
    }
}