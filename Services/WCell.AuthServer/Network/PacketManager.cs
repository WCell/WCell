/*************************************************************************
 *
 *   file		: PacketManager.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-03-17 22:00:27 +0100 (ti, 17 mar 2009) $
 
 *   revision		: $Rev: 813 $
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
using WCell.Core.Initialization;
using WCell.Core.Network;
using WCell.Util;
using WCell.Util.NLog;
using resources = WCell.AuthServer.Res.WCell_AuthServer;

namespace WCell.AuthServer.Network
{
	/// <summary>
	/// Manages packet handlers and the execution of them.
	/// </summary>
	public class AuthPacketManager : PacketManager<IAuthClient, AuthPacketIn, ClientPacketHandlerAttribute>
	{
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

		static AuthPacketManager()
		{
			Instance = new AuthPacketManager();
		}

		public override uint MaxHandlers
		{
			get { return (uint)AuthServerOpCode.Maximum; }
		}

		public static readonly AuthPacketManager Instance;

		#region Handle Packets
		/// <summary>
		/// Attempts to handle an incoming packet. 
		/// Constraints:
		/// OpCode must be valid.
		/// GamePackets cannot be sent if ActiveCharacter == null.
		/// </summary>
		/// <param name="client">the client the packet is from</param>
		/// <param name="packet">the packet to be handled</param>
		/// <returns>true if the packet handler executed instantly; false if handling failed or was delayed</returns>
		public override bool HandlePacket(IAuthClient client, AuthPacketIn packet)
		{
			var dispose = true;

			try
			{
				if (!client.IsConnected)
				{
					return true;
				}
				
				var handlerDesc = m_handlers.Get(packet.PacketId.RawId);

				if (handlerDesc != null)
				{
					var pktMessage = new AuthPacketMessage(handlerDesc.Handler, client, packet);

					AuthenticationServer.IOQueue.AddMessage(pktMessage);
					dispose = false;
					return false;
				}
				else
				{
					HandleUnhandledPacket(client, packet);
					return true;
				}
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, resources.PacketHandleException, packet.PacketId);
				return false;
			}
			finally
			{
				if (dispose)
				{
					((IDisposable)packet).Dispose();
				}
			}
		}

		#endregion

		[Initialization(InitializationPass.Second, "Register packet handlers")]
		public static void RegisterPacketHandlers()
		{
			Instance.RegisterAll(Assembly.GetExecutingAssembly());

			s_log.Debug(resources.RegisteredAllHandlers);
		}
	}
}