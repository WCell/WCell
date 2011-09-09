using System;
using System.Collections.Generic;
using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Gossips;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.Util.Graphics;

///
/// This file was automatically created, using WCell's CodeFileWriter
/// Date: 9/1/2009
///
public enum Steps
{
	NotStarted = 0,
	GrandChampionTrash = 1,
	GrandChampionBosses = 2,
	ArgentChampionTrash = 3,
	ArgentChampionBosses = 4,
	TheBlackNight = 5
}

namespace WCell.Addons.Default.Instances
{
	public class TrialOfTheChampion : BaseInstance
	{
		public NPC announcerNPC;
		public NPC tirionNPC;
		public int grandChampionsDied;

		public List<NPC> spectatorsNPC = new List<NPC>(15);
		public List<NPC> grandChampions = new List<NPC>(3);

		// Neutral NPCs
		static NPCEntry arelasEntry;
		static NPCEntry jaerenEntry;
		static NPCEntry tirionEntry;

		//static NPCEntry[] grandChampionsEntries;

		//// Grand Champions Horde with their mounts
		//static NPCEntry eresseaEntry;
		//static NPCEntry runokEntry;
		//static NPCEntry zulToreEntry;
		//static NPCEntry visceriEntry;
		//static NPCEntry mokraEntry;

		//static NPCEntry eresseaMountEntry;
		//static NPCEntry runokMountEntry;
		//static NPCEntry zulToreMountEntry;
		//static NPCEntry visceriMountEntry;
		//static NPCEntry mokraMountEntry;

		//// Grand Champions Alliance with their mounts
		//static NPCEntry ambroseEntry;
		//static NPCEntry colososEntry;
		//static NPCEntry jaelyneEntry;
		//static NPCEntry lanaEntry;
		//static NPCEntry jacobEntry;

		//static NPCEntry ambroseMountEntry;
		//static NPCEntry colososMountEntry;
		//static NPCEntry jaelyneMountEntry;
		//static NPCEntry lanaMountEntry;
		//static NPCEntry jacobMountEntry;

		//// Grand Champions Alliance trashs
		//static NPCEntry exodarChampionEntry;
		//static NPCEntry ironforgeChampionEntry;
		//static NPCEntry gnomereganChampionEntry;
		//static NPCEntry darnassusChampionEntry;
		//static NPCEntry stormwindChampionEntry;

		//// Grand Champions Horde trash
		//static NPCEntry orgrimmarChampionEntry;
		//static NPCEntry senJinChampionEntry;
		//static NPCEntry thunderBluffChampionEntry;
		//static NPCEntry silvermoonChampionEntry;
		//static NPCEntry undercityChampionEntry;

		// Vehicules horde and alliance
		static NPCEntry warhorseEntry;
		static NPCEntry battleworgEntry;

		//static GOEntry lanceRackEntry;

		private Steps _step;
		private Vector3 announcerPlaceInCombat = new Vector3(734.9166f, 661.2474f, 412.7828f);
		//private Vector3 announcerPlaceOutCombat = new Vector3(748.309f, 619.4879f, 411.1724f);

		public void ChangeAnnouncer(NPC announcer)
		{
			if (OwningFaction == FactionGroup.Horde)
			{
				announcer.SetEntry(jaerenEntry);
			}
			announcerNPC = announcer;
		}

		public void NextStep(Steps step)
		{
			_step = step;

			if (step == Steps.GrandChampionTrash)
			{
				tirionNPC.Yell("Welcome, champions. " +
						"Today, before the eyes of your leaders and peers, you will prove yourselves worthy combatants.");
				tirionNPC.Emote(EmoteType.SimpleTalk);
				CallDelayed(5000, () =>
				{
					tirionNPC.Yell("You will first be facing three of the Grand Champions of the Tournament! " +
						"These fierce contenders have beaten out all others to reach the pinnacle of skill in the joust.");
					tirionNPC.Emote(EmoteType.SimpleTalk);
					SetupGrandChampions();
				});

			}
		}

		public void SetupGrandChampions()
		{
			Random rand = new Random();
			int firstChampionRand = rand.Next(0, 5);
			int secondChampionRand, thirdChampionRand;
			do
			{
				secondChampionRand = rand.Next(0, 5);
			} while (secondChampionRand == firstChampionRand);

			do
			{
				thirdChampionRand = rand.Next(0, 5);
			} while (thirdChampionRand == secondChampionRand || thirdChampionRand == firstChampionRand);
		}

		public void IncreaseGrandChampionsDied()
		{
			grandChampionsDied++;
			if (grandChampionsDied == 9)
			{
				NextStep(Steps.ArgentChampionTrash);
			}
			else
			{
				NextGroupGrandChampions();
			}
		}

		public void NextGroupGrandChampions()
		{
		}

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			arelasEntry = NPCMgr.GetEntry(NPCId.ArelasBrightstar);
			jaerenEntry = NPCMgr.GetEntry(NPCId.JaerenSunsworn);
			tirionEntry = NPCMgr.GetEntry(NPCId.HighlordTirionFordring_9);

			//eresseaEntry = NPCMgr.GetEntry(NPCId.EresseaDawnsinger_2);
			//colososEntry = NPCMgr.GetEntry(NPCId.Colosos_2);
			//jaelyneEntry = NPCMgr.GetEntry(NPCId.JaelyneEvensong_2);
			//lanaEntry = NPCMgr.GetEntry(NPCId.LanaStouthammer_2);
			//jacobEntry = NPCMgr.GetEntry(NPCId.MarshalJacobAlerius_2);

			//exodarChampionEntry = NPCMgr.GetEntry(NPCId.ExodarChampion_3);
			//stormwindChampionEntry = NPCMgr.GetEntry(NPCId.StormwindChampion_3);
			//ironforgeChampionEntry = NPCMgr.GetEntry(NPCId.IronforgeChampion_3);
			//gnomereganChampionEntry = NPCMgr.GetEntry(NPCId.GnomereganChampion_3);
			//darnassusChampionEntry = NPCMgr.GetEntry(NPCId.DarnassusChampion_3);

			warhorseEntry = NPCMgr.GetEntry(NPCId.ArgentWarhorse_6);
			battleworgEntry = NPCMgr.GetEntry(NPCId.ArgentBattleworg_2);
			//colososMountEntry = NPCMgr.GetEntry(NPCId.ColososMount);

			// Wrong DB datas...
			battleworgEntry.HordeFactionId = warhorseEntry.AllianceFactionId = FactionTemplateId.Friendly;
			warhorseEntry.HordeFactionId = battleworgEntry.AllianceFactionId = FactionTemplateId.Monster;
			NPCMgr.GetEntry(NPCId.WorldTrigger).SpawnEntries.Find(spawn => spawn.MapId == MapId.TrialOfTheChampion).AutoSpawns = false;

			jaerenEntry.UnitFlags = arelasEntry.UnitFlags |= UnitFlags.NotAttackable;
			jaerenEntry.UnitFlags = arelasEntry.UnitFlags &= ~UnitFlags.Passive;
			jaerenEntry.NPCFlags = arelasEntry.NPCFlags |= NPCFlags.Gossip;

			arelasEntry.DefaultGossip = jaerenEntry.DefaultGossip = new GossipMenu(new DynamicGossipEntry(91802, new GossipStringFactory(
				convo =>
				{
					var instance = convo.Character.Map as TrialOfTheChampion;
					if (instance != null)
					{
						if (convo.Character.Vehicle != null && (convo.Character.Vehicle.Entry == warhorseEntry
							|| convo.Character.Vehicle.Entry == battleworgEntry))
						{
							return string.Format("Are you ready for your first challenge, {0} ?", convo.Character.Class);
						}
						else if (instance._step == Steps.NotStarted)
						{
							return string.Format("The First Challenge requires you to be mounted on an {0}." +
								"You will find these mounts along the walls of this coliseum.",
								convo.Character.FactionGroup == FactionGroup.Horde ? "Argent Battleworg" : "Argent Warhorse");
						}
						else
						{
							return string.Format("Are you ready for your next challenge, {0} ?", convo.Character.Class);
						}
					}
					else return string.Empty;
				})),
				new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCArelas1),
					new NonNavigatingDecidingGossipAction(new GossipActionHandler(
					convo =>
					{
						((NPC)convo.Speaker).NPCFlags &= ~NPCFlags.Gossip;
						var instance = convo.Character.Map as TrialOfTheChampion;
						if (instance != null)
						{
							instance.NextStep(Steps.GrandChampionTrash);
							instance.announcerNPC.MoveToThenExecute(instance.announcerPlaceInCombat,
								unit => unit.Orientation = 4.714f);
						}
					}), new GossipActionDecider(
								convo =>
								{
									if (convo.Character.Vehicle != null && (convo.Character.Vehicle.Entry == warhorseEntry
											|| convo.Character.Vehicle.Entry == battleworgEntry))
										return true;
									else
										return false;
								}))),
				new MultiStringGossipMenuItem(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.NPCArelas2),
					new NonNavigatingDecidingGossipAction(new GossipActionHandler(
						convo =>
						{
							((NPC)convo.Speaker).NPCFlags &= ~NPCFlags.Gossip;
							var instance = convo.Character.Map as TrialOfTheChampion;
							if (instance != null)
							{
								instance.NextStep(instance._step++);
								instance.announcerNPC.MoveToThenExecute(instance.announcerPlaceInCombat,
									unit => unit.Orientation = 4.714f);
							}
						}), new GossipActionDecider(
									convo =>
									{
										var instance = convo.Speaker.Map as TrialOfTheChampion;
										if (instance != null)
										{
											if (instance._step == Steps.GrandChampionBosses
												|| instance._step == Steps.ArgentChampionBosses)
											{
												return true;
											}
										}
										return false;
									}))));

			arelasEntry.Activated += arelas =>
			{
				var instance = arelas.Map as TrialOfTheChampion;
				if (instance != null)
				{
					instance.ChangeAnnouncer(arelas);
				}
			};

			tirionEntry.Activated += tirion =>
			{
				var instance = tirion.Map as TrialOfTheChampion;
				if (instance != null)
				{
					instance.tirionNPC = tirion;
				}
			};
				
			/* List of spectators
			 * spectatorsNPC.Add(NPCId.DwarvenColiseumSpectator);
			spectatorsNPC.Add(NPCId.TrollColiseumSpectator);
			spectatorsNPC.Add(NPCId.TaurenColiseumSpectator);
			spectatorsNPC.Add(NPCId.OrcishColiseumSpectator);
			spectatorsNPC.Add(NPCId.ForsakenColiseumSpectator);
			spectatorsNPC.Add(NPCId.BloodElfColiseumSpectator);
			spectatorsNPC.Add(NPCId.DraeneiColiseumSpectator);
			spectatorsNPC.Add(NPCId.GnomishColiseumSpectator);
			spectatorsNPC.Add(NPCId.HumanColiseumSpectator);
			spectatorsNPC.Add(NPCId.NightElfColiseumSpectator);
			spectatorsNPC.Add(NPCId.ArgentCrusadeSpectator);
			spectatorsNPC.Add(NPCId.ArgentCrusadeSpectator_2);
			spectatorsNPC.Add(NPCId.ArgentCrusadeSpectator_3);
			spectatorsNPC.Add(NPCId.ArgentCrusadeSpectator_4);
			spectatorsNPC.Add(NPCId.ArgentCrusadeSpectator_5);
			spectatorsNPC.Add(NPCId.ArgentCrusadeSpectator_6);*/
		}

		//[Initialization]
		//[DependentInitialization(typeof(GOMgr))]
		//public static void InitGOs()
		//{
		//    lanceRackEntry = GOMgr.GetEntry(GOEntryId.LanceRack_5) as GOSpellCasterEntry;
		//}
	}
}