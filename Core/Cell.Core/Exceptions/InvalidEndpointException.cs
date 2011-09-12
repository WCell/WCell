using System;
using System.Net;

namespace Cell.Core.Exceptions
{
    [Serializable]
	public class InvalidEndpointException : Exception
	{
		private IPEndPoint _endpoint;

		public InvalidEndpointException(IPEndPoint ep)
			: base()
		{
			_endpoint = ep;
		}

		public InvalidEndpointException(IPEndPoint ep, string message)
			: base(message)
		{ 
			_endpoint = ep;
		}

		public IPEndPoint Endpoint
		{
			get
			{
				return _endpoint;
			}
		}
	}
}