using System;
using WCell.Util.Logging;
using WCell.Core;
using WCell.Core.Cryptography;
using WCell.Core.Network;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Network
{
	public class FakeRealmClient : FakeClientBase<IRealmClient, RealmPacketIn, RealmPacketOut, FakePacketMgr>, IRealmClient
	{
		public static event Action<IRealmClient, RealmPacketIn> ServerReceivedPacket;

		protected static Logger s_log = LogManager.GetCurrentClassLogger();

		public FakeRealmClient(RealmAccount acc)
			: base(RealmServer.Instance, FakePacketMgr.Instance)
		{
			Account = acc;
			//if (!Account.Initialize())
			//{
			//    throw new Exception("Could not initialize Account!");
			//}
		}

		#region Properties

		/// <summary>
		/// The Addon data for the client.
		/// </summary>
		public byte[] Addons
		{
			get;
			set;
		}

	    /// <summary>
		/// The account on this session.
		/// </summary>
		public RealmAccount Account
		{
			get;
			set;
		}

		/// <summary>
		/// Determines whether the incoming and outgoing packets are encrypted.
		/// </summary>
		public bool IsEncrypted
		{
			get;
			set;
		}

		/// <summary>
		/// The amount of time skipped by the client.
		/// </summary>
		/// <remarks>Deals with the the way we calculate movement delay.</remarks>
		public uint OutOfSyncDelay
		{
			get;
			set;
		}

		public uint LastClientMoveTime
		{
			get; set;
		}

		/// <summary>
		/// The client tick count.
		/// </summary>
		/// <remarks>It is set by opcodes 912/913, and seems to be a client ping sequence that is
		/// local to the map, and thus it resets to 0 on a map change.  Real usage isn't known.</remarks>
		public uint TickCount
		{
			get;
			set;
		}

		/// <summary>
		/// The local system uptime of the client.
		/// </summary>
		public uint ClientTime
		{
			get;
			set;
		}

		/// <summary>
		/// Connection latency between client and server.
		/// </summary>
		public int Latency
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="Character" /> that the client is actively playing.
		/// </summary>
		public Character ActiveCharacter
		{
			get;
			set;
		}

		/// <summary>
		/// The <see cref="ClientInformation"/> for this client.
		/// </summary>
		public ClientInformation Info
		{
			get;
			set;
		}

		public bool IsOffline
		{
			get;
			set;
		}

		public new RealmServer Server
		{
			get
			{
				return (RealmServer)m_server;
			}
		}

		/// <summary>
		/// The client seed sent by the client during re-authentication.
		/// </summary>
		public uint ClientSeed { get; set; }

		/// <summary>
		/// The authentication message digest received from the client during re-authentication.
		/// </summary>
		public BigInteger ClientDigest { get; set; }


		protected override IRealmClient _ThisClient
		{
			get { return this; }
		}
		#endregion

		#region Receive
		/// <summary>
		/// Lets the server handle the given packet.
		/// </summary>
		public void ReceiveCMSG(DisposableRealmPacketIn packet, bool wait)
		{
			ReceiveCMSG(packet, true, wait);
		}

		/// <summary>
		/// Lets the server handle the given packet.
		/// </summary>
		public void ReceiveCMSG(DisposableRealmPacketIn packet, bool dispose, bool wait)
		{
			HandleCMSG(packet, wait);

			if (dispose)
			{
				((IDisposable)packet).Dispose();
			}
		}

		/// <summary>
		/// Lets the server handle the given packet, as though it was sent by this Client.
		/// </summary>
		public void ReceiveCMSG(RealmPacketOut packet)
		{
			ReceiveCMSG(packet, true, false);
		}

		/// <summary>
		/// Lets the server handle the given packet, as though it was sent by this Client.
		/// </summary>
		/// <param name="wait">Whether to wait for the Packet finish processing</param>
		public void ReceiveCMSG(RealmPacketOut packet, bool wait)
		{
			ReceiveCMSG(packet, true, wait);
		}

		/// <summary>
		/// Lets the server handle the given packet, as though it was sent by this Client.
		/// </summary>
		/// <param name="dispose">Whether to dispose the packet after handling</param>
		/// <param name="wait">Whether to wait for the Packet finish processing</param>
		public void ReceiveCMSG(RealmPacketOut packet, bool dispose, bool wait)
		{
			var inPacket = DisposableRealmPacketIn.CreateFromOutPacket(packet);
			HandleCMSG(inPacket, wait);
			var evt = ServerReceivedPacket;
			if (evt != null)
			{
				evt(this, inPacket);
			}
			if (dispose)
			{
				packet.Close();
			}
		}

		/// <param name="wait">Whether to wait for the Packet to be fully processed</param>
		protected virtual void HandleCMSG(RealmPacketIn inPacket, bool wait)
		{
			if (!RealmPacketMgr.Instance.HandlePacket(this, inPacket) ||
				m_server == null)
			{
				throw new Exception("Packet Processing failed");
			}
			if (wait)
			{
				ActiveCharacter.Map.WaitTicks(2);
			}
		}
		#endregion

		#region Send
		protected override bool HandleSMSG(RealmPacketIn packet)
		{
			bool result = m_packetManager.HandlePacket(_ThisClient, packet);

			if (!result)
			{
				if (m_server == null)
				{
					throw new Exception("Processing of Packet " + packet + " failed!");
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Sends the given Packet and waits until after it was processed,
		/// in case that it was enqueued
		/// </summary>
		public void SendAndWait(RealmPacketOut packet)
		{
			SendAndWait(packet, true);
		}

		/// <summary>
		/// Sends the given Packet and waits until after it was processed,
		/// in case that it was enqueued
		/// </summary>
		public void SendAndWait(RealmPacketOut packet, bool dispose)
		{
			var inPacket = CreatePacket(packet);
			if (!HandleSMSG(inPacket))
			{
				// delayed
				ActiveCharacter.Map.WaitOneTick();
			}
			if (dispose)
			{
				packet.Close();
			}
		}
		#endregion

		/// <summary>
		/// Shutdown Client and remove from server.
		/// </summary>
		public void Disconnect()
		{
			Dispose();
		}

		/// <summary>
		/// Creates a new PacketIn from the buffer of an existing PacketOut
		/// </summary>
		protected override RealmPacketIn CreatePacket(byte[] bytesOut, int offset, int length)
		{
			return DisposableRealmPacketIn.CreateFromOutPacket(bytesOut, offset, length);
		}

		protected override RealmPacketIn CreatePacket(RealmPacketOut packet)
		{
			return DisposableRealmPacketIn.CreateFromOutPacket(packet);
		}

		public override string ToString()
		{
			return "FakeRealmClient '" + Account.Name + "'";
		}

		#region IRealmClient Members


		public byte[] SessionKey
		{
			get;
			set;
		}

		#endregion
	}
}