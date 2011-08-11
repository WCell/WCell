/*************************************************************************
 *
 *   file		: ChannelMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-08-20 04:35:29 +0800 (Wed, 20 Aug 2008) $

 *   revision		: $Rev: 605 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants.Chat;
using WCell.Constants.Factions;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Chat
{
	public struct ChatChannelFlagsEntry
	{
		public ChatChannelFlags Flags;
		public ChatChannelFlagsClient ClientFlags;
	}

	///<summary>
	/// Manager for chat channels of one Faction.
	///</summary>
	public class ChatChannelGroup
	{
		public static readonly Dictionary<uint, ChatChannelFlagsEntry> DefaultChannelFlags =
			new Dictionary<uint, ChatChannelFlagsEntry>();

		public static readonly ChatChannelFlags GeneralFlags = ChatChannelFlags.ZoneSpecific | ChatChannelFlags.AutoJoin;
		public static readonly ChatChannelFlags TradeFlags = ChatChannelFlags.Trade | ChatChannelFlags.CityOnly;
		public static readonly ChatChannelFlags LFGFlags = ChatChannelFlags.LookingForGroup;
		public static readonly ChatChannelFlags LocalDefenseFlags = ChatChannelFlags.Defense | ChatChannelFlags.CityOnly | ChatChannelFlags.AutoJoin;

		/// <summary>
		/// Default static constructor.
		/// </summary>
		static ChatChannelGroup()
		{
			Alliance = new ChatChannelGroup(FactionGroup.Alliance);
			Horde = new ChatChannelGroup(FactionGroup.Horde);
			Global = new ChatChannelGroup(FactionGroup.Invalid);
		}

		/// <summary>
		/// Channel manager for Global channels.
		/// </summary>
		public static readonly ChatChannelGroup Global;

		/// <summary>
		/// Channel manager for the Alliance channels.
		/// </summary>
		public static readonly ChatChannelGroup Alliance;

		/// <summary>
		/// Channel manager for the Horde channels.
		/// </summary>
		public static readonly ChatChannelGroup Horde;


		/// <summary>
		/// Initializes all the default zone channels. (general, local defense, etc)
		/// </summary>
		[Initialization(InitializationPass.Fifth, "Create default channels")]
		public static void InitializeDefaultChannels()
		{
			World.InitializeWorld();
			// Establish trade channels since we want to pass the same one to 
			// all of our cities for each faction to make them appear "linked"

			// TODO: Missing World Defense

			Alliance.TradeChannel = new ChatChannel(Alliance, "Trade - City", TradeFlags, true);
			//Alliance.CreateZoneChannel("GuildRecruitment - City");
			Alliance.LFGChannel = new ChatChannel(Alliance, "LookingForGroup", LFGFlags, true);

			Horde.TradeChannel = new ChatChannel(Horde, "Trade - City", TradeFlags, true);
			//Horde.CreateZoneChannel("GuildRecruitment - City");
			Horde.LFGChannel = new ChatChannel(Horde, "LookingForGroup", LFGFlags, true);
		}

		/// <summary>
		/// All based Channels that exist per Zone, indexed by <see cref="ZoneId"/>
		/// </summary>
		public readonly List<ChatChannel>[] ZoneChannels;
		public readonly FactionGroup FactionGroup;

		private Dictionary<string, ChatChannel> m_Channels;
		private ChatChannel m_tradeChannel, m_lfgChannel;

		/// <summary>
		/// Default constructor
		/// </summary>
		public ChatChannelGroup(FactionGroup factionGroup)
		{
			FactionGroup = factionGroup;

			ZoneChannels = new List<ChatChannel>[(int)ZoneId.End];
			Channels = new Dictionary<string, ChatChannel>(StringComparer.InvariantCultureIgnoreCase);
		}

		public ChatChannel TradeChannel
		{
			get { return m_tradeChannel; }
			private set
			{
				m_tradeChannel = value;
				m_Channels.Add(m_tradeChannel.Name, m_tradeChannel);
			}
		}

		public ChatChannel LFGChannel
		{
			get { return m_lfgChannel; }
			private set
			{
				m_lfgChannel = value;
				m_Channels.Add(m_lfgChannel.Name, m_lfgChannel);
			}
		}

		/// <summary>
		/// The channels for this manager;
		/// </summary>
		public Dictionary<string, ChatChannel> Channels
		{
			get { return m_Channels; }
			set { m_Channels = value; }
		}

		/// <summary>
		/// Creates a zone channel, which is a constant, non-moderated channel specific to one or more Zones.
		/// TODO: Fix: Channels exist per Zone instance
		/// </summary>
		internal ChatChannel CreateGeneralChannel(ZoneTemplate zone)
		{
			var name = string.Format("General - {0}", zone.Name);

			ChatChannel channel;
			if (!m_Channels.TryGetValue(name, out channel))
			{
				channel = new ChatChannel(this, name, GeneralFlags, true, null)
				{
					Announces = false
				};
				m_Channels.Add(channel.Name, channel);
			}
			return channel;
		}

		/// <summary>
		/// Creates a zone channel, which is a constant, non-moderated channel specific to one or more Zones.
		/// </summary>
		internal ChatChannel CreateLocalDefenseChannel(ZoneTemplate zone)
		{
			var name = string.Format("LocalDefense - {0}", zone.Name);

			ChatChannel channel;
			if (!m_Channels.TryGetValue(name, out channel))
			{
				channel = new ChatChannel(this, name, LocalDefenseFlags, true, null)
				{
					Announces = false
				};
				m_Channels.Add(channel.Name, channel);
			}
			return channel;
		}

		/// <summary>
		/// Deletes a channel.
		/// </summary>
		/// <param name="chnl">the channel to delete</param>
		public void DeleteChannel(ChatChannel chnl)
		{
			// TODO: Also delete zone-only channels from their array
			m_Channels.Remove(chnl.Name);
		}

		/// <summary>
		/// Attempts to retrieve a specific channel.
		/// </summary>
		/// <param name="name">the name of the channel</param>
		/// <param name="create">whether or not to create the channel if it doesn't exist</param>
		/// <returns>the channel instance</returns>
		public ChatChannel GetChannel(string name, bool create)
		{
		    if (Channels.ContainsKey(name))
			{
				return Channels[name];
			}
		    if (create)
		    {
		        var chnl = new ChatChannel(this, name);
		        m_Channels.Add(name, chnl);

		        return chnl;
		    }

		    return null;
		}

	    public ChatChannel GetChannel(string name, uint channelId, bool create)
		{
			ChatChannel chn;
			if (!m_Channels.TryGetValue(name, out chn))
			{
				if (create)
				{
					chn = new ChatChannel(this, channelId, name);
					m_Channels.Add(name, chn);
				}
			}
			return chn;
		}

		public bool CanJoin(IUser user)
		{
			return FactionGroup == FactionGroup.Invalid || user.FactionGroup == FactionGroup || user.Role.IsStaff;
		}

		/// <summary>
		/// Gets the appropriate <see cref="ChatChannelGroup"/> for the given <see cref="FactionGroup"/>.
		/// </summary>
		/// <param name="faction">the faction</param>
		/// <returns>the appropriate channel manager, or null if an invalid faction is given</returns>
		public static ChatChannelGroup GetGroup(FactionGroup faction)
		{
		    if (faction == FactionGroup.Alliance)
			{
				return Alliance;
			}
		    if (faction == FactionGroup.Horde)
		    {
		        return Horde;
		    }

		    throw new Exception("Invalid FactionGroup: " + faction);
		}

	    /// <summary>
		/// Tries to retrieve a channel for the given character.
		/// </summary>
		/// <param name="channelName">the channel name</param>
		/// <returns>the requested channel; null if there was an error or it doesn't exist</returns>
		public static ChatChannel RetrieveChannel(IUser user, string channelName)
		{
			if (channelName == string.Empty)
				return null;

			var chan = Global.GetChannel(channelName, false);
			// TODO: Improve lookup
			if (chan == null)
			{
				var group = GetGroup(user.FactionGroup);
				if (group == null)
				{
					return null;
				}
			    chan = group.GetChannel(channelName, false);
			}
			return chan;
		}
	}
}