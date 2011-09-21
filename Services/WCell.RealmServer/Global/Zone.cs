/*************************************************************************
 *
 *   file		: Zone.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-15 22:27:33 +0800 (Sun, 15 Jun 2008) $

 *   revision		: $Rev: 500 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Login;
using WCell.Constants.World;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;

namespace WCell.RealmServer.Global
{
	public class Zone : IWorldSpace
	{
		private ChatChannel m_allianceGeneralChannel, m_allianceLocalDefenseChannel;
		private ChatChannel m_hordeGeneralChannel, m_hordeLocalDefenseChannel;

		public readonly ZoneTemplate Template;
		public readonly Map Map;
		public readonly IList<ChatChannel> AllianceChatChannels = new List<ChatChannel>();
		public readonly IList<ChatChannel> HordeChatChannels = new List<ChatChannel>();

		public Zone(Map rgn, ZoneTemplate template)
		{
			Map = rgn;
			Template = template;
			if (template.WorldStates != null)
			{
				WorldStates = new WorldStateCollection(this, template.WorldStates);
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
			get { return ParentZone ?? (IWorldSpace)Map; }
		}

		public int ExplorationBit
		{
			get { return Template.ExplorationBit; }
		}

		public int AreaLevel
		{
			get { return Template.AreaLevel; }
		}

		/// <summary>
		/// The name of the zone.
		/// </summary>
		public string Name
		{
			get { return Template.Name; }
		}

		/// <summary>
		/// The ID of the zone.
		/// </summary>
		public ZoneId Id
		{
			get { return Template.Id; }
		}

		/// <summary>
		/// The ID of the zone's parent zone.
		/// </summary>
		public ZoneId ParentZoneId
		{
			get { return Template.ParentZoneId; }
		}

		public Zone ParentZone
		{
			get { return Map.GetZone(ParentZoneId); }
		}

		/// <summary>
		/// The ID of this zone's parent map.
		/// </summary>
		public MapId MapId
		{
			get { return Template.MapId; }
		}

		/// <summary>
		/// The <see cref="MapTemplate">Map</see> to which this Zone belongs.
		/// </summary>
		public MapTemplate MapTemplate
		{
			get { return Template.MapTemplate; }
			set { Template.MapTemplate = value; }
		}

		/// <summary>
		/// The flags for the zone.
		/// </summary>
		public ZoneFlags Flags
		{
			get { return Template.Flags; }
		}

		/// <summary>
		/// Who does this Zone belong to (if anyone)
		/// </summary>
		public FactionGroupMask Ownership
		{
			get { return Template.Ownership; }
		}

		public void CallOnAllCharacters(Action<Character> action)
		{
			Map.CallOnAllCharacters(chr =>
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
			WorldStateHandler.SendInitWorldStates(chr, WorldStates, this);

			if (oldZone != null)
			{
				oldZone.LeaveZone(chr);
			}

			// update PvPState
			var isBg = Map.IsBattleground;
			if (RealmServerConfiguration.ServerType.HasAnyFlag(RealmServerType.PVP | RealmServerType.RPPVP) || isBg)
			{
				if (isBg || Template.IsHostileTo(chr))
				{
				    chr.UpdatePvPState(true, true);
					chr.PlayerFlags |= PlayerFlags.PVP;
                    chr.PlayerFlags &= ~(PlayerFlags.InPvPSanctuary | PlayerFlags.FreeForAllPVP);
				}
				else if (Template.IsSanctuary)
				{
					chr.PvPState = PvPState.InPvPSanctuary;
					chr.PlayerFlags |= PlayerFlags.InPvPSanctuary;
                    chr.PlayerFlags &= ~(PlayerFlags.PVP | PlayerFlags.FreeForAllPVP);
				}
				else
				{
                    chr.UpdatePvPState(false);
                    chr.PlayerFlags &= ~(PlayerFlags.PVP | PlayerFlags.InPvPSanctuary | PlayerFlags.FreeForAllPVP);
				}

                if (Template.IsArena)
                {
                    chr.PvPState = PvPState.FFAPVP;
                    chr.PlayerFlags |= PlayerFlags.FreeForAllPVP;
                    chr.PlayerFlags &= ~(PlayerFlags.InPvPSanctuary);
                }
			}

			Template.OnPlayerEntered(chr, oldZone);
		}

		internal void LeaveZone(Character chr)
		{
			Template.OnPlayerLeft(chr, this);
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

            if (!Template.Flags.HasFlag(ZoneFlags.Arena))
			{
                if (!Template.Flags.HasFlag(ZoneFlags.AlwaysContested))
				{
					AllianceChatChannels.Add(m_allianceLocalDefenseChannel = alliance.CreateLocalDefenseChannel(Template));
					HordeChatChannels.Add(m_hordeLocalDefenseChannel = horde.CreateLocalDefenseChannel(Template));
				}

				AllianceChatChannels.Add(m_allianceGeneralChannel = alliance.CreateGeneralChannel(Template));
				HordeChatChannels.Add(m_hordeGeneralChannel = horde.CreateGeneralChannel(Template));
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

				if (oldZone.Template.IsCity)
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

			if (Template.IsCity)
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