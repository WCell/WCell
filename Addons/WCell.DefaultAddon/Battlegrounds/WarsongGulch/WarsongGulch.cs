using System;
using WCell.Addons.Default.Lang;
using WCell.Constants;
using WCell.Constants.AreaTriggers;
using WCell.Constants.GameObjects;
using WCell.Constants.Spells;
using WCell.Constants.World;
using WCell.Core.Initialization;
using WCell.RealmServer.AreaTriggers;
using WCell.RealmServer.Battlegrounds;
using WCell.RealmServer.Chat;
using WCell.RealmServer.Entities;
using WCell.RealmServer.GameObjects;
using WCell.RealmServer.GameObjects.GOEntries;
using WCell.RealmServer.Global;
using WCell.RealmServer.Lang;
using WCell.RealmServer.Spells;
using WCell.RealmServer.Spells.Auras;
using WCell.Core.Timers;
using WCell.Util.Graphics;
using WCell.Util.Variables;

namespace WCell.Addons.Default.Battlegrounds.WarsongGulch
{
	/// <summary>
	/// Implementation of the famous battle at Warsong Gulch.
	/// Warsong Clan vs Silverwing
	/// 
	/// TODO: If certain auras are not removed. Remove manually.
	/// TODO: Check the timers. 
	/// </summary>
	public class WarsongGulch : Battleground
	{
		#region Static Fields
		[Variable("WSGMaxScore")]
		public static int MaxScoreDefault
		{
			get { return Constants.World.WorldStates.GetState(WorldStateId.WSGMaxScore).DefaultValue; }
			set { Constants.World.WorldStates.GetState(WorldStateId.WSGMaxScore).DefaultValue = value; }
		}

		static WarsongGulch()
		{
			if (MaxScoreDefault <= 0)
			{
				MaxScoreDefault = 3;
			}
		}

		[Variable("WSGFlagRespawnTimeMillis")]
		public static int FlagRespawnTimeMillis = 20 * 1000;

		[Variable("WSGPrepTimeMillis")]
		public static int WSGPreparationTimeMillis = 60 * 1000;

		/// <summary>
		/// The time in which the BG will be ended, no matter the score. If 0, will last til score reaches max.
		/// </summary>
		[Variable("WSGMaxDurationMinutes")]
		public static int MaxDuration = 20;

		public static int PowerUpRespawnTimeMillis = 2 * 60 * 1000;

		/// <summary>
		/// The delay after which a flag carrier will receive the flag carrier debuff (0 if deactivated)
		/// </summary>
		public static float DebuffFlagCarrierDelay = 10;
		public static SpellId AllianceFlagDebuffSpellId = SpellId.AllianceFlagExtraDamageDebuff;
		public static SpellId HordeFlagDebuffSpellId = SpellId.HordeFlagExtraDamageDebuff;
		public static GOEntryId SilverwingFlagStandId = GOEntryId.SilverwingFlag_2;
		public static GOEntryId WarsongClanFlagStandId = GOEntryId.WarsongFlag_2;
		public static GOEntryId SilverwingFlagId = GOEntryId.SilverwingFlag;
		public static GOEntryId WarsongFlagId = GOEntryId.WarsongFlag;
		#endregion

		#region Fields

		public readonly WSGFaction[] Factions;

		private GameObject _allianceDoor1;
		private GameObject _allianceDoor2;
		private GameObject _allianceDoor3;

		private GameObject _hordeDoor1;
		private GameObject _hordeDoor2;

		private GameObject _allianceBerserkerBuff;
		private GameObject _allianceFoodBuff;
		private GameObject _allianceSpeedBuff;

		private GameObject _hordeBerserkerBuff;
		private GameObject _hordeFoodBuff;
		private GameObject _hordeSpeedBuff;
		#endregion

		public WarsongGulch()
		{
			Factions = new WSGFaction[(int)BattlegroundSide.End];
		}

		#region Props
		public override int PreparationTimeMillis
		{
			get { return WSGPreparationTimeMillis; }
		}

		public WSGFaction GetFaction(BattlegroundSide side)
		{
			return Factions[(int)side];
		}

		public WarsongClan WarsongClan
		{
			get { return (WarsongClan)Factions[(int)BattlegroundSide.Horde]; }
		}

		public Silverwing Silverwing
		{
			get { return (Silverwing)Factions[(int)BattlegroundSide.Alliance]; }
		}

		public int MaxScore
		{
			get { return WorldStates.GetInt32(WorldStateId.WSGMaxScore); }
			set { WorldStates.SetInt32(WorldStateId.WSGMaxScore, value); }
		}
		#endregion

		#region Overrides

		protected override void InitMap()
		{
			base.InitMap();

			Factions[(int)BattlegroundSide.Alliance] = new Silverwing(this);
			Factions[(int)BattlegroundSide.Horde] = new WarsongClan(this);

			MaxScore = MaxScoreDefault;
		}

		/// <summary>
		/// Called when the battle starts (perparation ends now)
		/// </summary>
		protected override void OnStart()
		{
			base.OnStart();

			WarsongClan.RespawnFlag();
			Silverwing.RespawnFlag();

			SpawnAllianceBerserkerBuff();
			SpawnAllianceFoodBuff();
			SpawnAllianceSpeedBuff();

			SpawnHordeBerserkerBuff();
			SpawnHordeFoodBuff();
			SpawnHordeSpeedBuff();

			DropGates();

			// In X minutes, WSG will end. Winner is evaluated.
			if (MaxDuration != 0.0)
			{
				CallDelayed(MaxDuration * 60, FinishFight);
			}
			Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.WSOnStart));
		}

		protected override void OnPrepareHalftime()
		{
			base.OnPrepareHalftime();

			string time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis / 2000);
			Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.WSOnPrepareHalfTime), time);
		}


		protected override void OnPrepare()
		{
			base.OnPrepare();

			string time = RealmLocalizer.FormatTimeSecondsMinutes(PreparationTimeMillis / 1000);
			Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.WSOnPrepare), time);
		}

		protected override void OnFinish(bool disposing)
		{
			base.OnFinish(disposing);
            Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.WSOnFinish), 
                                                                                                        Winner.Side.ToString());
		}

		/// <summary>
		/// Removes and drops the flag and it's aura when a player leaves.
		/// </summary>
		/// <param name="chr"></param>
		protected override void OnLeave(Character chr)
		{
			chr.Auras.Cancel(SpellId.WarsongFlag);
			chr.Auras.Cancel(SpellId.WarsongFlag_2);
			chr.Auras.Cancel(SpellId.SilverwingFlag);

			Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.WSOnLeave), chr.Name);

			base.OnLeave(chr);
		}

		/// <summary>
		/// Messages the players of a new character entering the battle.
		/// </summary>
		/// <param name="chr"></param>
		protected override void OnEnter(Character chr)
		{
			base.OnEnter(chr);

			Characters.SendMultiStringSystemMessage(DefaultAddonLocalizer.Instance.GetTranslations(AddonMsgKey.WSOnEnter), chr.Name);
		}

		protected override BattlegroundStats CreateStats()
		{
			return new WSGStats();
		}

		protected override void RewardPlayers()
		{
            /* Obsolete since patch 3.3.3. See http://www.wowwiki.com/Patch_3.3.3
             * BattlegroundTeam allianceTeam = GetTeam(BattlegroundSide.Alliance);
            if (allianceTeam == Winner)
            {
                foreach (Character chr in allianceTeam.GetCharacters())
                {
                    chr.SpellCast.TriggerSelf(SpellId.CreateWarsongMarkOfHonorWInner);
                }
            }
            else
            {
                foreach (Character chr in GetTeam(BattlegroundSide.Alliance).GetCharacters())
                {
                    chr.SpellCast.TriggerSelf(SpellId.CreateWarsongMarkOfHonorLoser);
                }
            }*/
		}

		public override void DeleteNow()
		{
			WarsongClan.Dispose();
			Silverwing.Dispose();

			for (Int32 i = 0; i < Factions.Length; i++)
			{
				Factions[i] = null;
			}

			base.DeleteNow();
		}

		#endregion

		/// <summary>
		/// spawns all of our GOs, including {buffs} and doors.
		/// </summary>
		protected override void SpawnGOs()
		{
			base.SpawnGOs();

			GOEntry allianceDoorEntry1 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive04);
            GOEntry allianceDoorEntry2 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive02_2);
            GOEntry allianceDoorEntry3 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive01_2);

            GOEntry hordeDoor1 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor01);
            GOEntry hordeDoor2 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor02);

			_allianceDoor1 = allianceDoorEntry1.FirstSpawnEntry.Spawn(this);
			_allianceDoor2 = allianceDoorEntry2.FirstSpawnEntry.Spawn(this);
			_allianceDoor3 = allianceDoorEntry3.FirstSpawnEntry.Spawn(this);

			_hordeDoor1 = hordeDoor1.FirstSpawnEntry.Spawn(this);
			_hordeDoor2 = hordeDoor2.FirstSpawnEntry.Spawn(this);

			// adjust anim progress so the door appears upright right off the bat
			_allianceDoor1.AnimationProgress = 255;
			_allianceDoor2.AnimationProgress = 255;
			_allianceDoor3.AnimationProgress = 255;
			_hordeDoor1.AnimationProgress = 255;
			_hordeDoor2.AnimationProgress = 255;

			RegisterPowerupEvents();
		}

		/// <summary>
		/// Removes the spawned gates.
		/// </summary>
		private void DropGates()
		{
			// Despawn the doors when the battle starts (if we have any)
			if (_allianceDoor1 != null)
			{
				_allianceDoor1.State = GameObjectState.Disabled;
				_allianceDoor2.State = GameObjectState.Disabled;
				_allianceDoor3.State = GameObjectState.Disabled;
				_hordeDoor1.State = GameObjectState.Disabled;
				_hordeDoor2.State = GameObjectState.Disabled;

				// Doesn't seem to do anything =\ (Probably safe to delete)
				_allianceDoor1.SendDespawn();
				_allianceDoor2.SendDespawn();
				_allianceDoor3.SendDespawn();

				_hordeDoor1.SendDespawn();
				_hordeDoor2.SendDespawn();

				// In about ~5s the doors are deleted (confirmed)
				CallDelayed(5000, () =>
									{
										if (_allianceDoor1 != null)
										{
											_allianceDoor1.Delete();
											_allianceDoor2.Delete();
											_allianceDoor2.Delete();
											_hordeDoor1.Delete();
											_hordeDoor2.Delete();
										}
									});
			}
		}

		/// <summary>
		/// 3.2 logic:  There is a now a 20 minute timer on this battleground. 
		/// After that time, the team with the most flag captures wins. 
		/// If this would result in a tie, the team that captured the first flag wins. 
		/// If neither side has captured a flag, then the game ends in a tie. 
		/// </summary>
		internal BattlegroundTeam EvaluateWinner()
		{
			if (Silverwing.Score == 0 && WarsongClan.Score == 0)
				return null; // Finish with a tie

			if (Silverwing.Score > WarsongClan.Score)
				//Winner = GetTeam(BattlegroundSide.Alliance);
				return GetTeam(BattlegroundSide.Alliance);

			if (Silverwing.Score < WarsongClan.Score)
				//Winner = GetTeam(BattlegroundSide.Horde);
				return GetTeam(BattlegroundSide.Horde);

			// Silverwing.Score == WarsongClan.Score)
			/*Winner*/
			return Silverwing.FlagCaptures[0].CaptureTime.Ticks > WarsongClan.FlagCaptures[0].CaptureTime.Ticks
				? GetTeam(BattlegroundSide.Horde) : GetTeam(BattlegroundSide.Alliance);
		}

		public override void FinishFight()
		{
			Winner = EvaluateWinner();
			base.FinishFight();
		}

		#region Spell/GO fixes and event registration

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void FixGOs()
		{
			//Getting our entries
			GOEntry allianceDoor1 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive04);
			GOEntry allianceDoor2 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive02_2);
			GOEntry allianceDoor3 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive01_2);

			GOEntry hordeDoor1 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor01);
			GOEntry hordeDoor2 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor02);


			// Manually fixing each entry's template. (should be replaced by DB values)
			allianceDoor1.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceDoor1.FirstSpawnEntry.Position = new Vector3(1471.555f, 1458.778f, 362.6332f);
			allianceDoor1.FirstSpawnEntry.Orientation = 3.115414f;
			allianceDoor1.FirstSpawnEntry.Scale = 1.5f;
			allianceDoor1.FirstSpawnEntry.Rotations = new float[] { 3.115414f, 0, 0, 0.9999143f, 0.01308903f };
			allianceDoor1.FirstSpawnEntry.State = GameObjectState.Enabled; // Spawn the door closed
			allianceDoor1.FirstSpawnEntry.AutoSpawns = false;
			allianceDoor1.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			allianceDoor2.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceDoor2.FirstSpawnEntry.Position = new Vector3(1492.478f, 1457.912f, 342.9689f);
			allianceDoor2.FirstSpawnEntry.Orientation = 3.115414f;
			allianceDoor2.FirstSpawnEntry.Scale = 2.5f;
			allianceDoor2.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.9999143f, 0.01308903f };
			allianceDoor2.FirstSpawnEntry.State = GameObjectState.Enabled; // Spawn the door closed
			allianceDoor2.FirstSpawnEntry.AutoSpawns = false;
			allianceDoor2.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			allianceDoor3.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceDoor3.FirstSpawnEntry.Position = new Vector3(1503.335f, 1493.466f, 352.1888f);
			allianceDoor3.FirstSpawnEntry.Orientation = 3.115414f;
			allianceDoor3.FirstSpawnEntry.Scale = 2f;
			allianceDoor3.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.9999143f, 0.01308903f };
			allianceDoor3.FirstSpawnEntry.State = GameObjectState.Enabled; // Spawn the door closed
			allianceDoor3.FirstSpawnEntry.AutoSpawns = false;
			allianceDoor3.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			hordeDoor1.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			hordeDoor1.FirstSpawnEntry.Position = new Vector3(949.1663f, 1423.772f, 345.6241f);
			hordeDoor1.FirstSpawnEntry.Orientation = -0.5756807f;
			hordeDoor1.FirstSpawnEntry.Rotations = new float[] { -0.01673368f, -0.004956111f, -0.2839723f, 0.9586737f };
			hordeDoor1.FirstSpawnEntry.State = GameObjectState.Enabled; // Spawn the door closed
			hordeDoor1.FirstSpawnEntry.AutoSpawns = false;
			hordeDoor1.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			hordeDoor2.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			hordeDoor2.FirstSpawnEntry.Position = new Vector3(953.0507f, 1459.842f, 340.6526f);
			hordeDoor2.FirstSpawnEntry.Orientation = -1.99662f;
			hordeDoor2.FirstSpawnEntry.Rotations = new float[] { -0.1971825f, 0.1575096f, -0.8239487f, 0.5073641f };
			hordeDoor2.FirstSpawnEntry.State = GameObjectState.Enabled; // Spawn the door closed
			hordeDoor2.FirstSpawnEntry.AutoSpawns = false;
			hordeDoor2.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			GOEntry allianceFlag = GOMgr.GetEntry(GOEntryId.SilverwingFlag_2); // The flagstand
			GOEntry hordeFlag = GOMgr.GetEntry(GOEntryId.WarsongFlag_2); // The flagstand.

			allianceFlag.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceFlag.FirstSpawnEntry.Position = new Vector3(1540.423f, 1481.325f, 351.8284f);
			allianceFlag.FirstSpawnEntry.Orientation = 3.089233f;
			allianceFlag.FirstSpawnEntry.Scale = 2f;
			allianceFlag.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.9996573f, 0.02617699f };
			allianceFlag.FirstSpawnEntry.AutoSpawns = false;

			hordeFlag.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			hordeFlag.FirstSpawnEntry.Position = new Vector3(916.0226f, 1434.405f, 345.413f);
			hordeFlag.FirstSpawnEntry.Orientation = 0.01745329f;
			hordeFlag.FirstSpawnEntry.Scale = 2f;
			hordeFlag.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.008726535f, 0.9999619f };
			hordeFlag.FirstSpawnEntry.AutoSpawns = false;

            GOEntry allianceSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff_2);
            GOEntry allianceBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff_2);
            GOEntry allianceFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff_2);

            GOEntry hordeSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff);
            GOEntry hordeBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff);
            GOEntry hordeFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff);

			allianceBerserkerBuff.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceBerserkerBuff.FirstSpawnEntry.Position = new Vector3(1320.09375f, 1378.78967285156f, 314.753234863281f);
			allianceBerserkerBuff.FirstSpawnEntry.Orientation = 1.18682384490967f;
			allianceBerserkerBuff.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.559192895889282f, 0.829037606716156f };
			allianceBerserkerBuff.FirstSpawnEntry.Scale = 1f;
			allianceBerserkerBuff.FirstSpawnEntry.AutoSpawns = false;

			allianceFoodBuff.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceFoodBuff.FirstSpawnEntry.Position = new Vector3(1317.50573730469f, 1550.85070800781f, 313.234375f);
			allianceFoodBuff.FirstSpawnEntry.Orientation = -0.26179963350296f;
			allianceFoodBuff.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.130526319146156f, -0.991444826126099f };
			allianceFoodBuff.FirstSpawnEntry.Scale = 1f;
			allianceFoodBuff.FirstSpawnEntry.AutoSpawns = false;

			allianceSpeedBuff.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			allianceSpeedBuff.FirstSpawnEntry.Position = new Vector3(1449.9296875f, 1470.70971679688f, 342.634552001953f);
			allianceSpeedBuff.FirstSpawnEntry.Orientation = -1.64060950279236f;
			allianceSpeedBuff.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.73135370016098f, -0.681998312473297f };
			allianceSpeedBuff.FirstSpawnEntry.Scale = 1f;
			allianceSpeedBuff.FirstSpawnEntry.AutoSpawns = false;


			hordeSpeedBuff.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			hordeSpeedBuff.FirstSpawnEntry.Position = new Vector3(1005.17071533203f, 1447.94567871094f, 335.903228759766f);
			hordeSpeedBuff.FirstSpawnEntry.Orientation = 1.64060950279236f;
			hordeSpeedBuff.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.73135370016098f, 0.681998372077942f };
			hordeSpeedBuff.FirstSpawnEntry.Scale = 1f;
			hordeSpeedBuff.FirstSpawnEntry.AutoSpawns = false;

			hordeBerserkerBuff.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			hordeBerserkerBuff.FirstSpawnEntry.Position = new Vector3(1139.68774414063f, 1560.28771972656f, 306.843170166016f);
			hordeBerserkerBuff.FirstSpawnEntry.Orientation = -2.4434609413147f;
			hordeBerserkerBuff.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.939692616462708f, -0.342020124197006f };
			hordeBerserkerBuff.FirstSpawnEntry.Scale = 1f;
			hordeBerserkerBuff.FirstSpawnEntry.AutoSpawns = false;

			hordeFoodBuff.FirstSpawnEntry.MapId = MapId.WarsongGulch;
			hordeFoodBuff.FirstSpawnEntry.Position = new Vector3(1110.45129394531f, 1353.65563964844f, 316.518096923828f);
			hordeFoodBuff.FirstSpawnEntry.Orientation = -0.68067866563797f;
			hordeFoodBuff.FirstSpawnEntry.Rotations = new float[] { 0, 0, 0.333806991577148f, -0.94264143705368f };
			hordeFoodBuff.FirstSpawnEntry.Scale = 1f;
			hordeFoodBuff.FirstSpawnEntry.AutoSpawns = false;
		}

		[Initialization(InitializationPass.Second)]
		public static void AddFlagEffectHandler()
		{
			Spell hordeFlagSpell = SpellHandler.Get(SpellId.WarsongFlag);
			SpellEffect heffect = hordeFlagSpell.AddAuraEffect(() => new WarsongFlagsHandler(), ImplicitTargetType.Duel);

			Spell allianceFlagSpell = SpellHandler.Get(SpellId.SilverwingFlag);
			SpellEffect aeffect = allianceFlagSpell.AddAuraEffect(() => new WarsongFlagsHandler(), ImplicitTargetType.Duel);

			// Replacing the spelleffectHandler
			SpellHandler.Apply(spell =>
			{
				spell.Effects[0].SpellEffectHandlerCreator =
					(cast, effct) => new SummonFlagEffectHandler(cast, effct);
			},
			SpellId.HordeFlagDrop,
			SpellId.AllianceFlagDrop);
		}

		[Initialization]
		[DependentInitialization(typeof(GOMgr))]
		public static void RegisterEvents()
		{
			var standEntry = GOMgr.GetEntry(SilverwingFlagStandId) as GOFlagStandEntry;
			standEntry.Side = BattlegroundSide.Alliance;
			var droppedEntry = GOMgr.GetEntry(SilverwingFlagId) as GOFlagDropEntry;
			droppedEntry.Side = BattlegroundSide.Alliance;

			standEntry = GOMgr.GetEntry(WarsongClanFlagStandId) as GOFlagStandEntry;
			standEntry.Side = BattlegroundSide.Horde;
			droppedEntry = GOMgr.GetEntry(WarsongFlagId) as GOFlagDropEntry;
			droppedEntry.Side = BattlegroundSide.Horde;

			// register AreaTrigger capture events
			AreaTrigger hordeFlagAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeFlagSpawn);
			AreaTrigger allianceFlagAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceFlagSpawn);

			hordeFlagAT.Triggered += HordeCaptureTriggered;
			allianceFlagAT.Triggered += AllianceCaptureTriggered;

			GOMgr.GetEntry(WarsongClanFlagStandId).Used += HandleFlagStandUsed;
			GOMgr.GetEntry(SilverwingFlagStandId).Used += HandleFlagStandUsed;

			GOMgr.GetEntry(WarsongFlagId).Used += HandleDroppedFlagUsed;
			GOMgr.GetEntry(SilverwingFlagId).Used += HandleDroppedFlagUsed;

		}

		/// <summary>
		/// Given user clicks on a flagstand
		/// </summary>
		/// <param name="flag"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private static bool HandleFlagStandUsed(GameObject flag, Character user)
		{
			return HandleFlagUsed(flag, user, false);
		}

		/// <summary>
		/// Given user clicks on a dropped flag
		/// </summary>
		/// <param name="flag"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		private static bool HandleDroppedFlagUsed(GameObject flag, Character user)
		{
			return HandleFlagUsed(flag, user, true);
		}

		private static bool HandleFlagUsed(GameObject flag, Character user, bool mayReturn)
		{
			BattlegroundTeam team = user.Battlegrounds.Team;
			var wsg = flag.Map as WarsongGulch;
			if (wsg != null && team != null)
			{
				var entry = flag.Entry as GOFlagEntry;

				if (entry != null)
				{
					if (entry.Side != team.Side)
					{
						// using opponent's flag
						WSGFaction flagFaction = wsg.GetFaction(entry.Side);
						if (flagFaction.CanPickupFlag(user))
						{
							flagFaction.PickupFlag(user);
						}
						return true;
					}
					else if (mayReturn)
					{
						// using own flag
						WSGFaction userFaction = wsg.GetFaction(team.Side);
						if (userFaction.CanReturnFlag(user))
						{
							userFaction.ReturnFlag(user);
						}
					}
				}
			}
			return false;
		}

		private void HandlePowerUp(Unit unit, SpellId spell, GameObject go, Action respawnCallback)
		{
			if (go != null && !go.IsDeleted)
			{
				if (spell != 0)
				{
					unit.SpellCast.TriggerSelf(spell);
				}
				go.Delete();
				CallDelayed(PowerUpRespawnTimeMillis, respawnCallback);
			}
		}

		/// <summary>
		/// Somebody stepped on the horde capture areatrigger
		/// </summary>
		/// <param name="at"></param>
		/// <param name="chr"></param>
		private static void HordeCaptureTriggered(AreaTrigger at, Character chr)
		{
			// Check whether the battle has started and the Character is actively participating
			BattlegroundTeam team = chr.Battlegrounds.Team;
			var wsg = chr.Map as WarsongGulch;
			if (team != null && wsg != null && wsg.IsActive)
			{
				if (team.Side == BattlegroundSide.Horde)
				{
					if (wsg.Silverwing.FlagCarrier == chr && wsg.WarsongClan.IsFlagHome)
					{
						wsg.Silverwing.CaptureFlag(chr);
					}
				}
			}
		}
		/// <summary>
		/// Somebody stepped on the Alliance capture areatrigger
		/// </summary>
		/// <param name="at"></param>
		/// <param name="chr"></param>
		private static void AllianceCaptureTriggered(AreaTrigger at, Character chr)
		{
			// Check whether the battle has started and the Character is actively participating
			BattlegroundTeam team = chr.Battlegrounds.Team;
			var wsg = chr.Map as WarsongGulch;
			if (team != null && wsg != null && wsg.IsActive)
			{
				if (team.Side == BattlegroundSide.Alliance)
				{
					if (wsg.WarsongClan.FlagCarrier == chr && wsg.Silverwing.IsFlagHome)
					{
						wsg.WarsongClan.CaptureFlag(chr);
					}
				}
			}
		}

		/// <summary>
		/// Register's the powerup AT triggers to cast the spell and remove the GO.
		/// </summary>
		private void RegisterPowerupEvents()
		{
			AreaTrigger allianceBerserker =
				AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceElexirOfBerserkSpawn);
            AreaTrigger allianceFood =
				AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceElexirOfRegenerationSpawn);
            AreaTrigger allianceSpeed = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceElexirOfSpeedSpawn);

            AreaTrigger hordeBerserker = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeElexirOfBerserkSpawn);
            AreaTrigger hordeFood = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeElexirOfRegenerationSpawn);
            AreaTrigger hordeSpeed = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeElexirOfSpeedSpawn);

			allianceBerserker.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _allianceBerserkerBuff, SpawnAllianceBerserkerBuff);
			allianceFood.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _allianceFoodBuff, SpawnAllianceFoodBuff);
			allianceSpeed.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.Speed_5, _allianceSpeedBuff, SpawnAllianceSpeedBuff);

			hordeBerserker.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _hordeBerserkerBuff, SpawnHordeBerserkerBuff);
			hordeFood.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _hordeFoodBuff, SpawnHordeFoodBuff);
			hordeSpeed.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.Speed_5, _hordeSpeedBuff, SpawnHordeSpeedBuff);

		}

		public void SpawnAllianceSpeedBuff()
		{
			GOEntry allianceSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff_2);
			_allianceSpeedBuff = allianceSpeedBuff.FirstSpawnEntry.Spawn(this);
		}

		public void SpawnAllianceFoodBuff()
		{
            GOEntry allianceFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff_2);
			_allianceFoodBuff = allianceFoodBuff.FirstSpawnEntry.Spawn(this);
		}

		public void SpawnAllianceBerserkerBuff()
		{
            GOEntry allianceBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff_2);
			_allianceBerserkerBuff = allianceBerserkerBuff.FirstSpawnEntry.Spawn(this);
		}

		public void SpawnHordeSpeedBuff()
		{
            GOEntry hordeSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff);
			_hordeSpeedBuff = hordeSpeedBuff.FirstSpawnEntry.Spawn(this);
		}

		public void SpawnHordeFoodBuff()
		{
            GOEntry hordeFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff);
			_hordeFoodBuff = hordeFoodBuff.FirstSpawnEntry.Spawn(this);
		}

		public void SpawnHordeBerserkerBuff()
		{
            GOEntry hordeBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff);
			_hordeBerserkerBuff = hordeBerserkerBuff.FirstSpawnEntry.Spawn(this);
		}

		#endregion
	}

	public delegate void FlagActionHandler(Character chr);
}