using System.Net;

namespace Cell.Core
{
	public static class NetworkUtil
	{
		private static IPHostEntry hostEntry;

		public static IPHostEntry CachedHostEntry
		{
			get
			{
				if (hostEntry == null)
				{
					hostEntry = Dns.GetHostEntry(Dns.GetHostName());
				}
				return hostEntry;
			}
		}

		public static IPAddress GetMatchingLocalIP(IPAddress clientAddr)
		{
			if (clientAddr.Equals(IPAddress.Any) ||
				clientAddr.Equals(IPAddress.Loopback))
			{
				return IPAddress.Loopback;
			}

			var clientBytes = clientAddr.GetAddressBytes();

			// find an address that matches the most significant n-1 bytes of the address of the connecting client
			foreach (var addr in CachedHostEntry.AddressList)
			{
				var bytes = addr.GetAddressBytes();
				if (bytes.Length != clientBytes.Length) continue;

				var match = true;
				for (var i = bytes.Length-2; i >= 0; i--)
				{
					if (bytes[i] != clientBytes[i])
					{
						match = false;
						break;
					}
				}
				if (match) return addr;
			}
			return null;
		}
	}
}