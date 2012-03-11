/*************************************************************************
 *
 *   file		: Character.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2010-02-20 06:16:32 +0100 (l�? 20 feb 2010) $

 *   revision		: $Rev: 1257 $
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
using WCell.Constants.Achievements;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core.Timers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Items;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Looting;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Modifiers;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.NPCs.Spawns;
using WCell.RealmServer.Quests;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.Util;
using WCell.Util.Commands;
using WCell.Util.Graphics;

namespace WCell.RealmServer.Entities
{
    ///<summary>
    /// Represents a unit controlled by a player in the game world
    ///</summary>
    public partial class Character : Unit, IUser, IContainer, ITicketHandler, IInstanceHolderSet, ICharacterSet
    {
        public static new readonly List<Character> EmptyArray = new List<Character>();

        #region Globals

        /// <summary>
        /// The delay until a normal player may logout in millis.
        /// </summary>
        public static int DefaultLogoutDelayMillis = 20000;

        /// <summary>
        /// Speed increase when dead and in Ghost form
        /// </summary>
        public static float DeathSpeedFactorIncrease = 0.25f;

        /// <summary>
        /// The level at which players start to suffer from repercussion after death
        /// </summary>
        public static int ResurrectionSicknessStartLevel = 10;

        /// <summary>
        /// whether to check for speedhackers
        /// </summary>
        public static bool SpeedHackCheck;

        /// <summary>
        /// The factor that is applied to the maximum distance before detecting someone as a SpeedHacker
        /// </summary>
        public static float SpeedHackToleranceFactor = 1.5f;

        #endregion Globals

        /// <summary>
        /// Clears all trade-related fields for the character.
        /// </summary>
        public void ClearTrade()
        {
        }

        public void UpdatePlayedTime()
        {
            var now = DateTime.Now;
            var timeDiff = now - m_lastPlayTimeUpdate;

            LevelPlayTime += (uint)timeDiff.TotalSeconds;
            TotalPlayTime += (uint)timeDiff.TotalSeconds;
            m_lastPlayTimeUpdate = now;
        }

        #region Properties

        /// <summary>
        /// Check to see if character is in an instance
        /// </summary>
        public bool IsInInstance
        {
            get
            {
                return m_Map != null && m_Map.IsInstance;
            }
        }

        /// <summary>
        /// Check to see if character is in a group
        /// </summary>
        public bool IsInGroup
        {
            get { return m_groupMember != null; }
        }

        /// <summary>
        /// Check to see if character is in a Guild
        /// </summary>
        public bool IsInGuild
        {
            get { return m_guildMember != null; }
        }

        /// <summary>
        /// Check to see if character is in a group
        /// </summary>
        public bool IsInRaid
        {
            get
            {
                return Group is RaidGroup;
            }
        }

        /// <summary>
        /// Check to see if character is in the same instance as group members
        /// </summary>
        public bool IsInGroupInstance
        {
            get
            {
                var group = Group;
                if (group != null)
                {
                    return group.GetActiveInstance(m_Map.MapTemplate) != null;
                }
                return false;
            }
        }

        /// <summary>
        /// Personal Dungeon Difficulty, might differ from current Difficulty
        /// </summary>
        public DungeonDifficulty DungeonDifficulty
        {
            get { return m_record.DungeonDifficulty; }
            set
            {
                m_record.DungeonDifficulty = value;
                if (m_groupMember == null)
                {
                    InstanceHandler.SendDungeonDifficulty(this);
                }
            }
        }

        public RaidDifficulty RaidDifficulty
        {
            get { return m_record.RaidDifficulty; }
            set
            {
                m_record.RaidDifficulty = value;
                if (m_groupMember == null)
                {
                    InstanceHandler.SendRaidDifficulty(this);
                }
            }
        }

        public bool IsAllowedLowLevelRaid
        {
            get { return PlayerFlags.HasFlag(PlayerFlags.AllowLowLevelRaid); }
            set
            {
                if (value)
                {
                    PlayerFlags |= PlayerFlags.AllowLowLevelRaid;
                    return;
                }
                PlayerFlags &= ~PlayerFlags.AllowLowLevelRaid;
            }
        }

        #endregion Properties

        public uint GetInstanceDifficulty(bool isRaid)
        {
            return m_groupMember == null ? (isRaid ? (uint)m_record.RaidDifficulty : (uint)m_record.DungeonDifficulty) : m_groupMember.Group.DungeonDifficulty;
        }

        #region Death/Resurrect

        public override bool IsAlive
        {
            get { return !(m_auras.GhostAura != null || Health == 0); }
        }

        /// <summary>
        /// whether the Corpse is reclaimable
        /// (Character must be ghost and the reclaim delay must have passed)
        /// </summary>
        public bool IsCorpseReclaimable
        {
            get
            {
                return IsGhost && DateTime.Now > m_record.LastResTime.AddMilliseconds(Corpse.MinReclaimDelay);
            }
        }

        /// <summary>
        /// Character can reclaim if Corpse is reclaimable and Character is close to Corpse,
        /// or if there is no Corpse, Character must be somewhere near a SpiritHealer
        /// </summary>
        public bool CanReclaimCorpse
        {
            get
            {
                return IsCorpseReclaimable && ((m_corpse != null && IsInRadiusSq(m_corpse, Corpse.ReclaimRadiusSq)) ||
                    // No Corpse but close to Spirithealer
                    (IsGhost && (m_corpse == null &&
                    KnownObjects.Contains(obj => obj is Unit && ((Unit)obj).IsSpiritHealer))));
            }
        }

        /// <summary>
        /// Last time this Character died
        /// </summary>
        public DateTime LastDeathTime
        {
            get { return m_record.LastDeathTime; }
            set { m_record.LastDeathTime = value; }
        }

        /// <summary>
        /// Last time this Character was resurrected
        /// </summary>
        public DateTime LastResTime
        {
            get { return m_record.LastResTime; }
            set { m_record.LastResTime = value; }
        }

        protected override bool OnBeforeDeath()
        {
            if (Health == 0)
            {
                // make sure, we have Health
                Health = 1;
            }

            if (!m_Map.MapTemplate.NotifyPlayerBeforeDeath(this))
            {
                return false;
            }

            if (IsDueling)
            {
                // cancel duel before IsAlive is false etc.
                Duel.OnDeath(this);
                return false;
            }
            return true;
        }

        protected override void OnDeath()
        {
            m_record.LastDeathTime = DateTime.Now;
            MarkDead();
            Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathAtMap, (uint)MapId, 1);
            Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.DeathInDungeon, (uint)MapId, 1);
            // start release timer
            m_corpseReleaseTimer = new TimerEntry(dt => ReleaseCorpse());
            m_corpseReleaseTimer.Start(Corpse.AutoReleaseDelay, 0);
        }

        internal protected override void OnResurrect()
        {
            base.OnResurrect();
            CorpseReleaseFlags &= ~CorpseReleaseFlags.ShowCorpseAutoReleaseTimer;

            if (m_corpse != null)
            {
                Corpse = null;
            }
            m_record.LastResTime = DateTime.Now;

            CharacterHandler.SendCorpseReclaimDelay(m_client, Corpse.MinReclaimDelay);

            if (m_Map != null)
            {
                m_Map.MapTemplate.NotifyPlayerResurrected(this);
            }
        }

        /// <summary>
        /// Resurrects, applies ResurrectionSickness and damages Items, if applicable
        /// </summary>
        public void ResurrectWithConsequences()
        {
            Resurrect();

            if (Level >= ResurrectionSicknessStartLevel)
            {
                // Apply resurrection sickness and durability loss (see http://www.wowwiki.com/Death)
                Auras.CreateSelf(SpellId.ResurrectionSickness, true);

                if (PlayerInventory.SHResDurabilityLossPct != 0)
                {
                    m_inventory.Iterate(item =>
                    {
                        if (item.MaxDurability > 0)
                        {
                            item.Durability = Math.Max(0, item.Durability -
                                (((item.Durability * PlayerInventory.SHResDurabilityLossPct) + 50) / 100));
                        }
                        return true;
                    });
                }
            }
        }

        /// <summary>
        /// Marks this Character dead (just died, Corpse not released)
        /// </summary>
        private void MarkDead()
        {
            CorpseReleaseFlags |= CorpseReleaseFlags.ShowCorpseAutoReleaseTimer;
            IncMechanicCount(SpellMechanic.Rooted);

            var healer = m_Map.GetNearestSpiritHealer(ref m_position);
            if (healer != null)
            {
                CharacterHandler.SendHealerPosition(m_client, healer);
            }
        }

        /// <summary>
        /// Characters become Ghosts after they released the Corpse
        /// </summary>
        private void BecomeGhost()
        {
            SpellCast.Start(SpellHandler.Get(SpellId.Ghost_2), true, this);
        }

        /// <summary>
        ///
        /// </summary>
        protected internal override void OnDamageAction(IDamageAction action)
        {
            base.OnDamageAction(action);

            if (action.Attacker != null)
            {
                // aggro pet and minions
                if (m_activePet != null)
                {
                    m_activePet.ThreatCollection.AddNewIfNotExisted(action.Attacker);
                }
                if (m_minions != null)
                {
                    foreach (var minion in m_minions)
                    {
                        minion.ThreatCollection.AddNewIfNotExisted(action.Attacker);
                    }
                }

                var pvp = action.Attacker.IsPvPing;
                var chr = action.Attacker.CharacterMaster;

                if (pvp && chr.IsInBattleground)
                {
                    // Add BG stats
                    var attackerStats = chr.Battlegrounds.Stats;
                    attackerStats.TotalDamage += action.ActualDamage;
                }
            }
        }

        protected override void OnKilled(IDamageAction action)
        {
            base.OnKilled(action);

            bool pvp;
            if (action.Attacker != null)
            {
                pvp = action.Attacker.IsPvPing;
                var chr = action.Attacker.CharacterMaster;

                if (pvp)
                {
                    if (chr.IsInBattleground)
                    {
                        // Add BG stats
                        var attackerStats = chr.Battlegrounds.Stats;
                        var victimStats = Battlegrounds.Stats;
                        attackerStats.KillingBlows++;
                        if (victimStats != null)
                        {
                            victimStats.Deaths++;
                        }
                    }
                    Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByPlayer, (uint)chr.FactionGroup);
                }
            }
            else
            {
                pvp = false;
            }

            if (!pvp)
            {
                // durability loss
                m_inventory.ApplyDurabilityLoss(PlayerInventory.DeathDurabilityLossPct);
                if (action.Attacker != null && action.Attacker is NPC)
                    Achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.KilledByCreature, (uint)((NPC)action.Attacker).Entry.NPCId);
            }

            m_Map.MapTemplate.NotifyPlayerDied(action);
        }

        /// <summary>
        /// Finds the item for the given slot. Unequips it if it may not currently be used.
        /// Returns the item to be equipped or null, if invalid.
        /// </summary>
        protected override IWeapon GetOrInvalidateItem(InventorySlotType type)
        {
            var slot = (int)ItemMgr.EquipmentSlotsByInvSlot[(int)type][0];
            var item = m_inventory[slot];
            if (item == null)
            {
                return null;
            }

            InventoryError err = InventoryError.OK;
            m_inventory.Equipment.CheckAdd(slot, 1, item, ref err);
            if (err == InventoryError.OK)
            {
                return item;
            }
            else
            {
                item.Unequip();
                return null;
            }
        }

        protected override void OnHeal(HealAction action)
        {
            base.OnHeal(action);
            var healer = action.Attacker;
            if (healer is Character)
            {
                var chr = (Character)healer;
                if (chr.IsInBattleground)
                {
                    chr.Battlegrounds.Stats.TotalHealing += action.Value;
                }
            }
        }

        /// <summary>
        /// Spawns the corpse and teleports the dead Character to the nearest SpiritHealer
        /// </summary>
        internal void ReleaseCorpse()
        {
            if (IsAlive)
            {
                return;
            }

            DecMechanicCount(SpellMechanic.Rooted);
            //ClearSelfKnowledge();
            BecomeGhost();

            Corpse = SpawnCorpse(false, false);
            m_record.CorpseX = m_corpse.Position.X;
            m_record.CorpseY = m_corpse.Position.Y;
            m_record.CorpseZ = m_corpse.Position.Z;
            m_record.CorpseO = m_corpse.Orientation;
            m_record.CorpseMap = m_Map.Id;		// we are spawning the corpse in the same map
            m_corpseReleaseTimer.Stop();

            // we need health to walk again
            SetUInt32(UnitFields.HEALTH, 1);

            m_Map.OnSpawnedCorpse(this);
        }

        /// <summary>
        /// Spawns and returns a new Corpse at the Character's current location
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="lootable"></param>
        /// <returns></returns>
        public Corpse SpawnCorpse(bool bones, bool lootable)
        {
            return SpawnCorpse(bones, lootable, m_Map, m_position, m_orientation);
        }

        /// <summary>
        /// Spawns and returns a new Corpse at the given location
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="lootable"></param>
        /// <returns></returns>
        public Corpse SpawnCorpse(bool bones, bool lootable, Map map, Vector3 pos, float o)
        {
            var corpse = new Corpse(this, pos, o, DisplayId, Facial, Skin,
                HairStyle, HairColor, FacialHair, GuildId, Gender, Race,
                bones ? CorpseFlags.Bones : CorpseFlags.None, lootable ? CorpseDynamicFlags.PlayerLootable : CorpseDynamicFlags.None);

            for (var i = EquipmentSlot.Head; i <= EquipmentSlot.Tabard; i++)
            {
                var item = m_inventory[(int)i];
                if (item != null)
                {
                    corpse.SetItem(i, item.Template);
                }
            }

            corpse.Position = pos;
            map.AddObjectLater(corpse);
            return corpse;
        }

        /// <summary>
        /// Tries to teleport to the next SpiritHealer, if there is any.
        ///
        /// TODO: Graveyards
        /// </summary>
        public void TeleportToNearestGraveyard()
        {
            TeleportToNearestGraveyard(true);
        }

        /// <summary>
        /// Tries to teleport to the next SpiritHealer, if there is any.
        ///
        /// TODO: Graveyards
        /// </summary>
        public void TeleportToNearestGraveyard(bool allowSameMap)
        {
            if (allowSameMap)
            {
                var healer = m_Map.GetNearestSpiritHealer(ref m_position);
                if (healer != null)
                {
                    TeleportTo(healer);
                    return;
                }
            }

            if (m_Map.MapTemplate.RepopMap != null)
            {
                TeleportTo(m_Map.MapTemplate.RepopMap, m_Map.MapTemplate.RepopPosition);
            }
            else
            {
                TeleportToBindLocation();
            }
        }

        #endregion Death/Resurrect

        #region Experience & Levels

        public LevelStatInfo ClassBaseStats
        {
            get { return m_archetype.GetLevelStats((uint)Level); }
        }

        internal void UpdateRest()
        {
            if (m_restTrigger != null)
            {
                var now = DateTime.Now;
                RestXp += RestGenerator.GetRestXp(now - m_lastRestUpdate, this);

                m_lastRestUpdate = now;
            }
        }

        /// <summary>
        /// Gain experience from combat
        /// </summary>
        public void GainCombatXp(int experience, INamed killed, bool gainRest)
        {
            if (Level >= MaxLevel)
            {
                return;
            }

            var xp = experience + (experience * KillExperienceGainModifierPercent / 100);

            if (m_activePet != null && m_activePet.MayGainExperience)
            {
                // give xp to pet
                m_activePet.PetExperience += xp;
                m_activePet.TryLevelUp();
            }

            if (gainRest && RestXp > 0)
            {
                // add rest bonus
                var bonus = Math.Min(RestXp, experience);
                xp += bonus;
                RestXp -= bonus;
                ChatMgr.SendCombatLogExperienceMessage(this, Locale, RealmLangKey.LogCombatExpRested, killed.Name, experience, bonus);
            }
            else
            {
                ChatMgr.SendCombatLogExperienceMessage(this, Locale, RealmLangKey.LogCombatExp, killed.Name, experience);
            }

            Experience += xp;
            TryLevelUp();
        }

        /// <summary>
        /// Gain non-combat experience (through quests etc)
        /// </summary>
        /// <param name="experience"></param>
        /// <param name="useRest">If true, subtracts the given amount of experience from RestXp and adds it ontop of the given xp</param>
        public void GainXp(int experience, bool useRest = false)
        {
            var xp = experience;
            if (useRest && RestXp > 0)
            {
                var bonus = Math.Min(RestXp, experience);
                xp += bonus;
                RestXp -= bonus;
            }

            Experience += xp;
            TryLevelUp();
        }

        internal bool TryLevelUp()
        {
            var level = Level;
            var xp = Experience;
            var nextLevelXp = NextLevelXP;
            var leveled = false;

            while (xp >= nextLevelXp && level < MaxLevel)
            {
                ++level;
                xp -= nextLevelXp;
                nextLevelXp = XpGenerator.GetXpForlevel(level + 1);
                leveled = true;
            }

            if (leveled)
            {
                Experience = xp;
                NextLevelXP = nextLevelXp;
                Level = level;
                return true;
            }
            return false;
        }

        protected override void OnLevelChanged()
        {
            base.OnLevelChanged();

            //check if we unlocked new glyphslots on every levelup!
            InitGlyphsForLevel();

            var level = Level;
            int freeTalentPoints = m_talents.GetFreeTalentPointsForLevel(level);
            if (freeTalentPoints < 0)
            {
                // need to remove talent points
                if (!GodMode)
                {
                    // remove the extra talents
                    m_talents.RemoveTalents(-freeTalentPoints);
                }
                freeTalentPoints = 0;
            }

            // check pet level
            if (m_activePet != null)
            {
                if (!m_activePet.IsHunterPet || m_activePet.Level > level)
                {
                    m_activePet.Level = level;
                }
                else if (level - PetMgr.MaxHunterPetLevelDifference > m_activePet.Level)
                {
                    m_activePet.Level = level - PetMgr.MaxHunterPetLevelDifference;
                }
            }

            FreeTalentPoints = freeTalentPoints;
            ModStatsForLevel(level);
            m_auras.ReapplyAllAuras();
            m_achievements.CheckPossibleAchievementUpdates(AchievementCriteriaType.ReachLevel, (uint)Level);

            // update level-depedent stats
            this.UpdateDodgeChance();
            this.UpdateBlockChance();
            this.UpdateCritChance();
            this.UpdateAllAttackPower();

            var evt = LevelChanged;
            if (evt != null)
            {
                evt(this);
            }

            SaveLater();
        }

        public void ModStatsForLevel(int level)
        {
            var lvlClassSetting = m_archetype.Class.GetLevelSetting(level);
            var lvlStats = m_archetype.GetLevelStats((uint)level);

            var oldPower = BasePower;
            var oldHealth = BaseHealth;
            var oldStrength = Strength;
            var oldAgility = Agility;
            var oldStamina = Stamina;
            var oldIntellect = Intellect;
            var oldSpirit = Spirit;

            SetBaseStat(StatType.Strength, lvlStats.Strength);
            SetBaseStat(StatType.Agility, lvlStats.Agility);
            SetBaseStat(StatType.Stamina, lvlStats.Stamina);
            SetBaseStat(StatType.Intellect, lvlStats.Intellect);
            SetBaseStat(StatType.Spirit, lvlStats.Spirit);

            if (PowerType == PowerType.Mana)
            {
                BasePower = lvlClassSetting.Mana;
            }

            BaseHealth = lvlClassSetting.Health;

            SetInt32(UnitFields.HEALTH, MaxHealth);

            UpdatePlayedTime();
            LevelPlayTime = 0;

            // includes boni through stat-changes
            var healthGain = (BaseHealth - oldHealth);
            var powerGain = (BasePower - oldPower);
            var strGain = lvlStats.Strength - oldStrength;
            var agiGain = lvlStats.Agility - oldAgility;
            var staGain = lvlStats.Stamina - oldStamina;
            var intGain = lvlStats.Intellect - oldIntellect;
            var spiGain = lvlStats.Spirit - oldSpirit;

            CharacterHandler.SendLevelUpInfo(Client, level, healthGain, powerGain, strGain, agiGain, staGain,
                                             intGain, spiGain);

            //skills
            Skills.UpdateSkillsForLevel(level);
        }

        #endregion Experience & Levels

        #region Chat

        /// <summary>
        /// Adds the given language
        /// </summary>
        public void AddLanguage(ChatLanguage lang)
        {
            var desc = LanguageHandler.GetLanguageDescByType(lang);
            AddLanguage(desc);
        }

        public void AddLanguage(LanguageDescription desc)
        {
            if (!Spells.Contains((uint)desc.SpellId))
            {
                Spells.AddSpell(desc.SpellId);
            }

            if (!m_skills.Contains(desc.SkillId))
            {
                m_skills.Add(desc.SkillId, 300, 300, true);
            }
        }

        /// <summary>
        /// Returns whether the given language can be understood by this Character
        /// </summary>
        public bool CanSpeak(ChatLanguage language)
        {
            return KnownLanguages.Contains(language);
        }

        public void SendMessage(string message)
        {
            ChatMgr.SendSystemMessage(this, message);
            ChatMgr.ChatNotify(null, message, ChatLanguage.Universal, ChatMsgType.System, this);
        }

        public void SendMessage(IChatter sender, string message)
        {
            ChatMgr.SendWhisper(sender, this, message);
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        public void SendSystemMessage(RealmLangKey key, params object[] args)
        {
            ChatMgr.SendSystemMessage(this, RealmLocalizer.Instance.Translate(Locale, key, args));
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        public void SendSystemMessage(string msg)
        {
            ChatMgr.SendSystemMessage(this, msg);
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        public void SendSystemMessage(string msgFormat, params object[] args)
        {
            ChatMgr.SendSystemMessage(this, string.Format(msgFormat, args));
        }

        public void Notify(RealmLangKey key, params object[] args)
        {
            Notify(RealmLocalizer.Instance.Translate(Locale, key, args));
        }

        /// <summary>
        /// Flashes a notification in the middle of the screen
        /// </summary>
        public void Notify(string msg, params object[] args)
        {
            MiscHandler.SendNotification(this, string.Format(msg, args));
        }

        public void SayGroup(string msg)
        {
            this.SayGroup(SpokenLanguage, msg);
        }

        public void SayGroup(string msg, params object[] args)
        {
            SayGroup(string.Format(msg, args));
        }

        public override void Say(float radius, string msg)
        {
            this.SayYellEmote(ChatMsgType.Say, SpokenLanguage, msg, radius);
        }

        public override void Yell(float radius, string msg)
        {
            this.SayYellEmote(ChatMsgType.Yell, SpokenLanguage, msg, radius);
        }

        public override void Emote(float radius, string msg)
        {
            this.SayYellEmote(ChatMsgType.Emote, SpokenLanguage, msg, radius);
        }

        #endregion Chat

        #region Interaction with NPCs & GameObjects

        /// <summary>
        /// Called whenever this Character interacts with any WorldObject
        /// </summary>
        /// <param name="obj"></param>
        public void OnInteract(WorldObject obj)
        {
            //Dismount();
            StandState = StandState.Stand;

            if (obj is NPC)
            {
                var npc = (NPC)obj;
                Reputations.OnTalkWith(npc);
                npc.Entry.NotifyInteracting(npc, this);
            }
        }

        /// <summary>
        /// Opens this character's bankbox
        /// </summary>
        public void OpenBank(WorldObject banker)
        {
            OnInteract(banker);
            m_inventory.CurrentBanker = banker;
            using (var packet = new RealmPacketOut(RealmServerOpCode.SMSG_SHOW_BANK, 8))
            {
                packet.Write(banker.EntityId);
                m_client.Send(packet);
            }
        }

        /// <summary>
        /// Tries to bind this Character to the given NPC.
        /// </summary>
        /// <returns>whether the given NPC is an actual InnKeeper and this char could be bound to that Inn.</returns>
        public bool TryBindTo(NPC innKeeper)
        {
            OnInteract(innKeeper);
            if (innKeeper.BindPoint != NamedWorldZoneLocation.Zero && innKeeper.CheckVendorInteraction(this))
            {
                BindTo(innKeeper);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Binds this Character to that Location and will teleport him/her whenever the Hearthston is used.
        /// Adds a new HearthStone if the Character doesn't have one.
        /// Make sure that the given NPC is an actual InnKeeper and has a valid BindPoint (else use <c>TryBindTo</c> instead).
        /// </summary>
        public void BindTo(NPC binder)
        {
            m_inventory.EnsureHearthStone();
            BindTo(binder, binder.BindPoint);
        }

        public void BindTo(WorldObject binder, IWorldZoneLocation location)
        {
            m_bindLocation = location;
            // NPCHandler.SendBindConfirm(this, innKeeper, innKeeper.BindPoint.Zone);
            CharacterHandler.SendBindUpdate(this, location);
            NPCHandler.SendPlayerBound(this, binder, location.ZoneId);
        }

        #endregion Interaction with NPCs & GameObjects

        #region Quests

        /// <summary>
        /// QuestLog of the character
        /// </summary>
        private QuestLog m_questLog;

        /// <summary>
        /// Gets the quest log.
        /// </summary>
        /// <value>The quest log.</value>
        public QuestLog QuestLog
        {
            get { return m_questLog; }
        }

        #endregion Quests

        #region Combat

        /// <summary>
        /// The <see cref="Duel"/> this Character is currently engaged in (or null if not dueling)
        /// </summary>
        public Duel Duel
        {
            get;
            internal set;
        }

        /// <summary>
        /// The opponent that this Character is currently dueling with (or null if not dueling)
        /// </summary>
        public Character DuelOpponent
        {
            get;
            internal set;
        }

        /// <summary>
        /// whether this Character is currently dueling with someone else
        /// </summary>
        public bool IsDueling
        {
            get { return Duel != null && Duel.IsActive; }
        }

        public override bool IsFriendlyWith(IFactionMember opponent)
        {
            if (IsAlliedWith(opponent))
            {
                return true;
            }

            var opFaction = opponent.Faction;
            var rep = m_reputations[opFaction.ReputationIndex];
            if (rep != null)
            {
                return rep.Standing >= Standing.Friendly;
            }
            return m_faction.IsFriendlyTowards(opFaction);
        }

        public override bool IsAtLeastNeutralWith(IFactionMember opponent)
        {
            if (IsFriendlyWith(opponent))
            {
                return true;
            }

            var opFaction = opponent.Faction;
            var rep = m_reputations[opFaction.ReputationIndex];
            if (rep != null)
            {
                return rep.Standing >= Standing.Neutral;
            }
            return m_faction.Neutrals.Contains(opFaction);
        }

        public override bool IsHostileWith(IFactionMember opponent)
        {
            if (ReferenceEquals(opponent, this) || (opponent is Unit && ((Unit)opponent).Master == this))
            {
                return false;
            }

            if (opponent is Character)
            {
                return CanPvP((Character)opponent);
            }

            var opFaction = opponent.Faction;

            if (opponent is NPC && opFaction.Neutrals.Contains(m_faction))
            {
                return ((NPC)opponent).ThreatCollection.HasAggressor(this);
            }

            if (m_faction.Friends.Contains(opFaction))
                return false;

            return m_faction.Enemies.Contains(opFaction) && m_reputations.CanAttack(opFaction);
        }

        public override bool MayAttack(IFactionMember opponent)
        {
            if (ReferenceEquals(opponent, this) || (opponent is Unit && ((Unit)opponent).Master == this))
            {
                return false;
            }

            if (opponent is Character)
            {
                return CanPvP((Character)opponent);
            }

            var opFaction = opponent.Faction;
            return m_faction.Enemies.Contains(opFaction) || (!m_faction.Friends.Contains(opFaction) && m_reputations.CanAttack(opFaction));
        }

        public bool CanPvP(Character chr)
        {
            var state = chr.PvPState < PvPState ? chr.PvPState : PvPState;

            if (state == PvPState.FFAPVP)
            {
                return true;
            }

            return
                (state == PvPState.PVP && chr.Faction.IsAlliance != m_faction.IsAlliance) ||					// world pvp
                (IsInBattleground && chr.IsInBattleground && chr.Battlegrounds.Team != Battlegrounds.Team) ||	// battlegrounds
                (DuelOpponent == chr && Duel.IsActive);															// duels
        }

        /// <summary>
        /// One can only cast beneficial spells on people that we are allied with
        /// </summary>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public override bool IsAlliedWith(IFactionMember opponent)
        {
            if (ReferenceEquals(opponent, this) ||
                (opponent is Unit && ((Unit)opponent).Master == this))
            {
                return true;
            }

            if (!(opponent is Character) && opponent is WorldObject)
            {
                opponent = ((WorldObject)opponent).Master;
            }

            if (opponent is Character)
            {
                if (IsInBattleground)
                {
                    return Battlegrounds.Team == ((Character)opponent).Battlegrounds.Team;
                }

                var group = Group;
                if (group != null && ((Character)opponent).Group == group)
                {
                    // cannot ally with duelists
                    return DuelOpponent == null && ((Character)opponent).DuelOpponent == null;
                }
            }

            return false;
        }

        public override bool IsInSameDivision(IFactionMember opponent)
        {
            if (ReferenceEquals(opponent, this) ||
                (opponent is Unit && ((Unit)opponent).Master == this))
            {
                return true;
            }

            if (!(opponent is Character) && opponent is WorldObject)
            {
                opponent = ((WorldObject)opponent).Master;
            }

            if (opponent is Character)
            {
                if (IsInBattleground)
                {
                    return Battlegrounds.Team == ((Character)opponent).Battlegrounds.Team;
                }

                var group = SubGroup;
                if (group != null && ((Character)opponent).SubGroup == group)
                {
                    // cannot ally with duelists
                    return DuelOpponent == null && ((Character)opponent).DuelOpponent == null;
                }
            }
            return false;
        }

        public override void OnAttack(Misc.DamageAction action)
        {
            if (action.Victim is NPC && m_dmgBonusVsCreatureTypePct != null)
            {
                var bonus = m_dmgBonusVsCreatureTypePct[(int)((NPC)action.Victim).Entry.Type];
                if (bonus != 0)
                {
                    action.Damage += (bonus * action.Damage + 50) / 100;
                }
            }
            base.OnAttack(action);
        }

        protected override void OnEnterCombat()
        {
            CancelLooting();
            CancelLogout();
        }

        protected override bool CheckCombatState()
        {
            if (!m_isFighting)
            {
                if (NPCAttackerCount == 0 &&
                    (m_activePet == null || m_activePet.NPCAttackerCount == 0) &&
                    !m_auras.HasHarmfulAura())
                {
                    if (m_minions != null)
                    {
                        // can't leave combat if any minion is in combat
                        foreach (var minion in m_minions)
                        {
                            if (minion.NPCAttackerCount > 0)
                            {
                                return base.CheckCombatState();
                            }
                        }
                    }
                    // leave combat if we didn't fight for a while and have no debuffs on us
                    IsInCombat = false;
                }
                return false;
            }
            return base.CheckCombatState();
        }

        public override int AddHealingModsToAction(int healValue, SpellEffect effect, DamageSchool school)
        {
            healValue += (int)((healValue * HealingDoneModPct) / 100f);
            healValue += HealingDoneMod;
            if (effect != null)
            {
                healValue = Auras.GetModifiedInt(SpellModifierType.SpellPower, effect.Spell, healValue);
            }

            return healValue;
        }

        public override int GetGeneratedThreat(int dmg, DamageSchool school, SpellEffect effect)
        {
            var threat = base.GetGeneratedThreat(dmg, school, effect);
            if (effect != null)
            {
                threat = Auras.GetModifiedInt(SpellModifierType.Threat, effect.Spell, threat);
            }
            return threat;
        }

        public override float CalcCritDamage(float dmg, Unit victim, SpellEffect effect)
        {
            dmg = base.CalcCritDamage(dmg, victim, effect);
            if (effect != null)
            {
                return Auras.GetModifiedFloat(SpellModifierType.CritDamage, effect.Spell, dmg);
            }
            return dmg;
        }

        /// <summary>
        /// Change target and/or amount of combo points
        /// </summary>
        public override bool ModComboState(Unit target, int amount)
        {
            if (base.ModComboState(target, amount))
            {
                CombatHandler.SendComboPoints(this);
                return true;
            }

            return false;
        }

        #endregion Combat

        #region Looting

        /// <summary>
        /// Whether this Character will automatically pass on loot rolls.
        /// </summary>
        public bool PassOnLoot
        {
            get;
            set;
        }

        /// <summary>
        /// The LooterEntry represents this Character's current loot status
        /// </summary>
        public LooterEntry LooterEntry
        {
            get
            {
                if (m_looterEntry == null)
                {
                    m_looterEntry = new LooterEntry(this);
                }
                return m_looterEntry;
            }
        }

        /// <summary>
        /// whether this Character is currently looting something
        /// </summary>
        public bool IsLooting
        {
            get
            {
                return m_looterEntry != null && m_looterEntry.Loot != null;
            }
        }

        /// <summary>
        /// Cancels looting (if this Character is currently looting something)
        /// </summary>
        public void CancelLooting()
        {
            if (m_looterEntry != null)
            {
                m_looterEntry.Release();
            }
        }

        #endregion Looting

        #region Summoning

        public SummonRequest SummonRequest
        {
            get
            {
                return m_summonRequest;
            }
        }

        /// <summary>
        /// May be executed from outside of this Character's map's context
        /// </summary>
        public void StartSummon(ISummoner summoner)
        {
            StartSummon(summoner, SummonRequest.DefaultTimeout);
        }

        /// <summary>
        /// May be executed from outside of this Character's map's context
        /// </summary>
        /// <param name="summoner"></param>
        /// <param name="timeoutSeconds"></param>
        public void StartSummon(ISummoner summoner, int timeoutSeconds)
        {
            m_summonRequest = new SummonRequest
            {
                ExpiryTime = DateTime.Now.AddSeconds(timeoutSeconds),
                TargetPos = summoner.Position,
                TargetZone = summoner.Zone,
                TargetMap = summoner.Map
            };

            // make sure the Map was set or else the summoner was disposed before the Request completed
            if (m_summonRequest.TargetMap != null)
            {
                var client = m_client;
                if (client != null)
                {
                    CharacterHandler.SendSummonRequest(client, summoner,
                        summoner.Zone != null ? summoner.ZoneTemplate.Id : ZoneId.None,
                        timeoutSeconds * 1000);
                }
            }
            else
            {
                //log.Warn("Tried to teleport {0} to a Summoner without a Map: {1}", this, summoner);
            }
        }

        /// <summary>
        /// Cancels a current summon request
        /// </summary>
        public void CancelSummon(bool notify)
        {
            if (m_summonRequest != null)
            {
                if (m_summonRequest.Portal != null && m_summonRequest.Portal.IsInWorld)
                {
                    m_summonRequest.Portal.Delete();
                }
                if (notify)
                {
                    CharacterHandler.SendCancelSummonRequest(this);
                }
                m_summonRequest = null;
            }
        }

        #endregion Summoning

        public override int GetBasePowerRegen()
        {
            return RegenerationFormulas.GetPowerRegen(this);
        }

        public void ActivateAllTaxiNodes()
        {
            for (var i = 0; i < TaxiMgr.PathNodesById.Length; i++)
            {
                var node = TaxiMgr.PathNodesById[i];
                if (node != null)
                {
                    TaxiNodes.Activate(node);
                    SendSystemMessage("Activated Node: " + node);
                }
            }
        }

        public override void SetZone(Zone newZone)
        {
            base.SetZone(newZone);
            if (newZone != null)
            {
                m_Map.CallDelayed(CharacterHandler.ZoneUpdateDelayMillis, () =>
                {
                    if (IsInWorld && Zone == newZone)
                    {
                        SetZoneExplored(m_zone.Template, true);
                    }
                });
            }
        }

        public override void CancelAllActions()
        {
            base.CancelAllActions();
            if (m_target != null)
            {
                ClearTarget();
            }
            if (TradeWindow != null)
            {
                TradeWindow.Cancel();
            }
        }

        public void ClearTarget()
        {
            CharacterHandler.SendClearTarget(this, m_target);
            Target = null;
        }

        #region Spells

        public override int GetPowerCost(DamageSchool school, Spell spell, int cost)
        {
            cost = base.GetPowerCost(school, spell, cost);
            cost = Auras.GetModifiedInt(SpellModifierType.PowerCost, spell, cost);
            return cost;
        }

        #endregion Spells

        #region Talent Specs / Glyphs

        public SpecProfile CurrentSpecProfile
        {
            get { return SpecProfiles[m_talents.CurrentSpecIndex]; }
        }

        /// <summary>
        /// Talent specs
        /// </summary>
        public SpecProfile[] SpecProfiles
        {
            get;
            protected internal set;
        }

        public void ApplyTalentSpec(int no)
        {
            var profile = SpecProfiles.Get(no);

            if (profile != null)
            {
                // TODO: Change talent spec
            }
        }

        public void InitGlyphsForLevel()
        {
            foreach (var slot in GlyphInfoHolder.GlyphSlots)
            {
                if (slot.Value.Order != 0)
                {
                    SetGlyphSlot((byte)(slot.Value.Order - 1), slot.Value.Id);
                }
            }

            var level = Level;
            uint value = 0;

            if (level >= 15)
                value |= (0x01 | 0x02);
            if (level >= 30)
                value |= 0x08;
            if (level >= 50)
                value |= 0x04;
            if (level >= 70)
                value |= 0x10;
            if (level >= 80)
                value |= 0x20;

            Glyphs_Enable = value;
        }

        public void ApplyGlyph(byte slot, GlyphPropertiesEntry gp)
        {
            //check if there is a already a glyph in there and remove it
            RemoveGlyph(slot);

            //slap in the new one
            SpellCast.Trigger(SpellHandler.Get(gp.SpellId), this);
            SetGlyph(slot, gp.Id);
            CurrentSpecProfile.GlyphIds[slot] = gp.Id;
            TalentHandler.SendTalentGroupList(m_talents);

            //Todo: save it somewhere and dualspec related things!
        }

        public void RemoveGlyph(byte slot)
        {
            var oldglyph = GetGlyph(slot);

            if (oldglyph != 0)
            {
                var spelltoremove = GlyphInfoHolder.GetPropertiesEntryForGlyph(oldglyph).SpellId;
                Auras.Remove(SpellHandler.Get(spelltoremove));
                CurrentSpecProfile.GlyphIds[slot] = 0;
                SetGlyph(slot, 0);
            }
        }

        #endregion Talent Specs / Glyphs

        #region Implementation of IInstanceHolder

        public Character InstanceLeader
        {
            get { return this; }
        }

        public InstanceCollection InstanceLeaderCollection
        {
            get { return Instances; }
        }

        public bool HasInstanceCollection
        {
            get { return m_InstanceCollection != null; }
        }

        /// <summary>
        /// Auto-created if not already existing
        /// </summary>
        public InstanceCollection Instances
        {
            get
            {
                if (m_InstanceCollection == null)
                {
                    m_InstanceCollection = new InstanceCollection(this);
                }
                return m_InstanceCollection;
            }
            set { m_InstanceCollection = value; }
        }

        public void ForeachInstanceHolder(Action<InstanceCollection> callback)
        {
            callback(Instances);
        }

        public BaseInstance GetActiveInstance(MapTemplate mapTemplate)
        {
            var map = m_Map;
            if (map != null && map.Id == map.Id)
            {
                return map as BaseInstance;
            }
            var instances = m_InstanceCollection;
            return instances != null ? instances.GetActiveInstance(mapTemplate) : null;
        }

        #endregion Implementation of IInstanceHolder

        #region Battlegrounds

        /// <summary>
        /// Whether this Character is in a Battleground at the moment
        /// </summary>
        public bool IsInBattleground
        {
            get { return m_bgInfo != null && m_bgInfo.Team != null; }
        }

        /// <summary>
        /// Represents all <see cref="Battleground"/>-related information of this Character
        /// </summary>
        public BattlegroundInfo Battlegrounds
        {
            get
            {
                if (m_bgInfo == null)
                {
                    m_bgInfo = new BattlegroundInfo(this);
                }
                return m_bgInfo;
            }
        }

        #endregion Battlegrounds

        #region Honor

        /// <summary>
        /// Is called when the Character kills an Honorable target.
        /// </summary>
        /// <param name="victim">The Honorable character killed.</param>
        internal void OnHonorableKill(IDamageAction action)
        {
            var victim = (Character)action.Victim;
            var ptsForKill = CalcHonorForKill(victim);
            if (ptsForKill == 0) return;

            if (IsInBattleground)
            {
                var team = m_bgInfo.Team;
                var victimStats = victim.Battlegrounds.Stats;

                if (team == victim.Battlegrounds.Team ||
                    (victimStats == null || victimStats.Deaths > BattlegroundMgr.MaxHonorableDeaths))
                {
                    // don't count kills of town team, visitors or permanent losers
                    return;
                }

                var stats = m_bgInfo.Stats;
                ++stats.HonorableKills;
                team.DistributeSharedHonor(this, victim, ptsForKill);
            }
            else if (Group != null)
            {
                if (Faction.Group == victim.Faction.Group) return;
                Group.DistributeGroupHonor(this, victim, ptsForKill);
            }
            else
            {
                GiveHonorPoints(ptsForKill);
                KillsToday++;
                LifetimeHonorableKills++;
                HonorHandler.SendPVPCredit(this, ptsForKill * 10, victim);
            }

            if (m_zone != null)
            {
                m_zone.Template.OnHonorableKill(this, victim);
            }
        }

        private uint CalcHonorForKill(Character victim)
        {
            if (victim == this) return 0;
            if (!victim.YieldsXpOrHonor) return 0;

            var kLvl = Level;
            var vLvl = victim.Level;
            var maxLvlDiff = BattlegroundMgr.MaxLvlDiff;
            var maxHonor = (BattlegroundMgr.MaxHonor - 1);
            if (maxHonor < 0)
            {
                maxHonor = 0;
            }

            var lvlDiff = kLvl - vLvl;
            lvlDiff += maxLvlDiff;
            if (lvlDiff < 0) return 0;

            var slope = (maxHonor / (2.0f * maxLvlDiff));
            return (uint)Math.Round((slope * lvlDiff) + 1);
        }

        public void GiveHonorPoints(uint points)
        {
            HonorPoints += points;
            HonorToday += points;
        }

        public uint MaxPersonalArenaRating
        {
            get { return 0; }
        }

        #endregion Honor

        #region PvP Flag

        /// <summary>
        /// Auto removes PvP flag after expiring
        /// </summary>
        protected TimerEntry PvPEndTime;

        public void TogglePvPFlag()
        {
            SetPvPFlag(!PlayerFlags.HasFlag(PlayerFlags.PVP));
        }

        public void SetPvPFlag(bool state)
        {
            if (state)
            {
                // if the pvp timer is set, override the pvp state to on
                UpdatePvPState(true, (PvPEndTime != null && PvPEndTime.IsRunning));
                PlayerFlags |= PlayerFlags.PVP;
                return;
            }

            // The flag is down and the character has been pvp-ing in a non-hostile area
            // Set the timer to turn things off
            if (Zone != null)
            {
                if (!Zone.Template.IsHostileTo(this) && PvPState.HasFlag(PvPState.PVP))
                {
                    SetPvPResetTimer();
                }
            }
        }

        public void UpdatePvPState(bool state, bool overridden = false)
        {
            if (!state || overridden)
            {
                SetPvPState(state);
                ClearPvPResetTimer();
                return;
            }

            // Flag is up. Check for running reset timer.
            // If running, reset it.
            if (PvPEndTime != null && PvPEndTime.IsRunning)
            {
                SetPvPResetTimer(true);
                return;
            }

            // Flag is up. No running reset timer.
            SetPvPState(true);
        }

        private void SetPvPResetTimer(bool overridden = false)
        {
            if (PvPEndTime == null)
                PvPEndTime = new TimerEntry(dt => OnPvPTimerEnded());

            if (!PvPEndTime.IsRunning || overridden)
                PvPEndTime.Start(300000);

            IsPvPTimerActive = true;
        }

        private void ClearPvPResetTimer()
        {
            if (PvPEndTime != null)
                PvPEndTime.Stop();

            IsPvPTimerActive = false;
        }

        private void OnPvPTimerEnded()
        {
            PlayerFlags &= ~PlayerFlags.PVP;
            IsPvPTimerActive = false;
            SetPvPState(false);
        }

        private void SetPvPState(bool state)
        {
            // TODO: minions?

            if (ActivePet != null)
            {
                if (state)
                {
                    PvPState = PvPState.PVP;
                    ActivePet.PvPState = PvPState.PVP;
                }
                else
                {
                    PvPState &= ~PvPState.PVP;
                    ActivePet.PvPState &= ~PvPState.PVP;
                }
                return;
            }

            if (state)
            {
                PvPState = PvPState.PVP;
                return;
            }

            PvPState &= ~PvPState.PVP;
        }

        #endregion PvP Flag

        #region Barbershops

        /// <summary>
        /// Calculates the price of a purchase in a berber shop.
        /// </summary>
        /// <param name="newstyle"></param>
        /// <param name="newcolor"></param>
        /// <param name="newfacial"></param>
        /// <returns>The total price.</returns>
        public uint CalcBarberShopCost(byte newStyle, byte newColor, byte newFacial)
        {
            var level = Level;
            var style = HairStyle;
            var color = HairColor;
            var facial = FacialHair;

            // Happens if character chooses same style as before.
            if ((style == newStyle) && (color == newColor) && (facial == newFacial))
                return 0;

            var price = GameTables.BarberShopCosts[level - 1];

            // Should not happen?
            if (price == 0)
                return 0xFFFFFFFF;

            float cost = 0;

            if (style != newStyle)
            {
                cost += price;
            }
            else if (color != newColor)
            {
                cost += price * 0.5f;
            }

            if (facial != newFacial)
            {
                cost += price * 0.75f;
            }

            // We send in uint.
            return (uint)cost;
        }

        #endregion Barbershops

        #region IUser

        public BaseCommand<RealmServerCmdArgs> SelectedCommand
        {
            get
            {
                var info = m_ExtraInfo;
                if (info != null)
                {
                    return info.m_selectedCommand;
                }
                return null;
            }
            set
            {
                var info = m_ExtraInfo;
                if (info != null)
                {
                    info.m_selectedCommand = value;
                }
            }
        }

        #endregion IUser

        #region AI

        public override LinkedList<WaypointEntry> Waypoints
        {
            get { return null; }
        }

        public override NPCSpawnPoint SpawnPoint
        {
            get { return null; }
        }

        #endregion AI

        #region ITicketHandler

        /// <summary>
        /// The ticket that is currently being handled by this <see cref="ITicketHandler"/>
        /// </summary>
        public Ticket HandlingTicket
        {
            get
            {
                var info = m_ExtraInfo;
                if (info != null)
                {
                    return info.m_handlingTicket;
                }
                return null;
            }
            set
            {
                var info = ExtraInfo;
                if (info != null)
                {
                    info.m_handlingTicket = value;
                }
            }
        }

        public bool MayHandle(Ticket ticket)
        {
            var handler = ticket.m_handler;
            return handler == null || handler.Role <= Role;
        }

        #endregion ITicketHandler

        #region ICharacterSet

        public int CharacterCount
        {
            get { return 1; }
        }

        public void ForeachCharacter(Action<Character> callback)
        {
            callback(this);
        }

        public Character[] GetAllCharacters()
        {
            return new[] { this };
        }

        #endregion ICharacterSet

        public void Send(RealmPacketOut packet)
        {
            m_client.Send(packet);
        }

        public void Send(byte[] packet)
        {
            m_client.Send(packet);
        }

        public override string ToString()
        {
            return Name + " (ID: " + EntityId + ", Account: " + Account + ")";
        }
    }
}