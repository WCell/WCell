/*************************************************************************
 *
 *   file		: PacketManager.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-13 20:56:18 +0800 (Fri, 13 Mar 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 801 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Reflection;
using NLog;
using WCell.Constants;
using WCell.Constants.Items;
using WCell.Core;
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.PacketAnalysis;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Res;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Network
{
	/// <summary>
	/// Manages packet handlers and the execution of them.
	/// </summary>
	public class RealmPacketMgr : PacketManager<IRealmClient, RealmPacketIn, PacketHandlerAttribute>
	{
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		static RealmPacketMgr()
		{
			Instance = new RealmPacketMgr();
		}

		public static readonly RealmPacketMgr Instance;

		public override uint MaxHandlers
		{
			get { return (uint)RealmServerOpCode.Maximum; }
		}

		#region Handle Packets
		/// <summary>
		/// Attempts to handle an incoming packet. 
		/// Constraints: OpCode must be valid.
		/// GamePackets cannot be sent if ActiveCharacter == null or Account == null.
		/// The packet is disposed after being handled.
		/// </summary>
		/// <param name="client">the client the packet is from</param>
		/// <param name="packet">the packet to be handled</param>
		/// <returns>true if the packet could be handled or false otherwise</returns>
		public override bool HandlePacket(IRealmClient client, RealmPacketIn packet)
		{
			var dispose = true;

			try
			{
                Console.WriteLine("Got " + packet.PacketId + " (" + packet.PacketId.RawId + ")");
#if DEBUG
				DebugUtil.DumpPacket(client.Account, packet, PacketSender.Client);
#endif
				if (packet.PacketId.RawId == (int)RealmServerOpCode.CMSG_PING)
				{
					// we want to instantly respond to pings, otherwise it throws off latency
				    MiscHandler.PingRequest(client, packet);
					return true;
				}

				var handlerDesc = m_handlers.Get(packet.PacketId.RawId);

				try
				{
					if (handlerDesc == null)
					{
						HandleUnhandledPacket(client, packet);
						return true;
					}

				    var context = CheckConstraints(client, handlerDesc, packet);
					if (context != null)
					{
						context.AddMessage(new PacketMessage(handlerDesc.Handler, client, packet));
						dispose = false;
						return true;
					}
					return false;
				}
				catch (Exception e)
				{
					LogUtil.ErrorException(e, Resources.PacketHandleException, client, packet.PacketId);
					return false;
				}
			}
			finally
			{
				if (dispose)
				{
					packet.Close();
				}
			}
		}

		/// <summary>
		/// Gets the <see cref="IContextHandler"/> that should handle this incoming packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public IContextHandler CheckConstraints(IRealmClient client, PacketHandler<IRealmClient, RealmPacketIn> handlerDesc, RealmPacketIn packet)
		{
			if (!client.IsConnected)
			{
				return null;
			}

			var chr = client.ActiveCharacter;
			if (handlerDesc.RequiresLogIn)
			{
				if (chr == null)
				{
					// silently ignore invalid packets packets
					s_log.Warn("Client {0} sent Packet {1} without selected Character.", client, packet);
					return null;
				}
				if (chr.IsLoggingOut)
				{
					if (chr.IsPlayerLogout)
					{
						// when doing anything while logging out (except for Chat), cancel it
						chr.CancelLogout(true);
					}
					else
					{
						// ignore requests when being kicked
						ItemHandler.SendCantDoRightNow(client);
						return null;
					}
				}
			}

			var acc = client.Account;
			if (!handlerDesc.IsGamePacket)
			{
				if (acc != null && acc.IsEnqueued)
				{
					// Enqueued clients may not do anything
					s_log.Warn("Enqueued client {0} sent: {1}", client, packet);
					return null;
				}

				// not a game packet, so process it on the global thread
				return RealmServer.IOQueue;
			}

		    if (chr == null || acc == null)
		    {
		        // game packet needs ActiveCharacter and Account set
		        s_log.Warn("Client {0} sent Packet {1} before login completed.", client, packet);
		        client.Disconnect();
		    }
		    else
		    {
		        if (chr.Map != null)
		        {
		            return chr;
		        }

		        s_log.Warn("Received packet {0} from Character {1} while not in world.", packet, chr);
		        client.Disconnect();
		    }

		    return null;
		}

		#endregion

		[Initialization(InitializationPass.Second, "Register packet handlers")]
		public static void RegisterPacketHandlers()
		{
			Instance.RegisterAll(Assembly.GetExecutingAssembly());

			s_log.Debug(Resources.RegisteredAllHandlers);
		}
	}
}