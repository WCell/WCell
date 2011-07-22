using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Misc;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Targeting;

namespace WCell.Addons.Default.NPCs
{
	public static class DKAcherusCorrections
	{

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void FixThem()
		{
			var entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate);
			entry.Activated += UnworthyInitiateActivated;
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_2);
			entry.Activated += UnworthyInitiateActivated;
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_3);
			entry.Activated += UnworthyInitiateActivated;
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_4);
			entry.Activated += UnworthyInitiateActivated;
			entry = NPCMgr.GetEntry(NPCId.UnworthyInitiate_5);
			entry.Activated += UnworthyInitiateActivated;
		}

		private static void UnworthyInitiateActivated(NPC npc)
		{
			npc.StandState = StandState.Kneeling;
			npc.AddMessage( 
				() =>
					{
						var nearest = npc.GetNearbyNPC(NPCId.UnworthyInitiateAnchor, 7);
						if (nearest == null) return;
						nearest.SpellCast.Trigger(SpellId.ChainedPeasantChest, npc);
					});
		}

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void FixGOs()
		{
			var entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_2);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_3);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_4);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_5);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_6);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_7);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_8);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_9);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_10);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_11);
			entry.Used += SoulPrisonUsed;
			entry = GOMgr.GetEntry(GOEntryId.AcherusSoulPrison_12);
			entry.Used += SoulPrisonUsed;

		}

		private static bool SoulPrisonUsed(GameObject go, Character user)
		{
			var nearest = go.GetNearbyNPC(NPCId.UnworthyInitiateAnchor, 2);
			if (nearest == null) return false;
			var shackled = nearest.ChannelObject as NPC;
			if (shackled == null) return false;

			var transformSpellId = GetTransformSpellIdFor(shackled.DisplayId);
			shackled.Auras.Remove(SpellId.ChainedPeasantChest);
			go.Flags = GameObjectFlags.None;
			shackled.StandState = StandState.Stand;
			shackled.Emote(EmoteType.SimpleTalk);
			shackled.Yell("They brand me unworthy? I will show them unworthy!");
			shackled.Spells.AddSpell(
				SpellId.ClassSkillPlagueStrikeRank1_2,
				SpellId.ClassSkillIcyTouchRank1_2,
				SpellId.ClassSkillBloodStrikeRank1_2,
				SpellId.ClassSkillDeathCoil,
				SpellId.ClassSkillDeathCoilRank1_3);
			shackled.CallDelayed(2000, wObj => ((NPC)wObj).MoveInFrontThenExecute(go, npc =>
			{
				npc.Emote(EmoteType.SimpleLoot);
				npc.CallDelayed(2000, obj =>
				{
					obj.SpellCast.TriggerSelf(SpellId.DeathKnightInitiateVisual);
					obj.SpellCast.TriggerSelf(transformSpellId);
					((NPC)obj).VirtualItem1 = ItemId.RunedSoulblade;
					obj.FactionId = FactionId.ActorEvil;
				});
				
				npc.CallDelayed(5000, obj =>
				{
					((NPC)obj).Emote(EmoteType.SimplePointNosheathe);
					obj.Say("To battle!");
				});

				npc.CallDelayed(6000, obj =>
				{
					((NPC)obj).ThreatCollection[user] += 10000;
					((NPC)obj).UnitFlags &= ~UnitFlags.SelectableNotAttackable;
					((NPC)obj).UnitFlags &= ~UnitFlags.NotAttackable;
					((NPC)obj).Brain.State = BrainState.Combat;
				});
			}));

	return true;
		}

		private static SpellId GetTransformSpellIdFor(uint displayId)
		{
			SpellId transformSpellId;
			switch (displayId)
			{
				case 25355:
					transformSpellId = SpellId.DeathKnightInitiateFemaleHuman;
					break;
				case 25360:
					transformSpellId = SpellId.DeathKnightInitiateFemaleNightElf;
					break;
				case 25356:
					transformSpellId = SpellId.DeathKnightInitiateMaleDwarf;
					break;
				case 25362:
					transformSpellId = SpellId.DeathKnightInitiateFemaleGnome;
					break;
				case 25363:
					transformSpellId = SpellId.DeathKnightInitiateFemaleDraenei;
					break;
				case 25368:
					transformSpellId = SpellId.DeathKnightInitiateFemaleOrc;
					break;
				case 25365:
					transformSpellId = SpellId.DeathKnightInitiateMaleTroll;
					break;
				case 25371:
					transformSpellId = SpellId.DeathKnightInitiateFemaleTauren;
					break;
				case 25372:
					transformSpellId = SpellId.DeathKnightInitiateFemaleForsaken;
					break;
				case 24369:
					transformSpellId = SpellId.DeathKnightInitiateFemaleBloodElf;
					break;
				case 25354:
					transformSpellId = SpellId.DeathKnightInitiateMaleHuman;
					break;
				case 25358:
					transformSpellId = SpellId.DeathKnightInitiateMaleNightElf;
					break;
				case 25361:
					transformSpellId = SpellId.DeathKnightInitiateFemaleDwarf;
					break;
				case 25359:
					transformSpellId = SpellId.DeathKnightInitiateMaleGnome;
					break;
				case 25357:
					transformSpellId = SpellId.DeathKnightInitiateMaleDraenei;
					break;
				case 25364:
					transformSpellId = SpellId.DeathKnightInitiateMaleOrc;
					break;
				case 25370:
					transformSpellId = SpellId.DeathKnightInitiateFemaleTroll;
					break;
				case 25366:
					transformSpellId = SpellId.DeathKnightInitiateMaleTauren;
					break;
				case 25367:
					transformSpellId = SpellId.DeathKnightInitiateMaleForsaken;
					break;
				case 25373:
					transformSpellId = SpellId.DeathKnightInitiateMaleBloodElf;
					break;
				default: //uh oh!
					transformSpellId = SpellId.DeathKnightInitiateFemaleBloodElf;
					break;
			}
			return transformSpellId;
		}
	}
}
