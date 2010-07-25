using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Graphics;
using WCell.Util.Threading;
using WCell.Core.Timers;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Commands;
using WCell.RealmServer.Database;
using WCell.RealmServer.Debugging;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Formulas;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Help.Tickets;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Items;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Network;
using WCell.RealmServer.Quests;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Skills;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Talents;
using WCell.RealmServer.Taxi;
using WCell.RealmServer.Modifiers;
using WCell.Util.NLog;
using WCell.Util;
using WCell.RealmServer.Battlegrounds;
using Castle.ActiveRecord;

namespace WCell.RealmServer.Entities
{
	public partial class Character
	{
		#region Creation

		/// <summary>
		/// Creates a new character and loads all required character data from the database
		/// </summary>
		/// <param name="acc">The account the character is associated with</param>
		/// <param name="record">The name of the character to load</param>
		/// <param name="client">The client to associate with this character</param>
		internal protected void Create(RealmAccount acc, CharacterRecord record, IRealmClient client)
		{
			client.ActiveCharacter = this;
			acc.ActiveCharacter = this;

			Type |= ObjectTypes.Player;
			ChatChannels = new List<ChatChannel>();

			m_logoutTimer = new TimerEntry(0.0f, DefaultLogoutDelay, totalTime => FinishLogout());

			Account = acc;
			m_client = client;

			m_record = record;
			EntityId = EntityId.GetPlayerId(m_record.EntityLowId);
			m_name = m_record.Name;

			Archetype = ArchetypeMgr.GetArchetype(record.Race, record.Class);
			MainWeapon = GenericWeapon.Fists;
			PowerType = m_archetype.Class.PowerType;

			StandState = StandState.Sit;

			Money = (uint)m_record.Money;
			Outfit = m_record.Outfit;
			//ScaleX = m_archetype.Race.Scale;
			ScaleX = 1;
			Gender = m_record.Gender;
			Skin = m_record.Skin;
			Facial = m_record.Face;
			HairStyle = m_record.HairStyle;
			HairColor = m_record.HairColor;
			FacialHair = m_record.FacialHair;
			UnitFlags = UnitFlags.PlayerControlled;
			XP = m_record.Xp;
			RestXp = m_record.RestXp;
			Level = m_record.Level;
			NextLevelXP = XpGenerator.GetXpForlevel(m_record.Level + 1);
			MaxLevel = RealmServerConfiguration.MaxCharacterLevel;

			RestState = RestState.Normal;

			Orientation = m_record.Orientation;

			m_bindLocation = new ZoneWorldLocation(
				m_record.BindRegion,
				new Vector3(m_record.BindX, m_record.BindY, m_record.BindZ),
				m_record.BindZone);

			PvPRank = 1;
			YieldsXpOrHonor = true;

			foreach (var school in WCellDef.AllDamageSchools)
			{
				SetFloat(PlayerFields.MOD_DAMAGE_DONE_PCT + (int)school, 1);
			}
			SetFloat(PlayerFields.DODGE_PERCENTAGE, 1.0f);

			// Auras
			m_auras = new PlayerAuraCollection(this);

			// spells
			PlayerSpellCollection spells;
			if (!record.JustCreated && SpellHandler.PlayerSpellCollections.TryGetValue(EntityId.Low, out spells))
			{
				SpellHandler.PlayerSpellCollections.Remove(EntityId.Low);
				m_spells = spells;
				((PlayerSpellCollection)m_spells).OnReconnectOwner(this);
			}
			else
			{
				m_spells = new PlayerSpellCollection(this);
			}

			// factions
			WatchedFaction = m_record.WatchedFaction;
			Faction = FactionMgr.ByRace[(uint)record.Race];
			m_reputations = new ReputationCollection(this);

			// skills
			m_skills = new SkillCollection(this);

			// talents
			m_talents = new TalentCollection(this);

			// Items
			m_inventory = new PlayerInventory(this);

			m_mailAccount = new MailAccount(this);

			m_questLog = new QuestLog(this);

			// Talents
			m_record.SpecProfile = SpecProfile.NewSpecProfile(this);
			FreeTalentPoints = m_record.FreeTalentPoints;

			// tutorial flags
			TutorialFlags = new TutorialFlags(m_record.TutorialFlags);

			// Make sure client and internal state is updated with combat base values
			UnitUpdates.UpdateSpellCritChance(this);

			// Mask of activated TaxiNodes
			m_taxiNodeMask = new TaxiNodeMask();

			PowerCostMultiplier = 1f;

			m_lastPlayTimeUpdate = DateTime.Now;

			MoveControl.Mover = this;
			MoveControl.CanControl = true;

			CanMelee = true;

			// basic setup
			if (record.JustCreated)
			{
				ModStatsForLevel(1);
				//Power = PowerType == PowerType.Rage ? 0 : MaxPower;
				//SetInt32(UnitFields.HEALTH, MaxHealth);
			}
			else
			{
				BaseHealth = m_record.BaseHealth;
				SetBasePowerDontUpdate(m_record.BasePower);

				SetBaseStat(StatType.Strength, m_record.BaseStrength);
				SetBaseStat(StatType.Stamina, m_record.BaseStamina);
				SetBaseStat(StatType.Spirit, m_record.BaseSpirit);
				SetBaseStat(StatType.Intellect, m_record.BaseIntellect);
				SetBaseStat(StatType.Agility, m_record.BaseAgility);

				Power = m_record.Power;
				SetInt32(UnitFields.HEALTH, m_record.Health);
			}

		}
		#endregion

		#region Load
		/// <summary>
		/// Loads this Character from DB when logging in.
		/// </summary>
		/// <remarks>Requires IO-Context.</remarks>
		internal protected void Load()
		{
			var nativeModel = m_archetype.Race.GetModel(m_record.Gender);
			NativeDisplayId = nativeModel.DisplayId;
			var model = nativeModel;
			if (m_record.DisplayId != model.DisplayId)
			{
				model = UnitMgr.GetModelInfo(m_record.DisplayId) ?? nativeModel;
			}
			Model = model;

			if (m_record.JustCreated)
			{
				if (m_zone != null)
				{
					SetZoneExplored(m_zone.Template, true);
				}

				//m_record.FreeTalentPoints = 0;
				UpdateFreeTalentPointsSilently(0);

				// Honor and Arena
				m_record.KillsTotal = 0u;
				m_record.HonorToday = 0u;
				m_record.HonorYesterday = 0u;
				m_record.LifetimeHonorableKills = 0u;
				m_record.HonorPoints = 0u;
				m_record.ArenaPoints = 0u;
			}
			else
			{
				try
				{
					m_record.LoadSpells();
					m_skills.Load();
					m_mailAccount.Load();
					m_reputations.Load();
					m_talents.InitTalentPoints();
					var auras = m_record.LoadAuraRecords();
					AddPostUpdateMessage(() => m_auras.PlayerInitialize(auras));

					if (QuestMgr.Loaded)
					{
						InitQuests();
					}

					if (m_record.FinishedQuests != null)
					{
						m_questLog.FinishedQuests.AddRange(m_record.FinishedQuests);
					}
				}
				catch (Exception e)
				{
					RealmDBUtil.OnDBError(e);
					m_client.Disconnect();
					return;
				}

				// existing Character
				if (m_record.ExploredZones.Length != UpdateFieldMgr.ExplorationZoneFieldSize * 4)
				{
					var zones = m_record.ExploredZones;
					Array.Resize(ref zones, (int)UpdateFieldMgr.ExplorationZoneFieldSize * 4);
					m_record.ExploredZones = zones;
				}

				if (ActionButtons == null)
				{
					// make sure to create the array, if loading or the last save failed
					ActionButtons = (byte[])Archetype.ActionButtons.Clone();
				}
				else if (m_record.ActionButtons.Length != Archetype.ActionButtons.Length)
				{
					var buts = m_record.ActionButtons;
					ArrayUtil.EnsureSize(ref buts, Archetype.ActionButtons.Length);
					m_record.ActionButtons = buts;
				}

				foreach (var spell in m_record.Spells.Values)
				{
					((PlayerSpellCollection)m_spells).OnlyAdd(spell);
				}


				if (m_record.RestTriggerId != 0 &&
					(m_restTrigger = AreaTriggerMgr.GetTrigger((uint)m_record.RestTriggerId)) != null)
				{
					RestState = RestState.Resting;
				}

				if (m_record.LastLogout != null)
				{
					var now = DateTime.Now;
					RestXp += RestGenerator.GetRestXp(now - m_record.LastLogout.Value, this);

					m_lastRestUpdate = now;
				}
				else
				{
					m_lastRestUpdate = DateTime.Now;
				}

				m_taxiNodeMask.Mask = m_record.TaxiMask;

				// TODO: Load Pets

				// Talents
				UpdateFreeTalentPointsSilently(0);

				// Honor and Arena
				KillsTotal = m_record.KillsTotal;
				HonorToday = m_record.HonorToday;
				HonorYesterday = m_record.HonorYesterday;
				LifetimeHonorableKills = m_record.LifetimeHonorableKills;
				HonorPoints = m_record.HonorPoints;
				ArenaPoints = m_record.ArenaPoints;
			}

			if (m_record.PetEntryId != 0)
			{
				LoadPets();
			}

			//foreach (var skill in m_skills)
			//{
			//    if (skill.SkillLine.Category == SkillCategory.ArmorProficiency) {
			//        CharacterHandler.SendProfiency(m_client, ItemClass.Armor, (uint)skill.SkillLine.Id);
			//    }
			//}			

			// this prevents a the Char from re-sending a value update when being pushed to world AFTER creation
			ResetUpdateInfo();
		}

		internal void InitQuests()
		{
			m_questLog.Load();
		}

		private void LoadEquipmentState()
		{
			if (m_record.CharacterFlags.HasFlag(CharEnumFlags.HideCloak))
			{
				PlayerFlags |= PlayerFlags.HideCloak;
			}
			if (m_record.CharacterFlags.HasFlag(CharEnumFlags.HideHelm))
			{
				PlayerFlags |= PlayerFlags.HideHelm;
			}
		}

		private void LoadDeathState()
		{
			if (m_record.CorpseX != null)
			{
				// we were dead and released the corpse
				var region = World.GetRegion(m_record.CorpseRegion);
				if (region != null)
				{
					m_corpse = SpawnCorpse(false, false, region,
										   new Vector3(m_record.CorpseX.Value, m_record.CorpseY, m_record.CorpseZ), m_record.CorpseO);
					BecomeGhost();
				}
				else
				{
					// can't spawn corpse -> revive
					if (log.IsWarnEnabled)
					{
						log.Warn("Player {0}'s Corpse was spawned in invalid region: {1}", this, m_record.CorpseRegion);
					}

				}
			}
			else if (m_record.Health == 0)
			{
				// we were dead and did not release yet
				var diff = (float)DateTime.Now.Subtract(m_record.LastDeathTime.AddMilliseconds(Corpse.AutoReleaseDelay)).TotalSeconds;
				m_corpseReleaseTimer = new TimerEntry(dt => ReleaseCorpse());

				if (diff > 0)
				{
					// mark dead and start release timer
					MarkDead();
					m_corpseReleaseTimer.Start(diff, 0);
				}
				else
				{
					// auto release
					ReleaseCorpse();
				}
			}
			else
			{
				// we are alive and kicking
			}
		}

		#endregion

		#region Login / Init
		/// <summary>
		/// Loads and adds the Character to its Region.
		/// </summary>
		/// <remarks>Called initially from the IO-Context</remarks>
		internal void LoadAndLogin()
		{
			// set Zone *before* Region
			// TODO: Also retrieve Battlegrounds
			m_region = World.GetRegion(m_record);

			InstanceMgr.RetrieveInstances(this);

			if (!Role.IsStaff)
			{
				Stunned++;
			}

			var isStaff = Role.IsStaff;
			if (m_region == null)
			{
				TeleportToBindLocation();
				Load();
			}
			else
			{
				Load();
				if (m_region.IsDisposed ||
					(m_region.IsInstance && (m_region.CreationTime > m_record.LastLogout ||
											 (!m_region.CanEnter(this) && !isStaff))))
				{
					// invalid Region or not allowed back in (might be an Instance)
					m_region.TeleportOutside(this);

					AddMessage(InitializeCharacter);
				}
				else
				{
					m_region.AddMessage(() =>
					{
						if (m_region is Battleground)
						{
							var bg = (Battleground)m_region;
							if (!bg.LogBackIn(this))
							{
								return;
							}
						}

						m_position = new Vector3(m_record.PositionX,
												 m_record.PositionY,
												 m_record.PositionZ);

						m_zone = m_region.GetZone(m_record.Zone);

						if (m_zone != null && m_record.JustCreated)
						{
							// set initial zone explored automatically
							SetZoneExplored(m_zone.Id, false);
						}

						// during the next Region-wide Character-update, the Character is going to be added to the region 
						// and created/initialized immediately afterwards
						m_region.AddObjectNow(this);

						InitializeCharacter();
					});
				}
			}
		}


		/// <summary>
		/// Is called after Character has been added to a region the first time and 
		/// before it receives the first Update packet
		/// </summary>
		internal protected void InitializeCharacter()
		{
			World.AddCharacter(this);
			m_initialized = true;

			try
			{
				InitializeRegeneration();
				((PlayerSpellCollection)m_spells).PlayerInitialize();

				OnLogin();

#if DEV
				// do this check in case that we did not load Items yet
				if (ItemMgr.Loaded)
#endif
					InitItems();

				if (m_record.JustCreated)
				{
					if (!m_client.Account.Role.IsStaff)
					{
						CharacterHandler.SendCinematic(this);
					}
					if (m_zone != null)
					{
						m_zone.EnterZone(this, null);
					}

					m_spells.AddDefaultSpells();
					m_reputations.Initialize();

                    if(Class == ClassId.Warrior && Spells.Contains(SpellId.ClassSkillBattleStance))
                    {
                        CallDelayed(1000, obj => SpellCast.Start(SpellId.ClassSkillBattleStance, false));
                    }
                    if(Class == ClassId.DeathKnight && Spells.Contains(SpellId.ClassSkillBloodPresence))
                    {
                        CallDelayed(1000, obj => SpellCast.Start(SpellId.ClassSkillBloodPresence, false));
                    }
				}
				else
				{
					LoadDeathState();
					LoadEquipmentState();
				}

				var ticket = TicketMgr.Instance.GetTicket(EntityId.Low);
				if (ticket != null)
				{
					Ticket = ticket;
					Ticket.OnOwnerLogin(this);
				}

				GroupMgr.Instance.OnCharacterLogin(this);
				GuildMgr.Instance.OnCharacterLogin(this);
				RelationMgr.Instance.OnCharacterLogin(this);

				LastLogin = DateTime.Now;
				var isNew = m_record.JustCreated;

				AddMessage(() =>
				{
					if (LastLogout == null)
					{
						RealmCommandHandler.ExecFirstLoginFileFor(this);
					}

					RealmCommandHandler.ExecAllCharsFileFor(this);

					if (Account.Role.IsStaff)
					{
						RealmCommandHandler.ExecFileFor(this);
					}

					Stunned--;

					if (m_record.NextTaxiVertexId != 0)
					{
						// we are on a Taxi
						var vertex = TaxiMgr.GetVertex(m_record.NextTaxiVertexId);
						if (vertex != null &&
							vertex.MapId == m_region.Id &&
							vertex.ListEntry.Next != null &&
							IsInRadius(vertex.Pos, vertex.ListEntry.Next.Value.DistFromPrevious))
						{
							TaxiPaths.Enqueue(vertex.Path);
							TaxiMgr.FlyUnit(this, true, vertex.ListEntry);
						}
						else
						{
							m_record.NextTaxiVertexId = 0;
						}
					}
					else
					{
						// cannot stand up instantly because else no one will see the char sitting in the first place
						StandState = StandState.Stand;
					}
					GodMode = m_record.GodMode;

					if (isNew)
					{
						// newly created Char logs in the first time
						var evt = Created;
						if (evt != null)
						{
							evt(this);
						}
					}

					//if (Role.IsStaff)
					if (GodMode)
					{
						//Notify("Your GodMode is " + (GodMode ? "ON" : "OFF") + "!");
						Notify(LangKey.GodModeIsActivated);
					}

					var login = LoggedIn;
					if (login != null)
					{
						login(this, true);
					}
				});

				if (m_record.JustCreated)
				{
					SaveLater();
					m_record.JustCreated = false;
				}
				else
				{
					RealmServer.Instance.AddMessage(() =>
					{
						try
						{
							m_record.Update();
						}
						catch (Exception ex)
						{
							SaveLater();
							LogUtil.ErrorException(ex, "Failed to Update CharacterRecord: " + m_record);
						}
					});
				}
			}
			catch (Exception e)
			{
				World.RemoveCharacter(this);
				LogUtil.ErrorException(e, "Failed to initialize Character: " + this);
				m_client.Disconnect();
			}
		}

		/// <summary>
		/// Load items from DB or (if new char) add initial Items.
		/// Happens either on login or when items have been loaded during runtime
		/// </summary>
		protected internal void InitItems()
		{
			if (m_record.JustCreated)
			{
				m_inventory.AddDefaultItems();
			}
			else
			{
				m_inventory.AddOwnedItems();
#if DEBUG
				// let's keep this for now for test purposes
				if (m_inventory.TotalCount == 0)
				{
					// no Items -> Add default Items
					m_inventory.AddDefaultItems();
				}
#endif
			}
		}

		/// <summary>
		/// Called within Region Context.
		/// Sends initial packets
		/// </summary>
		private void OnLogin()
		{
			InstanceHandler.SendDungeonDifficulty(this);
			CharacterHandler.SendVerifyWorld(this);
			AccountDataHandler.SendAccountDataTimes(m_client);
			VoiceChatHandler.SendSystemStatus(this, VoiceSystemStatus.Disabled);
			// SMSG_GUILD_EVENT
			// SMSG_GUILD_BANK_LIST
			CharacterHandler.SendBindUpdate(this, BindLocation);
			TutorialHandler.SendTutorialFlags(this);
			SpellHandler.SendSpellsAndCooldowns(this);
			CharacterHandler.SendActionButtons(this);
			FactionHandler.SendFactionList(this);
			// SMSG_INIT_WORLD_STATES
			// SMSG_EQUIPMENT_SET_LIST
			// SMSG_ALL_ACHIEVEMENT_DATA
			// SMSG_EXPLORATION_EXPERIENCE
			CharacterHandler.SendTimeSpeed(this);
			TalentHandler.SendTalentGroupList(this);
			AuraHandler.SendAllAuras(this);
			// SMSG_PET_GUIDS
		}

		/// <summary>
		/// Reconnects a client to a character that was logging out.
		/// Resends required initial packets.
		/// Called from within the region context.
		/// </summary>
		/// <param name="newClient"></param>
		internal void ReconnectCharacter(IRealmClient newClient)
		{
			// if (chr.LastLogout != null && chr.LastLogout)
			CancelLogout(false);

			newClient.ActiveCharacter = this;
			m_client = newClient;

			ClearSelfKnowledge();
			OnLogin();

			m_lastPlayTimeUpdate = DateTime.Now;

			var evt = LoggedIn;
			if (evt != null)
			{
				evt(this, false);
			}

			// since we let them sit down during logout, we stand them up again
			AddMessage(() =>
			{
				StandState = StandState.Stand;
			});
		}

		#endregion

		#region Save

		/// <summary>
		/// Enqueues saving of this Character to the IO-Queue.
		/// <see cref="SaveNow"/>
		/// </summary>
		public void SaveLater()
		{
			RealmServer.Instance.AddMessage(new Message(() => SaveNow()));
		}

		/// <summary>
		/// Saves the Character to the DB instantly.
		/// Blocking call.
		/// See: <see cref="SaveLater()"/>.
		/// When calling this method directly, make sure to set m_saving = true
		/// </summary>
		internal protected bool SaveNow()
		{
			if (!m_record.CanSave)
			{
				return false;
			}

			if (DebugUtil.Dumps)
			{
				var writer = DebugUtil.GetTextWriter(m_client.Account);
				writer.WriteLine("Saving {0}...", Name);
			}

			try
			{
				if (m_record == null)
				{
					throw new InvalidOperationException("Cannot save Character while not in world.");
				}

				UpdatePlayedTime();

				// always make sure that the values saved to DB, will not be influenced by buffs etc
				m_record.Race = Race;
				m_record.Class = Class;
				m_record.Gender = Gender;
				m_record.Skin = Skin;
				m_record.Face = Facial;
				m_record.HairStyle = HairStyle;
				m_record.HairColor = HairColor;
				m_record.FacialHair = FacialHair;
				m_record.Outfit = Outfit;
				m_record.Name = Name;
				m_record.Level = Level;
				m_record.PositionX = Position.X;
				m_record.PositionY = Position.Y;
				m_record.PositionZ = Position.Z;
				m_record.Orientation = Orientation;
				m_record.RegionId = m_region.Id;
				m_record.InstanceId = m_region.InstanceId;
				m_record.Zone = ZoneId;
				m_record.DisplayId = DisplayId;
				m_record.BindX = m_bindLocation.Position.X;
				m_record.BindY = m_bindLocation.Position.Y;
				m_record.BindZ = m_bindLocation.Position.Z;
				m_record.BindRegion = m_bindLocation.RegionId;
				m_record.BindZone = m_bindLocation.ZoneId;

				m_record.Health = Health;
				m_record.BaseHealth = BaseHealth;
				m_record.Power = Power;
				m_record.BasePower = BasePower;
				m_record.Money = Money;
				m_record.FreeTalentPoints = FreeTalentPoints;
				m_record.WatchedFaction = WatchedFaction;
				m_record.BaseStrength = GetBaseStatValue(StatType.Strength);
				m_record.BaseStamina = GetBaseStatValue(StatType.Stamina);
				m_record.BaseSpirit = GetBaseStatValue(StatType.Spirit);
				m_record.BaseIntellect = GetBaseStatValue(StatType.Intellect);
				m_record.BaseAgility = GetBaseStatValue(StatType.Agility);
				m_record.Xp = XP;
				m_record.RestXp = RestXp;

				// Honor and Arena
				m_record.KillsTotal = KillsTotal;
				m_record.HonorToday = HonorToday;
				m_record.HonorYesterday = HonorYesterday;
				m_record.LifetimeHonorableKills = LifetimeHonorableKills;
				m_record.HonorPoints = HonorPoints;
				m_record.ArenaPoints = ArenaPoints;

				// Finished quests
				if (m_questLog.FinishedQuests.Count > 0)
				{
					m_record.FinishedQuests = new uint[m_questLog.FinishedQuests.Count];
					m_questLog.FinishedQuests.CopyTo(m_record.FinishedQuests);
				}

				// Taxis
				if (LatestTaxiPathNode != null && LatestTaxiPathNode.Next != null)
				{
					m_record.NextTaxiVertexId = (int)LatestTaxiPathNode.Next.Value.Id;
				}
				else
				{
					m_record.NextTaxiVertexId = 0;
				}
				m_record.TaxiMask = m_taxiNodeMask.Mask;

				if (m_record.Level > 1 &&
					m_record.Level > Account.HighestCharLevel)
				{
					Account.HighestCharLevel = m_record.Level;
				}
			}
			catch (Exception e)
			{
				OnSaveFailed(e);
				return false;
			}

			try
			{
				using (var saveScope = new SessionScope(FlushAction.Never))
				{
					// Interface settings
					Account.AccountData.Save();

					// Items
					var items = new List<ItemRecord>();
					m_inventory.SaveAll(items);
					m_record.UpdateItems(items);

					// Skills
					foreach (var skill in m_skills)
					{
						skill.Save();
					}

					// Pets
					SaveEntourage();

					// Quests
					m_questLog.SaveQuests();

					// Talents
					//m_record.SpecProfile.Save();

					// Auras
					m_auras.SaveAurasNow();

					// General Character data
					m_record.Save();

					saveScope.Flush();
				}
				m_record.LastSaveTime = DateTime.Now;

				if (DebugUtil.Dumps)
				{
					var writer = DebugUtil.GetTextWriter(m_client.Account);
					writer.WriteLine("Saved {0} (Region: {1}).", Name, m_record.RegionId);
				}

				return true;
			}
			catch (Exception ex)
			{
				try
				{
					m_record.Save();
					return true;
				}
				catch //(Exception ex2)
				{
					OnSaveFailed(ex);
				}
				return false;
			}
			finally
			{
			}
		}

		void OnSaveFailed(Exception ex)
		{
			SendSystemMessage("Saving failed - Please excuse the inconvenience!");

			if (DebugUtil.Dumps)
			{
				var writer = DebugUtil.GetTextWriter(m_client.Account);
				writer.WriteLine("Failed to save {0}: {1}", Name, ex);
			}

			LogUtil.ErrorException(ex, "Could not save Character " + this);
		}

		#endregion

		#region Logout
		public bool CanLogoutInstantly
		{
			get
			{
				return Role.IsStaff ||
					(!m_isInCombat && Zone != null && Zone.Template.IsCity) ||
					IsOnTaxi;
			}
		}

		/// <summary>
		/// whether the Logout sequence initialized (Client might already be disconnected)
		/// </summary>
		public bool IsLoggingOut
		{
			get { return m_isLoggingOut; }
		}

		/// <summary>
		/// whether the player is currently logging out by itself (not forcefully being logged out).
		/// Players who are forced to logout cannot cancel.
		/// Is false while Client is logged in.
		/// </summary>
		public bool IsPlayerLogout
		{
			get;
			internal set;
		}

		public bool CanLogout
		{
			get { return !m_IsPinnedDown && !IsInCombat; }
		}

		/// <summary>
		/// Enqueues logout of this player to the Region's queue
		/// </summary>
		/// <param name="forced">whether the Character is forced to logout (as oppose to initializeing logout oneself)</param>
		public void LogoutLater(bool forced)
		{
			AddMessage(() => Logout(forced));
		}

		/// <summary>
		/// Starts the logout process with the default delay (or instantly if
		/// in city or staff)
		/// Requires region context.
		/// </summary>
		/// <param name="forced"></param>
		public void Logout(bool forced)
		{
			Logout(forced, CanLogoutInstantly ? 0 : DefaultLogoutDelay);
		}

		/// <summary>
		/// Starts the logout process.
		/// Disconnects the Client after the given delay in seconds, if not in combat (or instantly if delay = 0)
		/// Requires region context.
		/// </summary>
		/// <param name="forced">whether the Character is forced to logout (as opposed to initializing logout oneself)</param>
		/// <param name="delay">The delay until the client will be disconnected in seconds</param>
		public void Logout(bool forced, float delay)
		{
			if (!m_isLoggingOut)
			{
				m_isLoggingOut = true;

				IsPlayerLogout = !forced;

				CancelAllActions();

				if (!IsOnTaxi)
				{
					// sit down
					StandState = StandState.Sit;
				}

				if (forced)
				{
					Stunned++;
				}

				if (delay <= 0)
				{
					FinishLogout();
				}
				else
				{
					CharacterHandler.SendLogoutReply(m_client, LogoutResponseCodes.LOGOUT_RESPONSE_ACCEPTED);

					// update items for the login screen
					var items = new List<ItemRecord>();
					foreach (var item in m_inventory.GetAllItems(false))
					{
						items.Add(item.Record);
					}
					m_record.UpdateItems(items);

					m_logoutTimer.Start(delay);
				}
			}
			else
			{
				if (forced)
				{
					// logout is now mandatory
					IsPlayerLogout = false;

					// reset timer
					if (delay <= 0)
					{
						m_logoutTimer.Stop();
						FinishLogout();
					}
					else
					{
						m_logoutTimer.Start(delay);
					}
				}
			}
		}

		/// <summary>
		/// Cancels whatever this Character currently does
		/// </summary>
		//public override void CancelAllActions()
		//{
		//    base.CancelAllActions();
		//}

		/// <summary>
		/// Cancels logout of this Character.
		/// Requires region context.
		/// </summary>
		public void CancelLogout()
		{
			CancelLogout(true);
		}

		/// <summary>
		/// Cancels whatever this Character currently does
		/// </summary>
		//public override void CancelAllActions()
		//{
		//    base.CancelAllActions();
		//}

		/// <summary>
		/// Cancels logout of this Character.
		/// Requires region context.
		/// </summary>
		/// <param name="sendCancelReply">whether to send the Cancel-reply (if client did not disconnect in the meantime)</param>
		public void CancelLogout(bool sendCancelReply)
		{
			if (m_isLoggingOut)
			{
				if (!IsPlayerLogout)
				{
					Stunned--;
				}

				m_isLoggingOut = false;
				IsPlayerLogout = false;

				m_logoutTimer.Stop();

				DecMechanicCount(SpellMechanic.Frozen);

				StandState = StandState.Stand;

				if (sendCancelReply)
				{
					CharacterHandler.SendLogoutCancelReply(Client);
				}
			}
		}

		/// <summary>
		/// Saves and then removes Character
		/// </summary>
		/// <remarks>Requires region context for synchronization.</remarks>
		internal void FinishLogout()
		{
			RealmServer.Instance.AddMessage(new Message(() =>
			{
				SaveNow();

				var handler = ContextHandler;
				if (handler != null)
				{
					ContextHandler.AddMessage(() => DoFinishLogout());
				}
				else
				{
					DoFinishLogout();
				}
			}));
		}

		internal void DoFinishLogout()
		{
			if (!m_isLoggingOut)
			{
				// cancel if logout was cancelled
				return;
			}

			var evt = LoggingOut;
			if (evt != null)
			{
				evt(this);
			}

			// client might now do other things
			m_client.ActiveCharacter = null;
			Account.ActiveCharacter = null;

			// take Player out of world context
			if (!World.RemoveCharacter(this))
			{
				// was already removed
				return;
			}

			// set to false so it can't be cancelled anymore
			m_isLoggingOut = false;

			// remove from the channels they're in
			for (var i = ChatChannels.Count - 1; i >= 0; i--)
			{
				ChatChannels[i].Leave(this, true);
			}

			if (Ticket != null)
			{
				Ticket.OnOwnerLogout();
				Ticket = null;
			}

			if (m_TaxiMovementTimer != null)
			{
				m_TaxiMovementTimer.Stop();
			}

			GroupMgr.Instance.OnCharacterLogout(m_groupMember);
			GuildMgr.Instance.OnCharacterLogout(m_guildMember);
			RelationMgr.Instance.OnCharacterLogout(this);
			InstanceMgr.OnCharacterLogout(this);
			Battlegrounds.OnLogout();

			LastLogout = DateTime.Now;

			// delete and re-spawn corpses and pets when logging back in
			if (m_corpse != null)
			{
				m_corpse.Delete();
			}

			CancelAllActions();
			m_auras.CleanupAuras();

			m_region.RemoveObjectNow(this);

			((PlayerSpellCollection)m_spells).OnOwnerLoggedOut();

			//if (!IsPlayerLogout)
			if (!Account.IsActive)
			{
				// Character has been banned
				m_client.Disconnect();
			}
			else
			{
				CharacterHandler.SendPlayerImmediateLogoutReply(m_client);
			}

			if (CharacterHandler.NotifyPlayerStatus)
			{
				World.Broadcast("{0} is now " + ChatUtility.Colorize("Offline", Color.Red) + ".", Name);
			}

			m_initialized = false;

			Dispose();
		}
		#endregion

		#region Packet Sending
		public override void SendPacketToArea(RealmPacketOut packet)
		{
			if (AreaCharCount == 1)
			{
				// no one else around
				Client.Send(packet);
			}
			else
			{
				foreach (var obj in NearbyObjects)
				{
					if (obj is Character && obj.IsInWorld)
					{
						((Character)obj).Client.Send(packet);
					}
				}
			}
		}

		public override void SendPacketToArea(RealmPacketOut packet, bool includeSelf)
		{
			if (AreaCharCount == 1)
			{
				// no one else around
				if (includeSelf)
				{
					Client.Send(packet);
				}
			}
			else
			{
				foreach (var obj in NearbyObjects)
				{
					if (obj is Character && (includeSelf || obj != this) && obj.IsInWorld)
					{
						((Character)obj).Client.Send(packet);
					}
				}
			}
		}

		#endregion

		#region Kick
		// TODO: Log Kicking

		/// <summary>
		/// Kicks this Character with the given msg instantly.
		/// </summary>
		/// <remarks>
		/// Requires region context.
		/// </remarks>
		public void Kick(string msg)
		{
			Kick(null, msg, 0);
		}

		/// <summary>
		/// Kicks this Character with the given msg after the given delay in seconds.
		/// Requires region context.
		/// </summary>
		/// <param name="delay">The delay until the Client should be disconnected in seconds</param>
		public void Kick(string reason, float delay)
		{
			Kick(reason, delay);
		}

		/// <summary>
		/// Broadcasts a kick message and then kicks this Character after the default delay.
		/// Requires region context.
		/// </summary>
		public void Kick(Character kicker, string reason)
		{
			Kick(kicker, reason, DefaultLogoutDelay);
		}

		/// <summary>
		/// Broadcasts a kick message and then kicks this Character after the default delay.
		/// Requires region context.
		/// </summary>
		public void Kick(INamed kicker, string reason, float delay)
		{
			var other = (kicker != null ? " by " + kicker.Name : "") +
				(!string.IsNullOrEmpty(reason) ? " (" + reason + ")" : ".");
			World.Broadcast("{0} has been kicked{1}", Name, other);

			SendSystemMessage("You have been kicked" + other);

			CancelTaxiFlight();
			Logout(true, delay);
		}
		#endregion

		#region Dispose
		/// <summary>
		/// Performs any needed object/object pool cleanup.
		/// </summary>
		public override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			CancelSummon(false);

			if (m_bgInfo != null)
			{
				m_bgInfo.Character = null;
				m_bgInfo = null;
			}

			m_InstanceCollection = null;

			if (m_spellCast != null)
			{
				m_spellCast.Cancel();
				m_spellCast = null;
			}

			if (m_activePet != null)
			{
				m_activePet.Delete();
				m_activePet = null;
			}

			m_minions = null;
			m_activePet = null;

			m_skills.m_owner = null;
			m_skills = null;

			m_talents.Owner = null;
			m_talents = null;

			m_inventory.m_container = null;
			m_inventory.m_owner = null;
			m_inventory.m_ammo = null;
			m_inventory.m_currentBanker = null;
			m_inventory = null;

			m_mailAccount.Owner = null;
			m_mailAccount = null;

			m_groupMember = null;

			m_reputations.Owner = null;
			m_reputations = null;

			if (m_InstanceCollection != null)
			{
				m_InstanceCollection.Dispose();
			}

			if (m_casterInfo != null)
			{
				m_casterInfo.Caster = null;
				m_casterInfo = null;
			}

			if (m_looterEntry != null)
			{
				m_looterEntry.m_owner = null;
				m_looterEntry = null;
			}

			if (m_ExtraInfo != null)
			{
				m_ExtraInfo.Dispose();
				m_ExtraInfo = null;
			}

			KnownObjects.Clear();

		}

		/// <summary>
		/// Throws an exception, since logged in Characters may not be deleted
		/// </summary>
		protected internal override void DeleteNow()
		{
			//throw new InvalidOperationException("Cannot delete logged in Character.");
			Client.Disconnect();
		}

		/// <summary>
		/// Throws an exception, since logged in Characters may not be deleted
		/// </summary>
		public override void Delete()
		{
			//throw new InvalidOperationException("Cannot delete logged in Character.");
			Client.Disconnect();
		}
		#endregion
	}
}