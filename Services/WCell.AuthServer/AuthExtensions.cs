using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace WCell.AuthServer
{
	public static class AuthExtensions
	{
		public static bool IsIPV4(this IPAddress addr)
		{
			return addr.AddressFamily == AddressFamily.InterNetwork;
		}

		public static bool IsIPV6(this IPAddress addr)
		{
			return addr.AddressFamily == AddressFamily.InterNetworkV6;
		}

		public static int GetLength(this IPAddress addr)
		{
			if (addr.IsIPV4())
			{
				return 4;
			}
			if (addr.IsIPV6())
			{
				return 6;
			}
			throw new InvalidDataException("IPAddress is not of InterNetwork type: " + addr);
		}
	}
}
