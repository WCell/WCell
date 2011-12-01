/*************************************************************************
 *
 *   file		: PacketHandlerAttribute.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-18 18:10:11 +0200 (on, 18 jun 2008) $
 
 *   revision		: $Rev: 515 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;

namespace WCell.Core.Network
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class PacketHandlerAttribute : Attribute
    {
        public PacketId Id
        {
            get;
            set;
		}

		public bool IsGamePacket
		{
			get;
			set;
		}

		public bool RequiresLogin
		{
			get;
			set;
		}

        public ServiceType Service
        {
            get { return Id.Service; }
        }

        public PacketHandlerAttribute(PacketId identifier)
        {
            Id = identifier;
            IsGamePacket = true;
			RequiresLogin = true;
        }

        public PacketHandlerAttribute(AuthServerOpCode identifier)
        {
            Id = identifier;
			IsGamePacket = true;
			RequiresLogin = true;
        }

        public PacketHandlerAttribute(RealmServerOpCode identifier)
        {
            Id = identifier;
			IsGamePacket = true;
			RequiresLogin = true;
        }
    }

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public sealed class ClientPacketHandlerAttribute : PacketHandlerAttribute
	{

        public ClientPacketHandlerAttribute(PacketId identifier)
			: base(identifier)
        {
        }

		public ClientPacketHandlerAttribute(AuthServerOpCode identifier)
			: base(identifier)
        {
        }

		public ClientPacketHandlerAttribute(RealmServerOpCode identifier)
			: base(identifier)
        {
        }
	}
}