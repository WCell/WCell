using System;
using System.ServiceModel;

namespace WCell.Intercommunication.Client
{
    public class ServiceClient<TService> : ClientBase<TService>
        where TService : class
    {
        public ServiceClient(string uri)
            : this(new Uri(uri))
        {
            if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException("uri");
        }

        public ServiceClient(Uri uri)
            : base(new NetTcpBinding(SecurityMode.None, true), new EndpointAddress(uri))
        {
            if (uri == null)
				throw new ArgumentNullException("uri");
        }

        public TService ServiceChannel
        {
            get { return Channel; }
        }

    	public bool IsConnected
    	{
    		get { return State == CommunicationState.Opened; }
    	}
    }
}