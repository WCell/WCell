using System;
using System.ServiceModel;

namespace WCell.Intercommunication.Client
{
	public class DuplexServiceClient<TService, TCallback> : DuplexClientBase<TService>
		where TService : class
		where TCallback : class, new()
	{
		public DuplexServiceClient(TCallback callback, string uri)
			: this(callback, new Uri(uri))
		{
			if (callback == null)
				throw new ArgumentNullException("callback");
			if (uri == null)
				throw new ArgumentNullException("uri");
		}

		public DuplexServiceClient(TCallback callback, Uri uri)
			: base(callback, new NetTcpBinding(SecurityMode.None, true), new EndpointAddress(uri))
		{
			if (callback == null)
				throw new ArgumentNullException("callback");
			if (uri == null)
				throw new ArgumentNullException("uri");

			CallbackChannel = callback;
		}

		public bool IsConnected
		{
			get { return State == CommunicationState.Opened; }
		}

		public TService ServiceChannel
		{
			get { return Channel; }
		}

		public TCallback CallbackChannel { get; private set; }
	}
}
