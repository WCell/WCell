using WCell.Constants.Factions;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.AI;
using WCell.RealmServer.AI.Actions.States;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Targeting;
using WCell.Util.Graphics;

namespace WCell.Addons.Default.NPCs
{
	/// <summary>
	/// NPC corrections that don't belong to any Quest, Instance or BG, go here
	/// </summary>
	public static class NPCCorrections
	{
		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void FixNPCs()
		{
			var warhorseEntry = NPCMgr.GetEntry(NPCId.ArgentWarhorse_6);
			var battleworgEntry = NPCMgr.GetEntry(NPCId.ArgentBattleworg_2);

			battleworgEntry.HordeFactionId = warhorseEntry.AllianceFactionId = FactionTemplateId.Friendly;
			warhorseEntry.HordeFactionId = battleworgEntry.AllianceFactionId = FactionTemplateId.Monster;

			var battleworg = NPCMgr.GetEntry(NPCId.ArgentBattleworg);
			if(battleworg != null)
			{
				battleworg.AllianceFactionId = FactionTemplateId.Monster;
				battleworg.HordeFactionId = FactionTemplateId.Friendly;
			}

			var warhorse = NPCMgr.GetEntry(NPCId.ArgentWarhorse_5);
			if (warhorse != null)
			{
				warhorse.AllianceFactionId = FactionTemplateId.Friendly;
				warhorse.HordeFactionId = FactionTemplateId.Monster;
			}

			var tank = NPCMgr.GetEntry(NPCId.ReedsSteamTank);
			if (tank != null)
			{
				tank.VehicleId = 192;
				tank.VehicleAimAdjustment = 3.455752f;
				tank.HoverHeight = 1;
                tank.InfoString = "vehichleCursor"; //This is not a typo, "vehichleCursor" denotes the vehicle icon
			}

			var mammoth = NPCMgr.GetEntry(NPCId.EnragedMammoth);
			if (mammoth != null)
			{
				mammoth.VehicleId = 145;
				mammoth.VehicleEntry.Seats[0].AttachmentOffset = new Vector3 { X = 0.5554f, Y = 0.0392f, Z = 7.867001f };
				mammoth.VehicleAimAdjustment = 1.658063f;
				mammoth.HoverHeight = 1;
				mammoth.SpellTriggerInfo = new SpellTriggerInfo
				{
					QuestId = 0,
					SpellId = SpellId.EnragedMammoth
				};
			}

			FixAcherusDKTeleporters();

			//NPCMgr.Apply(entry =>
			//{
			//    entry.AddSpell(SpellId.EffectFireNovaRank1);
			//}, NPCId.FireNovaTotem);
			SetupMirrorImage();
		}

		private static void FixAcherusDKTeleporters()
		{
			var dKPortalHeartToHall = NPCMgr.GetEntry(NPCId.TeleportHeartHall);
			if (dKPortalHeartToHall != null)
			{
				var hallPortSpell = SpellHandler.Get(SpellId.EffectTeleportToHallOfCommand);
				hallPortSpell.OverrideAITargetDefinitions(
					DefaultTargetAdders.AddAreaSource,
					DefaultTargetEvaluators.NearestEvaluator,
					DefaultTargetFilters.IsPlayer);
				hallPortSpell.AISettings.SetCooldown(0);
				dKPortalHeartToHall.AddSpell(hallPortSpell);
				dKPortalHeartToHall.AggroBaseRange = 10;
				dKPortalHeartToHall.Activated += DKPortalActivated;
			}

			var dKPortalHallToHeart = NPCMgr.GetEntry(NPCId.TeleportHallHeart);
			if (dKPortalHallToHeart == null) return;
			var heartPortSpell = SpellHandler.Get(SpellId.EffectTeleportToHeartOfAcherus);
			heartPortSpell.OverrideAITargetDefinitions(
				DefaultTargetAdders.AddAreaSource,
				DefaultTargetEvaluators.NearestEvaluator,
				DefaultTargetFilters.IsPlayer);
			heartPortSpell.AISettings.SetCooldown(0);
			dKPortalHallToHeart.AddSpell(heartPortSpell);
			dKPortalHallToHeart.AggroBaseRange = 10;
			dKPortalHallToHeart.Activated += DKPortalActivated;
		}

		private static void DKPortalActivated(NPC npc)
		{
			var roamAction = npc.Brain.Actions[BrainState.Roam] as AIRoamAction;
			if(roamAction != null)
				roamAction.MinimumRoamSpellCastDelay = 1000;
		}

		static void SetupMirrorImage()
		{
			NPCEntry mirrorimage = NPCMgr.GetEntry(NPCId.MirrorImage);
			mirrorimage.BrainCreator = mirror => new MirrorImageBrain(mirror);

			mirrorimage.Activated += image =>
			{
				image.PlayerOwner.SpellCast.Start(SpellHandler.Get(SpellId.CloneMe), true, image);

				//image.SpellCast.Start(SpellId.HallsOfReflectionClone_2);//id 69837 is this even needed?
				//EFF0: Aura Id 279 (SPELL_AURA_279), value = 1
				
				//other spells ???
				//58838 Inherit Master's Threat List
				//SPELL_FIREBLAST       = 59637,
				//SPELL_FROSTBOLT       = 59638,
			};

		}
		public class MirrorImageBrain : MobBrain
		{
			public MirrorImageBrain(NPC image)
				: base(image) { }
		}
	}
}