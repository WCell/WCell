using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Core;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Database.Entities;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells.Auras;
using WCell.Util.Graphics;
using WCell.Util.Logging;
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
using WCell.Util;
using WCell.RealmServer.Battlegrounds;

namespace WCell.RealmServer.Entities
{
	// Anything related to creation/login/logout/saving/loading is in this file

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
			ChatChannels = new List<ChatChannel>(5);

			m_logoutTimer = new TimerEntry(0, DefaultLogoutDelayMillis, totalTime => FinishLogout());

			Account = acc;
			m_client = client;

			m_record = record;
			EntityId = EntityId.GetPlayerId(m_record.EntityLowId);
			m_name = m_record.Name;

			Archetype = ArchetypeMgr.GetArchetype(record.Race, record.Class);
			MainWeapon = GenericWeapon.Fists;
			PowerType = m_archetype.Class.DefaultPowerType;

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
			Experience = m_record.Xp;
			RestXp = m_record.RestXp;

			SetInt32(UnitFields.LEVEL, m_record.Level);
			// cannot use Level property, since it will trigger certain events that we don't want triggered
			NextLevelXP = XpGenerator.GetXpForlevel(m_record.Level + 1);
			MaxLevel = RealmServerConfiguration.MaxCharacterLevel;

			RestState = RestState.Normal;

			Orientation = m_record.Orientation;

			m_bindLocation = new WorldZoneLocation(
				m_record.BindMap,
				new Vector3(m_record.BindX, m_record.BindY, m_record.BindZ),
				m_record.BindZone);

			PvPRank = 1;
			YieldsXpOrHonor = true;

			foreach (var school in SpellConstants.AllDamageSchools)
			{
				SetFloat(PlayerFields.MOD_DAMAGE_DONE_PCT + (int)school, 1);
			}
			SetFloat(PlayerFields.DODGE_PERCENTAGE, 1.0f);

			// Auras
			m_auras = new PlayerAuraCollection(this);

			// spells
			m_spells = PlayerSpellCollection.Obtain(this);

			// factions
			WatchedFaction = m_record.WatchedFaction;
			Faction = FactionMgr.ByRace[(uint)record.Race];
			m_reputations = new ReputationCollection(this);

			// skills
			m_skills = new SkillCollection(this);

			// talents
			m_talents = new PlayerTalentCollection(this);

			// achievements
			m_achievements = new AchievementCollection(this);

			// Items
			m_inventory = new PlayerInventory(this);

			m_mailAccount = new MailAccount(this);

			m_questLog = new QuestLog(this);

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

			IncMeleePermissionCounter();

			SpeedFactor = DefaultSpeedFactor;

			// basic setup
			if (record.JustCreated)
			{
				ModStatsForLevel(m_record.Level);
				BasePower = RegenerationFormulas.GetPowerForLevel(this);
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

			// set FreeTalentPoints silently
			UpdateFreeTalentPointsSilently(0);
			if (m_record.JustCreated)
			{
				// newly created Character
				SpecProfiles = new[] { SpecProfile.NewSpecProfile(this, 0) };

				if (m_zone != null)
				{
					SetZoneExplored(m_zone.Template, true);
				}

				//m_record.FreeTalentPoints = 0;

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
				// existing Character
				try
				{
					//Set Playerfields for glyphs on load
					InitGlyphsForLevel();
					// load & validate SpecProfiles
					SpecProfiles = SpecProfile.LoadAllOfCharacter(this);
					if (SpecProfiles.Length == 0)
					{
						log.Warn("Character had no SpecProfiles: {0}", this);
						SpecProfiles = new[] { SpecProfile.NewSpecProfile(this, 0) };
					}
					if (m_record.CurrentSpecIndex >= SpecProfiles.Length)
					{
						log.Warn("Character had invalid CurrentSpecIndex: {0} ({1})", this, m_record.CurrentSpecIndex);
						m_record.CurrentSpecIndex = 0;
					}

					// load all the rest
					m_achievements.Load();
					((PlayerSpellCollection)m_spells).LoadSpellsAndTalents();
					((PlayerSpellCollection)m_spells).LoadCooldowns();
					m_skills.Load();
					m_mailAccount.Load();
					m_reputations.Load();
					var auras = AuraRecord.LoadAuraRecords(EntityId.Low);
					AddPostUpdateMessage(() => m_auras.InitializeAuras(auras));

					if (QuestMgr.Loaded)
					{
						LoadQuests();
					}

					if (m_record.FinishedQuests != null)
					{
						m_questLog.FinishedQuests.AddRange(m_record.FinishedQuests);
					}
				}
				catch (Exception e)
				{
					RealmWorldDBMgr.OnDBError(e);
					throw new Exception(string.Format("Failed to load Character \"{0}\" for Client: {1}", this, Client), e);
				}

				SetExploredZones();

                //Add existing talents to the character
                ((PlayerSpellCollection)m_spells).PlayerInitialize();

				// calculate amount of spent talent points per tree
				m_talents.CalcSpentTalentPoints();

				// update RestState
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

				// Honor and Arena
				KillsTotal = m_record.KillsTotal;
				HonorToday = m_record.HonorToday;
				HonorYesterday = m_record.HonorYesterday;
				LifetimeHonorableKills = m_record.LifetimeHonorableKills;
				HonorPoints = m_record.HonorPoints;
				ArenaPoints = m_record.ArenaPoints;
			}

			// Set FreeTalentPoints, after SpecProfile was loaded
		    var freePointsForLevel = m_talents.GetFreeTalentPointsForLevel(m_record.Level);
            m_talents.UpdateFreeTalentPointsSilently(freePointsForLevel);

			// Load pets (if any)
			LoadPets();

			//foreach (var skill in m_skills)
			//{
			//    if (skill.SkillLine.Category == SkillCategory.ArmorProficiency) {
			//        CharacterHandler.SendProfiency(m_client, ItemClass.Armor, (uint)skill.SkillLine.Id);
			//    }
			//}			

			// this prevents a the Char from re-sending a value update when being pushed to world AFTER creation
			ResetUpdateInfo();
		}

		/// <summary>
		/// Ensure correct size of array of explored zones and  copy explored zones to UpdateValues array
		/// </summary>
		private unsafe void SetExploredZones()
		{
			if (m_record.ExploredZones.Length != UpdateFieldMgr.ExplorationZoneFieldSize * 4)
			{
				var zones = m_record.ExploredZones;
				Array.Resize(ref zones, UpdateFieldMgr.ExplorationZoneFieldSize * 4);
				m_record.ExploredZones = zones;
			}

			fixed (byte* ptr = m_record.ExploredZones)
			{
				int index = 0;
				for (var field = PlayerFields.EXPLORED_ZONES_1; field < PlayerFields.EXPLORED_ZONES_1 + UpdateFieldMgr.ExplorationZoneFieldSize; field++)
				{
					SetUInt32(field, *(uint*)(&ptr[index]));
					index += 4;
				}
			}
		}

		internal void LoadQuests()
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
				var map = World.GetNonInstancedMap(m_record.CorpseMap);
				if (map != null)
				{
					m_corpse = SpawnCorpse(false, false, map,
										   new Vector3(m_record.CorpseX.Value, m_record.CorpseY, m_record.CorpseZ), m_record.CorpseO);
					BecomeGhost();
				}
				else
				{
					// can't spawn corpse -> revive
					if (log.IsWarnEnabled)
					{
						log.Warn("Player {0}'s Corpse was spawned in invalid map: {1}", this, m_record.CorpseMap);
					}
				}
			}
			else if (m_record.Health == 0)
			{
				// we were dead and did not release yet
				var diff = DateTime.Now.Subtract(m_record.LastDeathTime).ToMilliSecondsInt() + Corpse.AutoReleaseDelay;
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
		/// Loads and adds the Character to its Map.
		/// </summary>
		/// <remarks>Called initially from the IO-Context</remarks>
		internal void LoadAndLogin()
		{
			// set Zone *before* Map
			// TODO: Also retrieve Battlegrounds
			m_Map = World.GetMap(m_record);

			InstanceMgr.RetrieveInstances(this);

			AreaCharCount++;		// Characters are always in active regions

			if (!Role.IsStaff)
			{
				Stunned++;
			}

			var isStaff = Role.IsStaff;
			if (m_Map == null && (!isStaff || (m_Map = InstanceMgr.CreateInstance(this, m_record.MapId)) == null))
			{
				// map does not exist anymore
				Load();
				TeleportToBindLocation();
				AddMessage(InitializeCharacter);
				return;
			}
			else
			{
				Load();
				if (m_Map.IsDisposed ||
					(m_Map.IsInstance && !isStaff && (m_Map.CreationTime > m_record.LastLogout || !m_Map.CanEnter(this))))
				{
					// invalid Map or not allowed back in (might be an Instance)
					m_Map.TeleportOutside(this);

					AddMessage(InitializeCharacter);
				}
				else
				{
					m_Map.AddMessage(() =>
					{
						// add to map
						if (m_Map is Battleground)
						{
							var bg = (Battleground)m_Map;
							if (!bg.LogBackIn(this))
							{
								// teleport out of BG
								AddMessage(InitializeCharacter);
								return;
							}
						}

						m_position = new Vector3(m_record.PositionX,
												 m_record.PositionY,
												 m_record.PositionZ);

						m_zone = m_Map.GetZone(m_record.Zone);

						if (m_zone != null && m_record.JustCreated)
						{
							// set initial zone explored automatically
							SetZoneExplored(m_zone.Id, false);
						}

						// during the next Map-wide Character-update, the Character is going to be added to the map 
						// and created/initialized immediately afterwards
						m_Map.AddObjectNow(this);

						InitializeCharacter();
					});
				}
			}
		}


		/// <summary>
		/// Is called after Character has been added to a map the first time and 
		/// before it receives the first Update packet
		/// </summary>
		internal protected void InitializeCharacter()
		{
			World.AddCharacter(this);
			m_initialized = true;

			try
			{
				Regenerates = true;
				((PlayerSpellCollection)m_spells).PlayerInitialize();

				OnLogin();

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

					if (Class == ClassId.Warrior && Spells.Contains(SpellId.ClassSkillBattleStance))
					{
						CallDelayed(1000, obj => SpellCast.Start(SpellId.ClassSkillBattleStance, false));
					}
					else if (Class == ClassId.DeathKnight && Spells.Contains(SpellId.ClassSkillBloodPresence))
					{
						CallDelayed(1000, obj => SpellCast.Start(SpellId.ClassSkillBloodPresence, false));
					}

					// set initial weapon skill max values
					Skills.UpdateSkillsForLevel(Level);
				}
				else
				{
					LoadDeathState();
					LoadEquipmentState();
				}

				// load items
#if DEV
				// do this check in case that we did not load Items yet
				if (ItemMgr.Loaded)
#endif
					InitItems();

				// load ticket information
				var ticket = TicketMgr.Instance.GetTicket(EntityId.Low);
				if (ticket != null)
				{
					Ticket = ticket;
					Ticket.OnOwnerLogin(this);
				}

				// initialize sub systems
				GroupMgr.Instance.OnCharacterLogin(this);
				GuildMgr.Instance.OnCharacterLogin(this);
				RelationMgr.Instance.OnCharacterLogin(this);

				// set login date
				LastLogin = DateTime.Now;
				var isNew = m_record.JustCreated;

				// perform some stuff ingame
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
							vertex.MapId == m_Map.Id &&
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
						Notify(RealmLangKey.GodModeIsActivated);
					}

					var login = LoggedIn;
					if (login != null)
					{
						login(this, true);
					}
				});

				if (isNew)
				{
					SaveLater();
					m_record.JustCreated = false;
				}
				else
				{
					RealmServer.IOQueue.AddMessage(() =>
					{
						try
						{
							RealmWorldDBMgr.DatabaseProvider.SaveOrUpdate(m_record);
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
				if (m_record.JustCreated)
				{
					m_record.CanSave = false;
					m_record.Delete();
				}
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
		/// Called within Map Context.
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
			AchievementHandler.SendAchievementData(this);
			// SMSG_EXPLORATION_EXPERIENCE
			CharacterHandler.SendTimeSpeed(this);
			TalentHandler.SendTalentGroupList(m_talents);
			AuraHandler.SendAllAuras(this);
			// SMSG_PET_GUIDS
		}

		/// <summary>
		/// Reconnects a client to a character that was logging out.
		/// Resends required initial packets.
		/// Called from within the map context.
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
			RealmServer.IOQueue.AddMessage(new Message(() => SaveNow()));
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
				if (m_Map != null)
				{
					// only save position information if we are in world
					m_record.PositionX = Position.X;
					m_record.PositionY = Position.Y;
					m_record.PositionZ = Position.Z;
					m_record.Orientation = Orientation;
					m_record.MapId = m_Map.Id;
					m_record.InstanceId = m_Map.InstanceId;
					m_record.Zone = ZoneId;
				}
				m_record.DisplayId = DisplayId;
				m_record.BindX = m_bindLocation.Position.X;
				m_record.BindY = m_bindLocation.Position.Y;
				m_record.BindZ = m_bindLocation.Position.Z;
				m_record.BindMap = m_bindLocation.MapId;
				m_record.BindZone = m_bindLocation.ZoneId;

				m_record.Health = Health;
				m_record.BaseHealth = BaseHealth;
				m_record.Power = Power;
				m_record.BasePower = BasePower;
				m_record.Money = Money;
				m_record.WatchedFaction = WatchedFaction;
				m_record.BaseStrength = GetBaseStatValue(StatType.Strength);
				m_record.BaseStamina = GetBaseStatValue(StatType.Stamina);
				m_record.BaseSpirit = GetBaseStatValue(StatType.Spirit);
				m_record.BaseIntellect = GetBaseStatValue(StatType.Intellect);
				m_record.BaseAgility = GetBaseStatValue(StatType.Agility);
				m_record.Xp = Experience;
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

				// cooldowns & runes
				PlayerSpells.OnSave();

				// taxi mask
				m_record.TaxiMask = m_taxiNodeMask.Mask;

				if (m_record.Level > 1 &&
					m_record.Level > Account.HighestCharLevel)
				{
					// tell auth server about the new highest level
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
		        // Interface settings
		        RealmWorldDBMgr.DatabaseProvider.Save(Account.AccountData);

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

		        // Specs
		        foreach (var spec in SpecProfiles)
		        {
		            RealmWorldDBMgr.DatabaseProvider.Save(spec);
		        }

		        // Achievements
		        m_achievements.SaveNow();

		        // Auras
		        m_auras.SaveAurasNow();

		        // General Character data
		        RealmWorldDBMgr.DatabaseProvider.Save(m_record);

		        RealmWorldDBMgr.DatabaseProvider.CommitTransaction();
                RealmWorldDBMgr.DatabaseProvider.Session.Flush();
		        m_record.LastSaveTime = DateTime.Now;

		        if (DebugUtil.Dumps)
		        {
		            var writer = DebugUtil.GetTextWriter(m_client.Account);
		            writer.WriteLine("Saved {0} (Map: {1}).", Name, m_record.MapId);
		        }

		        return true;
		    }
		    catch (Exception ex)
		    {
		        OnSaveFailed(ex);
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
		/// Enqueues logout of this player to the Map's queue
		/// </summary>
		/// <param name="forced">whether the Character is forced to logout (as oppose to initializeing logout oneself)</param>
		public void LogoutLater(bool forced)
		{
			AddMessage(() => Logout(forced));
		}

		/// <summary>
		/// Starts the logout process with the default delay (or instantly if
		/// in city or staff)
		/// Requires map context.
		/// </summary>
		/// <param name="forced"></param>
		public void Logout(bool forced)
		{
			Logout(forced, CanLogoutInstantly ? 0 : DefaultLogoutDelayMillis);
		}

		/// <summary>
		/// Starts the logout process.
		/// Disconnects the Client after the given delay in seconds, if not in combat (or instantly if delay = 0)
		/// Requires map context.
		/// </summary>
		/// <param name="forced">whether the Character is forced to logout (as opposed to initializing logout oneself)</param>
		/// <param name="delay">The delay until the client will be disconnected in seconds</param>
		public void Logout(bool forced, int delay)
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
		/// Requires map context.
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
		/// Requires map context.
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
		/// <remarks>Requires map context for synchronization.</remarks>
		internal void FinishLogout()
		{
			RealmServer.IOQueue.AddMessage(new Message(() =>
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

			// take Player out of world context
			if (!World.RemoveCharacter(this))
			{
				// was already removed
				return;
			}

			// client might now do other things
			m_client.ActiveCharacter = null;
			Account.ActiveCharacter = null;

			// set to false so it can't be cancelled anymore
			m_isLoggingOut = false;

			// get rid of any minions, totems and their summons
			RemoveSummonedEntourage();

			// jump out of vehicle
			DetatchFromVechicle();

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

			m_Map.RemoveObjectNow(this);

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
		/// Requires map context.
		/// </remarks>
		public void Kick(string msg)
		{
			Kick(null, msg, 0);
		}

		/// <summary>
		/// Kicks this Character with the given msg after the given delay in seconds.
		/// Requires map context.
		/// </summary>
		/// <param name="delay">The delay until the Client should be disconnected in seconds</param>
		public void Kick(string reason, float delay)
		{
			Kick(reason, delay);
		}

		/// <summary>
		/// Broadcasts a kick message and then kicks this Character after the default delay.
		/// Requires map context.
		/// </summary>
		public void Kick(Character kicker, string reason)
		{
			Kick(kicker, reason, DefaultLogoutDelayMillis);
		}

		/// <summary>
		/// Broadcasts a kick message and then kicks this Character after the default delay.
		/// Requires map context.
		/// </summary>
		public void Kick(INamed kicker, string reason, int delay)
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

			if (m_activePet != null)
			{
				m_activePet.Delete();
				m_activePet = null;
			}

			m_minions = null;
			m_activePet = null;

			if (m_skills != null)
			{
				m_skills.m_owner = null;
				m_skills = null;
			}

			if (m_talents != null)
			{
				m_talents.Owner = null;
				m_talents = null;
			}

			if (m_inventory != null)
			{
				m_inventory.m_container = null;
				m_inventory.m_owner = null;
				m_inventory.m_ammo = null;
				m_inventory.m_currentBanker = null;
				m_inventory = null;
			}

			if (m_mailAccount != null)
			{
				m_mailAccount.Owner = null;
				m_mailAccount = null;
			}

			m_groupMember = null;

			if (m_reputations != null)
			{
				m_reputations.Owner = null;
				m_reputations = null;
			}

			if (m_InstanceCollection != null)
			{
				m_InstanceCollection.Dispose();
			}

			if (m_achievements != null)
			{
				m_achievements.m_owner = null;
				m_achievements = null;
			}

			if (m_CasterReference != null)
			{
				m_CasterReference.Object = null;
				m_CasterReference = null;
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
			WorldObjectSetPool.Recycle(KnownObjects);
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