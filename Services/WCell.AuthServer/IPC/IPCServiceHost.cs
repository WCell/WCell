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

		private static ServiceHost host;

		public static bool IsOpen
		{
			get { return host != null && host.State == CommunicationState.Opened; }
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
					host = new ServiceHost(typeof(IPCServiceAdapter), uri);

					var endPoint = host.AddServiceEndpoint(
						typeof(IWCellIntercomService),
						new NetTcpBinding(SecurityMode.None),
						uri);

					host.Open();

					log.Info(resources.IPCServiceStarted, uri.AbsoluteUri);
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
				if (host != null && host.State != CommunicationState.Closed && host.State != CommunicationState.Faulted)
				{
					try
					{
						host.Close();
					}
					catch (Exception)
					{
						// do nada
					}
					log.Info(resources.IPCServiceShutdown);
				}

				host = null;
			}
		}
	}
}