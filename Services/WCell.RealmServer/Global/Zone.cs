/*************************************************************************
 *
 *   file		: Zone.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-15 22:27:33 +0800 (Sun, 15 Jun 2008) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 500 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Login;
using WCell.Constants.World;
using WCell.Core.DBC;
using WCell.RealmServer.Entities;
using WCell.Util.Data;
using WCell.RealmServer.Chat;
using System.Collections.Generic;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Global
{
	public class Zone : IWorldSpace
	{
		private ChatChannel m_allianceGeneralChannel, m_allianceLocalDefenseChannel;
		private ChatChannel m_hordeGeneralChannel, m_hordeLocalDefenseChannel;

		public readonly ZoneInfo Info;
		public readonly Region Region;
		public readonly IList<ChatChannel> AllianceChatChannels = new List<ChatChannel>();
		public readonly IList<ChatChannel> HordeChatChannels = new List<ChatChannel>();

		public Zone(Region rgn, ZoneInfo info)
		{
			Region = rgn;
			Info = info;
			if (info.WorldStates != null)
			{
				WorldStates = new WorldStateCollection(this, info.WorldStates);
			}

			CreateChatChannels();
		}

		public WorldStateCollection WorldStates
		{
			get;
			private set;
		}

		public IWorldSpace ParentSpace
		{
			get { return ParentZone ?? (IWorldSpace)Region; }
		}

		public int ExplorationBit
		{
			get { return Info.ExplorationBit; }
		}

		public int AreaLevel
		{
			get { return Info.AreaLevel; }
		}

		/// <summary>
		/// The name of the zone.
		/// </summary>
		public string Name
		{
			get { return Info.Name; }
		}

		/// <summary>
		/// The ID of the zone.
		/// </summary>
		public ZoneId Id
		{
			get { return Info.Id; }
		}

		/// <summary>
		/// The ID of the zone's parent zone.
		/// </summary>
		public ZoneId ParentZoneId
		{
			get { return Info.ParentZoneId; }
		}

		public Zone ParentZone
		{
			get { return Region.GetZone(ParentZoneId); }
		}

		/// <summary>
		/// The ID of this zone's parent region.
		/// </summary>
		public MapId RegionId
		{
			get { return Info.RegionId; }
		}

		/// <summary>
		/// The <see cref="Global.RegionInfo">Region</see> to which this Zone belongs.
		/// </summary>
		public RegionInfo RegionInfo
		{
			get { return Info.RegionInfo; }
			set { Info.RegionInfo = value; }
		}

		/// <summary>
		/// The flags for the zone.
		/// </summary>
		public ZoneFlags Flags
		{
			get { return Info.Flags; }
		}

		/// <summary>
		/// Who does this Zone belong to (if anyone)
		/// </summary>
		public FactionGroupMask Ownership
		{
			get { return Info.Ownership; }
		}

		public void CallOnAllCharacters(Action<Character> action)
		{
			Region.CallOnAllCharacters(chr =>
			{
				if (chr.Zone.Id == Id)
				{
					action(chr);
				}
			});
		}

		internal void EnterZone(Character chr, Zone oldZone)
		{
			UpdateChannels(chr, oldZone);
			MiscHandler.SendInitWorldStates(chr, WorldStates, this);

			if (oldZone != null)
			{
				oldZone.LeaveZone(chr);
			}

			// update PvPState
			var isBg = Region.IsBattleground;
			if (RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.PVP | RealmServerType.RPPVP) || isBg)
			{
				if (isBg || Info.IsHostileTo(chr))
				{
					chr.PvPState = PvPState.PVP;
					chr.PlayerFlags |= PlayerFlags.PVP;
				}
				else if (Info.IsSanctuary)
				{
					chr.PvPState = PvPState.InPvPSanctuary;
					chr.PlayerFlags |= PlayerFlags.InPvPSanctuary;
				}
				else
				{
					chr.PvPState = PvPState.None;
					chr.PlayerFlags &= ~(PlayerFlags.PVP | PlayerFlags.InPvPSanctuary);
				}
			}

			Info.OnPlayerEntered(chr, oldZone);
		}

		internal void LeaveZone(Character chr)
		{
			Info.OnPlayerLeft(chr, this);
		}

		#region Channels
		public ChatChannel AllianceLocalDefenseChannel
		{
			get { return m_allianceLocalDefenseChannel; }
		}

		public ChatChannel AllianceGeneralChannel
		{
			get { return m_allianceGeneralChannel; }
		}

		public ChatChannel HordeLocalDefenseChannel
		{
			get { return m_hordeLocalDefenseChannel; }
		}

		public ChatChannel HordeGeneralChannel
		{
			get { return m_hordeGeneralChannel; }
		}

		public IList<ChatChannel> GetChatChannels(FactionGroup group)
		{
			return group == FactionGroup.Alliance ? AllianceChatChannels : HordeChatChannels;
		}

		void CreateChatChannels()
		{
			var alliance = ChatChannelGroup.Alliance;
			var horde = ChatChannelGroup.Horde;

            if (!Info.Flags.HasFlag(ZoneFlags.Arena))
			{
                if (!Info.Flags.HasFlag(ZoneFlags.AlwaysContested))
				{
					AllianceChatChannels.Add(m_allianceLocalDefenseChannel = alliance.CreateLocalDefenseChannel(Info));
					HordeChatChannels.Add(m_hordeLocalDefenseChannel = horde.CreateLocalDefenseChannel(Info));
				}

				AllianceChatChannels.Add(m_allianceGeneralChannel = alliance.CreateGeneralChannel(Info));
				HordeChatChannels.Add(m_hordeGeneralChannel = horde.CreateGeneralChannel(Info));
			}
		}


		/// <summary>
		/// Lets the player join/leave the appropriate chat-channels
		/// </summary>
		/// <param name="chr">the player</param>
		private void UpdateChannels(Character chr, Zone oldZone)
		{
			var newChannels = GetChatChannels(chr.FactionGroup);
			if (oldZone != null)
			{
				var oldChannels = oldZone.GetChatChannels(chr.FactionGroup);

				if (oldZone.Info.IsCity)
				{
					ChatChannelGroup.GetGroup(chr.FactionGroup).TradeChannel.Leave(chr, false);
				}

				foreach (var oldChnl in oldChannels)
				{
					if (!newChannels.Contains(oldChnl))
					{
						oldChnl.Leave(chr, false);
					}
				}

				foreach (var newChnl in newChannels)
				{
					if (!oldChannels.Contains(newChnl))
					{
						newChnl.TryJoin(chr);
					}
				}
			}
			else
			{
				foreach (var newChnl in newChannels)
				{
					newChnl.TryJoin(chr);
				}
			}

			if (Info.IsCity)
			{
				ChatChannelGroup.GetGroup(chr.FactionGroup).TradeChannel.TryJoin(chr);
			}
		}
		#endregion

		public override string ToString()
		{
			return Name + " (Id: " + (uint)Id + ")";
		}
	}


}