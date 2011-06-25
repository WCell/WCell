/*************************************************************************
 *
 *   file		: RealmStruct.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-19 00:10:11 +0800 (Thu, 19 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 515 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Threading;
using Cell.Core;
using WCell.AuthServer.Accounts;
using WCell.AuthServer.Network;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Realm;
using WCell.Core;
using WCell.Core.Network;
using System.ServiceModel;

namespace WCell.AuthServer
{
    /// <summary>
    /// Defines one Entry in the Realm-list
    /// </summary>
    public class RealmEntry
    {
        private static readonly int MaintenanceInterval = (int)(WCellConstants.RealmServerUpdateInterval.TotalMilliseconds);
        private static readonly TimeSpan MaxUpdateOfflineDelay = TimeSpan.FromSeconds(WCellConstants.RealmServerUpdateInterval.TotalSeconds * 1.5);
        private static readonly TimeSpan MaxUpdateDeadDelay = TimeSpan.FromSeconds(WCellConstants.RealmServerUpdateInterval.TotalSeconds * 10);
        //private readonly Timer m_maintenanceTimer;

        public RealmEntry()
        {
            //m_maintenanceTimer = new Timer(Maintain);
            StartMaintain();
            LastUpdate = DateTime.Now;
        }

        #region Properties
    	public bool IsOnline
    	{
    		get { return !Flags.HasFlag(RealmFlags.Offline); }
    	}

        public DateTime LastUpdate
        {
            get;
            set;
        }

        /// <summary>
        /// Internally assigned unique Id of this Realm.
        /// </summary>
        public string ChannelId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm icon as shown in the realm list. (aka PVP, PVE, etc) 
        /// </summary>
        public RealmServerType ServerType
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm status as shown in the realm list. (Good, Locked)
        /// </summary>
        public RealmStatus Status
        {
            get;
            internal set;
        }

        /// <summary>
        /// </summary>
        public RealmFlags Flags
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm name. 
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm address
        /// </summary>
        public string Address
        {
            get;
            internal set;
        }

        public int Port
        {
            get;
            internal set;
        }

		public string GetAddress(string address)
		{
			return address + ":" + Port;
		}

        public string AddressString
        {
            get { return GetAddress(Address); }
        }

		/// <summary>
		/// The supported client version of this realm
		/// </summary>
		public ClientVersion ClientVersion
		{
			get;
			internal set;
		}

        public int CharCount
        {
            get;
            internal set;
        }

        public int CharCapacity
        {
            get;
            internal set;
        }

        /// <summary>
        /// Realm population. (1.6+ for high value and 1.6- for lower) 
        /// </summary>
        public float Population
        {
            get
            {
                return (CharCount > CharCapacity * 0.75 ? 1.7f : (CharCount > CharCapacity / 3 ? 1.6f : 1.5f));
            }
        }

        /// <summary>
        /// Characters the client has on this realm. 
        /// </summary>
		public byte Chars
		{
			get;
			internal set;
		}

        /// <summary>
        /// Realm timezone.
        /// </summary>
        public RealmCategory Category
        {
            get;
            internal set;
		}

		/// <summary>
		/// Remote address of the Realm for Auth/Realm - communication
		/// </summary>
		public IContextChannel Channel
		{
			get;
			internal set;
		}

		/// <summary>
		/// Remote address of the Realm for Auth/Realm - communication
		/// </summary>
		public string ChannelAddress
		{
			get;
			internal set;
		}

		/// <summary>
		/// Remote port of the Realm for Auth/Realm - communication
		/// </summary>
		public int ChannelPort
		{
			get;
			internal set;
		}
        #endregion

        void StartMaintain()
        {
            //m_maintenanceTimer.Change(Timeout.Infinite, MaintenanceInterval);
        }

		//void Maintain(object sender)
		//{
		//    var delay = DateTime.Now - LastUpdate;
		//    if (delay > MaxUpdateOfflineDelay)
		//    {
		//        // server didn't react in a long time -> Consider it offline
		//        Flags = RealmFlags.Offline;

		//        if (delay > MaxUpdateDeadDelay)
		//        {
		//            SetOffline();
		//        }
		//    }
		//}

		/// <summary>
		/// Disconnects the given Realm and flags it as offline, or removes it from the RealmList entirely.
		/// Also removes all logged in accounts.
		/// </summary>
		/// <param name="remove">Whether to remove it entirely (true) or only show as offline (false)</param>
		public void Disconnect(bool remove)
		{
			if (Channel != null)
			{
				Channel.Abort();
			}
			SetOffline(remove);
		}

        /// <summary>
        /// Flags this realm as offline, or removes it from the RealmList entirely.
        /// Also removes all logged in accounts.
        /// </summary>
        internal void SetOffline(bool remove)
		{
			AuthenticationServer.RemoveRealm(this, remove);
        }

        #region Send + Receive
        /// <summary>
        /// Handles an incoming realm list request.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(AuthServerOpCode.REALM_LIST)]
        public static void RealmListRequest(IAuthClient client, PacketIn packet)
        {
            SendRealmList(client);
        }

        /// <summary>
        /// Sends the realm list to the client.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        public static void SendRealmList(IAuthClient client)
        {
			if (client.Account == null)
			{
				AuthenticationHandler.OnLoginError(client, AccountStatus.Failure);
				return;
			}
            using (var packet = new AuthPacketOut(AuthServerOpCode.REALM_LIST))
            {
            	packet.Position += 2;							// Packet length
                packet.Write(0);								// Unknown Value (0x0000)
            	//var cpos = packet.Position;
            	//packet.Position = cpos + 2;

            	packet.Write((short)AuthenticationServer.RealmCount);

            	//var count = 0;
                foreach (var realm in AuthenticationServer.Realms)
                {
                	// check for client version
                	//if (realm.ClientVersion.IsSupported(client.Info.Version))
                	realm.WriteRealm(client, packet);
                }

            	//packet.Write((byte)0x15);
                packet.Write((byte)0x10);
				packet.Write((byte)0x00);

				//packet.Position = cpos;
				//packet.WriteShort(count);

                packet.Position = 1;
                packet.Write((short)packet.TotalLength - 3);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Writes the realm information to the specified packet.
        /// </summary>
        /// <param name="packet">the packet to write the realm info to</param>
        public void WriteRealm(IAuthClient client, AuthPacketOut packet)
        {
			var status = Status;
			var flags = Flags;
			var name = Name;

			if (!ClientVersion.IsSupported(client.Info.Version))
			{
				// if client is not supported, flag realm as offline and append the required client version
				flags = RealmFlags.Offline;
				name += " [" + ClientVersion.BasicString + "]";
			}
            else if (Flags.HasFlag(RealmFlags.Offline) && Status == RealmStatus.Locked)
            {
				// let staff members join anyway
				if (client.Account.Role.IsStaff)
                {
                    status = RealmStatus.Open;
                	flags = RealmFlags.None;
                }
			}


			var addr = NetworkUtil.GetMatchingLocalIP(client.ClientAddress) ?? (object)Address;

			packet.Write((byte)ServerType);
            packet.Write((byte)status);
            packet.Write((byte)flags);
            packet.WriteCString(name);

			packet.WriteCString(GetAddress(addr.ToString()));

            packet.WriteFloat(Population);
			packet.Write(Chars); // TODO: Change to amount of Characters of the querying account on this Realm
			packet.Write((byte)Category);

			if (flags.HasFlag(RealmFlags.SpecifyBuild))
			{
				packet.Write(ClientVersion.Major);
				packet.Write(ClientVersion.Minor);
				packet.Write(ClientVersion.Revision);
				packet.Write(ClientVersion.Build);
			}

			packet.WriteByte(0x00);
        }

        #endregion

		public override string ToString()
		{
			return Name + " @ " + AddressString;
		}
    }
}