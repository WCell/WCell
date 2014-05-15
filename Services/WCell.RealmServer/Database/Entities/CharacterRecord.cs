using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NHibernate.Criterion;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.Login;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.Constants.World;
using WCell.Core;
using WCell.RealmServer.Achievements;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Global;
using WCell.RealmServer.Groups;
using WCell.RealmServer.Guilds;
using WCell.RealmServer.Instances;
using WCell.RealmServer.Interaction;
using WCell.RealmServer.Mail;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Pets;
using WCell.RealmServer.RacesClasses;
using WCell.RealmServer.Talents;
using WCell.Util.Threading;

namespace WCell.RealmServer.Database.Entities
{
	public class CharacterRecord : ILivingEntity, IMapId, IActivePetSettings
	{
		#region Static
		private static readonly Logger Log = LogManager.GetCurrentClassLogger();

		public static readonly CharacterRecord[] EmptyArray = new CharacterRecord[] { };

	    private static bool _idGeneratorInitialised;
	    private static long _highestId;
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
        private static void Init()
        {
            long highestId;
            try
            {
				CharacterRecord highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<CharacterRecord>().OrderBy(record => record.EntityLowId).Desc.Take(1).SingleOrDefault();
				highestId = highestItem != null ? highestItem.EntityLowId : 0;

				//var records = RealmWorldDBMgr.DatabaseProvider.Query<CharacterRecord>().ToList();
	            //highestId = records.Any() ? records.Max(characterRecord => characterRecord.EntityLowId) : 0;
	            //highestId = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<CharacterRecord>().Select(Projections.ProjectionList().Add(Projections.Max<CharacterRecord>(x => x.Guid))).List<long>().First();
            }
            catch (Exception e)
            {
                RealmWorldDBMgr.OnDBError(e);
				CharacterRecord highestItem = null;
				highestItem = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<CharacterRecord>().OrderBy(record => record.EntityLowId).Desc.Take(1).SingleOrDefault();
				highestId = highestItem != null ? highestItem.EntityLowId : 0;
			}

            _highestId = (long)Convert.ChangeType(highestId, typeof(long));

            if (_highestId < LowestCharId)
            {
                _highestId = LowestCharId;
            }
            _idGeneratorInitialised = true;
        }

        public static long LastId
        {
            get
            {
                if (!_idGeneratorInitialised)
                    Init();
                return Interlocked.Read(ref _highestId);
            }
        }

        public static long NextId()
        {
            if (!_idGeneratorInitialised)
                Init();

            return Interlocked.Increment(ref _highestId);
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
                    EntityLowId = (uint)NextId(),
					Name = name,
					Created = DateTime.Now
				};
			}
			catch (Exception ex)
			{
				Log.Error("Character creation error (DBS: " + RealmServerConfiguration.DBType + "): ", ex);
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
				return RealmWorldDBMgr.DatabaseProvider.FindOne<CharacterRecord>(x => x.Name == name);
			}
			catch (Exception ex)
			{
                RealmWorldDBMgr.OnDBError(ex);
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
                return RealmWorldDBMgr.DatabaseProvider.Exists<CharacterRecord>(x => x.Name == characterName);
			}
			catch (Exception ex)
			{
				RealmWorldDBMgr.OnDBError(ex);
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
                return RealmWorldDBMgr.DatabaseProvider.Exists<CharacterRecord>(x => x.EntityLowId == entityLowId);
			}
			catch (Exception ex)
			{
                RealmWorldDBMgr.OnDBError(ex);
				return false;
			}
		}

		/// <summary>
		/// Retrieves a CharacterRecord based on a character's entity ID
		/// </summary>
		/// <param name="lowUid">the character unique ID</param>		/// <returns>the corresponding <seealso cref="CharacterRecord"/></returns>
		public static CharacterRecord LoadRecordByEntityId(uint lowUid)
		{
            return RealmWorldDBMgr.DatabaseProvider.FindOne<CharacterRecord>(x => x.EntityLowId == lowUid);
		}

		/// <summary>
		/// Retrieves a CharacterRecord based on a character's entity ID.
		/// </summary>
		/// <returns>the corresponding <seealso cref="CharacterRecord"/></returns>
		public static CharacterRecord LoadRecordById(long guid)
		{
            return RealmWorldDBMgr.DatabaseProvider.FindOne<CharacterRecord>(x => x.Guid == guid);
		}

		public static int GetCount()
		{
            return RealmWorldDBMgr.DatabaseProvider.Count<CharacterRecord>();
		}
		#endregion

		private DateTime? _lastLogin;

		private ICollection<ItemRecord> _loadedItems;

		protected CharacterRecord()
		{
			CanSave = true;
			AbilitySpells = new List<SpellRecord>();
		}

		public CharacterRecord(long accountId)
			: this()
		{
			JustCreated = true;

			AccountId = accountId;
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

		public long Guid
		{
			get;
			set;
		}

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
                Guid = (long)EntityId.GetPlayerId(value).Full;
            }
		}

		public EntityId EntityId
		{
			get
			{
				return EntityId.GetPlayerId(EntityLowId);
			}
		}

		public string Name
		{
			get;
			set;
		}

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

		public DateTime? LastLogin
		{
			get
			{
				return _lastLogin;
			}
			set
			{
				_lastLogin = value;
				if (_lastLogin == null)
				{
					JustCreated = true;
				}
			}
		}

		public DateTime? LastLogout
		{
			get;
			set;
		}

		public CharEnumFlags CharacterFlags
		{
			get;
			set;
		}

		public RaceId Race
		{
			get;
			set;
		}

		public ClassId Class
        {
            get;
            set;
        }

		public GenderType Gender
		{
			get;
			set;
		}

		public byte ActionBarMask { get; set; }

		public byte Skin
		{
			get;
			set;
		}

		public byte Face
		{
			get;
			set;
		}

		public byte HairStyle
		{
			get;
			set;
		}

		public byte HairColor
		{
			get;
			set;
		}

		public byte FacialHair
		{
			get;
			set;
		}

		public byte Outfit
		{
			get;
			set;
		}

		public int Level
		{
			get;
			set;
		}

		public int Xp
		{
			get;
			set;
		}

		public int WatchedFaction
        {
            get;
            set;
        }

		public uint DisplayId
		{
		    get;
            set;
        }

		public int TotalPlayTime
		{
			get;
			set;
		}

		public int LevelPlayTime
		{
			get;
			set;
		}

		public byte[] TutorialFlags
		{
			get;
			set;
		}

		public byte[] ExploredZones
		{
			get;
			set;
		}

		#endregion

		#region Location
		public float PositionX
		{
			get;
			set;
		}

		public float PositionY
		{
			get;
			set;
		}

		public float PositionZ
		{
			get;
			set;
		}

		public float Orientation
		{
			get;
			set;
		}

		public MapId MapId
        {
            get;
            set;
        }

		public uint InstanceId
		{
			get;
			set;
		}

		public ZoneId Zone
        {
            get;
            set;
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

		public MapId CorpseMap
        {
            get;
            set;
        }

		/// <summary>
		/// If CorpseX is null, there is no Corpse
		/// </summary>
		public float? CorpseX
		{
			get;
			set;
		}

		public float CorpseY
		{
			get;
			set;
		}

		public float CorpseZ
		{
			get;
			set;
		}

		public float CorpseO
		{
			get;
			set;
		}
		#endregion

		#region InnKeeper binding
		public float BindX
		{
			get;
			set;
		}

		public float BindY
		{
			get;
			set;
		}

		public float BindZ
		{
			get;
			set;
		}

		public MapId BindMap
        {
            get;
            set;
        }

		public ZoneId BindZone
        {
            get;
            set;
        }

		#endregion

		#region Spells & Auras & Runes
		/// <summary>
		/// Default spells; talents excluded.
		/// Talent spells can be found in <see cref="SpecProfile"/>.
		/// </summary>
		public List<SpellRecord> AbilitySpells
		{
			get;
			private set;
		}

		public int RuneSetMask
		{
			get;
			set;
		}

		public float[] RuneCooldowns
		{

			get;
			set;
		}
		#endregion

		#region Stats
		public int BaseStrength
		{
			get;
			set;
		}

		public int BaseStamina
		{
			get;
			set;
		}

		public int BaseSpirit
		{
			get;
			set;
		}

		public int BaseIntellect
		{
			get;
			set;
		}

		public int BaseAgility
		{
			get;
			set;
		}
		#endregion

		public bool GodMode
		{
			get;
			set;
		}

		#region Dynamic Properties
		public int Health
		{
			get;
			set;
		}

		public int BaseHealth
		{
			get;
			set;
		}

		public int Power
		{
			get;
			set;
		}

		public int BasePower
		{
			get;
			set;
		}

		public long Money
		{
			get;
			set;
		}
		#endregion

		#region Skills
		public IEnumerable<SkillRecord> LoadSkills()
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
		public uint[] FinishedQuests
		{
			get;
			set;
		}

		public uint[] FinishedDailyQuests
		{
			get;
			set;
		}
		#endregion

		#region Mail
		public int MailCount
		{
			get { return Convert.ToInt32(RealmWorldDBMgr.DatabaseProvider.Count<MailMessage>(x => x.ReceiverId == EntityLowId)); }
		}
		#endregion

		#region Guilds

		public uint GuildId
		{
		    get; set; }

		#endregion

		#region Items
		/// <summary>
		/// Returns loaded Items.
		/// Call GetOrLoadItems from the IO-context if Items arent loaded yet.
		/// Characters will have their Items loaded before the Char-selection screen.
		/// </summary>
		public ICollection<ItemRecord> LoadedItems
		{
			get { return _loadedItems; }
		}

		private void LoadItems()
		{
			try
			{
				_loadedItems = ItemRecord.LoadItems(EntityLowId);
			}
			catch (Exception e)
			{
				RealmWorldDBMgr.OnDBError(e);
				_loadedItems = ItemRecord.LoadItems(EntityLowId);
			}
		}

		public ICollection<ItemRecord> GetOrLoadItems()
		{
			if (_loadedItems == null)
			{
				LoadItems();
			}

			return _loadedItems;
		}

		internal void UpdateItems(List<ItemRecord> records)
		{
			_loadedItems = records;
		}

		public List<ItemRecord> GetMailItems(long mailId, int count)
		{
			var items = new List<ItemRecord>(count);
		    items.AddRange(LoadedItems.Where(item => item.MailId == mailId));
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
		public int RestXp
		{
			get;
			set;
		}

		/// <summary>
		/// The id of the AreaTrigger which is letting us rest (or 0 if there is none)
		/// </summary>
		public int RestTriggerId
		{
			get;
			set;
		}
		#endregion

		#region Taxis
		public int NextTaxiVertexId
		{
			get;
			set;
		}

		public uint[] TaxiMask
		{
			get;
			set;
		}
		#endregion


        #region Pets
        public bool IsPetActive
		{
			get;
			set;
		}

		public int StableSlotCount
		{
			get;
			set;
		}

		/// <summary>
		/// Amount of action-bar information etc for summoned pets
		/// </summary>
		public int PetSummonedCount
		{
			get;
			set;
		}

		/// <summary>
		/// Amount of Hunter pets
		/// </summary>
		public int PetCount
		{
			get;
			set;
		}

		public NPCId PetEntryId
		{
		    get; set;
        }

		public NPCEntry PetEntry
		{
			get { return PetEntryId != 0 ? NPCMgr.GetEntry(PetEntryId) : null; }
		}

        public int PetHealth
		{
			get;
			set;
		}

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
		    get; set;
        }

		/// <summary>
		/// Remaining duration in millis
		/// </summary>
		public int PetDuration
		{
			get;
			set;
		}
		#endregion

		#region Talents
		public int CurrentSpecIndex
		{
			get;
			set;
		}

		public DateTime? LastTalentResetTime
		{
			get;
			set;
		}

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
				if (value > (TalentMgr.PlayerTalentResetPricesPerTier.Length - 1))
				{
					value = (TalentMgr.PlayerTalentResetPricesPerTier.Length - 1);
				}
				_talentResetPriceTier = value;
			}
		}
		#endregion

		#region Instances & BGs
		private BattlegroundSide _battlegroundTeam = BattlegroundSide.End;

		public DungeonDifficulty DungeonDifficulty
		{
			get;
			set;
		}

		public RaidDifficulty RaidDifficulty
		{
			get;
			set;
		}

		public BattlegroundSide BattlegroundTeam
		{
			get { return _battlegroundTeam; }
			set { _battlegroundTeam = value; }
		}
		#endregion

		#region Honor & Arena

		public uint KillsTotal
		{
		    get; set;
        }

		public uint HonorToday
		{
		    get; set;
        }

		public uint HonorYesterday
		{
		    get; set;
        }

		public uint LifetimeHonorableKills
		{
		    get; set;
        }

		public uint HonorPoints
		{
		    get; set;
        }

		public uint ArenaPoints
		{
		    get; set;
        }

		#endregion

		#region Delete
		public void DeleteLater()
		{
			RealmServer.IOQueue.AddMessage(new Message(Delete));
		}

		public void Delete()
		{
			TryDelete();
		}

		public void DeleteAndFlush()
		{
			TryDelete();
		}

		public LoginErrorCode TryDelete()
		{
			if (DeleteCharAccessories(EntityLowId))
			{
				DeleteFromGuild(EntityLowId, GuildId);
			    RealmWorldDBMgr.DatabaseProvider.Delete(this);
				return LoginErrorCode.CHAR_DELETE_SUCCESS;
			}
			return LoginErrorCode.CHAR_DELETE_FAILED;
		}

		public static void DeleteChar(uint charId)
		{
			RealmServer.IOQueue.ExecuteInContext(() =>
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
				    RealmWorldDBMgr.DatabaseProvider.Delete<CharacterRecord>(x => x.EntityLowId == charId);
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
                RealmWorldDBMgr.DatabaseProvider.Delete<SpellRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<AuraRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<ItemRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<SkillRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<SpecProfile>(x => x.CharacterGuid == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<ReputationRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<QuestRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<SummonedPetRecord>(x => x.OwnerId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<PermanentPetRecord>(x => x.OwnerId == charId);

				MailMgr.ReturnValueMailFor(charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<MailMessage>(x => x.ReceiverId == charId);

				RelationMgr.Instance.RemoveRelations(charId);
				InstanceMgr.RemoveLog(charId);
				GroupMgr.Instance.RemoveOfflineCharacter(charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<AchievementRecord>(x => x.CharacterId == charId);
                RealmWorldDBMgr.DatabaseProvider.Delete<AchievementProgressRecord>(x => x.CharacterGuid == charId);

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
			Level = archetype.Class.ActualStartLevel;
			PositionX = archetype.StartPosition.X;
			PositionY = archetype.StartPosition.Y;
			PositionZ = archetype.StartPosition.Z;
			Orientation = archetype.StartOrientation;
			MapId = archetype.StartMapId;
			Zone = archetype.StartZoneId;
			TotalPlayTime = 0;
			LevelPlayTime = 0;
			TutorialFlags = new byte[32];
			WatchedFaction = -1;

			DisplayId = archetype.Race.GetDisplayId(Gender);
		}
		#endregion

		#region Find & Get
		/// <summary>
		/// Gets the characters for the given account.
		/// </summary>
		/// <param name="account">the account</param>
		/// <returns>a collection of character objects of the characters on the given account</returns>
		public static IEnumerable<CharacterRecord> FindAllOfAccount(RealmAccount account)
		{
			IEnumerable<CharacterRecord> chrs;
			try
			{
				chrs = RealmWorldDBMgr.DatabaseProvider.FindAll<CharacterRecord>(x => x.AccountId == account.AccountId);
				//var chrs = FindAllByProperty("Created", "AccountId", account.AccountId);
				//chrs.Reverse();
				//return chrs;
			}
			catch (Exception ex)
			{
				RealmWorldDBMgr.OnDBError(ex);
				chrs = RealmWorldDBMgr.DatabaseProvider.FindAll<CharacterRecord>(x => x.AccountId == account.AccountId);
			}
			//chrs.Reverse();
			return chrs;
		}

		public static CharacterRecord GetRecord(uint id)
		{
			var senderChr = World.GetCharacter(id);
            return senderChr == null ? LoadRecordByEntityId(id) : senderChr.Record;
		}

		public static uint GetIdByName(string name)
		{
		    var result = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<CharacterRecord>()
		                   .Where(Restrictions.Where<CharacterRecord>(x => x.Name == name))
		                   .Select(x => x.EntityLowId).Take(1).List<uint>();
		    return result[0];
		}

		public static uint GetGuildId(uint charId)
		{
            var result = RealmWorldDBMgr.DatabaseProvider.Session.QueryOver<CharacterRecord>()
                           .Where(Restrictions.Where<CharacterRecord>(x => x.EntityLowId == charId))
                           .Select(x => x.GuildId).Take(1).List<uint>();
            return result[0];
		}
		#endregion

		#region Aliases
		//[Property]
		public byte[] RawAliases
		{
			get;
			set;
		}

		public void SetAliases(IEnumerable<KeyValuePair<string, string>> aliases)
		{
			var bytes = new List<byte>(100);
			foreach (var alias in aliases)
			{
				// todo: Use client locale to identify correct encoding
				bytes.AddRange(Encoding.UTF8.GetBytes(alias.Key));
				bytes.Add(0);	// 0 is definitely neither in key, nor in value
				bytes.AddRange(Encoding.UTF8.GetBytes(alias.Value));
				bytes.Add(0);	// 0 is definitely neither in key, nor in value
			}
			RawAliases = bytes.ToArray();
		}

		public Dictionary<string, string> ParseAliases()
		{
			var map = new Dictionary<string, string>();
			if (RawAliases != null)
			{
				var isKey = true;
				var keyIndex = 0;
				var valueIndex = -1;
				for (var i = 0; i < RawAliases.Length; i++)
				{
					var b = RawAliases[i];
				    if (b != 0) continue;

				    // found new key or value
				    isKey = !isKey;
				    if (isKey)
				    {
				        // new alias
				        if (valueIndex >= 0)
				        {
				            var key = Encoding.UTF8.GetString(RawAliases, keyIndex, valueIndex - keyIndex);
				            var value = Encoding.UTF8.GetString(RawAliases, valueIndex, i - valueIndex);
				            map[key] = value;
				        }
				        keyIndex = i;
				        valueIndex = -1;
				    }
				    else
				    {
				        // read key already, now read value
				        valueIndex = i;
				    }
				}
			}
			return map;
		}
		#endregion

		public override string ToString()
		{
			return string.Format("{0} (Id: {1}, Account: {2})", Name, EntityLowId, AccountId);
		}
	}
}