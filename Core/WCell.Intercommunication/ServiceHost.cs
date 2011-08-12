using System;
using System.ServiceModel;

namespace WCell.Intercommunication
{
    public sealed class ServiceHost<TInterface, TService> : ServiceHost
        where TInterface : class
        where TService : class
    {
        public ServiceHost(TService instance, Uri uri)
            : base(typeof(TService))
        {
			if(instance == null)
				throw new ArgumentNullException("instance");

			if (uri == null)
				throw new ArgumentNullException("uri");

            AddServiceEndpoint(typeof(TInterface), new NetTcpBinding(SecurityMode.None, true), uri);
        }

		public ServiceHost(Uri uri)
			: base(typeof(TService))
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			AddServiceEndpoint(typeof(TInterface), new NetTcpBinding(SecurityMode.None, true), uri);
		}

        public ServiceHost(TService instance, string uri)
            : this(instance, new Uri(uri))
        {
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException("uri");
        }

		public ServiceHost(string uri)
			: this(new Uri(uri))
		{

			if (string.IsNullOrWhiteSpace(uri))
				throw new ArgumentNullException("uri");
		}

        public TService Channel
        {
            get { return (TService)SingletonInstance; }
        }
    }
}