using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Queries;
using NHibernate.Criterion;
using NLog;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.Core.Database;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.Talents;
using WCell.Util.Threading;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.RacesClasses;
using WCell.Util;
using WCell.Util.NLog;
using WCell.RealmServer.Global;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Groups;
using WCell.Constants.NPCs;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Mail;
using WCell.RealmServer.Guilds;

namespace WCell.RealmServer.Database
{
	[ActiveRecord(Access = PropertyAccess.Property)]
	public class CharacterRecord : WCellRecord<CharacterRecord>, ILivingEntity, IRegionId, IActivePetSettings
	{
		#region Static
		private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
		private static readonly Order CreatedOrder = new Order("Created", true);

		public static readonly CharacterRecord[] EmptyArray = new CharacterRecord[] { };

		/// <summary>
		/// Character will not have Ids below this threshold. 
		/// You can use those unused ids for self-implemented mechanisms, eg to fake participants in chat-channels etc.
		/// </summary>
		/// <remarks>
		/// Do not change this value once the first Character exists.
		/// If you want to change this value to reserve more (or less) ids for other use, make sure
		/// that none of the ids below this threshold are in the DB.
		/// </remarks>
		public const long LowestCharId = 1000;

		protected static readonly NHIdGenerator _idGenerator = new NHIdGenerator(typeof(CharacterRecord), "Guid", LowestCharId);

		/// <summary>
		/// Returns the next unique Id for a new Character
		/// </summary>
		public static uint NextId()
		{
			return (uint)_idGenerator.Next();
		}

		/// <summary>
		/// Creates a new CharacterRecord row in the database with the given information.
		/// </summary>
		/// <param name="account">the account this character is on</param>
		/// <param name="name">the name of the new character</param>
		/// <returns>the <seealso cref="CharacterRecord"/> object</returns>
		public static CharacterRecord CreateNewCharacterRecord(RealmAccount account, string name)
		{
			CharacterRecord record;

			try
			{
				record = new CharacterRecord(account.AccountId)
				{
					EntityLowId = (uint)_idGenerator.Next(),
					Name = name,
					Created = DateTime.Now
				};
			}
			catch (Exception ex)
			{
				s_log.Error("Character creation error (DBS: " + RealmServerConfiguration.DBType + "): ", ex);
				record = null;
			}

			return record;
		}

		/// <summary>
		/// Retrieves a CharacterRecord based on the character name
		/// </summary>
		/// <param name="name">the character name</param>
		/// <returns>the corresponding <seealso cref="CharacterRecord"/></returns>
		public static CharacterRecord GetRecordByName(string name)
		{
			try
			{
				return FindOne(Restrictions.Like("Name", name));
			}
			catch (Exception ex)
			{
				RealmDBUtil.OnDBError(ex);
				return null;
			}
		}

		/// <summary>
		/// Checks if a character with the given name already exists.
		/// </summary>
		/// <param name="characterName">the name to check for</param>
		/// <returns>true if the character exists; false otherwise</returns>
		public static bool Exists(string characterName)
		{
			try
			{
				return Exists((ICriterion)Restrictions.Like("Name", characterName));
			}
			catch (Exception ex)
			{
				RealmDBUtil.OnDBError(ex);
				return false;
			}
		}

		/// <summary>
		/// Checks if a character with the given Id already exists.
		/// </summary>
		public static bool Exists(uint entityLowId)
		{
			try
			{
				return Exists((ICriterion)Restrictions.Eq("Guid", (long)entityLowId));
			}
			catch (Exception ex)
			{
				RealmDBUtil.OnDBError(ex);
				return false;
			}
		}

		/// <summary>
		/// Retrieves a CharacterRecord based on a character's entity ID
		/// </summary>
		/// <param name="lowUid">the character unique ID</param>		/// <returns>the corresponding <seealso cref="CharacterRecord"/></returns>
		public static CharacterRecord LoadRecordByEntityId(uint lowUid)
		{
			return FindOne(Restrictions.Eq("Guid", (long)lowUid));
		}

		/// <summary>
		/// Retrieves a CharacterRecord based on a character's entity ID.
		/// </summary>
		/// <returns>the corresponding <seealso cref="CharacterRecord"/></returns>
		public static CharacterRecord LoadRecordByID(long guid)
		{
			return FindOne(Restrictions.Eq("CharacterId", guid));
		}

		public static int GetCount()
		{
			return Count();
		}
		#endregion


		[Field("DisplayId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _displayId;
		[Field("WatchedFaction", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int _watchedFaction;
		[Field("ClassId", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int m_Class;
		[Field("Region", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int m_Region;
		[Field("CorpseRegion", Access = PropertyAccess.FieldCamelcase)]
		private int m_CorpseRegion;
		[Field("Zone", Access = PropertyAccess.FieldCamelcase)]
		private int m_zoneId;
		[Field("BindZone", Access = PropertyAccess.FieldCamelcase)]
		private int m_BindZone;
		[Field("BindRegion", NotNull = true, Access = PropertyAccess.FieldCamelcase)]
		private int m_BindRegion;

		private DateTime? m_lastLogin;

		private ICollection<ItemRecord> m_loadedItems;

		protected CharacterRecord()
		{
			CanSave = true;
		}

		public CharacterRecord(long accountId)
		{
			New = true;
			JustCreated = true;

			AccountId = accountId;
			CanSave = true;
			Spells = new Dictionary<uint, SpellRecord>();
			ExploredZones = new byte[UpdateFieldMgr.ExplorationZoneFieldSize * 4];
		}

		public virtual Character CreateCharacter()
		{
			return new Character();
		}

		public bool JustCreated
		{
			get;
			internal set;
		}

		/// <summary>
		/// Whether this record should be saved to DB
		/// </summary>
		public bool CanSave
		{
			get;
			set;
		}

		public DateTime LastSaveTime
		{
			get;
			internal set;
		}

		#region Misc

		[PrimaryKey(PrimaryKeyType.Assigned, "EntityLowId")]
		public long Guid
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public long AccountId
		{
			get;
			set;
		}

		public uint EntityLowId
		{
			get
			{
				return (uint)Guid;
			}
			set
			{
				Guid = (int)value;
			}
		}

		public EntityId EntityId
		{
			get
			{
				return EntityId.GetPlayerId(EntityLowId);
			}
		}

		[Property(Length = 12, NotNull = true, Unique = true)]
		public string Name
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public DateTime Created
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the Character that this Record belongs to is currently logged in.
		/// </summary>
		public bool IsOnline
		{
			get { return LastLogin != null && LastLogin > RealmServer.StartTime && (LastLogout == null || LastLogout < LastLogin); }
		}

		[Property]
		public DateTime? LastLogin
		{
			get
			{
				return m_lastLogin;
			}
			set
			{
				m_lastLogin = value;
				if (m_lastLogin == null)
				{
					JustCreated = true;
				}
			}
		}

		[Property]
		public DateTime? LastLogout
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public CharEnumFlags CharacterFlags
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public RaceId Race
		{
			get;
			set;
		}

		public ClassId Class
		{
			get { return (ClassId)m_Class; }
			set { m_Class = (int)value; }
		}

		[Property(NotNull = true)]
		public GenderType Gender
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public byte Skin
		{
			get;
			set;
		}

		[Property("face", NotNull = true)]
		public byte Face
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public byte HairStyle
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public byte HairColor
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public byte FacialHair
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public byte Outfit
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Level
		{
			get;
			set;
		}

		[Property]
		public int Xp
		{
			get;
			set;
		}

		public int WatchedFaction
		{
			get { return _watchedFaction; }
			set { _watchedFaction = value; }
		}

		public uint DisplayId
		{
			get { return (uint)_displayId; }
			set { _displayId = (int)value; }
		}

		[Property(NotNull = true)]
		public int TotalPlayTime
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int LevelPlayTime
		{
			get;
			set;
		}

		[Property(ColumnType = "BinaryBlob", Length = 32, NotNull = true)]
		public byte[] TutorialFlags
		{
			get;
			set;
		}

		[Property(ColumnType = "BinaryBlob")]
		public byte[] ActionButtons
		{
			get;
			set;
		}

		[Property(ColumnType = "BinaryBlob")]
		public byte[] ExploredZones
		{
			get;
			set;
		}

		#endregion

		#region Location
		[Property(NotNull = true)]
		public float PositionX
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public float PositionY
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public float PositionZ
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public float Orientation
		{
			get;
			set;
		}

		public MapId RegionId
		{
			get { return (MapId)m_Region; }
			set { m_Region = (int)value; }
		}

		public uint InstanceId
		{
			get;
			set;
		}

		public ZoneId Zone
		{
			get { return (ZoneId)m_zoneId; }
			set { m_zoneId = (int)value; }
		}

		#endregion

		#region Death
		public DateTime LastDeathTime
		{
			get;
			set;
		}

		/// <summary>
		/// Time of last resurrection
		/// </summary>
		public DateTime LastResTime
		{
			get;
			set;
		}

		public MapId CorpseRegion
		{
			get { return (MapId)m_CorpseRegion; }
			set { m_CorpseRegion = (int)value; }
		}

		/// <summary>
		/// If CorpseX is null, there is no Corpse
		/// </summary>
		[Property]
		public float? CorpseX
		{
			get;
			set;
		}

		[Property]
		public float CorpseY
		{
			get;
			set;
		}

		[Property]
		public float CorpseZ
		{
			get;
			set;
		}

		[Property]
		public float CorpseO
		{
			get;
			set;
		}
		#endregion

		#region InnKeeper binding
		[Property(NotNull = true)]
		public float BindX
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public float BindY
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public float BindZ
		{
			get;
			set;
		}

		public MapId BindRegion
		{
			get { return (MapId)m_BindRegion; }
			set { m_BindRegion = (int)value; }
		}

		public ZoneId BindZone
		{
			get { return (ZoneId)m_BindZone; }
			set { m_BindZone = (int)value; }
		}

		#endregion

		#region Spells & Auras & Runes

		/// <summary>
		/// Adds the given Spell and returns the newly created SpellRecord object.
		/// Returns null if Spell already existed
		/// </summary>
		internal SpellRecord AddSpell(uint spellId)
		{
			if (Spells != null)
			{
				if (Spells.ContainsKey(spellId))
				{
					LogUtil.ErrorException(new ArgumentException("Spell cannot be added twice"),
						string.Format("Spell ({0}, Id: {1}) added twice to Character {2} (Race: {3}, Class: {4})",
									  (SpellId)spellId,
									  spellId,
									  this,
									  Race,
									  Class));
					return null;
				}

				var record = new SpellRecord(spellId, EntityLowId);
				Spells.Add(spellId, record);

				RealmServer.Instance.AddMessage(new Message(record.Save));

				return record;
			}
			return null;
		}

		internal bool RemoveSpell(uint id)
		{
			SpellRecord spell;
			if (Spells.TryGetValue(id, out spell))
			{
				return RemoveSpell(spell);
			}
			return false;
		}

		bool RemoveSpell(SpellRecord record)
		{
			RealmServer.Instance.AddMessage(new Message(record.Delete));
			Spells.Remove(record.SpellId);
			return true;
		}

		internal void LoadSpells()
		{
			if (Spells == null)
			{
				Spells = new Dictionary<uint, SpellRecord>();
				var dbSpells = SpellRecord.FindAllByProperty("m_OwnerId", (int)EntityLowId);
				foreach (var spell in dbSpells)
				{
					try
					{
						Spells.Add(spell.SpellId, spell);
					}
					catch (Exception e)
					{
						LogUtil.ErrorException(e,
							string.Format("Spell {0} of {1} was fetched twice from DB. Loaded spells: {2}", spell.SpellId, this, dbSpells.ToString(", ")));
					}
				}
			}
		}

		public IDictionary<uint, SpellRecord> Spells
		{
			get;
			private set;
		}

		public AuraRecord[] LoadAuraRecords()
		{
			return AuraRecord.FindAllByProperty("m_OwnerId", (int)EntityLowId);
		}

		[Property]
		public int RuneSetMask
		{
			get;
			set;
		}

		[Property]
		public float[] RuneCooldowns
		{
			get;
			set;
		}
		#endregion

		#region Stats
		[Property(NotNull = true)]
		public int BaseStrength
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int BaseStamina
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int BaseSpirit
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int BaseIntellect
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int BaseAgility
		{
			get;
			set;
		}
		#endregion

		[Property]
		public bool GodMode
		{
			get;
			set;
		}

		#region Dynamic Properties
		[Property(NotNull = true)]
		public int Health
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int BaseHealth
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int Power
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public int BasePower
		{
			get;
			set;
		}

		[Property(NotNull = true)]
		public long Money
		{
			get;
			set;
		}
		#endregion

		#region Skills
		public SkillRecord[] LoadSkills()
		{
			return SkillRecord.GetAllSkillsFor(Guid);
		}
		#endregion

		#region Reputations
		internal ReputationRecord CreateReputationRecord()
		{
			return new ReputationRecord { OwnerId = Guid };
		}
		#endregion

		#region Auras
		#endregion

		#region Quests
		[Property("FinishedQuests", NotNull = false)]
		public uint[] FinishedQuests
		{
			get;
			set;
		}

		[Property("FinishedDailyQuests", NotNull = false)]
		public uint[] FinishedDailyQuests
		{
			get;
			set;
		}
		#endregion

		#region Mail
		public int MailCount
		{
			get
			{
				return MailMessage.Count("ReceiverId = " + (uint)Guid);
			}
		}
		#endregion

		#region Guilds
		[Field("GuildId", Access = PropertyAccess.FieldCamelcase)]
		private int m_GuildId;

		public uint GuildId
		{
			get { return (uint)m_GuildId; }
			set { m_GuildId = (int)value; }
		}

		#endregion

		#region Items
		/// <summary>
		/// Returns loaded Items.
		/// Call GetOrLoadItems from the IO-context if Items arent loaded yet.
		/// Characters will have their Items loaded before the Char-selection screen.
		/// </summary>
		public ICollection<ItemRecord> LoadedItems
		{
			get { return m_loadedItems; }
		}

		private void LoadItems()
		{
			try
			{
				m_loadedItems = ItemRecord.LoadItems(EntityLowId);
			}
			catch (Exception e)
			{
				RealmDBUtil.OnDBError(e);
				m_loadedItems = ItemRecord.LoadItems(EntityLowId);
			}
		}

		public ICollection<ItemRecord> GetOrLoadItems()
		{
			if (m_loadedItems == null)
			{
				LoadItems();
			}

			return m_loadedItems;
		}

		internal void UpdateItems(List<ItemRecord> records)
		{
			m_loadedItems = records;
		}

		public List<ItemRecord> GetMailItems(long mailId, int count)
		{
			var items = new List<ItemRecord>(count);
			foreach (var item in LoadedItems)
			{
				if (item.MailId == mailId)
				{
					items.Add(item);
				}
			}
			return items;
		}

		//[HasMany(typeof(EquipmentSet), Inverse = true, Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public IList<EquipmentSet> EquipmentSets
		{
			get;
			set;
		}
		#endregion

		#region Resting
		/// <summary>
		/// Amount of accumulated rest-XP 
		/// </summary>
		[Property]
		public int RestXp
		{
			get;
			set;
		}

		/// <summary>
		/// The id of the AreaTrigger which is letting us rest (or 0 if there is none)
		/// </summary>
		[Property]
		public int RestTriggerId
		{
			get;
			set;
		}
		#endregion

		#region Taxis
		[Property]
		public int NextTaxiVertexId
		{
			get;
			set;
		}

		[Property]
		public uint[] TaxiMask
		{
			get;
			set;
		}
		#endregion

		#region Pets
		[Field("SummonSpell")]
		private int m_SummonSpellId;

		[Field("PetEntryId")]
		private int m_PetEntryId;

		[Property]
		public bool IsPetActive
		{
			get;
			set;
		}

		[Property]
		public int StableSlotCount
		{
			get;
			set;
		}

		/// <summary>
		/// Action-bar information etc for summoned pets
		/// </summary>
		[Property]
		public int PetSummonedCount
		{
			get;
			set;
		}

		/// <summary>
		/// Hunter pets
		/// </summary>
		[Property]
		public int PetCount
		{
			get;
			set;
		}

		public NPCId PetEntryId
		{
			get { return (NPCId)m_PetEntryId; }
			set { m_PetEntryId = (int)value; }
		}

		public NPCEntry PetEntry
		{
			get { return PetEntryId != 0 ? NPCMgr.GetEntry(PetEntryId) : null; }
		}

		[Property]
		public int PetHealth
		{
			get;
			set;
		}

		[Property]
		public int PetPower
		{
			get;
			set;
		}

		/// <summary>
		/// 
		/// </summary>
		public SpellId PetSummonSpellId
		{
			get { return (SpellId)m_SummonSpellId; }
			set { m_SummonSpellId = (int)value; }
		}

		/// <summary>
		/// Remaining duration in millis
		/// </summary>
		[Property]
		public int PetDuration
		{
			get;
			set;
		}
		#endregion

		# region Talents

		[Property("FreeTalentPoints")]
		public int FreeTalentPoints
		{
			get;
			set;
		}

		[Property("LastTalentResetTime")]
		public DateTime? LastTalentResetTime
		{
			get;
			set;
		}

		[Field("TalentResetPriceTier", NotNull = true)]
		private int _talentResetPriceTier;

		public int TalentResetPriceTier
		{
			get { return _talentResetPriceTier; }
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > (TalentMgr.TalentResetPriceTiers.Length - 1))
				{
					value = (TalentMgr.TalentResetPriceTiers.Length - 1);
				}
				_talentResetPriceTier = value;
			}
		}

		public SpecProfile SpecProfile
		{
			get;
			set;
		}

		# endregion

		#region Instances & BGs
		private BattlegroundSide m_BattlegroundTeam = BattlegroundSide.End;

		[Property]
		public DungeonDifficulty DungeonDifficulty
		{
			get;
			set;
		}

		[Property]
		public RaidDifficulty RaidDifficulty
		{
			get;
			set;
		}

		[Property]
		public BattlegroundSide BattlegroundTeam
		{
			get { return m_BattlegroundTeam; }
			set { m_BattlegroundTeam = value; }
		}
		#endregion

		#region Honor & Arena

		[Field("KillsTotal", NotNull = true)]
		private int _killsTotal;
		[Field("HonorToday", NotNull = true)]
		private int _honorToday;
		[Field("HonorYesterday", NotNull = true)]
		private int _honorYesterday;
		[Field("LifetimeHonorableKills", NotNull = true)]
		private int _lifetimeHonorableKills;
		[Field("HonorPoints", NotNull = true)]
		private int _honorPoints;
		[Field("ArenaPoints", NotNull = true)]
		private int _arenaPoints;

		public uint KillsTotal
		{
			get { return (uint)_killsTotal; }
			set { _killsTotal = (int)value; }
		}

		public uint HonorToday
		{
			get { return (uint)_honorToday; }
			set { _honorToday = (int)value; }
		}

		public uint HonorYesterday
		{
			get { return (uint)_honorYesterday; }
			set { _honorYesterday = (int)value; }
		}

		public uint LifetimeHonorableKills
		{
			get { return (uint)_lifetimeHonorableKills; }
			set { _lifetimeHonorableKills = (int)value; }
		}

		public uint HonorPoints
		{
			get { return (uint)_honorPoints; }
			set { _honorPoints = (int)value; }
		}

		public uint ArenaPoints
		{
			get { return (uint)_arenaPoints; }
			set { _arenaPoints = (int)value; }
		}

		#endregion

		#region Delete
		public void DeleteLater()
		{
			RealmServer.Instance.AddMessage(new Message(Delete));
		}

		public override void Delete()
		{
			TryDelete();
		}

		public override void DeleteAndFlush()
		{
			TryDelete();
		}

		public LoginErrorCode TryDelete()
		{
			if (DeleteCharAccessories(EntityLowId))
			{
				DeleteFromGuild(EntityLowId, GuildId);
				base.DeleteAndFlush();
				return LoginErrorCode.CHAR_DELETE_SUCCESS;
			}
			return LoginErrorCode.CHAR_DELETE_FAILED;
		}

		public static void DeleteChar(uint charId)
		{
			RealmServer.Instance.ExecuteInContext(() =>
			{
				var chr = World.GetCharacter(charId);
				uint guildId;
				if (chr != null)
				{
					guildId = chr.GuildId;
					chr.Client.Disconnect();
				}
				else
				{
					guildId = GetGuildId(charId);
				}

				if (DeleteCharAccessories(charId))
				{
					DeleteFromGuild(charId, guildId);
					DeleteAll("Guid = " + charId);
				}
			});
		}

		private static void DeleteFromGuild(uint charId, uint guildId)
		{
			if (guildId != 0)
			{
				var guild = GuildMgr.GetGuild(guildId);
				if (guild != null)
				{
					guild.RemoveMember(charId);
				}
			}
		}

		static bool DeleteCharAccessories(uint charId)
		{
			try
			{
				SpellRecord.DeleteAll("OwnerId = " + charId);
				AuraRecord.DeleteAll("OwnerId = " + charId);
				ItemRecord.DeleteAll("OwnerId = " + charId);
				SkillRecord.DeleteAll("OwnerId = " + charId);
				ReputationRecord.DeleteAll("OwnerId = " + charId);
				QuestRecord.DeleteAll("OwnerId = " + charId);
				SummonedPetRecord.DeleteAll("OwnerLowId = " + charId);
				PermanentPetRecord.DeleteAll("OwnerLowId = " + charId);

				MailMgr.ReturnValueMailFor(charId);
				MailMessage.DeleteAll("ReceiverId = " + charId);

				RelationMgr.Instance.RemoveRelations(charId);
				InstanceMgr.RemoveLog(charId);
				GroupMgr.Instance.RemoveOfflineCharacter(charId);

				return true;
			}
			catch (Exception ex)
			{
				LogUtil.ErrorException(ex, "Failed to delete character with Id: " + charId);

				return false;
			}
		}
		#endregion

		#region Setup
		public void SetupNewRecord(Archetype archetype)
		{
			Race = archetype.Race.Id;
			Class = archetype.Class.Id;
			Level = Math.Max(archetype.Class.StartLevel, BaseClass.DefaultStartLevel);
			PositionX = archetype.StartPosition.X;
			PositionY = archetype.StartPosition.Y;
			PositionZ = archetype.StartPosition.Z;
			Orientation = archetype.StartOrientation;
			RegionId = archetype.StartMapId;
			Zone = archetype.StartZoneId;
			TotalPlayTime = 0;
			LevelPlayTime = 0;
			TutorialFlags = new byte[32];
			WatchedFaction = -1;

			DisplayId = archetype.Race.GetDisplayId(Gender);
			ActionButtons = (byte[])archetype.ActionButtons.Clone();
		}
		#endregion

		#region Find & Get
		/// <summary>
		/// Gets the characters for the given account.
		/// </summary>
		/// <param name="account">the account</param>
		/// <returns>a collection of character objects of the characters on the given account</returns>
		public static CharacterRecord[] FindAllOfAccount(RealmAccount account)
		{
			try
			{
				return FindAll(CreatedOrder, Restrictions.Eq("AccountId", account.AccountId));
				//var chrs = FindAllByProperty("Created", "AccountId", account.AccountId);
				//chrs.Reverse();
				//return chrs;
			}
			catch (Exception ex)
			{
				//log.ErrorException("Failed to get characters for account!", ex);
				RealmDBUtil.OnDBError(ex);
				//return FindAll(CreatedOrder, Restrictions.Eq("AccountId", account.AccountId));
				var chrs = FindAllByProperty("Created", "AccountId", account.AccountId);
				chrs.Reverse();
				return chrs;
			}
		}

		public static CharacterRecord GetRecord(uint id)
		{
			var senderChr = World.GetCharacter(id);
			CharacterRecord sender;
			if (senderChr == null)
			{
				sender = LoadRecordByEntityId(id);
			}
			else
			{
				sender = senderChr.Record;
			}
			return sender;
		}

		public static uint GetIdByName(string name)
		{
			var sql = string.Format("SELECT {0} FROM {1} WHERE {2} = {3} LIMIT 1",
				DatabaseUtil.Dialect.QuoteForColumnName("EntityLowId"),
				DatabaseUtil.Dialect.QuoteForTableName(typeof(CharacterRecord).Name),
				DatabaseUtil.Dialect.QuoteForColumnName("Name"),
				DatabaseUtil.ToSqlValueString(name));
			var query = new ScalarQuery<long>(typeof(CharacterRecord), QueryLanguage.Sql, sql);
			return (uint)query.Execute();
		}

		public static uint GetGuildId(uint charId)
		{
			var sql = string.Format("SELECT {0} FROM {1} WHERE {2} = {3} LIMIT 1",
				DatabaseUtil.Dialect.QuoteForColumnName("GuildId"),
				DatabaseUtil.Dialect.QuoteForTableName(typeof(CharacterRecord).Name),
				DatabaseUtil.Dialect.QuoteForColumnName("Guid"),
				charId);
			var query = new ScalarQuery<int>(typeof(CharacterRecord), QueryLanguage.Sql, sql);
			return (uint)query.Execute();
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0} (Id: {1}, Account: {2})", Name, EntityLowId, AccountId);
		}
	}
}