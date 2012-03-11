using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Handlers
{
    public static class CombatLogHandler
    {
        /// <summary>
        /// Used for any PeriodicAura (repeating ticks)
        /// </summary>
        /// <param name="extra">Always seems to be one</param>
        public static void SendPeriodicAuraLog(IPacketReceiver client, WorldObject caster, WorldObject target,
            uint spellId, uint extra, AuraTickFlags flags, int amount)
        {
            // TODO: Update struct
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PERIODICAURALOG, 32))
            {
                caster.EntityId.WritePacked(packet);
                target.EntityId.WritePacked(packet);
                packet.WriteUInt(spellId);
                packet.WriteUInt(extra);
                packet.WriteUInt((uint)flags);
                packet.WriteUInt(amount);

                target.SendPacketToArea(packet, true);
            }
        }

        /// <summary>
        /// Used for Periodic leech effects, mostly Cannibalism
        /// </summary>
        /// <returns></returns>
        public static void SendPeriodicDamage(WorldObject caster, WorldObject target, uint spellId, AuraTickFlags type,
                                              int amount)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PERIODICAURALOG, 32))
            {
                caster.EntityId.WritePacked(packet);
                target.EntityId.WritePacked(packet);
                packet.WriteUInt(spellId);

                packet.WriteUInt(1);				// count
                packet.WriteUInt((uint)type);
                packet.WriteUInt(amount);

                target.SendPacketToArea(packet, true);
            }
        }

        /// <summary>
        /// Correct 3.0.9
        /// </summary>
        public static void SendSpellMiss(SpellCast cast, bool display, ICollection<MissedTarget> missedTargets)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLLOGMISS, 34))
            {
                packet.Write(cast.Spell.Id);
                packet.Write(cast.CasterReference.EntityId);
                packet.Write(display);// TODO: test this value. Its a bool that seems to determine whether to display this packet in the combat log
                packet.Write(missedTargets.Count);
                foreach (var miss in missedTargets)
                {
                    packet.Write(miss.Target.EntityId);
                    packet.Write((byte)miss.Reason);
                }
                cast.SendPacketToArea(packet);
            }
        }

        /// <summary>
        /// Correct for 3.0.9
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="spellId"></param>
        /// <param name="b1"></param>
        public static void SendSpellOrDamageImmune(IPacketReceiver client, ObjectBase obj1, ObjectBase obj2, int spellId, bool b1)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLORDAMAGE_IMMUNE, 21))
            {
                packet.Write(obj1.EntityId);
                packet.Write(obj2.EntityId);
                packet.Write(spellId);
                packet.Write(b1);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Correct for 3.0.9
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="target"></param>
        /// <param name="spellId"></param>
        /// <param name="powerType"></param>
        /// <param name="value"></param>
        public static void SendEnergizeLog(WorldObject caster, Unit target, uint spellId, PowerType powerType, int value)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLENERGIZELOG, 25))
            {
                target.EntityId.WritePacked(packet);
                caster.EntityId.WritePacked(packet);
                packet.Write(spellId);
                packet.Write((int)powerType);
                packet.Write(value);
                caster.SendPacketToArea(packet, true);
            }
        }

        ///// <summary>
        ///// Unused
        ///// </summary>
        //public static void SendMagicResist(ObjectBase caster, ObjectBase target, uint spellId)
        //{
        //    using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESISTLOG, 21))
        //    {
        //        caster.EntityId.WritePacked(packet);
        //        target.EntityId.WritePacked(packet);
        //        packet.WriteUInt(spellId);
        //        packet.WriteByte((byte)0);
        //    }
        //}

        /// <summary>
        /// Correct for 3.0.9
        /// </summary>
        /// <param name="target">Optional</param>
        /// <param name="value">Optional</param>
        public static RealmPacketOut SendSpellLogExecute(ObjectBase caster, uint spellId, SpellEffectType effect, ObjectBase target, uint value)
        {
            // TODO: Info we still need for this packet: spellId of interrupted spell, itemId of created item

            var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLLOGEXECUTE, 37);
            caster.EntityId.WritePacked(packet);
            packet.Write(spellId);
            const int effectCount = 1;
            packet.Write(effectCount);

            for (int i = 0; i < effectCount; i++)
            {
                packet.Write((int)effect);
                const int targetCount = 1; // unsure

                for (int j = 0; j < targetCount; j++)
                {
                    switch (effect)
                    {
                        case SpellEffectType.PowerDrain:
                            {
                                target.EntityId.WritePacked(packet);
                                packet.Write(0);
                                packet.Write(0);
                                packet.Write(0.0f);
                                break;
                            }
                        case SpellEffectType.AddExtraAttacks:
                            {
                                target.EntityId.WritePacked(packet);
                                packet.Write(0);
                                break;
                            }
                        case SpellEffectType.InterruptCast:
                            {
                                packet.Write(0); // spellId of interrupted spell
                                break;
                            }
                        case SpellEffectType.DurabilityDamage:
                            {
                                packet.Write(0);
                                packet.Write(0);
                                break;
                            }
                        case SpellEffectType.OpenLock:
                        case SpellEffectType.OpenLockItem:
                            {
                                if (target is Item)
                                {
                                    target.EntityId.WritePacked(packet);
                                }
                                else
                                {
                                    packet.Write((byte)0);
                                }
                                break;
                            }
                        case SpellEffectType.CreateItem:
                        case SpellEffectType.CreateItem2:
                            {
                                packet.Write(0); // itemId
                                break;
                            }
                        case SpellEffectType.Summon:
                        case SpellEffectType.TransformItem:
                        case SpellEffectType.SummonPet:
                        case SpellEffectType.SummonObjectWild:
                        case SpellEffectType.CreateHouse:
                        case SpellEffectType.Duel:
                        case SpellEffectType.SummonObjectSlot1:
                        case SpellEffectType.SummonObjectSlot2:
                        case SpellEffectType.SummonObjectSlot3:
                        case SpellEffectType.SummonObjectSlot4:
                            {
                                if (target is Unit)
                                {
                                    target.EntityId.WritePacked(packet); // summon recipient
                                }
                                else
                                {
                                    packet.Write((byte)0);
                                }
                                break;
                            }
                        case SpellEffectType.FeedPet:
                            {
                                if (target is Item)
                                {
                                    packet.Write(target.EntryId);
                                }
                                else
                                {
                                    packet.Write(0);
                                }
                                break;
                            }
                        case SpellEffectType.DismissPet:
                            {
                                target.EntityId.WritePacked(packet);
                                break;
                            }
                        case SpellEffectType.Resurrect:
                        case SpellEffectType.ResurrectFlat:
                            {
                                if (target is Unit)
                                {
                                    target.EntityId.WritePacked(packet);
                                }
                                else
                                {
                                    packet.Write((byte)0);
                                }
                                break;
                            }
                    }
                }
            }

            return packet;
        }

        public static void SendHealLog(WorldObject caster, Unit target, uint spellId, int value, bool critical, int overheal)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLHEALLOG, 25))
            {
                target.EntityId.WritePacked(packet);
                caster.EntityId.WritePacked(packet);

                packet.Write(spellId);
                packet.Write(value);
                packet.Write(overheal);		// overheal
                packet.Write(0);			// absorb
                packet.Write((byte)(critical ? 1 : 0));
                packet.Write((byte)0);		// unused

                target.SendPacketToArea(packet, true);
            }
        }

        /// <summary>
        /// Usually caused by jumping too high, diving too long, standing too close to fire etc
        /// </summary>
        public static void SendEnvironmentalDamage(WorldObject target, EnviromentalDamageType type, uint totalDamage)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ENVIRONMENTALDAMAGELOG, 21))
            {
                target.EntityId.WritePacked(packet);

                packet.WriteByte((byte)type);
                packet.WriteUInt(totalDamage);
                packet.WriteULong(0);

                target.SendPacketToArea(packet, true);
            }
        }

        /// <summary>
        /// Any spell and ranged damage
        /// SMSG_SPELLNONMELEEDAMAGELOG
        /// </summary>
        /*
            Target: High: 0xF530 (Unit) - Low: 619710 - Entry: UmbralBrute (30922)
            Caster: High: 0x0000 (Player) - Low: 2211871
            Spell: ClassSkillArcaneShot_11 (49045)
            Damage: 776
            Overkill: 0
            SchoolMask: Arcane (64)
            Absorbed: 0
            Resisted: 0
            UnkByte1: 0
            UnkByte2 (Unused): 0
            Blocked: 0
            HitFlags: 37
            UnkByte3 (Unused): 0
         */

        public static void SendMagicDamage(DamageAction state)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLNONMELEEDAMAGELOG, 40))
            {
                state.Victim.EntityId.WritePacked(packet);
                if (state.Attacker != null)
                {
                    state.Attacker.EntityId.WritePacked(packet);
                }
                else
                {
                    packet.Write((byte)0);
                }
                packet.Write(state.SpellEffect != null ? state.SpellEffect.Spell.Id : 0);

                packet.Write(state.Damage);
                packet.Write(0); // overkill?
                packet.Write((byte)state.Schools);
                packet.Write(state.Absorbed);
                packet.Write(state.Resisted);
                //packet.Write(0);				// is always 0
                packet.Write(state.Schools.HasAnyFlag(DamageSchoolMask.Physical));
                packet.Write((byte)0);			// 0 or 1
                packet.Write(state.Blocked);

                // also flags 0x8, 0x10,
                var hitFlags = state.IsCritical ? SpellLogFlags.Critical : SpellLogFlags.None;
                packet.Write((int)hitFlags);
                packet.Write((byte)0);// unused by client

                state.Victim.SendPacketToArea(packet, true);
            }
        }

        public static void SendMagicDamage(WorldObject victim, IEntity attacker,
            SpellId spell, uint damage, uint overkill, DamageSchoolMask schools,
            uint absorbed, uint resisted, uint blocked,
            bool unkBool, SpellLogFlags flags)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SPELLNONMELEEDAMAGELOG, 40))
            {
                victim.EntityId.WritePacked(packet);
                if (attacker != null)
                {
                    attacker.EntityId.WritePacked(packet);
                }
                else
                {
                    packet.Write((byte)0);
                }
                packet.Write((uint)spell);

                packet.Write(damage);
                packet.Write(overkill);
                packet.Write((byte)schools);
                packet.Write(absorbed);
                packet.Write(resisted);
                packet.Write(0);				// apparently always 0
                packet.Write(unkBool);			// 0 or 1
                packet.Write(blocked);
                // also flags 0x8, 0x10,
                packet.Write((uint)flags);
                packet.Write((byte)0);// unused by client

                victim.SendPacketToArea(packet, true);
            }
        }
    }
}