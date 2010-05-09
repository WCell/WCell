/*************************************************************************
 *
 *   file		: PacketId.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-16 05:30:51 +0100 (ma, 16 feb 2009) $
 *   last author	: $LastChangedBy: ralekdev $
 *   revision		: $Rev: 757 $
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
    public enum ServiceType
    {
		None = 0,
        Authentication = 1,
        Realm = 2,
		Count
    }

    public struct PacketId
    {
		public static readonly PacketId Unknown = new PacketId(ServiceType.None, uint.MaxValue);

        public ServiceType Service;
        public uint RawId;

        public PacketId(ServiceType service, uint id)
        {
            Service = service;
            RawId = id;
        }

        public PacketId(AuthServerOpCode id)
        {
            Service = ServiceType.Authentication;
            RawId = (uint) id;
        }

        public PacketId(RealmServerOpCode id)
        {
            Service = ServiceType.Realm;
            RawId = (uint) id;
        }

    	public bool IsUpdatePacket
    	{
			get {
				return RawId == (uint) RealmServerOpCode.SMSG_UPDATE_OBJECT ||
				       RawId == (uint) RealmServerOpCode.SMSG_COMPRESSED_UPDATE_OBJECT;
			}
    	}

        public static implicit operator PacketId(AuthServerOpCode val)
        {
            return new PacketId(val);
        }

        public static implicit operator PacketId(RealmServerOpCode val)
        {
            return new PacketId(val);
        }

        public static bool operator ==(PacketId a, PacketId b)
        {
            return a.RawId == b.RawId && a.Service == b.Service;
        }

        public static bool operator !=(PacketId a, PacketId b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj != null && this == (PacketId) obj;
        }

        public override int GetHashCode()
        {
			return RawId.GetHashCode() ^ (int.MaxValue * (int)Service);
        }

        public override string ToString()
        {
        	string name;
            switch (Service)
            {
                case ServiceType.Authentication:
                    name = Enum.GetName(typeof (AuthServerOpCode), RawId);
            		break;
                case ServiceType.Realm:
                    name = Enum.GetName(typeof (RealmServerOpCode), RawId);
            		break;
                default:
                    return "Unknown";
            }

			if (name != null)
			{
				return name;
			}

        	return RawId.ToString();
        }
    }
}