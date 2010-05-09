using System;
using WCell.Util.Threading;
using WCell.Util.NLog;

namespace WCell.AuthServer.Network
{
	public struct AuthPacketMessage : IMessage
	{
		private Action<IAuthClient, AuthPacketIn> m_handler;
		private IAuthClient m_client;
		private AuthPacketIn m_packet;

		public AuthPacketMessage(Action<IAuthClient, AuthPacketIn> handler, IAuthClient client, AuthPacketIn packet)
		{
			m_handler = handler;
			m_client = client;
			m_packet = packet;
		}

		public void Execute()
		{
			try
			{
				m_handler(m_client, m_packet);
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Packet failed to process! Packet ID: {0}", m_packet.ToString());
			}
			finally
			{
				((IDisposable)m_packet).Dispose();
			}
		}
	}
}