using Cell.Core;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Network
{
	public interface IPacketReceiver
	{
		/// <summary>
		/// Sends a packet to the target.
		/// </summary>
		/// <param name="packet">the packet to send</param>
		void Send(RealmPacketOut packet);
	}

    /// <summary>
    /// The interface for any kind of IRealmClient (can be used to create fake-IRealmClients)
    /// </summary>
    public interface IRealmClient : IClient, IPacketReceiver
    {
        /// <summary>
        /// The server this client is serviced by.
        /// </summary>
        new RealmServer Server { get; }

        /// <summary>
        /// The <see cref="ClientInformation">system information</see> for this client.
        /// </summary>
        ClientInformation Info { get; set; }

        /// <summary>
        /// The compressed addon data sent by the client.
        /// </summary>
        byte[] Addons { get; set; }

        /// <summary>
        /// The account on this session.
        /// </summary>
        RealmAccount Account { get; set; }

        /// <summary>
        /// The <see cref="Character" /> that the client is currently playing.
        /// </summary>
        Character ActiveCharacter { get; set; }

        /// <summary>
        /// Whether or not this client is currently offline (not connected anymore)
        /// </summary>
        bool IsOffline { get; set; }

        /// <summary>
        /// Whether or not communication with this client is encrypted.
        /// </summary>
        bool IsEncrypted
        { 
            get;
        }

        /// <summary>
        /// The local system uptime of the client.
        /// </summary>
        uint ClientTime { get; set; }

        /// <summary>
        /// Connection latency between client and server.
        /// </summary>
		int Latency { get; set; }

		/// <summary>
		/// The amount of time skipped by the client.
		/// </summary>
		/// <remarks>Deals with the the way we calculate movement delay.</remarks>
		uint OutOfSyncDelay { get; set; }

		/// <summary>
		/// The time that was sent by the Client in the last movement-packet
		/// </summary>
		uint LastClientMoveTime { get; set; }

        /// <summary>
        /// The client tick count.
        /// </summary>
        /// <remarks>It is set by opcodes 912/913, and seems to be a client ping sequence that is
        /// local to the map, and thus it resets to 0 on a map change.  Real usage isn't known.</remarks>
        uint TickCount { get; set; }

        /// <summary>
        /// The client seed sent by the client during re-authentication.
        /// </summary>
        uint ClientSeed { get; set; }

        /// <summary>
        /// The authentication message digest received from the client during re-authentication.
        /// </summary>
        BigInteger ClientDigest { get; set; }
	
        /// <summary>
        /// The current session key
        /// </summary>
        byte[] SessionKey
        {
            get;
            set;
        }

        /// <summary>
        /// Disconnects the client.
        /// </summary>
        void Disconnect();
    }
}