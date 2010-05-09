/*************************************************************************
 *
 *   file		: AuthQueue.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-31 18:15:44 +0100 (to, 31 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1162 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using Cell.Core.Collections;
using WCell.Constants;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Stats;
using WCell.Util.NLog;

namespace WCell.RealmServer.Network
{
	/// <summary>
	/// Manages the queue of overflowed clients connecting to the server when it is full.
	/// </summary>
	public static class AuthQueue
	{
		private static Timer s_checkTimer;
		private static LockfreeQueue<IRealmClient> s_queuedClients;

		static AuthQueue()
		{
			s_queuedClients = new LockfreeQueue<IRealmClient>();
			s_checkTimer = new Timer(ProcessQueuedClients);

			s_checkTimer.Change(TimeSpan.FromSeconds(15.0), TimeSpan.FromSeconds(15.0));
		}

		/// <summary>
		/// The number of clients currently waiting in the queue.
		/// </summary>
		public static int QueuedClients
		{
			get
			{
				return s_queuedClients.Count;
			}
		}

		/// <summary>
		/// Adds a client to the queue.
		/// </summary>
		/// <param name="client">the client to add to the queue</param>
		public static void EnqueueClient(IRealmClient client)
		{
			client.Account.IsEnqueued = true;
			s_queuedClients.Enqueue(client);

			LoginHandler.SendAuthQueueStatus(client);
		}

		/// <summary>
		/// Goes through the queue, pulling out clients for the number of slots available at the time.
		/// </summary>
		/// <param name="state">the timer object</param>
		private static void ProcessQueuedClients(object state)
		{
			var acceptedClients = new List<IRealmClient>();

			try
			{
				var clientAccepts = RealmServerConfiguration.MaxClientCount - RealmServer.Instance.AcceptedClients;

				IRealmClient client;

				while (clientAccepts != 0)
				{
					if (s_queuedClients.TryDequeue(out client))
					{
						acceptedClients.Add(client);
						clientAccepts--;
					}
					else
					{
						break;
					}
				}

				int clientPosition = 0;

				using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_AUTH_RESPONSE))
				{
					packet.Write((byte)LoginErrorCode.AUTH_WAIT_QUEUE);
					packet.Write(0);

					foreach (var waitingClient in s_queuedClients)
					{
						packet.InsertIntAt(clientPosition++, 5, false);
						waitingClient.Send(packet);
					}
				}
			}
			catch (Exception e)
			{
				LogUtil.ErrorException(e, "AuthQueue raised an Exception.");
			}
			finally
			{
				PerformanceCounters.NumbersOfClientsInAuthQueue.RawValue = s_queuedClients.Count;

				foreach (var acceptedClient in acceptedClients)
				{
					acceptedClient.Account.IsEnqueued = false;
					LoginHandler.InviteToRealm(acceptedClient);
				}
			}
		}
	}
}