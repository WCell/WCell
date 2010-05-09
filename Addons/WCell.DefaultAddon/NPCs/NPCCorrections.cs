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
		}
	}
}
