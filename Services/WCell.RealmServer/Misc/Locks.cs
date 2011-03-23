using System.Collections.Generic;
using System.Linq;
using WCell.Util.Logging;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Looting;
using WCell.Constants.Skills;
using WCell.Core;
using WCell.Core.DBC;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Looting;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
	/// <summary>
	/// Represents Lock information that are applied to lockable GOs and Items
	/// TODO: Check whether we need to send a animations when interaction with certain LockTypes
	/// </summary>
	public class LockEntry
	{
		internal static int LockInteractionLength = (int)Utility.GetMaxEnum<LockInteractionType>() + 1;
		private static Logger log = LogManager.GetCurrentClassLogger();

		public static readonly SkillId[] InteractionSkills = new SkillId[LockInteractionLength];
		public static readonly InteractionHandler[] InteractionHandlers = new InteractionHandler[LockInteractionLength];
		public delegate void InteractionHandler(ILockable lockable, Character user);

		public static void Handle(Character chr, ILockable lockable, LockInteractionType type)
		{
			var handler = InteractionHandlers.Get((uint)type);
			if (handler != null)
			{
				handler(lockable, chr);
			}
			else
			{
				log.Error("{0} trying to interact with lockable \"{1}\", but the used InteractionType \"{2}\" is not handled.",
					chr, lockable, type);
			}
		}

		/// <summary>
		/// All Lock-entries, indexed by their id
		/// </summary>
		public static readonly LockEntry[] Entries = new LockEntry[2000];

		public readonly uint Id;
		/// <summary>
		/// 0 or more professions that can be used to open the Lock
		/// </summary>
		public LockOpeningMethod[] OpeningMethods;

		/// <summary>
		/// 0 or more key-entries that can be used to open the Lock
		/// </summary>
		public LockKeyEntry[] Keys;

		/// <summary>
		/// Whether the lock requires kneeling when being used
		/// </summary>
		public bool RequiresKneeling;

		/// <summary>
		/// Whether the user swings at the object when using its lock
		/// </summary>
		public bool RequiresAttack;

		/// <summary>
		/// 
		/// </summary>
		public bool RequiresVehicle;

		/// <summary>
		/// Whether the lock does not require any skill or key to be opened.
		/// </summary>
		public bool IsUnlocked;

		/// <summary>
		/// Whether the lock can be closed
		/// </summary>
		public bool CanBeClosed;

		public LockEntry(uint id)
		{
			Id = id;
		}

		public override string ToString()
		{
			var strs = new List<string>();
			if (OpeningMethods.Length > 0)
			{
				strs.Add("Opening Methods: " + OpeningMethods.ToString(", "));
			}
			if (Keys.Length > 0)
			{
				strs.Add("Keys: " + Keys.ToString(", "));
			}
			else if (IsUnlocked)
			{
				strs.Add("Unlocked");
			}
			if (CanBeClosed)
			{
				strs.Add("Closable");
			}
			return strs.ToString("; ");
		}

		[Initialization(InitializationPass.First, null)] // anonymous Init-step
		public static void Initialize()
		{
			if (InteractionHandlers.First() == null)
			{
				InitTypes();

				LoadLocks();
			}
		}

		private static void InitTypes()
		{
			InteractionSkills[(int)LockInteractionType.Blasting] = SkillId.Engineering;
			InteractionSkills[(int)LockInteractionType.Fishing] = SkillId.Fishing;
			InteractionSkills[(int)LockInteractionType.Herbalism] = SkillId.Herbalism;
			InteractionSkills[(int)LockInteractionType.Mining] = SkillId.Mining;
			InteractionSkills[(int)LockInteractionType.PickLock] = SkillId.Lockpicking;
			InteractionSkills[(int)LockInteractionType.Inscription] = SkillId.Inscription;


			InteractionHandlers[(int)LockInteractionType.Blasting] = Loot;
			InteractionHandlers[(int)LockInteractionType.Close] = Close;
			InteractionHandlers[(int)LockInteractionType.DisarmTrap] = DisarmTrap;
			InteractionHandlers[(int)LockInteractionType.Fishing] = Loot;
			InteractionHandlers[(int)LockInteractionType.Herbalism] = Loot;
			InteractionHandlers[(int)LockInteractionType.Mining] = Loot;
			InteractionHandlers[(int)LockInteractionType.None] = Open;
			InteractionHandlers[(int)LockInteractionType.Open] = Open;
			InteractionHandlers[(int)LockInteractionType.OpenAttacking] = Open;
			InteractionHandlers[(int)LockInteractionType.OpenKneeling] = Open;
			InteractionHandlers[(int)LockInteractionType.OpenTinkering] = Open;
			InteractionHandlers[(int)LockInteractionType.PickLock] = Loot;
			InteractionHandlers[(int)LockInteractionType.QuickClose] = Close;
			InteractionHandlers[(int)LockInteractionType.QuickOpen] = Open;
			InteractionHandlers[(int)LockInteractionType.PvPClose] = Close;
			InteractionHandlers[(int)LockInteractionType.PvPOpen] = Open;
		}

		private static void LoadLocks()
		{
			//DBCReader<LockEntry, LockConverter> reader =
			new MappedDBCReader<LockEntry, LockConverter>(
                RealmServerConfiguration.GetDBCFile(WCellDef.DBC_LOCKS));
		}

		class LockConverter : AdvancedDBCRecordConverter<LockEntry>
		{
			public override LockEntry ConvertTo(byte[] rawData, ref int id)
			{
				var entry = new LockEntry((uint)(id = rawData.GetInt32(0)));

				var methods = new List<LockOpeningMethod>(5);
				var keys = new List<LockKeyEntry>(5);

				uint typeIndex = 1;
				uint methodIndex = 9;
				uint skillIndex = 17;
				for (uint i = 0; i < 5; i++)
				{
					var type = (LockInteractionGroup)rawData.GetUInt32(typeIndex++);
					if (type == LockInteractionGroup.Key)
					{
						var key = rawData.GetUInt32(methodIndex);
						keys.Add(new LockKeyEntry(i, key));
					}
					else if (type == LockInteractionGroup.Profession)
					{
						var method = (LockInteractionType)rawData.GetUInt32(methodIndex);
						if (method == LockInteractionType.None)
						{
							continue;
						}

						if (method == LockInteractionType.Close ||
							method == LockInteractionType.QuickClose ||
							method == LockInteractionType.PvPClose)
						{
							entry.CanBeClosed = true;
						}
						else if (method == LockInteractionType.OpenKneeling)
						{
							entry.RequiresKneeling = true;
						}
						else if (method == LockInteractionType.OpenAttacking)
						{
							entry.RequiresAttack = true;
						}
						else
						{
							var skill = InteractionSkills[(uint)method];
							if (skill != SkillId.None)
							{
								var methodEntry = new LockOpeningMethod(i);
								methodEntry.InteractionType = method;
								methodEntry.RequiredSkill = skill;
								methodEntry.RequiredSkillValue = rawData.GetUInt32(skillIndex);
								methods.Add(methodEntry);
							}
						}
					}

					methodIndex++;
					skillIndex++;
				}

				// no professions or keys required to open this lock
				entry.IsUnlocked = methods.Count == 0 && keys.Count == 0;

				entry.Keys = keys.ToArray();
				entry.OpeningMethods = methods.ToArray();

				Entries[entry.Id] = entry;

				return entry;
			}
		}

		/// <summary>
		/// Whether this lock can be interacted with, using the given type
		/// </summary>
		public bool Supports(LockInteractionType type)
		{
			foreach (var method in OpeningMethods)
			{
				if (method.InteractionType == type)
				{
					return true;
				}
			}
			return false;
		}


		#region Lock Interaction Handlers
		static void BreakOpen(ILockable lockable, Character user)
		{

		}

		/// <summary>
		/// Open an object
		/// </summary>
		public static void Close(ILockable lockable, Character user)
		{
			if (lockable is GameObject)
			{
				var go = lockable as GameObject;
				go.State = GameObjectState.Disabled;
			}
		}

		/// <summary>
		/// Disarm a trap
		/// </summary>
		public static void DisarmTrap(ILockable trap, Character user)
		{

		}

		/// <summary>
		/// Loot a container's contents
		/// </summary>
		public static void Loot(ILockable lockable, Character user)
		{
			if (lockable is Item)
			{
				LootMgr.CreateAndSendObjectLoot(lockable, user, LootEntryType.Item, user.Region.IsHeroic);
			}
			else if (lockable is GameObject)
			{
				((GameObject)lockable).Handler.Use(user);
			}
			else
			{
				log.Error("{0} tried to loot invalid object: " + lockable, user);
				return;
			}
		}

		/// <summary>
		/// Open a GameObject.
		/// </summary>
		public static void Open(ILockable lockable, Character chr)
		{
			if (lockable is GameObject)
			{
				var go = lockable as GameObject;
				//go.State = GameObjectState.Enabled;
				go.Use(chr);
			}
		}
		#endregion
	}

	/// <summary>
	/// Different ways of interacting with a lock
	/// </summary>
	public class LockOpeningMethod
	{
		/// <summary>
		/// The index within the LockEntry
		/// </summary>
		public readonly uint Index;

		/// <summary>
		/// What kind of method is this (we don't use a key)
		/// </summary>
		public LockInteractionType InteractionType;

		/// <summary>
		/// The profession required to open this Lock
		/// </summary>
		public SkillId RequiredSkill;

		/// <summary>
		/// Required value in the Profession
		/// </summary>
		public uint RequiredSkillValue;

		public LockOpeningMethod(uint index)
		{
			Index = index;
		}

		public override string ToString()
		{
			return InteractionType + (RequiredSkillValue > 0 ? " (Requires: " + RequiredSkillValue + " " + RequiredSkill + ")" : "");
		}
	}

	public class LockKeyEntry
	{
		/// <summary>
		/// The index within the LockEntry
		/// </summary>
		public uint Index;

		/// <summary>
		/// Id of the required Key-Item
		/// </summary>
		public readonly ItemId KeyId;

		public LockKeyEntry(uint index, uint keyId)
		{
			Index = index;
			KeyId = (ItemId)keyId;
		}

		public override string ToString()
		{
			return KeyId.ToString();
		}
	}
}