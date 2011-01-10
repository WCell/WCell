using WCell.Addons.Default.Lang;
using WCell.RealmServer.NPCs;
using WCell.Constants.NPCs;
using WCell.Constants.Factions;
using WCell.Core.Initialization;
using WCell.RealmServer.Items;
using WCell.Constants.Items;
using WCell.Constants;
using WCell.Util.Graphics;
using WCell.Util.Variables;
using WCell.Constants.Spells;
using WCell.RealmServer.Global;
using WCell.RealmServer.Gossips;

namespace WCell.Addons.Default.Samples
{
	/// <summary>
	/// Contains some hardcoded content to illustrate the API
	/// and provide test data for Developers to test on without having to load the content.
	/// </summary>
	public static class MixedSamples
	{
		/// <summary>
		/// Choose a random id
		/// </summary>
		private const uint BearId = 414256;

		private const uint ItemMinId = 2121232;

		[NotVariable]
		public static NPCEntry GrizzlyBear;

		[NotVariable]
		public static ItemTemplate Bow;

		static uint sampleGossipTextId = 33424u;		// use a random fixed Id (must never change due to the Client cache problem)

		#region Grizzly
		[Initialization()]
		static void SetupGrizzly()
		{
			// default settings
			GrizzlyBear = new NPCEntry
			{
				Id = BearId,
				DefaultName = "Sample Grizzly",
				EntryFlags = NPCEntryFlags.Tamable,
				Type = CreatureType.Humanoid,
				DisplayIds = new[] { 21635u },
				Scale = 1,
				MinLevel = 73,
				MaxLevel = 74,
				HordeFactionId = FactionTemplateId.Monster_2,
				MinHealth = 100000,
				MaxHealth = 500000,
				AttackPower = 314,
				AttackTime = 1500,
				MinDamage = 250,
				MaxDamage = 360,
				WalkSpeed = 2.5f,
				RunSpeed = 8f,
				FlySpeed = 14f,

				MinMana = 2000
			};

			GrizzlyBear.SetResistance(DamageSchool.Physical, 7600);

			// AOE damage spell
			GrizzlyBear.AddSpell(SpellId.ConeOfFire);

			// A spell with a freeze debuff
			GrizzlyBear.AddSpell(SpellId.Chilled);

			// Sample gossip menu
			GossipMgr.AddText(sampleGossipTextId, new GossipText
			{
				Probability = 1,
				TextMale = "Sample Gossip Menu",
				TextFemale = "Take a good look"
			});
			GrizzlyBear.DefaultGossip = CreateSampleGossipMenu();

			GrizzlyBear.FinalizeDataHolder();

			//NPCMgr.AddEntry(BearId, GrizzlyBear);
		}

		/*
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void SetupCustomGossips()
		{
			var entry = NPCMgr.GetEntry(NPCId.StormwindCaptain);
			entry.DefaultGossip = CreateSampleGossipMenu();
		}
		 */

		/// <summary>
		/// A sample GossipMenu
		/// </summary>
		public static GossipMenu CreateSampleGossipMenu()
		{
			return new GossipMenu(
				new GossipMenuItem("Click Me!", convo =>
				{
					convo.Character.SendSystemMessage("You clicked me!");
				}),
				new GossipMenuItem("Icon List", convo =>
				{
					convo.Character.SendSystemMessage("A list of all available Gossip Icons");
				},
					new GossipMenu(										// nested menu
						new MultiStringGossipMenuItem(GossipMenuIcon.Trade, DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.Trade), convo =>
						{
							// Character selected "Trade"
							convo.Speaker.Say("I am not a vendor!");
						}),
						new GossipMenuItem(GossipMenuIcon.Taxi, "Taxi"),
						new GossipMenuItem(GossipMenuIcon.Train, "Train"),
						new GossipMenuItem(GossipMenuIcon.Resurrect, "Resurrect"),
						new GossipMenuItem(GossipMenuIcon.Bind, "Bind"),
						new GossipMenuItem(GossipMenuIcon.Bank, "Bank"),
						new GossipMenuItem(GossipMenuIcon.Guild, "Guild"),
						new GossipMenuItem(GossipMenuIcon.Tabard, "Tabard"),
						new GossipMenuItem(GossipMenuIcon.Battlefield, "Battlefield")
					)
					{
						BodyTextId = sampleGossipTextId
					}
					),
				new GossipMenuItem(GossipMenuIcon.Talk, "I want to go to Stormwind", convo =>
				{
					// Character wants to go to Stormwind
					convo.Character.TeleportTo(WorldLocationMgr.Stormwind);
					convo.StayOpen = false;	// convo is over
				}),
				new GossipMenuItem(GossipMenuIcon.Talk, "I want to go to (0, 0, 1000) in Eastern Kingdoms", convo =>
				{
					// Character wants to be teleported into the Eastern Kingdoms sky
					convo.Character.TeleportTo(World.EasternKingdoms, new Vector3(0, 0, 1000));
					convo.StayOpen = false;	// convo is over
				}),
				new QuitGossipMenuItem()	// convo is over
				)
			{
				// Don't close the menu, unless the user selected a final option
				KeepOpen = true
			};
		}

		#endregion

		#region Bow
		[Initialization]
		static void SetupBow()
		{
			// Farstrider's Bow (ID: 24136 [FarstridersBow])
			Bow = new ItemTemplate
			{
				Id = ItemMinId,
				AttackTime = 1000,
				DefaultName = "Sample Bow",
				Class = ItemClass.Weapon,
				SubClass = ItemSubClass.WeaponBow,
				DisplayId = 18350,
				Quality = ItemQuality.Uncommon,
				BuyPrice = 1421,
				SellPrice = 284,
				Level = 12,
				InventorySlotType = InventorySlotType.WeaponRanged,
				Damages = new[] { new DamageInfo { Minimum = 14, Maximum = 25 } },
				BondType = ItemBondType.OnPickup,
				Material = Material.Wood,
				MaxDurability = 35,

				RequiredDisenchantingLevel = 1,
				RequiredClassMask = ClassMask.Hunter
			};

			Bow.FinalizeDataHolder();
		}
		#endregion
	}
}