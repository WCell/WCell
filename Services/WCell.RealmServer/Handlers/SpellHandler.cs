using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Core;
using WCell.Constants.Items;
using WCell.Constants.Spells;
using WCell.Core.Network;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Network;
using WCell.Util;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells
{
	public static partial class SpellHandler
	{
		#region OUT Packets
		/// <summary>
		/// Sends initially all spells and item cooldowns to the character
		/// </summary>
		public static void SendSpellsAndCooldowns(Character chr)
		{
			var spells = chr.PlayerSpells;

			var len = 5 + (4 * spells.Count); // +(14 * cooldowns);
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_INITIAL_SPELLS, len))
			{
				packet.Write((byte)0);
				packet.Write((ushort)spells.Count);

				foreach (var spell in spells.SpellsById.Values)
				{
					packet.Write(spell.Id);
					//packet.Write((ushort)0xEEEE);
					packet.Write((ushort)0);
				}

				var countPos = packet.Position;
				ushort cooldownCount = 0;
				packet.Position = countPos + 2;

				// cooldowns
				var now = DateTime.Now.Ticks;
				if (spells.IdCooldowns != null)
				{
					foreach (var idCd in spells.IdCooldowns.Values)
					{
						var delay = idCd.Until.Ticks - now;
						if (delay > 10)
						{
							cooldownCount++;
							packet.Write(idCd.SpellId);
							packet.Write((ushort)idCd.ItemId);
							packet.Write((ushort)0);
							packet.Write(delay / 10000);
							packet.Write(0);
						}
					}
				}

				if (spells.CategoryCooldowns != null)
				{
					foreach (var catCd in spells.CategoryCooldowns.Values)
					{
						var delay = catCd.Until.Ticks - now;
						if (delay > 10)
						{
							cooldownCount++;
							packet.Write(catCd.SpellId);
							packet.Write((ushort)catCd.ItemId);
							packet.Write((ushort)catCd.CategoryId);
							packet.Write(0);
							packet.Write(delay / 10000);
						}
					}
				}

				packet.Position = countPos;
				packet.Write(cooldownCount);

				chr.Client.Send(packet);
			}
		}

		public static void SendLearnedSpell(IPacketReceiver client, uint spellId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LEARNED_SPELL, 4))
			{
				packet.WriteUInt(spellId);
			    packet.WriteUShort(0); // 3.3.3a

				client.Send(packet);
			}
		}

		public static void SendSpellSuperceded(IPacketReceiver client, uint spellId, uint newSpellId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SUPERCEDED_SPELL, 8))
			{
				packet.Write(spellId);
				packet.Write(newSpellId);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Removes a spell from the client's spellbook
		/// </summary>
		public static void SendSpellRemoved(Character chr, uint spellId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_REMOVED_SPELL, 4))
			{
				packet.WriteUInt(spellId);

				chr.Client.Send(packet);
			}
		}

		public static SpellTargetFlags GetTargetFlags(WorldObject obj)
		{
			if (obj is Unit)
			{
				return SpellTargetFlags.Unit;
			}
			if (obj is GameObject)
			{
				return SpellTargetFlags.Object;
			}
			if (obj is Corpse)
			{
				return SpellTargetFlags.PvPCorpse;
			}
			return SpellTargetFlags.Self;
		}

        public static void SendUnitCastStart(IRealmClient client, SpellCast cast, WorldObject target)
		{
            // TODO: research and implement this.
            // maybe sent for all targets in SpellCast object?

            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_UNIT_SPELLCAST_START, 28))
            {
                cast.CasterReference.EntityId.WritePacked(packet);   // caster pguid
                target.EntityId.WritePacked(packet);        // target pguid
                packet.Write(cast.Spell.Id);                // spell id
                packet.Write(cast.Spell.CastDelay);         // cast time?
                packet.Write(cast.Spell.CastDelay);         // cast time mod?

                client.Send(packet);
            }
        }

		public static void SendCastStart(SpellCast cast)
		{
			if (cast.CasterObject != null && !cast.CasterObject.IsAreaActive) return;

			int len = 150;

			var spell = cast.Spell;

			if (spell == null) return;

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_START, len))
			{
				// Common to start and go packets
				WriteCaster(cast, packet);
				packet.Write(cast.Id);
				packet.Write(spell.Id);
				packet.Write((int)cast.StartFlags);

				// start specific stuff

				packet.Write(cast.CastDelay);
				WriteTargets(packet, cast);

				if (cast.StartFlags.HasFlag(CastFlags.Flag_0x800))
				{
					packet.Write(0);
				}

				if (cast.StartFlags.HasFlag(CastFlags.Flag_0x200000))
				{
					byte b1 = 0;
					byte b2 = 0;
					packet.Write(b1);
					packet.Write(b2);
					for (int i = 0; i < 6; i++)
					{
						byte mask = (byte)(1 << i);
						if ((mask & b1) != 0)
						{
							if ((mask & b2) == 0)
							{
								packet.WriteByte(0);
							}
						}
					}
				}

				if (cast.StartFlags.HasFlag(CastFlags.Ranged))
				{
					WriteAmmoInfo(cast, packet);
				}

				if (cast.StartFlags.HasFlag(CastFlags.Flag_0x4000000))
				{
					// since 3.2.0
					packet.Write(0);
					packet.Write(0);
				}

                if (cast.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
                {
                    packet.Write((byte)0); // unk 3.3.x?
                }

				cast.SendPacketToArea(packet);
			}
		}

		static void WriteAmmoInfo(SpellCast cast, RealmPacketOut packet)
		{
			var caster = cast.CasterObject;
			if (caster is Character)
			{
				var weapon = ((Unit)caster).RangedWeapon as Item;
				if (weapon != null)
				{
					var inv = ((Character)caster).Inventory;
					var templ = weapon.Template;
					if (!templ.IsThrowable)
					{
						var ammo = inv.Ammo;
						if (ammo != null)
						{
							templ = ammo.Template;
						}
					}
					packet.Write(templ.DisplayId);
					packet.Write((uint)templ.InventorySlotType);
					return;
				}
			}

			// resort to default			
			packet.Write(5996);
			packet.Write((uint)InventorySlotType.Ammo);
		}

		private static void WriteTargets(RealmPacketOut packet, SpellCast cast)
		{
			var flags = cast.TargetFlags;
			if (flags == 0 || flags == SpellTargetFlags.Self)
			{
				var spell = cast.Spell;
				if (cast.Selected is Unit && !spell.IsAreaSpell &&
					(spell.Visual != 0 || spell.IsPhysicalAbility))
				{
					flags = SpellTargetFlags.Unit;
				}
			}
			packet.Write((uint)flags);
			if (flags.HasAnyFlag(
				SpellTargetFlags.SpellTargetFlag_Dynamic_0x10000 |
				SpellTargetFlags.Corpse |
				SpellTargetFlags.Object |
				SpellTargetFlags.PvPCorpse |
				SpellTargetFlags.Unit))
			{
				if (cast.Selected == null)
				{
#if DEBUG
					log.Warn("{0} casted Spell {1} with TargetFlags {2} but with nothing Selected",
						cast.CasterObject, cast.Spell, flags);
#endif
					packet.Write((byte)0);
				}
				else
				{
					cast.Selected.EntityId.WritePacked(packet);
				}
			}
			// 0x1010
            if (flags.HasAnyFlag(SpellTargetFlags.TradeItem | SpellTargetFlags.Item))
			{
				if (cast.UsedItem != null)
				{
					cast.UsedItem.EntityId.WritePacked(packet);
				}
			}
			// 0x20
			if (flags.HasAnyFlag(SpellTargetFlags.SourceLocation))
			{
				if (cast.Selected != null)
				{
					cast.Selected.EntityId.WritePacked(packet);
				}
				else
				{
					packet.Write((byte)0);
				}
				packet.Write(cast.SourceLoc.X);
				packet.Write(cast.SourceLoc.Y);
				packet.Write(cast.SourceLoc.Z);
			}
			// 0x40
			if (flags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
			{
				if (cast.Selected != null)
				{
					cast.Selected.EntityId.WritePacked(packet);
				}
				else
				{
					packet.Write((byte)0);
				}
				packet.Write(cast.TargetLoc);
			}
			// 0x2000
			if (flags.HasAnyFlag(SpellTargetFlags.String))
			{
				packet.WriteCString(cast.StringTarget);
			}
		}

		/// <summary>
		/// Sent to hit targets before CastGo
		/// </summary>
		public static void SendCastSuccess(ObjectBase caster, uint spellId, Character target)
		{
			IRealmClient client = target.Client;

			if (client == null) return;

			/*using (RealmPacketOut packet = new RealmPacketOut(RealmServerOpCode.SMSG_CLEAR_EXTRA_AURA_INFO, 12))
			{
				caster.EntityId.WritePacked(packet);
				packet.WriteUInt(spellId);

				client.Send(packet);
			}*/
		}

		/// <summary>
		/// Sent after spell start. Triggers the casting animation
		/// </summary>
		public static void SendSpellGo(IEntity caster2, SpellCast cast,
			ICollection<WorldObject> hitTargets, ICollection<CastMiss> missedTargets)
		{
			if (cast.CasterObject != null && !cast.CasterObject.IsAreaActive) return;

			// TODO: Dynamic packet length?
			if (!cast.IsCasting)
			{
				return;
			}

			//int len = 200;
			int len = 24 + (hitTargets != null ? hitTargets.Count * 8 : 0) + (missedTargets != null ? missedTargets.Count * 10 : 0);

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_GO, len))
			{
				//caster1.EntityId.WritePacked(packet);
				cast.CasterReference.EntityId.WritePacked(packet);
				caster2.EntityId.WritePacked(packet);
				packet.Write(cast.Id);
				packet.Write(cast.Spell.Id);

				var castGoFlags = cast.GoFlags;
				packet.Write((int)castGoFlags);

				//packet.Write(Util.Utility.GetEpochTime());
				packet.Write(Utility.GetEpochTime());
				//packet.Write(cast.CastDelay);

				packet.WriteByte(hitTargets != null ? hitTargets.Count : 0);

				if (hitTargets != null && cast.CasterObject != null)
				{
					foreach (var target in hitTargets)
					{
						packet.Write(target.EntityId);

						if (target is Character)
						{
							SendCastSuccess(cast.CasterObject, cast.Spell.Id, target as Character);
						}
					}
				}

				packet.WriteByte(missedTargets != null ? missedTargets.Count : 0);

				if (missedTargets != null)
				{
					foreach (var miss in missedTargets)
					{
						packet.Write(miss.Target.EntityId);
						packet.Write((byte)miss.Reason);
						if (miss.Reason == CastMissReason.Reflect)
						{
							packet.Write((byte)0);// relfectreason
						}
					}
				}

				WriteTargets(packet, cast);

				if (castGoFlags.HasFlag(CastFlags.Flag_0x800))
				{
					packet.Write(0);
				}

				if (castGoFlags.HasFlag(CastFlags.Flag_0x200000))
				{
					byte b1 = 0;
					byte b2 = 0;
					packet.Write(b1);
					packet.Write(b2);
					for (int i = 0; i < 6; i++)
					{
						byte mask = (byte)(1 << i);
						if ((mask & b1) != 0)
						{
							if ((mask & b2) == 0)
							{
								packet.WriteByte(0);
							}
						}
					}
				}

				if (castGoFlags.HasFlag(CastFlags.Flag_0x20000))
				{
					packet.WriteFloat(0);
					packet.Write(0);
				}

				if (cast.StartFlags.HasFlag(CastFlags.Ranged))
				{
					WriteAmmoInfo(cast, packet);
				}

				if (castGoFlags.HasFlag(CastFlags.Flag_0x80000))
				{
					packet.Write(0);
					packet.Write(0);
				}

                if (cast.TargetFlags.HasAnyFlag(SpellTargetFlags.DestinationLocation))
                {
                    packet.Write((byte)0); // unk 3.3.x?
                }

				cast.SendPacketToArea(packet);
			}
		}

		private static void WriteCaster(SpellCast cast, RealmPacketOut packet)
		{
			if (cast.UsedItem != null)
			{
				//packet.Write(cast.UsedItem.EntityId);
				cast.CasterItem.EntityId.WritePacked(packet);
			}
			else
			{
				//cast.UsedItem.EntityId.WritePacked(packet);
				cast.CasterReference.EntityId.WritePacked(packet);
			}

			//packet.Write(cast.Caster.EntityId);
			cast.CasterReference.EntityId.WritePacked(packet);
		}

		/// <summary>
		/// This is sent to caster if spell fails
		/// </summary>
		internal static void SendCastFailed(IPacketReceiver client, byte castId, Spell spell, SpellFailedReason result)
		{
			var len = (result == SpellFailedReason.RequiresSpellFocus || result == SpellFailedReason.RequiresArea ? 10 : 6);

			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CAST_FAILED, len))
			{
				//packet.Write((uint)spellId);
				//packet.Write((byte)2);
				//packet.WriteByte((byte)result);

				packet.Write(castId);
				packet.Write(spell.Id);
				packet.Write((byte)result);

				if (result == SpellFailedReason.RequiresSpellFocus)
				{
					packet.Write((uint)spell.RequiredSpellFocus);
				}
				else if (result == SpellFailedReason.RequiresArea)
				{
					packet.Write(spell.AreaGroupId);
				}

				client.Send(packet);
			}
		}

		/// <summary>
		/// Spell went wrong or got cancelled
		/// </summary>
		internal static void SendCastFailPackets(SpellCast spellCast, SpellFailedReason reason)
		{
			if (spellCast.Client == null)
			{
				return;
			}
			SendCastFailed(spellCast.Client, spellCast.Id, spellCast.Spell, reason);

			SendSpellFailure(spellCast, reason);
			SendSpellFailedOther(spellCast, reason);
		}

		internal static void SendSpellFailure(SpellCast spellCast, SpellFailedReason reason)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_FAILURE, 15))
			{
				spellCast.CasterReference.EntityId.WritePacked(packet);
				packet.Write(spellCast.Id);
				packet.Write(spellCast.Spell.Id);
				packet.Write((byte)reason);

				spellCast.SendPacketToArea(packet);
			}
		}

		internal static void SendSpellFailedOther(SpellCast spellCast, SpellFailedReason reason)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_FAILED_OTHER, 15))
			{
				spellCast.CasterReference.EntityId.WritePacked(packet);
				packet.Write(spellCast.Id);
				packet.Write(spellCast.Spell.Id);
				packet.Write((byte)reason);

				spellCast.SendPacketToArea(packet);
			}
		}

		/// <summary>
		/// Delays the spell-cast
		/// </summary>
		public static void SendCastDelayed(SpellCast cast, int delay)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_DELAYED, 12))
			{
				cast.CasterReference.EntityId.WritePacked(packet);
				packet.Write(delay);

				cast.SendPacketToArea(packet);
			}
		}


		/// <summary>
		/// Starts Channeling
		/// </summary>
		public static void SendChannelStart(SpellCast cast, SpellId spellId, int duration)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_CHANNEL_START, 12))
			{
				cast.CasterReference.EntityId.WritePacked(packet);
				packet.Write((uint)spellId);
				packet.Write(duration);

				cast.SendPacketToArea(packet);
			}
		}

		/// <summary>
		/// Changes the time of the channel
		/// </summary>
		public static void SendChannelUpdate(SpellCast cast, uint delay)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_CHANNEL_UPDATE, 12))
			{
				cast.CasterReference.EntityId.WritePacked(packet);
				packet.Write(delay);

				cast.SendPacketToArea(packet);
			}
		}

		/// <summary>
		/// Shows a spell visual
		/// </summary>
		public static void SendVisual(WorldObject target, SpellId id)
		{
			var spell = Get(id);
			SendVisual(target, spell.Visual);
		}

		/// <summary>
		/// Shows a spell visual
		/// </summary>
		public static void SendVisual(WorldObject target, uint visualId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_SPELL_VISUAL, 12))
			{
				//target.EntityId.WritePacked(packet);
				packet.Write(target.EntityId);
				packet.Write(visualId);

				target.SendPacketToArea(packet, true);
			}
		}

		public static void SendImpact(WorldObject target, SpellId id)
		{
			var spell = Get(id);
			SendImpact(target, spell.Visual);
		}

		/// <summary>
		/// Shows a spell Impact animation
		/// </summary>
		public static void SendImpact(WorldObject target, uint visualId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAY_SPELL_IMPACT, 12))
			{
				//target.EntityId.WritePacked(packet);
				packet.Write(target.EntityId);
				packet.Write(visualId);

				target.SendPacketToArea(packet, true);
			}
		}

		/// <summary>
		/// Clears a single spell's cooldown
		/// </summary>
		public static void SendClearCoolDown(Character chr, SpellId spellId)
		{
			IRealmClient client = chr.Client;

			if (client != null)
			{
				using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CLEAR_COOLDOWN, 12))
				{
					packet.WriteUInt((uint)spellId);
					chr.EntityId.WritePacked(packet);

					client.Send(packet);
				}
			}
		}

		/// <summary>
		/// Send a custom cooldown time to the client
		/// </summary>
		public static void SendSpellCooldown(WorldObject caster, IRealmClient client, uint spellId, ushort cooldown)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELL_COOLDOWN, 14))
			{
				packet.Write(caster.EntityId.Full);
				packet.WriteByte(0x00);// 1 = use category cooldown
				packet.Write(spellId);
				packet.Write((uint)cooldown); // if > 0, use this. If 0, use spell.RecoveryTime

				client.Send(packet);
			}
		}

		/// <summary>
		/// Send a custom cooldown time to the client
		/// </summary>
		public static void SendItemCooldown(IRealmClient client, uint spellId, IEntity item)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ITEM_COOLDOWN, 14))
			{
				packet.Write(item.EntityId.Full);
				packet.Write(spellId);

				client.Send(packet);
			}
		}

		/// <summary>
		/// Forces the client to start or update a cooldown timer on the given single spell
		/// (mostly important for certain talents and item spells that don't automatically start cooling down)
		/// </summary>
		public static void SendCooldownUpdate(Character chr, SpellId spellId)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_COOLDOWN_EVENT, 12))
			{
				packet.WriteUInt((uint)spellId);
				chr.EntityId.WritePacked(packet);

				chr.Send(packet);
			}
		}

		/// <summary>
		/// Sends spell modifier update
		/// </summary>
		public static void SendSpellModifier(Character chr, byte groupBitNumber, SpellModifierType type, int amount, bool isPercent)
		{
			var opcode = isPercent ? RealmServerOpCode.SMSG_SET_PCT_SPELL_MODIFIER :
				RealmServerOpCode.SMSG_SET_FLAT_SPELL_MODIFIER;
			using (var packet = new RealmPacketOut(opcode, 6))
			{
				packet.Write(groupBitNumber);
				packet.Write((byte) type);
				packet.Write(amount);

				chr.Send(packet);
			}
		}
		#endregion

		#region IN Packets
		[ClientPacketHandler(RealmServerOpCode.CMSG_CAST_SPELL)]
		public static void HandleCastSpell(IRealmClient client, RealmPacketIn packet)
		{
			byte castCount = packet.ReadByte();
			uint spellId = packet.ReadUInt32();
			byte unkFlags = packet.ReadByte();

			Spell spell = client.ActiveCharacter.Spells[spellId];
			if (spell != null)
			{
				SpellCast cast = client.ActiveCharacter.SpellCast;
				cast.Start(spell, packet, castCount, unkFlags);
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_CANCEL_CAST)]
		public static void HandleCancelCastSpell(IRealmClient client, RealmPacketIn packet)
		{
			uint spellId = packet.ReadUInt32();

			if (client.ActiveCharacter.IsUsingSpell /* && cast.Spell.Id == spellId*/)
			{
				var cast = client.ActiveCharacter.SpellCast;
				cast.Cancel();
			}
		}

		/// <summary>
		/// Somehow seems to be the same as CMSG_CANCEL_CAST
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_CANCEL_CHANNELLING)]
		public static void HandleCancelChanneling(IRealmClient client, RealmPacketIn packet)
		{
			uint spellId = packet.ReadUInt32();

			var chr = client.ActiveCharacter;
			SpellCast cast = chr.SpellCast;
			if (cast != null /* && cast.Spell.Id == spellId*/)
			{
				cast.Cancel();
			}
		}

		[ClientPacketHandler(RealmServerOpCode.CMSG_CANCEL_AUTO_REPEAT_SPELL)]
		public static void HandleCancelAutoRepeat(IRealmClient client, RealmPacketIn packet)
		{
			client.ActiveCharacter.AutorepeatSpell = null;
		}

		/// <summary>
		/// Probably can only be sent by God client
		/// </summary>
		[ClientPacketHandler(RealmServerOpCode.CMSG_UNLEARN_SPELL)]
		public static void HandleUnlearnSpell(IRealmClient client, RealmPacketIn packet)
		{
			uint spellId = packet.ReadUInt32();

			//client.ActiveCharacter.Spells.Remove(spellId);
		}

		[PacketHandler(RealmServerOpCode.CMSG_SPELLCLICK)]
		public static void HandleSpellClick(IRealmClient client, RealmPacketIn packet)
		{
			var chr = client.ActiveCharacter;
			var guid = packet.ReadEntityId();
			var mob = chr.Region.GetObject(guid) as NPC;
			SpellTriggerInfo spellInfo;

			if (mob != null && (spellInfo = mob.Entry.SpellTriggerInfo) != null)
			{
				chr.SpellCast.Start(spellInfo.Spell, false);
			}
		}
		#endregion

		#region Runes
		public static void SendConvertRune(IRealmClient client, uint index, RuneType type)
		{
			using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CONVERT_RUNE, 2))
			{
				packet.Write((byte)index);
				packet.Write((byte)type);

				client.Send(packet);
			}
		}
		#endregion
	}
}