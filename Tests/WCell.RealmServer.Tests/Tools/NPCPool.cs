using System.Collections.Generic;
using Castle.ActiveRecord.Framework;
using WCell.Constants.Factions;
using WCell.Constants.Items;
using WCell.Constants.NPCs;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Items;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.NPCs.Vendors;
using WCell.RealmServer.Spells;

namespace WCell.RealmServer.Tests.Tools
{
	public class NPCPool
	{
		public const NPCId DefaultMobId = NPCId.UndeadScarab;
		private static readonly UnitModelInfo DefaultModel = new UnitModelInfo();

		public static NPCEntry DefaultMobEntry = new NPCEntry
		{
			DisplayIds = new[] { 10u },
			ModelInfos = new[] { DefaultModel },
			DefaultName = "Default Mob",
			MaxHealth = 100,
			Type = NPCType.Beast,
			InstanceTypeHandlers = new NPCTypeHandler[0],
			AllianceFaction = Faction.NullFaction,
			HordeFaction = Faction.NullFaction,
			NPCCreator = NPCEntry.DefaultCreator,
			BrainCreator = npc => new MobBrain(npc)
		};

		public static NPCEntry DefaultVendorEntry = new NPCEntry
		{
			DisplayIds = new[] { 10u },
			ModelInfos = new[] { DefaultModel },
			DefaultName = "Default Vendor",
			MaxHealth = 100,
			Type = NPCType.Humanoid,
			InstanceTypeHandlers = new NPCTypeHandler[0],
			AllianceFaction = Faction.NullFaction,
			HordeFaction = Faction.NullFaction,
			NPCCreator = NPCEntry.DefaultCreator,
			BrainCreator = npc => new MobBrain(npc)
		};

		public NPC CreateMob()
		{
			return CreateMob(FactionId.Creature);
		}

		public NPC CreateMob(FactionId factionId)
		{
			FactionMgr.Initialize();
			return CreateMob(FactionMgr.Get(factionId));
		}

		public NPC CreateMob(Faction faction)
		{
			Setup.EnsureNPCsLoaded();
			var newNpc = NPCMgr.GetEntry(DefaultMobId).Create();
			return newNpc;
		}

		public NPC CreateDummy()
		{
			return CreateDummy(FactionId.Creature);
		}

		public NPC CreateDummy(FactionId faction)
		{
			FactionMgr.Initialize();
			return CreateDummy(FactionMgr.Get(faction));
		}

		public NPC CreateDummy(Faction faction)
		{
			var npc = DefaultMobEntry.Create();
			npc.Name = "Dummy NPC";
			npc.Faction = faction;
			npc.BaseHealth = 100;
			return npc;
		}

		public NPC CreateVendor()
		{
			return CreateVendor(FactionId.Alliance);
		}

		public NPC CreateVendor(FactionId faction)
		{
			FactionMgr.Initialize();
			return CreateVendor(FactionMgr.Get(faction));
		}

		public NPC CreateVendor(Faction faction)
		{
			var npc = DefaultVendorEntry.Create();
			npc.Name = "Dummy Vendor";
			npc.Faction = faction;
			npc.BaseHealth = 100;
			return npc;
		}
	}
}