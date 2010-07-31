using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Factions;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Factions;
using WCell.Constants;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Taxi;
using System.Collections;
using WCell.Util;
using WCell.Util.Data;
using WCell.Constants.Items;

namespace WCell.RealmServer.NPCs
{
	public class NPCPrototypeStorage : IValueSetter
	{
		public object Get(object key)
		{
			return NPCMgr.GetPrototype((uint)key);
		}

		public void Set(object value)
		{
			ArrayUtil.Set(ref NPCMgr.Prototypes, ((NPCPrototype)value).Id, ((NPCPrototype)value));
		}
	}

	[DataHolder(typeof(NPCPrototypeStorage))]
	public class NPCPrototype : IDataHolder
	{
		public uint Id;

		public uint MaxLevel;

		public uint MinLevel;

		/// <summary>
		/// Faction might vary per spawn
		/// </summary>
		public FactionTemplateId DefaultFactionId;

		public uint MinHealth;

		public uint MaxHealth;

		public uint Mana;

		public float Scale;

		public NPCEntryMask Flags;

		public uint AttackTime;

		public uint RangedAttackTime;

		public uint OffhandAttackTime;

		public uint AttackPower;

		public uint RangedAttackPower;

		public uint OffhandAttackPower;

		public DamageSchool AttackType;

		public float MinDamage;

		public float MaxDamage;

		public float RangedMinDamage;

		public float RangedMaxDamage;

		public float OffhandMinDamage;

		public float OffhandMaxDamage;

		public uint EquipmentEntry;

		public int SpellGroup;

		public int RespawnTime;

		[Persistent(ItemConstants.MaxResCount)]
		public uint[] Resistances;

		public float CombatReach;

		public float BoundingRadius;

		// TODO: Tricky one
		[NotPersistent]
		public SpellId[] AuraIds;

		public bool IsBoss;

		public uint MoneyDrop;

		public byte InvisibilityType;

		public byte DeathState;

		public UnitDynamicFlags DynamicFlags;

		public int ExtraFlags;

		public int MovementType;

		public float WalkSpeed;

		public float RunSpeed;

		public float FlySpeed;

		public int InhabitType;

		public bool Regenerates;

		public int RespawnMod;

		public int ArmorMod;

		public int DamageMod;

		public int HealthMod;

		public UpdateFlags ExtraA9Flags;

		[NotPersistent]
		public GossipMenu DefaultGossip;

		[NotPersistent]
		/// <summary>
		/// Should be called when a new NPC is created
		/// </summary>
		public NPCTypeHandler[] InstanceTypeHandlers;

		[NotPersistent]
		/// <summary>
		/// Should be called when a new NPCSpawnEntry is created
		/// </summary>
		public NPCSpawnTypeHandler[] SpawnTypeHandlers;

		[NotPersistent]
		public Spell[] Auras;

		[NotPersistent]
		public NPCEntry Entry;

		[NotPersistent]
		public Faction DefaultFaction;

		[NotPersistent]
		public NPCId NPCId;

		[NotPersistent]
		/// <summary>
		/// All bits of <see cref="Flags"/> that are set
		/// </summary>
		public uint[] SetFlagIndeces;

		public object GetId() { return Id; }
		/// <summary>
		/// Is called to initialize the object; usually after a set of other operations have been performed or if
		/// the right time has come and other required steps have been performed.
		/// </summary>
		public void FinalizeAfterLoad()
		{
			NPCId = (NPCId)Id;
			SetFlagIndeces = Utility.GetSetIndices((uint)Flags);
			DefaultFaction = FactionMgr.Get(DefaultFactionId);

			if (AuraIds != null)
			{
				var auras = new List<Spell>(AuraIds.Length);
				foreach (var auraId in AuraIds)
				{
					var spell = SpellHandler.Get(auraId);
					if (spell != null)
					{
						auras.Add(spell);
					}
				}
				Auras = auras.ToArray();
			}
			else
			{
				Auras = Spell.EmptyArray;
			}

			var entry = NPCMgr.GetEntry(Id);
			if (entry != null)
			{
				// set the prototype
				entry.Prototype = this;
				Entry = entry;
			}

			InstanceTypeHandlers = NPCMgr.GetNPCTypeHandlers(this);
			SpawnTypeHandlers = NPCMgr.GetNPCSpawnTypeHandlers(this);
		}

	}
}