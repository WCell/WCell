/*************************************************************************
 *
 *   file		: ServiceHost.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 
 *   revision		: $Rev: 458 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Description;
using WCell.Util.Logging;
using resources = WCell.AuthServer.Res.WCell_AuthServer;
using WCell.Core;
using WCell.Constants;
using WCell.Core.Initialization;
using WCell.Intercommunication;
using WCell.AuthServer;

namespace WCell.AuthServer.IPC
{
	/// <summary>
	/// Defines a host with the authentication service.
	/// </summary>
	public static class IPCServiceHost
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		internal static ServiceHost<IWCellIntercomService, IPCServiceAdapter> _host;

		public static bool IsOpen
		{
			get { return _host != null && _host.State == CommunicationState.Opened; }
		}

		/// <summary>
		/// Starts the authentication service
		/// </summary>
		public static void StartService()
		{
			if (!IsOpen)
			{
				StopService();		// make sure, there is no half-open connection pending

				lock (typeof(IPCServiceHost))
				{
					var uri = new Uri(AuthServerConfiguration.IPCAddress);

					//var adapter = new IPCServiceAdapter();
					//_host = new ServiceHost<IWCellIntercomService, IPCServiceAdapter>(adapter, uri);
					_host = new ServiceHost<IWCellIntercomService, IPCServiceAdapter>(uri);
					_host.Open();
					//adapter.HookIpcChannelEvents();
					log.Info(resources.IPCServiceStarted, _host.Description.Endpoints[0].ListenUri.AbsoluteUri);
				}
			}
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		public static void StopService()
		{
			lock (typeof(IPCServiceHost))
			{
				if (_host != null && _host.State != CommunicationState.Closed && _host.State != CommunicationState.Faulted)
				{
					try
					{
						_host.Close();
					}
					catch (Exception e)
					{
						// do nada
					}
					log.Info(resources.IPCServiceShutdown);
				}

				_host = null;
			}
		}
	}
}