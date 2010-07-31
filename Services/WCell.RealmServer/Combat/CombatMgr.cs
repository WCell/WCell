/*************************************************************************
 *
 *   file		: CombatMgr.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-03-31 16:58:40 +0200 (ma, 31 mar 2008) $
 *   last author	: $LastChangedBy: domiii $
 *   revision		: $Rev: 209 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.RealmServer.Entities;
using WCell.Core.Network;
using WCell.Core;
using WCell.RealmServer.World;
using NLog;

namespace WCell.RealmServer.Combat
{
	public static class CombatMgr
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		#region Packets: Combat State
		[PacketHandler(RealmServerOpCode.CMSG_ATTACKSWING)]
		public static void HandleAttackSwing(RealmClient client, RealmPacketIn packet)
		{
			if (client.ActiveCharacter.CanDoHarm) {
				EntityId targetId = packet.ReadEntityId();
				Unit target = WorldMgr.GetUnit(targetId);
				if (target != null && target.CanDoHarm && client.ActiveCharacter.IsHostileWith(target)) {
					client.ActiveCharacter.SetCombatState(true, false);
				}
			}
		}

		[PacketHandler(RealmServerOpCode.CMSG_SETSHEATHED)]
		public static void HandleSheathedStateChanged(RealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.SheathType = (SheathType)packet.ReadByte();
		}

		[PacketHandler(RealmServerOpCode.CMSG_ATTACKSTOP)]
		public static void HandleAttackStop(RealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.SetCombatState(false, false);
		}

		/// <summary>
		/// Make sure, we have a valid target before calling this method
		/// </summary>
		public static void SendCombatStart(Unit chr, Unit opponent, bool includeSelf)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSTART, 16)) {
				chr.EntityId.WritePacked(packet);
				opponent.EntityId.WritePacked(packet);

				chr.PushPacketToSurroundingArea(packet, includeSelf, false);
			}
		}

		/// <summary>
		/// Make sure, we have a valid target before calling this method
		/// </summary>
		public static void SendCombatStop(Unit chr, Unit opponent, bool includeSelf)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKSTOP, 20)) {
				chr.EntityId.WritePacked(packet);
				if (opponent != null)
					opponent.EntityId.WritePacked(packet);
				packet.Write(0);

				chr.PushPacketToSurroundingArea(packet, includeSelf, false);
			}
		}
		#endregion


		#region Packets: Damage / Heal
		/// <param name="swingFlag">usually 1</param>
		/// <returns>The actual damage (all resistances subtracted)</returns>
		public static uint SendMeleeDamage(WorldObject attacker, WorldObject target, DamageType type, HitInfo hitInfo,
										uint totalAmount, uint absorbed, uint resisted, uint blocked, VictimState victimState)
		{
			uint amount = totalAmount - blocked - absorbed - resisted;
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ATTACKERSTATEUPDATE, 70)) {
				packet.WriteUInt((uint)hitInfo);
				attacker.EntityId.WritePacked(packet);
				target.EntityId.WritePacked(packet);

				packet.WriteUInt(totalAmount);
				packet.WriteByte(1);

				packet.WriteByte((uint)type);
				packet.WriteFloat(amount);
				packet.WriteUInt(amount);
				packet.WriteUInt(absorbed);
				packet.WriteUInt(resisted);

				packet.WriteUInt((uint)victimState);
				packet.Write(absorbed == 0 ? 0 : -1);
				packet.WriteUInt(0);
				packet.WriteUInt(blocked);

				target.PushPacketToSurroundingArea(packet, true, false);
			}
			return amount;
		}

		/// <summary>
		/// Used for Periodic leech effects, mostly Cannibalism
		/// </summary>
		/// <param name="school">AuraTickFlags.PeriodicHeal</param>
		/// <returns></returns>
		public static void SendPeriodicDamage(WorldObject caster, WorldObject target, uint spellId, Spells.AuraTickFlags type,
										   uint amount)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_PERIODICAURALOG, 32)) {
				caster.EntityId.WritePacked(packet);
				target.EntityId.WritePacked(packet);
				packet.WriteUInt(spellId);

				packet.WriteUInt(1);				// count
				packet.WriteUInt((uint)type);
				packet.WriteUInt(amount);

				target.PushPacketToSurroundingArea(packet, true, false);
			}
		}

		/// <summary>
		/// Usually caused by jumping too high, diving too long, standing too close to fire etc
		/// </summary>
		public static void SendEnvironmentDamage(WorldObject target, EnviromentalDamageType type, uint totalDamage)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_ENVIRONMENTALDAMAGELOG, 21)) {
				target.EntityId.WritePacked(packet);

				packet.WriteByte((byte)type);
				packet.WriteUInt(totalDamage);
				packet.WriteULong(0);
				target.PushPacketToSurroundingArea(packet, true, false);
			}
		}

		/// <summary>
		/// Any spell and ranged damage
		/// </summary>
		/// <param name="totalAmount">Total amount</param>
		/// <returns>The actual amount, with all mali subtracted</returns>
		public static void SendMagicDamage(EntityId caster, WorldObject target, uint spellId, uint amount, DamageType type,
										uint absorbed, uint resisted, uint blocked, bool critical)
		{
			/*if (victimState == VICTIMSTATE.DEFLECT) // resist
				return MagicResist(target,submitter,spellId);*/
			// TODO: Magic miss, ranged dodge, etc.

			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLNONMELEEDAMAGELOG, 40)) {
				target.EntityId.WritePacked(packet);
				caster.WritePacked(packet);
				packet.Write(spellId);

				packet.WriteUInt(amount);
				packet.WriteByte((byte)type);
				packet.WriteUInt(absorbed);
				packet.WriteUInt(resisted);
				packet.WriteByte(type == DamageType.Physical ? 1 : 0); // pyshical damage?
				packet.WriteByte(0);
				packet.WriteUInt(blocked);
				packet.WriteByte(critical ? 7 : 6);
				packet.Write(0);

				target.PushPacketToSurroundingArea(packet, true, false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target">Optional</param>
		/// <param name="value">Optional</param>
		public static RealmPacketOut SendSpellLogExecute(ObjectBase caster, uint spellId, Spells.SpellEffectType effect, ObjectBase target, uint value)
		{
			RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLLOGEXECUTE, 37);
			caster.EntityId.WritePacked(packet);
			packet.WriteUInt(spellId);
			packet.Write(1);
			packet.Write((int)effect);

			if (target != null && caster != target) {
				target.EntityId.WritePacked(packet);
			}

			if (value > 0) {
				packet.WriteUInt(value);
			}
			return packet;
		}

		public static void SendHeal(WorldObject caster, Unit target, uint spellId, uint value, bool critical)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_HEALSPELL_ON_PLAYER_OBSOLETE, 25)) {
				target.EntityId.WritePacked(packet);
				caster.EntityId.WritePacked(packet);
				packet.Write(spellId);
				packet.Write(value);
				packet.WriteByte(critical ? 1 : 0);

			}
			//target.PushPacketToSurroundingArea(SendSpellLogExecute(caster, spellId, Spells.SpellEffectType.Heal, target, value), true, true);
		}

		/// <summary>
		/// Unused
		/// </summary>
		public static void SendMagicResist(ObjectBase caster, ObjectBase target, uint spellId)
		{
			using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESISTLOG, 21)) {
				caster.EntityId.WritePacked(packet);
				target.EntityId.WritePacked(packet);
				packet.WriteUInt(spellId);
				packet.WriteByte((byte)0);
			}
		}
		#endregion
	}
}