/*************************************************************************
 *
 *   file		: TalentHandler.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2009-12-27 15:48:42 +0100 (sø, 27 dec 2009) $
 *   last author	: $LastChangedBy: dominikseifert $
 *   revision		: $Rev: 1159 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using WCell.Constants;
using WCell.Constants.Talents;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.RealmServer.Talents;

namespace WCell.RealmServer.Handlers
{
	public static class TalentHandler
	{
		[ClientPacketHandler(RealmServerOpCode.CMSG_LEARN_TALENT)]
		public static void HandleLearnTalent(IRealmClient client, RealmPacketIn packet)
		{
			var talentId = (TalentId)packet.ReadUInt32();
			var rank = packet.ReadInt32();	// zero-based Rank-mask

			var talents = client.ActiveCharacter.Talents;

			if (talents.Learn(talentId, rank) == null) return;
			
			// send new talent group list
			SendTalentGroupList(talents);
		}

		[ClientPacketHandler(RealmServerOpCode.MSG_TALENT_WIPE_CONFIRM)]
		public static void HandleClearTalents(IRealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.Talents.ResetTalents();
		}

        [ClientPacketHandler(RealmServerOpCode.CMSG_LEARN_PREVIEW_TALENTS)]
		public static void HandleSaveTalentGroup(IRealmClient client, RealmPacketIn packet)
		{
			var count = packet.ReadInt32();

			var list = new List<SimpleTalentDescriptor>(count);
			for (var i = 0; i < count; i++)
			{
				list.Add(new SimpleTalentDescriptor()
				{
					TalentId = (TalentId)packet.ReadUInt32(),
					Rank = packet.ReadInt32()
				});
			}

			var chr = client.ActiveCharacter.CurrentSpecProfile;
			// TODO: Set Talent Group
			//chr.SpecProfile.LearnTalentGroupTalents(list);
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_REMOVE_GLYPH)]
		public static void HandleRemoveGlyph(IRealmClient client, RealmPacketIn packet)
		{
			var slot = packet.ReadUInt32();
			var chr = client.ActiveCharacter;
			chr.RemoveGlyph((byte)slot);
			SendTalentGroupList(chr.Talents);
		}
		/// <summary>
		/// Sends a request to wipe all talents, which must be confirmed by the player
		/// </summary>
		public static void SendClearQuery(TalentCollection talents)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_TALENT_WIPE_CONFIRM, 12))
			{
				packet.Write(talents.Owner.EntityId);
				packet.Write(talents.GetResetPrice());

				talents.OwnerCharacter.Send(packet);
			}
		}

		public static void SendTalentGroupList(TalentCollection talents)
		{
			SendTalentGroupList(talents, talents.CurrentSpecIndex);
		}

		/// <summary>
		/// Sends the client the list of talents
		/// </summary>
		/// <param name="hasTalents">The IHasTalents to send the list from</param>
		public static void SendTalentGroupList(TalentCollection talents, int talentGroupId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TALENTS_INFO))
			{
				var owner = talents.Owner;
				var isPlayer = owner is Character;

				packet.Write((byte)(isPlayer ? 0 : 1));
				if (isPlayer)
				{
					WritePlayerTalentList(packet, (Character)owner, talentGroupId);
				}
				else
				{
					packet.Write(talents.FreeTalentPoints);
					packet.Write((byte)talents.Count);
					foreach (var talent in talents)
					{
						packet.Write((int)talent.Entry.Id);
						packet.Write((byte)talent.Rank);
					}
				}
				talents.OwnerCharacter.Send(packet);
			}
		}

		public static void SendInspectTalents(Character chr)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INSPECT_TALENT))
			{
				chr.EntityId.WritePacked(packet);
				WritePlayerTalentList(packet, chr, chr.Talents.CurrentSpecIndex);

				chr.Client.Send(packet);
			}
		}

		private static void WritePlayerTalentList(BinaryWriter packet, Character chr, int talentGroupId)
		{
			var spec = chr.CurrentSpecProfile;
			var talentGroupCount = (byte)chr.Talents.SpecProfileCount;

			packet.Write(chr.FreeTalentPoints);
			packet.Write(talentGroupCount);
			packet.Write((byte)talentGroupId);

			if (talentGroupCount <= 0) return;

			var talentList = chr.Talents.ById;
			var glyphs = spec.GlyphIds;

			for (var i = 0; i < talentGroupCount; i++)
			{
				packet.Write((byte)talentList.Count);
				foreach (var pair in talentList)
				{
					packet.Write((int)pair.Key);
					packet.Write((byte)pair.Value.Rank);
				}

				if (glyphs != null)
				{
					// TODO: Glyphs
					//packet.Write((byte) 0);
					packet.Write((byte) 6);//max_glyphs per spec
					for (int k = 0; k < 6; k++)
					{
						packet.Write((short)glyphs[k]);
					}
					//packet.Write((byte)glyphList.Length);
					//foreach (var record in glyphList)
					//{
					//    packet.Write(record.GlyphPropertiesId);
					//}
				}
				else
				{
					packet.Write((byte)0);
				}
			}
		}
	}
}