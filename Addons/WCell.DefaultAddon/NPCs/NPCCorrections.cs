using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCell.Constants.Spells;
using WCell.Core.Initialization;
using WCell.RealmServer.NPCs;
using WCell.Constants.NPCs;
using WCell.Util;
using WCell.Util.Graphics;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.Instances;
using WCell.RealmServer.NPCs;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Entities;
using WCell.Constants.Spells;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.RealmServer.AI.Actions.Combat;

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

			//NPCMgr.Apply(entry =>
			//{
			//    entry.AddSpell(SpellId.EffectFireNovaRank1);
			//}, NPCId.FireNovaTotem);
			SetupMirrorImage();
		}
		static void SetupMirrorImage()
		{
			NPCEntry mirrorimage = NPCMgr.GetEntry(NPCId.MirrorImage);
			mirrorimage.BrainCreator = mirror => new MirrorImageBrain(mirror);

			mirrorimage.Activated += image =>
			{
				image.PlayerOwner.SpellCast.Start(SpellHandler.Get(SpellId.CloneMe), true, image);
				//EFF0: Aura Id 247 (SPELL_AURA_247), value = 2 
				//EFF1: SPELL_EFFECT_SCRIPT_EFFECT BasePoints = 41055 --> Copy Weapon
				//EFF2: SPELL_EFFECT_SCRIPT_EFFECT BasePoints = 45206 --> Copy Off-hand Weapon

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