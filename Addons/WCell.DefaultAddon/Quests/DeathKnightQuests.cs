using System.Linq;
using WCell.Constants;
using WCell.Constants.Factions;
using WCell.Constants.GameObjects;
using WCell.Constants.Items;
using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Quests;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.RealmServer.Spells.Targeting;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.Quests
{
	public static class DeathKnightQuests
	{
		
		[Initialization]
        [DependentInitialization(typeof(QuestMgr))]
        public static void FixIt()
        {
			//The Emblazoned Runeblade
        	var quest = QuestMgr.GetTemplate(12619);
        	quest.QuestFinished += EmblazonRuneBladeQuestFinished;

			//The Endless Hunger
        	quest = QuestMgr.GetTemplate(12848);
        	var index = quest.GetInteractionTemplateFor(NPCId.UnworthyInitiate).Index;
			quest.AddLinkedNPCInteractions(index,
				NPCId.UnworthyInitiate_2,
				NPCId.UnworthyInitiate_3,
				NPCId.UnworthyInitiate_4,
				NPCId.UnworthyInitiate_5);

			//Death Comes From On High
			quest = QuestMgr.GetTemplate(12641);

			//make the go sparkle, we dont click the eye
			//we must click the holder under it!
			quest.AddGOInteraction(GOEntryId.EyeOfAcherusControlMechanism, 0);
        }

		#region Emblazoned Rune Blade

		private static SpellId _emblazonRunebladeId = SpellId.EmblazonRuneblade_3;
		private static SpellId[] _emblazonRunebladeLearnSpellIds = { 
																	   SpellId.ClassSkillRuneOfRazorice,
																	   SpellId.ClassSkillRuneOfCinderglacier,
																	   SpellId.ClassSkillRuneforging};

		/// <summary>
		/// Custom filter to ensure the given spell only targets a specific NPC
		/// </summary>
		public static void IsRunebladeTriggerNPC(SpellEffectHandler effectHandler, WorldObject target, ref SpellFailedReason failedReason)
		{
			if(!(target is NPC))
			{
				failedReason =  SpellFailedReason.OutOfRange;
				return;
			}

			var entryid = ((NPC) target).EntryId;
			if(entryid == (uint)NPCId.RuneforgeSE)
			{
				return;
			}

			if(entryid == (uint)NPCId.RuneforgeSW)
			{
				failedReason =  SpellFailedReason.Ok;
				return;
			}
			failedReason = SpellFailedReason.OutOfRange;
		}

    	private static void EmblazonRuneBladeQuestFinished(Quest obj)
    	{
    		var chr = obj.Owner;
			if(chr == null)
				return;

			foreach (var spell in _emblazonRunebladeLearnSpellIds.Select(SpellHandler.Get).Where(spell => spell != null))
			{
				chr.PlayerSpells.AddSpellRequirements(spell);
				chr.Spells.AddSpell(spell);
			}
    	}

    	[Initialization(InitializationPass.Second)]
		public static void FixSpells()
		{
			var emblazonRuneblade = SpellHandler.Get(_emblazonRunebladeId);
			//need to be able to add multiple targets by Id that are in range
			//emblazonRuneblade.RequiredTargetIds = {(uint)NPCId.RuneforgeSE, (uint)NPCId.RuneforgeSW };
			var effect = emblazonRuneblade.GetEffect(SpellEffectType.ApplyAura);
			effect.Radius = 8;
			effect.AuraEffectHandlerCreator = () => new EmblazonRuneBladeAuraHandler();
			emblazonRuneblade.RequiredTargetType = RequiredSpellTargetType.NPCAlive;
			emblazonRuneblade.OverrideCustomTargetDefinitions(
				DefaultTargetAdders.AddAreaSource,
				IsRunebladeTriggerNPC);
		}

		#endregion

		#region The Endless Hunger
		
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
			//just clear all auras! uh oh...not
			shackled.Auras.Clear();
			shackled.IsInvulnerable = false;
			go.Flags = GameObjectFlags.None;
			shackled.StandState = StandState.Stand;
			shackled.Emote(EmoteType.SimpleTalk);
			shackled.Say("They brand me unworthy? I will show them unworthy!");
			shackled.Spells.Clear();
			shackled.Spells.AddSpell(
				SpellId.ClassSkillPlagueStrikeRank1_2,
				SpellId.ClassSkillIcyTouchRank1_2,
				SpellId.ClassSkillBloodStrikeRank1_2,
				SpellId.ClassSkillDeathCoil,
				SpellId.ClassSkillDeathCoilRank1_3);
			shackled.CallDelayed(3000, wObj => ((NPC)wObj).MoveInFrontThenExecute(go, npc =>
			{
				npc.Emote(EmoteType.SimpleLoot);
				npc.CallDelayed(4000, obj =>
				{
					obj.SpellCast.TriggerSelf(SpellId.DeathKnightInitiateVisual);
					obj.SpellCast.TriggerSelf(transformSpellId);
					((NPC) obj).VirtualItem1 = ItemId.RunedSoulblade;
					obj.FactionId = FactionId.ActorEvil;
				});
				
				npc.CallDelayed(7000, obj =>
				{
					((NPC)obj).Emote(EmoteType.SimplePointNosheathe);
					obj.Say("To battle!");
				});

				npc.CallDelayed(9000, obj =>
				{
					((NPC)obj).ThreatCollection[user] += 10000;
					((NPC)obj).UnitFlags &= ~UnitFlags.SelectableNotAttackable;
					((NPC)obj).UnitFlags &= ~UnitFlags.NotAttackable;
					((NPC) obj).Target = user;
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
		#endregion

		#region Death Comes From On High
		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void FixControlMechanism()
		{
			var entry = GOMgr.GetEntry(GOEntryId.EyeOfAcherusControlMechanism);
			entry.Used += ControlMechanismUsed;
		}

		private static bool ControlMechanismUsed(GameObject go, Character user)
		{
			go.SpellCast.Trigger(SpellId.SummonEyeOfAcherus, user);
			user.SpellCast.Trigger(SpellId.NotDisplayedOpening_2, go);
			return true;
		}

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void FixEyeOfAcherus()
		{
			var eye = NPCMgr.GetEntry(NPCId.EyeOfAcherus);
			eye.Activated += EyeOfAcherusActivated;
			eye.Deleted += EyeOfAcherusDeleted;
		}

		private static void EyeOfAcherusDeleted(NPC npc)
		{
			var chr = npc.Map.GetObject(npc.Creator) as Character;
			if (chr != null)
			{
				chr.UnPossess(npc);
				chr.Phase = 1;
			}
		}

		private static void EyeOfAcherusActivated(NPC npc)
		{
			//npc.SpellCast.TriggerSelf((SpellId)51860);
			npc.SpellCast.TriggerSelf(SpellId.EyeOfAcherusVisual);
			npc.Phase = 2;
			
			//TODO: Set the creator as the chr that used the eye of acherus control mechanism
			var chr = npc.Map.GetObject(npc.Creator) as Character;
			if (chr != null)
			{
				//TODO localise!
				ChatMgr.SendRaidBossWhisper(npc, chr, "The Eye of Acherus launches towards its destination.");

				npc.SpellCast.Trigger(SpellId.EyeOfAcherusFlightBoost, npc, chr);
				npc.Flying++;
				//npc.IncMechanicCount(SpellMechanic.Rooted);
				//Move to destination
				npc.MoveToThenExecute(new Vector3(1758.007f, -5876.785f, 166.8667f), unit =>
				                                                                     	{
				                                                                     		unit.SpellCast.TriggerSelf(
				                                                                     			SpellId.EyeOfAcherusFlight);
				                                                                     		npc.Auras.Remove(
				                                                                     			SpellId.EyeOfAcherusFlightBoost);
				                                                                     		ChatMgr.SendRaidBossWhisper(unit, chr,
				                                                                     		                            "The Eye of Acherus is in your control.");
																							//unit.DecMechanicCount(SpellMechanic.Rooted);
				                                                                     	});
			}
		}

		[Initialization(InitializationPass.Second)]
		public static void FixEyeOfAcherusControlSpell()
		{
			var eyeSpell = SpellHandler.Get(SpellId.EffectTheEyeOfAcherus);
			eyeSpell.AuraRemoved += EffectTheEyeOfAcherusRemoved;
		}

		private static void EffectTheEyeOfAcherusRemoved(Aura obj)
		{
			if (obj.Owner is Character)
			{
				var chr = obj.Owner as Character;
				chr.UnPossess(chr.Charm);
				chr.Auras.Remove(SpellId.EyeOfAcherusVisual);
				chr.Phase = 1;
			}
		}

		#endregion
	}

		

		#region EmblazonRuneBladeAuraHandler
	public class EmblazonRuneBladeAuraHandler : AuraEffectHandler
	{
		protected override void Apply()
		{
			if (!(m_aura.CasterUnit is Character))
				return;

			var chr = m_aura.CasterUnit as Character;
			var npc = chr.ChannelObject;
			npc.SpellCast.TriggerSelf(SpellId.ShadowStorm_3);

			var runebladedSwordNPC = chr.GetNearbyNPC(NPCId.RunebladedSword);
			if (runebladedSwordNPC != null)
			{
				runebladedSwordNPC.MovementFlags |= MovementFlags.DisableGravity;

				var acherusDummy = chr.GetNearbyNPC(NPCId.AcherusDummy);
				if(acherusDummy != null)
					runebladedSwordNPC.SpellCast.Trigger(SpellId.Rotate_2, acherusDummy);

				runebladedSwordNPC.SpellCast.TriggerSelf(SpellId.ShadowStorm_3);
			}

			base.Apply();
		}

		protected override void Remove(bool cancelled)
		{
			if (!cancelled)
			{
				if (!(m_aura.CasterUnit is Character))
					return;

				var chr = m_aura.CasterUnit as Character;

				chr.SpellCast.TriggerSelf(SpellId.EmblazonRuneblade_4);
			}
			
			base.Remove(cancelled);
		}

	}
	#endregion
}
