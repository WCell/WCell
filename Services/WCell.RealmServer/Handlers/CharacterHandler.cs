using System;
using System.Linq;
using NLog;
using WCell.Constants;
using WCell.Constants.Achievements;
using WCell.Constants.Items;
using WCell.Constants.Login;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Network;
using WCell.RealmServer.Database;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Items;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Res;
using WCell.Util;
using WCell.Util.NLog;
using WCell.Util.Threading;

namespace WCell.RealmServer.Handlers
{
    /// <summary>
    /// Contains all kinds of player packets that don't belong to a full sub-system
    /// </summary>
    public static class CharacterHandler
    {
        private const int CharEnumItemBytes = 4 + 1 + 4;
        private const int CharEnumItemCount = 23; // 19 items, 4 bags
        public const int MaxCharNameLength = 12;
        public const int MinCharNameLength = 3;

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Delay (in seconds) to wait until a new zone is considered as discovered (Default: 3s)
        /// </summary>
        public static int ZoneUpdateDelayMillis = 3000;

        /// <summary>
        /// Whether to notify everyone on the server when players log in/out
        /// </summary>
        public static bool NotifyPlayerStatus = true;

        #region Create

        /// <summary>
        /// Handles an incoming character creation request.
        /// TODO: Add protection against char creation/deletion spam
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_CHAR_CREATE, IsGamePacket = false, RequiresLogin = false)]
        public static void HandleCharCreateRequest(IRealmClient client, RealmPacketIn packet)
        {
            var acc = client.Account;
            if (acc == null || client.ActiveCharacter != null)
                return;

            try
            {
                var characterName = packet.ReadCString();
                var errorCode = IsNameValid(ref characterName);

                if (errorCode != LoginErrorCode.RESPONSE_SUCCESS)
                {
                    SendCharCreateReply(client, errorCode);
                    return;
                }

                if (CharacterRecord.Exists(characterName))
                {
                    SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_NAME_IN_USE);
                    return;
                }

                var chrRace = (RaceId)packet.ReadByte();
                var chrClass = (ClassId)packet.ReadByte();

                var archetype = ArchetypeMgr.GetArchetype(chrRace, chrClass);
                if (archetype == null)
                {
                    SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_ERROR);
                    return;
                }

                if (!acc.Role.IsStaff)
                {
                    if (archetype.Class.StartLevel > BaseClass.DefaultStartLevel && acc.HighestCharLevel < archetype.Class.StartLevel)
                    {
                        SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_LEVEL_REQUIREMENT);
                        return;
                    }

                    //if (!RealmServer.Instance.ServerRules.CanCreateCharacter(client, chrRace, chrClass, out errorCode))
                    //{
                    //    SendCharCreateReply(client, errorCode);
                    //    return;
                    //}
                }

                var record = CharacterRecord.CreateNewCharacterRecord(client.Account, characterName);

                if (record == null)
                {
                    log.Error("Unable to create character record!");
                    SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_ERROR);

                    return;
                }

                var gender = (GenderType)packet.ReadByte();
                record.Gender = gender;
                record.Skin = packet.ReadByte();
                record.Face = packet.ReadByte();
                record.HairStyle = packet.ReadByte();
                record.HairColor = packet.ReadByte();
                record.FacialHair = packet.ReadByte();
                record.Outfit = packet.ReadByte();
                record.GodMode = acc.Role.AppearAsGM;

                record.SetupNewRecord(archetype);

                var charCreateTask = new Message2<IRealmClient, CharacterRecord>
                {
                    Callback = CharCreateCallback,
                    Parameter1 = client,
                    Parameter2 = record
                };

                RealmServer.IOQueue.AddMessage(charCreateTask);
            }
            catch (Exception e)
            {
                LogUtil.ErrorException(e);
                SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_ERROR);
            }
        }

        private static void CharCreateCallback(IRealmClient client, CharacterRecord newCharRecord)
        {
            // check again, to avoid people creating 2 chars with the same name at the same time screwing up the server
            if (CharacterRecord.Exists(newCharRecord.Name))
            {
                SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_NAME_IN_USE);
            }
            else
            {
                try
                {
                    newCharRecord.CreateAndFlush();
                }
                catch (Exception e)
                {
                    //LogUtil.ErrorException(e, "Could not create Character \"{0}\" for: {1}", newCharRecord.Name, client.Account);
                    //SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_ERROR);
                    try
                    {
                        RealmDBMgr.OnDBError(e);
                        newCharRecord.CreateAndFlush();
                    }
                    catch (Exception)
                    {
                        SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_ERROR);
                        return;
                    }
                }

                client.Account.Characters.Add(newCharRecord);

                SendCharCreateReply(client, LoginErrorCode.CHAR_CREATE_SUCCESS);
            }
        }

        /// <summary>
        /// Sends a character creation reply to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="error">the error code</param>
        public static void SendCharCreateReply(IPacketReceiver client, LoginErrorCode error)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_CREATE, 1))
            {
                packet.WriteByte((byte)error);

                client.Send(packet);
            }
        }

        #endregion Create

        #region Delete

        /// <summary>
        /// Handles an incmming character deletion request.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_CHAR_DELETE, IsGamePacket = false, RequiresLogin = false)]
        public static void CharDeleteRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.Account == null || client.ActiveCharacter != null)
                return;

            var acc = client.Account;
            var charId = packet.ReadEntityId().Low;
            var record = acc.GetCharacterRecord(charId);

            if (record == null)
            {
                SendCharDeleteReply(client, LoginErrorCode.CHAR_DELETE_FAILED);
            }
            else if (record.AccountId != acc.AccountId)
            {
                // cannot delete while still being logged in or if CharacterRecord is not owned
                SendCharDeleteReply(client, LoginErrorCode.CHAR_DELETE_FAILED);
            }
            else
            {
                acc.RemoveCharacterRecord(record.EntityLowId);
                if (record.IsOnline)
                {
                    var chr = World.GetCharacter(record.EntityLowId);
                    if (chr != null)
                    {
                        chr.FinishLogout();
                        var context = chr.ContextHandler;
                        if (context != null)
                        {
                            context.AddMessage(() =>
                            RealmServer.IOQueue.AddMessage(new Message(() =>
                            {
                                var returnCode = record.TryDelete();
                                SendCharDeleteReply(client, returnCode);
                            })));
                            return;
                        }
                    }
                }

                RealmServer.IOQueue.AddMessage(new Message(() =>
                {
                    var returnCode = record.TryDelete();
                    SendCharDeleteReply(client, returnCode);
                }));
            }
        }

        /// <summary>
        /// Sends a character delete reply to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="error">the rror code</param>
        public static void SendCharDeleteReply(IRealmClient client, LoginErrorCode error)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_DELETE, 1))
            {
                packet.WriteByte((byte)error);

                client.Send(packet);
            }

            SendCharEnum(client);
        }

        #endregion Delete

        #region Char Enum

        /// <summary>
        /// Handles an incoming character enum request.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_CHAR_ENUM, IsGamePacket = false, RequiresLogin = false)]
        public static void CharEnumRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.Account == null || client.ActiveCharacter != null)
                return;

            AccountDataHandler.SendAccountDataTimes(client, AccountDataHandler.CacheMask.GlobalCache);
            RealmServer.IOQueue.AddMessage(() => SendCharEnum(client));
        }

        /// <summary>
        /// Sends the character list to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <remarks>They probably meant 'enumeration,' but 'list' would have made so much more sense. :)</remarks>
        public static void SendCharEnum(IRealmClient client)
        {
            CharacterRecord curRecord = null;
            try
            {
                using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_ENUM))
                {
                    var chrs = client.Account.Characters;

                    packet.WriteByte(chrs.Count);

                    foreach (var record in chrs)
                    {
                        curRecord = record;

                        packet.Write(EntityId.GetPlayerId(record.EntityLowId)); // 8
                        packet.WriteCString(record.Name); // 9 + namelength
                        packet.Write((byte)record.Race); // 10 + namelength
                        packet.Write((byte)record.Class); // 11 + namelength
                        packet.Write((byte)record.Gender); // 12 + namelength
                        packet.Write(record.Skin); // 13 + namelength
                        packet.Write(record.Face); // 14 + namelength
                        packet.Write(record.HairStyle); // 15 + namelength
                        packet.Write(record.HairColor); // 16 + namelength
                        packet.Write(record.FacialHair); // 17 + namelength
                        packet.Write((byte)record.Level); // 18 + namelength
                        packet.Write((int)record.Zone); // 22 + namelength
                        packet.Write((int)record.MapId); // 26 + namelength

                        packet.Write(record.PositionX); // 30 + namelength
                        packet.Write(record.PositionY); // 34 + namelength
                        packet.Write(record.PositionZ); // 38 + namelength

                        packet.WriteUInt(record.GuildId); // guild id							// 42 + namelength
                        packet.Write((int)record.CharacterFlags); // 46 + namelength

                        packet.Write(0); // TOOD: Customization flags

                        packet.Write((byte)1); // NEW 3.2.0 - first login y/n - set to 1 for now

                        // Deprecated: Rest State
                        // var restState = record.RestXp > 0 ? RestState.Resting : RestState.Normal;
                        // packet.WriteByte((byte)restState); // 47 + namelength

                        // pet info (51 - 63)
                        var petEntry = record.PetEntry;
                        if (petEntry != null)
                        {
                            packet.Write(petEntry.DisplayIds[0]);
                            packet.Write(record.Level);				// liars!
                            packet.Write((int)petEntry.FamilyId);
                        }
                        else
                        {
                            packet.Write(0); // 51 + namelength
                            packet.Write(0); // 55 + namelength
                            packet.Write(0); // 59 + namelength
                        }

                        var itemOffset = packet.Position;
                        //packet.TotalLength += CharEnumItemCount * CharEnumItemBytes; // 239 + namelength
                        packet.Zero(CharEnumItemCount * CharEnumItemBytes);
                        packet.Position = itemOffset;

                        if (record.JustCreated)
                        {
                            var archetype = ArchetypeMgr.GetArchetype(record.Race, record.Class);
                            if (archetype != null)
                            {
                                var items = archetype.GetInitialItems(record.Gender);
                                foreach (var item in items)
                                {
                                    var template = item.Template;
                                    if (template.EquipmentSlots != null)
                                    {
                                        packet.Position = itemOffset + ((int)template.EquipmentSlots[0] * CharEnumItemBytes);

                                        packet.Write(template.DisplayId);
                                        packet.Write((byte)template.InventorySlotType);
                                        packet.Write(0);
                                    }
                                }
                            }
                            else
                            {
                                log.Warn("Invalid Race/Class combination " +
                                         "({0} {1}) in CharacterRecord {2}.",
                                         record.Race, record.Class, record);
                            }
                        }
                        else
                        {
                            foreach (var item in record.GetOrLoadItems())
                            {
                                if (!item.IsOwned)
                                {
                                    continue;
                                }

                                var template = item.Template;
                                if (item.ContainerSlot == BaseInventory.INVALID_SLOT && item.Slot <= (int)EquipmentSlot.Bag4)
                                {
                                    packet.Position = itemOffset + (item.Slot * CharEnumItemBytes);
                                    if (template != null)
                                    {
                                        var enchant = item.GetEnchant(EnchantSlot.Permanent);

                                        packet.Write(template.DisplayId);
                                        packet.Write((byte)template.InventorySlotType);
                                        packet.Write(enchant != null ? enchant.Visual : 0);
                                    }
                                    else
                                    {
                                        packet.Write(0);
                                        packet.Write((byte)0);
                                        packet.Write(0);
                                    }
                                }
                            }
                        }

                        packet.Position = itemOffset + (CharEnumItemCount * CharEnumItemBytes);
                    }

                    client.Send(packet);
                }
            }
            catch (Exception e)
            {
                if (curRecord == null)
                {
                    throw e;
                }

                LogUtil.ErrorException(e,
                                       "Could not create Char-Enum " +
                                       "for Character \"{0}\" (Race: {1}, Class: {2}, Level: {3}, Map: {4}{5}).",
                                       curRecord, curRecord.Race, curRecord.Class, curRecord.Level, curRecord.MapId,
                                       curRecord.IsNew ? ", [New]" : "");
            }
        }

        #endregion Char Enum

        #region StandState

        /// <summary>
        /// Handles an incoming stand state change request.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_STANDSTATECHANGE)]
        public static void HandleStandStateChange(IRealmClient client, RealmPacketIn packet)
        {
            byte standState = packet.ReadByte();

            client.ActiveCharacter.StandState = (StandState)standState;
        }

        public static void SendStandStateUpdate(Character character, StandState newState)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_STANDSTATE_UPDATE, 1))
            {
                packet.Write((byte)newState);

                character.Client.Send(packet);
            }
        }

        #endregion StandState

        #region Time

        /// <summary>
        /// Sends a "tick count" packet to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        public static void SendTickQuery(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TIME_SYNC_REQ, 4))
            {
                packet.WriteUInt(client.TickCount);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Sends the world time speed to the client.
        /// </summary>
        /// <param name="chr">the character to send to</param>
        /// <remarks>This packet tells the client about the "speed" of time in the game world.</remarks>
        /// <remarks>Usually, this speed is equivalent to real-life.</remarks>
        public static void SendTimeSpeed(IPacketReceiver client)
        {
            SendTimeSpeed(client, RealmServer.IngameTime, RealmServerConfiguration.IngameMinutesPerSecond);
        }

        /// <summary>
        /// Sends the world time speed to the client.
        /// </summary>
        /// <param name="chr">the character to send to</param>
        /// <remarks>This packet tells the client about the "speed" of time in the game world.</remarks>
        /// <remarks>Usually, this speed is equivalent to real-life.</remarks>
        public static void SendTimeSpeed(IPacketReceiver client, float timeSpeed)
        {
            SendTimeSpeed(client, DateTime.Now, timeSpeed);
        }

        public static void SendTimeSpeed(IPacketReceiver client, DateTime time, float timeSpeed)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOGIN_SETTIMESPEED, 8))
            {
                packet.WriteDateTime(time);
                packet.WriteFloat(timeSpeed);
                packet.WriteInt(0);				// new, unknown

                client.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_TIME_SYNC_RESP)]
        public static void HandleTickQueryResponse(IRealmClient client, RealmPacketIn packet)
        {
            // TODO: Implement timeout-timer
            client.TickCount = packet.ReadUInt32();
            client.ClientTime = packet.ReadUInt32();
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_PLAYED_TIME)]
        public static void HandlePlayedTime(IRealmClient client, RealmPacketIn packet)
        {
            byte display = packet.ReadByte();
            client.ActiveCharacter.UpdatePlayedTime();
            SendPlayedTime(client, display);
        }

        public static void SendPlayedTime(IRealmClient client, byte display)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_PLAYED_TIME, 8))
            {
                packet.WriteUInt(client.ActiveCharacter.TotalPlayTime);
                packet.WriteUInt(client.ActiveCharacter.LevelPlayTime);
                packet.WriteByte(display);

                client.Send(packet);
            }
        }

        #endregion Time

        #region Level Up

        /// <summary>
        /// Sends level up info to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="level">the level of the character</param>
        /// <param name="hpGain">the HP gained</param>
        /// <param name="manaGain">the mana gained</param>
        /// <param name="strBonus">the Strength gained</param>
        /// <param name="agiBonus">the Agility gained</param>
        /// <param name="staBonus">the Stamina gained</param>
        /// <param name="intBonus">the Intellect gained</param>
        /// <param name="sprBonus">the Spirit gained</param>
        public static void SendLevelUpInfo(IPacketReceiver client, int level, int hpGain, int manaGain, int strBonus,
                                           int agiBonus, int staBonus, int intBonus, int sprBonus)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LEVELUP_INFO))
            {
                packet.Write(level);
                packet.Write(hpGain);

                packet.Write(manaGain);
                for (int i = 1; i < 7; i++)
                {
                    packet.Write(0);
                }

                packet.Write(strBonus);
                packet.Write(agiBonus);
                packet.Write(staBonus);
                packet.Write(intBonus);
                packet.Write(sprBonus);

                client.Send(packet);
            }
        }

        #endregion Level Up

        #region Selection

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_SELECTION)]
        public static void HandleSetSelection(IRealmClient client, RealmPacketIn packet)
        {
            var target = packet.ReadEntityId();

            if (target.Low == 0)
            {
                client.ActiveCharacter.Target = null;
            }
            else
            {
                var targetUnit = client.ActiveCharacter.Map.GetObject(target) as Unit;
                if (targetUnit != null &&
                    (client.ActiveCharacter.CanSee(targetUnit) ||
                    (targetUnit is Character && client.ActiveCharacter.Group == ((Character)targetUnit).Group)))
                {
                    client.ActiveCharacter.Target = targetUnit;
                }
            }
        }

        /// <summary>
        /// Makes the client un-select it's current target
        /// </summary>
        public static void SendClearTarget(Character chr, WorldObject target)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CLEAR_TARGET, 8))
            {
                packet.Write(target.EntityId);
                chr.Client.Send(packet);
            }
        }

        #endregion Selection

        #region ActionBar

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_ACTIONBAR_TOGGLES)]
        public static void HandleToggleActionBar(IRealmClient client, RealmPacketIn packet)
        {
            client.ActiveCharacter.ActionBarMask = packet.ReadByte();
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_ACTION_BUTTON)]
        public static void HandleSetActionButtons(IRealmClient client, RealmPacketIn packet)
        {
            var index = packet.ReadByte();

            if (index >= ActionButton.MaxAmount)
            {
                log.Warn("{0} sent an invalid ActionButton (Index: {1})", client, index);
                return;
            }

            var actionAndType = packet.ReadUInt32();

            var action = actionAndType & 0x00FFFFFF;
            var type = (byte)((actionAndType & 0xFF000000) >> 24);
            client.ActiveCharacter.BindActionButton(index, action, type, false);
        }

        public static void SendActionButtons(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ACTION_BUTTONS, chr.ActionButtons.Length))
            {
                packet.Write((byte)0); // talent spec
                packet.Write(chr.ActionButtons);

                chr.Client.Send(packet);
            }
        }

        #endregion ActionBar

        #region Death

        //0x0000 (Player) - Low: 2211871
        //- die
        //<- SMSG_DEATH_RELEASE_LOC
        //...
        //-> CMSG_REPOP_REQUEST
        //<- SMSG_PRE_RESURRECT (Packed GUID)
        //- Ghost Aura
        //<- SMSG_INIT_WORLD_STATES
        //<- MSG_CORPSE_QUERY (only map set)
        //- teleport to SH
        //...
        //-> MSG_CORPSE_QUERY
        //<- MSG_CORPSE_QUERY
        //- runs to corpse
        //...
        //- sees corpse
        //... (1 min)
        //-> CMSG_RECLAIM_CORPSE
        //- revive
        //    - ghost aura gets removed
        //    - health
        //    - etc

        [ClientPacketHandler(RealmServerOpCode.CMSG_REPOP_REQUEST)]
        public static void HandleRepopRequest(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (!chr.IsAlive && chr.Corpse == null)
            {
                if (chr.GodMode)
                {
                    chr.Resurrect();
                }
                else
                {
                    chr.ReleaseCorpse();
                }
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_RECLAIM_CORPSE)]
        public static void HandleReclaimCorpse(IRealmClient client, RealmPacketIn packet)
        {
            packet.ReadEntityId(); // unneeded
            var chr = client.ActiveCharacter;
            if (!chr.CanReclaimCorpse)
            {
                SendRessurectFailed(client, 1);
            }
            else
            {
                chr.Resurrect();
            }
        }

        public static void SendRessurectFailed(IRealmClient client, int error)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_RESURRECT_FAILED, 4))
            {
                packet.Write(error);

                client.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.MSG_CORPSE_QUERY)]
        public static void HandleCorpseQuery(IRealmClient client, RealmPacketIn packet)
        {
            if (!client.ActiveCharacter.IsAlive)
            {
                SendCorpseQueryReply(client, client.ActiveCharacter.Corpse != null
                                                ? (WorldObject)client.ActiveCharacter.Corpse
                                                : client.ActiveCharacter);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SPIRIT_HEALER_ACTIVATE)]
        public static void HandleSpiritHealerActivate(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            var shId = packet.ReadEntityId();
            var healer = chr.Map.GetObject(shId) as NPC;

            if (healer != null && healer.IsSpiritHealer && chr.IsCorpseReclaimable && healer.CheckVendorInteraction(chr))
            {
                chr.ResurrectWithConsequences();
            }
        }

        public static void SendHealerPosition(IPacketReceiver client, WorldObject healer)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_DEATH_RELEASE_LOC, 16))
            {
                packet.Write((uint)healer.Map.Id);
                packet.Write(healer.Position);

                client.Send(packet);
            }
        }

        public static void SendCorpseReclaimDelay(IPacketReceiver client, int millis)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CORPSE_RECLAIM_DELAY, 4))
            {
                packet.Write(millis);

                client.Send(packet);
            }
        }

        public static void SendCorpseQueryReply(IPacketReceiver client, WorldObject obj)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.MSG_CORPSE_QUERY, 21))
            {
                // TODO: instance id

                packet.Write((byte)1);
                packet.Write((uint)obj.Map.Id);
                packet.Write(obj.Position);
                packet.Write((uint)obj.Map.Id);

                client.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_CORPSE_MAP_POSITION_QUERY)]
        public static void HandleCorpseMapQueryOpcode(IRealmClient client, RealmPacketIn packet)
        {
            var unk = packet.ReadUInt32(); // corpse low guid

            SendCorpseMapQueryResponse(client); // just reply for now.
        }

        public static void SendCorpseMapQueryResponse(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CORPSE_MAP_POSITION_QUERY_RESPONSE, 16))
            {
                for (var i = 0; i < 4; i++)
                    packet.WriteFloat(0.0f); // unk

                client.Send(packet);
            }
        }

        #endregion Death

        #region Naming

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_PLAYER_DECLINED_NAMES, IsGamePacket = false, RequiresLogin = false)]
        public static void HandleSetDeclinedNames(IRealmClient client, RealmPacketIn packet)
        {
            var charId = packet.ReadEntityId();
            // TODO: Save declined names to db. Add to character list that declined names already set
            SendDeclinedNamesResult(client, charId, false);
        }

        public static void SendDeclinedNamesResult(IRealmClient client, EntityId chr, bool failed)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SET_PLAYER_DECLINED_NAMES_RESULT, 9))
            {
                packet.Write(failed ? 1 : 0);
                packet.Write(chr);
                client.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_CHAR_RENAME, IsGamePacket = false, RequiresLogin = false)]
        public static void CharacterRenameRequest(IRealmClient client, RealmPacketIn packet)
        {
            if (client.Account == null || client.ActiveCharacter != null)
                return;

            var guid = packet.ReadEntityId();
            var newName = packet.ReadCString();
            var record = client.Account.GetCharacterRecord(guid.Low);

            if (record == null)
            {
                log.Error(WCell_RealmServer.IllegalRenameAttempt, guid.ToString(), client);
            }
            else
            {
                if (record.CharacterFlags.HasFlag(CharEnumFlags.NeedsRename))
                {
                    // their character isn't flagged to be renamed, what do they think they're doing? ;)
                    client.Disconnect();
                }
                else
                {
                    LoginErrorCode errorCode = IsNameValid(ref newName);

                    if (errorCode != LoginErrorCode.RESPONSE_SUCCESS)
                    {
                        SendCharacterRenameError(client, errorCode);
                    }
                    else
                    {
                        log.Debug(WCell_RealmServer.RenamingCharacter, record.Name, newName);

                        record.Name = newName;

                        var charRenameTask =
                            new Message4<IRealmClient, CharacterRecord, string, EntityId>(CharacterRenameCallback)
                            {
                                Parameter1 = client,
                                Parameter2 = record,
                                Parameter3 = newName,
                                Parameter4 = guid
                            };

                        // only enqueue to IO Queue if we are in a map context?
                        RealmServer.IOQueue.AddMessage(charRenameTask);
                    }
                }
            }
        }

        private static void CharacterRenameCallback(IRealmClient client, CharacterRecord ch, string newName, EntityId guid)
        {
            if (CharacterRecord.Exists(newName))
            {
                SendCharacterRenameError(client, LoginErrorCode.CHAR_CREATE_NAME_IN_USE);
            }
            else
            {
                log.Debug(WCell_RealmServer.RenamingCharacter, ch.Name, newName);

                ch.Name = newName;

                SendCharacterRename(client, guid, ch.Name);
            }
        }

        public static void SendCharacterRenameError(IPacketReceiver client, LoginErrorCode error)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_RENAME, 1))
            {
                packet.WriteByte((byte)error);
                client.Send(packet);
            }
        }

        public static void SendCharacterRename(IPacketReceiver client, EntityId guid, string newName)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CHAR_RENAME, 10 + newName.Length))
            {
                packet.WriteByte((byte)LoginErrorCode.RESPONSE_SUCCESS);
                packet.Write(guid);
                packet.WriteCString(newName);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Checks if characters name is valid. Checks for size of name, words in name, invalid symbols.
        /// If server is set to use blizz like names, name may be adjusted according to blizz rules.
        /// </summary>
        /// <param name="characterName">the name of the character to check</param>
        /// <returns>true if characters name is ok; false otherwise (error codes will contain reason)</returns>
        public static LoginErrorCode IsNameValid(ref string characterName)
        {
            if (characterName.Length == 0)
            {
                return LoginErrorCode.CHAR_NAME_NO_NAME;
            }

            if (characterName.Length < MinCharNameLength)
            {
                return LoginErrorCode.CHAR_NAME_TOO_SHORT;
            }

            if (characterName.Length > MaxCharNameLength)
            {
                return LoginErrorCode.CHAR_NAME_TOO_LONG;
            }

            if (DoesNameViolate(characterName))
            {
                return LoginErrorCode.CHAR_NAME_PROFANE;
            }

            int spacesCount = 0;
            int apostrophesCount = 0;
            int capitalsCount = 0;
            int digitsCount = 0;
            int invalidcharsCount = 0;

            // go through character name and check for chars
            for (int i = 0; i < characterName.Length; i++)
            {
                var currentChar = characterName[i];
                if (!Char.IsLetter(currentChar))
                {
                    if (currentChar == '\'')
                    {
                        apostrophesCount++;
                    }
                    else if (currentChar == ' ')
                    {
                        spacesCount++;
                    }
                    else if (Char.IsDigit(currentChar))
                    {
                        digitsCount++;
                    }
                    else
                    {
                        invalidcharsCount++;
                    }
                }

                if (Char.IsUpper(currentChar))
                {
                    capitalsCount++;
                }
            }

            if (spacesCount > 0)
            {
                return LoginErrorCode.CHAR_NAME_INVALID_SPACE;
            }

            if (digitsCount > 0 || invalidcharsCount > 0)
            {
                return LoginErrorCode.CHAR_NAME_INVALID_CHARACTER;
            }

            if (apostrophesCount > 1)
            {
                // only 1 apostrophe allowed
                return LoginErrorCode.CHAR_NAME_MULTIPLE_APOSTROPHES;
            }

            // there is exactly one apostrophe
            if (apostrophesCount == 1)
            {
                int index = characterName.IndexOf("'");
                if (index == 0 || index == characterName.Length - 1)
                {
                    // you cannot use an apostrophe as the first or last char of your name
                    return LoginErrorCode.CHAR_NAME_INVALID_APOSTROPHE;
                }
            }
            // check for blizz like names flag
            if (RealmServerConfiguration.CapitalizeCharacterNames)
            {
                // do not check anything, just modify the name
                characterName = characterName.ToCapitalizedString();
            }

            return LoginErrorCode.RESPONSE_SUCCESS;
        }

        /// <summary>
        /// Checks if a character name contains any bad words based on configuration rules.
        /// </summary>
        /// <param name="characterName">the name of the character to check</param>
        /// <returns>true if the name contains a bad word; false otherwise</returns>
        public static bool DoesNameViolate(string characterName)
        {
            characterName = characterName.ToLower();

            return RealmServerConfiguration.BadWords.Where(word => characterName.Contains(word)).Any();
        }

        #endregion Naming

        #region Zones

        /// <summary>
        /// Handles an incoming zone update notification.
        /// But it only sends the current parent-zone and not the subzone.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_ZONEUPDATE)]
        public static void HandleZoneUpdate(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            var newZoneId = (ZoneId)packet.ReadUInt32();
            var oldZone = chr.Zone;
            var newZone = chr.Map.GetZone(chr.Position.X, chr.Position.Y);

            if (newZone == null)
            {
                if (chr.Map.MainZoneCount == 1)
                {
                    newZone = chr.Map.DefaultZone;
                }
                else
                {
                    newZone = chr.Map.GetZone(newZoneId);
                }
            }
            if (newZone != null)
            {
                chr.SetZone(newZone);
            }
        }

        public static void SendExplorationExperience(IPacketReceiver client, ZoneId zone, int xpAmount)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_EXPLORATION_EXPERIENCE, 8))
            {
                packet.Write((uint)zone);
                packet.Write(xpAmount);

                client.Send(packet);
            }
        }

        #endregion Zones

        #region Logout

        /// <summary>
        /// Handles an incomming immediate logout request.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_PLAYER_LOGOUT)]
        public static void PlayerImediateLogoutRequest(IRealmClient client, RealmPacketIn packet)
        {
            ReplyToLogout(client);
        }

        /// <summary>
        /// Handles an incoming logout request.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_LOGOUT_REQUEST)]
        public static void PlayerLogoutRequest(IRealmClient client, RealmPacketIn packet)
        {
            ReplyToLogout(client);
        }

        private static void ReplyToLogout(IRealmClient client)
        {
            var chr = client.ActiveCharacter;
            if (!chr.CanLogout)
            {
                SendLogoutReply(client, LogoutResponseCodes.LOGOUT_RESPONSE_DENIED);
            }
            else
            {
                chr.LogoutLater(false);
            }
        }

        /// <summary>
        /// Handles an incoming logout cancel request.
        /// </summary>
        /// <param name="client">the Session the incoming packet belongs to</param>
        /// <param name="packet">the full packet</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_LOGOUT_CANCEL, IsGamePacket = false, RequiresLogin = false)]
        public static void PlayerLogoutCancel(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (chr == null)
            {
                // ignore
                log.Debug("Client {0} sent LOGOUT_CANCEL after Logout.", client);
            }
            else if (chr.IsPlayerLogout)
            {
                chr.CancelLogout(true);
            }
        }

        /// <summary>
        /// Sends a "player immediate logout" reply to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <remarks>Sent by the server to acknowledge that the logout process is finalized.</remarks>
        /// <remarks>Sent in response to CMSG_LOGOUT.</remarks>
        public static void SendPlayerImmediateLogoutReply(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOGOUT_COMPLETE, 0))
            {
                client.Send(packet);
            }
        }

        /// <summary>
        /// Sends a logout response to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        /// <param name="error">the error code</param>
        public static void SendLogoutReply(IPacketReceiver client, LogoutResponseCodes error)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOGOUT_RESPONSE, 5))
            {
                packet.WriteUInt(0);
                packet.WriteByte((byte)error);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Sends a "logout cancel" reply to the client.
        /// </summary>
        /// <param name="client">the client to send to</param>
        public static void SendLogoutCancelReply(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOGOUT_CANCEL_ACK, 0))
            {
                client.Send(packet);
            }
        }

        #endregion Logout

        #region Summoning

        public static void SendSummonRequest(IPacketReceiver target, IEntity summoner, ZoneId zone, int decisionTimeout)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SUMMON_REQUEST, 16))
            {
                packet.Write(summoner.EntityId);
                packet.Write((uint)zone);
                packet.Write(decisionTimeout);

                target.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SUMMON_RESPONSE)]
        public static void HandleSummonResponse(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            var request = chr.SummonRequest;
            if (request != null)
            {
                if (request.ExpiryTime > DateTime.Now && chr.MayTeleport)
                {
                    var guid = packet.ReadEntityId();
                    var accept = packet.ReadBoolean();

                    if (accept)
                    {
                        chr.TeleportTo(request.TargetMap, request.TargetPos);
                        chr.Zone = request.TargetZone;
                    }
                }
                chr.CancelSummon(false);
                chr.m_summonRequest = null;
            }
        }

        public static void SendCancelSummonRequest(IPacketReceiver client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SUMMON_CANCEL, 0))
            {
                client.Send(packet);
            }
        }

        #endregion Summoning

        #region Helm/Cloak

        [ClientPacketHandler(RealmServerOpCode.CMSG_SHOWING_HELM)]
        public static void HandleHelmToggle(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (chr.PlayerFlags.HasFlag(PlayerFlags.HideHelm))
            {
                chr.PlayerFlags &= ~PlayerFlags.HideHelm;
                client.Account.ActiveCharacter.Record.CharacterFlags &= ~CharEnumFlags.HideHelm;
            }
            else
            {
                chr.PlayerFlags |= PlayerFlags.HideHelm;
                client.Account.ActiveCharacter.Record.CharacterFlags |= CharEnumFlags.HideHelm;
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_SHOWING_CLOAK)]
        public static void HandleCloakToggle(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;
            if (chr.PlayerFlags.HasFlag(PlayerFlags.HideCloak))
            {
                chr.PlayerFlags &= ~PlayerFlags.HideCloak;
                client.Account.ActiveCharacter.Record.CharacterFlags &= ~CharEnumFlags.HideCloak;
            }
            else
            {
                chr.PlayerFlags |= PlayerFlags.HideCloak;
                client.Account.ActiveCharacter.Record.CharacterFlags |= CharEnumFlags.HideCloak;
            }
        }

        #endregion Helm/Cloak

        #region WorldStateUI

        [ClientPacketHandler(RealmServerOpCode.CMSG_WORLD_STATE_UI_TIMER_UPDATE, RequiresLogin = false)]
        public static void HandleWorldStateUITimerUpdate(IRealmClient client, RealmPacketIn packet)
        {
            // CMSG_WORLD_STATE_UI_TIMER_UPDATE seemingly empty
            SendWorldStateUITimerUpdate(client);
        }

        public static void SendWorldStateUITimerUpdate(IRealmClient client)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_WORLD_STATE_UI_TIMER_UPDATE, 0))
            {
                packet.WriteDateTime(DateTime.Now); // unk?
                client.Send(packet);
            }
        }

        #endregion WorldStateUI

        #region Barbershops

        /// <summary>
        /// Tells the client that it's in a barbershop chair.
        /// </summary>
        /// <param name="character">The Character.</param>
        public static void SendEnableBarberShop(Character character)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_ENABLE_BARBER_SHOP))
            {
                // Empty, just notify.
                character.Client.Send(packet);
            }
        }

        /// <summary>
        /// Handles an incoming appearance alteration request from a barbershop.
        /// </summary>
        /// <param name="client">The IRealmClient.</param>
        /// <param name="packet">The packet.</param>
        [ClientPacketHandler(RealmServerOpCode.CMSG_ALTER_APPEARANCE)]
        public static void HandleAlterAppearance(IRealmClient client, RealmPacketIn packet)
        {
            var chr = client.ActiveCharacter;

            // It's a mystery to me why these aren't bytes in the packet.
            var reqStyle = (byte)packet.ReadUInt32();
            var reqColor = (byte)packet.ReadUInt32();
            var reqFacial = (byte)packet.ReadUInt32();

            // Check hair style.
            BarberShopStyleEntry style;
            NPCMgr.BarberShopStyles.TryGetValue(reqStyle, out style);
            if (style == null || style.Type != 0 || style.Race != chr.Race || style.Gender != chr.Gender)
                return;

            // Check facial hair.
            BarberShopStyleEntry facial;
            NPCMgr.BarberShopStyles.TryGetValue(reqFacial, out facial);
            if (facial == null || facial.Type != 2 || facial.Race != chr.Race || facial.Gender != chr.Gender)
                return;

            // TODO: No color check?

            // Get the cost.
            var cost = chr.CalcBarberShopCost(reqStyle, reqColor, reqFacial);

            if (chr.Money < cost)
            {
                SendBarberShopResult(chr, BarberShopPurchaseResult.LowMoney);
                return;
            }
            else
            {
                // We can continue.
                SendBarberShopResult(chr, BarberShopPurchaseResult.Success);
            }

            // Do changes.
            chr.SubtractMoney(cost);
            chr.HairStyle = (byte)style.HairId;
            chr.HairColor = reqColor;
            chr.FacialHair = (byte)facial.HairId;

            // Get up.
            chr.StandState = StandState.Stand;

            chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.GoldSpentAtBarber, cost);
            chr.Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.VisitBarberShop);
        }

        /// <summary>
        /// Sends the result of a barbershop purchase.
        /// </summary>
        /// <param name="chr">The Character.</param>
        /// <param name="ok">If true, purchase went through, if false, indicates not enough money.</param>
        public static void SendBarberShopResult(Character chr, BarberShopPurchaseResult result)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BARBER_SHOP_RESULT))
            {
                packet.Write((uint)result);
                chr.Client.Send(packet);
            }
        }

        #endregion Barbershops

        [ClientPacketHandler(RealmServerOpCode.CMSG_SET_ACTIVE_MOVER)]
        public static void HandleSetActiveMover(IRealmClient client, RealmPacketIn packet)
        {
            // TODO: Verify
            client.ActiveCharacter.MoveControl.Mover = client.ActiveCharacter.Map.GetObject(packet.ReadEntityId());

            SendTickQuery(client);
        }

        /// <summary>
        /// Sends SMSG_LOGIN_VERIFY_WORLD (first ingame packet, sends char-location: Seems unnecessary?)
        /// </summary>
        /// <param name="chr"></param>
        public static void SendVerifyWorld(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_LOGIN_VERIFY_WORLD, 20))
            {
                packet.Write((int)chr.Map.Id);
                packet.Write(chr.Position);
                packet.WriteFloat(chr.Orientation);

                chr.Client.Send(packet);
            }
        }

        public static void SendProficiency(IPacketReceiver client, ItemClass itemClass, ItemSubClassMask value)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SET_PROFICIENCY, 5))
            {
                packet.Write((byte)itemClass);
                packet.Write((uint)value);

                client.Send(packet);
            }
        }

        /// <summary>
        /// Triggers the introduction cinematic if the character is login for the first time
        /// </summary>
        public static void SendCinematic(Character chr)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_TRIGGER_CINEMATIC, 4))
            {
                packet.WriteUInt(chr.Archetype.Race.IntroductionMovie);
                chr.Client.Send(packet);
            }
        }

        /// <summary>
        /// Triggers the introduction cinematic if the character is login for the first time
        /// </summary>
        public static void SendPhaseShift(Character chr, uint phase)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SET_PHASE_SHIFT, 4))
            {
                packet.WriteUInt(phase);
                chr.Client.Send(packet);
            }
        }

        [ClientPacketHandler(RealmServerOpCode.CMSG_INSPECT)]
        public static void HandleInspect(IRealmClient client, RealmPacketIn packet)
        {
            var targetId = packet.ReadEntityId();

            if (client.ActiveCharacter.IsAlive)
            {
                var chr = client.ActiveCharacter.Map.GetObject(targetId) as Character;
                if (chr != null && client.ActiveCharacter.KnowsOf(chr))
                {
                    client.ActiveCharacter.Target = chr;
                    chr.AddObserver(client.ActiveCharacter);
                }
            }
        }

        public static void SendControlUpdate(IPacketReceiver rcvr, IEntity target, bool canControl)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_CLIENT_CONTROL_UPDATE, 9))
            {
                target.EntityId.WritePacked(packet);
                packet.Write(canControl);
                rcvr.Send(packet);
            }
        }

        public static void SendBindUpdate(Character chr, IWorldZoneLocation location)
        {
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_BINDPOINTUPDATE, 20))
            {
                packet.Write(location.Position.X);
                packet.Write(location.Position.Y);
                packet.Write(location.Position.Z);
                packet.Write((uint)location.MapId);
                packet.Write((uint)location.ZoneId);

                chr.Client.Send(packet);
            }
        }
    }
}