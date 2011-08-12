/*************************************************************************
 *
 *   file		: IPCDevice.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-02-07 02:16:59 +0800 (Sat, 07 Feb 2009) $

 *   revision		: $Rev: 737 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.ServiceModel;
using WCell.Util.Logging;
using WCell.Intercommunication.Client;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Res;
using WCell.Util.Variables;

namespace WCell.RealmServer.Network
{

	public sealed class IpcDevice<TService, TCallback> : IpcDeviceBase
		where TService : class
		where TCallback : class, new()
	{
		private DuplexServiceClient<TService, TCallback> _client;

		private readonly Func<DuplexServiceClient<TService, TCallback>> _creator;

		private bool _warned;
		private string _warnInfo;

		public bool IsRunning
		{
			get; private set;
		}

		public bool IsConnected
		{
			get { return _client != null && IsRunning && _client.State == CommunicationState.Opened; }
		}

		public IpcDevice(Func<DuplexServiceClient<TService, TCallback>> clientCreator)
		{
			if (clientCreator == null)
				throw new ArgumentNullException("clientCreator");

			_creator = clientCreator;
			_client = clientCreator();
			IsRunning = true;
			Log.Info(Resources.ConnectingToAuthServer);
			try
			{
				_client.Open();
				if(_client.State == CommunicationState.Opened)
					Log.Info(Resources.IPCProxySucceeded);
			}
			catch (Exception ex)
			{

				Log.Warn("Lost IPC connection. Scheduling reconnection attempt...");

				if (ex is CommunicationException)
				{
					RealmServer.IOQueue.AddMessage(Reconnect);
				}
				else
					LogUtil.WarnException(ex, Resources.CommunicationException);
			}
		}

		public void Call(Action<TService> action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			try
			{
				action(_client.ServiceChannel);
			}
			catch (Exception ex)
			{
				if (ex is CommunicationException)
				{
					if (!_warned)
						Log.Warn("Lost IPC connection. Scheduling reconnection attempt...");
					RealmServer.IOQueue.AddMessage(Reconnect);
				}
				else
					LogUtil.ErrorException(ex, Resources.CommunicationException);
			}
		}

		private void Connect()
		{
			if (!_warned)
			{
				AddDisconnectWarningToTitle();
				Log.Info(Resources.ConnectingToAuthServer);
			}

			try
			{
				_client = _creator();
				IsRunning = true;
				_client.Open();
				RearmDisconnectWarning();
				Log.Info(Resources.ConnectingToAuthServer);
			}
			catch (Exception e)
			{
				if (e is EndpointNotFoundException)
				{
					if (!_warned)
					{
						Log.Error(Resources.IPCProxyFailed, UpdateInterval);
					}
				}
				else if(!(e is TimeoutException))
				{
					LogUtil.ErrorException(e, Resources.IPCProxyFailedException, UpdateInterval);
				}
				_warned = true;
			}
		}

		private void Disconnect()
		{
			if (_client == null)
				return;

			var state = _client.State;
			if (state != CommunicationState.Closing && state != CommunicationState.Closed
				&& state != CommunicationState.Faulted)
				_client.Close();

			_client = null;
			if(state == CommunicationState.Opened)
				Log.Info(Resources.IPCProxyDisconnected);

			AddDisconnectWarningToTitle();
			IsRunning = false;
		}

		private void Reconnect()
		{
			CommunicationState state;
			if (_client != null)
			{
				state = _client.State;
				if (state == CommunicationState.Opening || state == CommunicationState.Opened)
					return;

				Disconnect();
			}

			Connect();

			if (_client == null) return;
			state = _client.State;
			if (state == CommunicationState.Opening || state == CommunicationState.Opened)
				Log.Info(Resources.IPCProxyReconnected);
		}

		private void AddDisconnectWarningToTitle()
		{
			_warnInfo = " - ######### " + RealmLocalizer.Instance.Translate(RealmLangKey.NotConnectedToAuthServer).ToUpper() +
						" #########";
			Console.Title += _warnInfo;
		}

		private void RearmDisconnectWarning()
		{
			_warned = false;
			if (_warnInfo != null)
			{
				Console.Title = Console.Title.Replace(_warnInfo, "");
				_warnInfo = null;
			}
		}

		public override void Dispose()
		{
			IsRunning = false;

			if (_client != null)
				Disconnect();
		}
	}

	public abstract class IpcDeviceBase : IDisposable
	{
		[Variable("IPCUpdateInterval")]
		public static int UpdateInterval = 5;
		protected static Logger Log = LogManager.GetCurrentClassLogger();
		public abstract void Dispose();
	}
}