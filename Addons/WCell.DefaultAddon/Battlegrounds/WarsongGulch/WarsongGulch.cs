using System;
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
			MaxScoreDefault = 3;
		}

		[Variable("WSGFlagRespawnTime")]
		public static int FlagRespawnTime = 20;

		[Variable("WSGPrepTimeSecs")]
		public static int PreparationTimeSecs = 60;

		/// <summary>
		/// The time in which the BG will be ended, no matter the score. If 0, will last til score reaches max.
		/// </summary>
		[Variable("WSGMaxDurationMinutes")]
		public static float MaxDuration = 20;

		public static float PowerUpRespawnTime = 2 * 60;

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
		public override float PreparationTimeSeconds
		{
			get { return PreparationTimeSecs; }
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

		protected override void InitRegion()
		{
			base.InitRegion();

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
			ChatMgr.SendSystemMessage(Characters, "Let the battle for Warsong Gulch begin!");
		}

		protected override void OnFinish(bool disposing)
		{
			base.OnFinish(disposing);
			foreach (var character in Characters)
			{
				character.SendSystemMessage("The battle has ended!");
			}
		}
		protected override void OnPrepareHalftime()
		{
			base.OnPrepareHalftime();
			var msg = "The battle for Warsong Gulch begins in " + PreparationTimeSeconds / 2f + " seconds.";
			ChatMgr.SendSystemMessage(Characters, msg);
		}


		protected override void OnPrepare()
		{
			base.OnPrepare();
			var msg = "The battle for Warsong Gulch begins in ";
			var secs = PreparationTimeSeconds;
			var mins = (int)secs / 60;
			if (mins < 1)
			{
				msg += (int)secs + " seconds";
			}
			else
			{
				msg += mins + (mins == 1 ? "minute" : "minutes");
				if (secs % 60 != 0)
				{
					msg += " and " + secs + (secs == 1 ? "second" : "seconds");
				}
			}

			Characters.SendSystemMessage(msg + ".");
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

			Characters.SendSystemMessage("{0} has left the battle!", chr.Name);

			base.OnLeave(chr);
		}

		/// <summary>
		/// Messages the players of a new character entering the battle.
		/// </summary>
		/// <param name="chr"></param>
		protected override void OnEnter(Character chr)
		{
			base.OnEnter(chr);

			Characters.SendSystemMessage("{0} has entered the battle!", chr.Name);
		}

		protected override BattlegroundStats CreateStats()
		{
			return new WSGStats();
		}

		protected override void RewardPlayers()
		{
			var allianceTeam = GetTeam(BattlegroundSide.Alliance);
			if (allianceTeam == Winner)
			{
				foreach (var chr in allianceTeam.GetCharacters())
				{
					chr.SpellCast.TriggerSelf(SpellId.CreateWarsongMarkOfHonorWInner);
				}
			}
			else
			{
				foreach (var chr in GetTeam(BattlegroundSide.Alliance).GetCharacters())
				{
					chr.SpellCast.TriggerSelf(SpellId.CreateWarsongMarkOfHonorLoser);
				}
			}
		}

		public override void DeleteNow()
		{
			WarsongClan.Dispose();
			Silverwing.Dispose();

			for (var i = 0; i < Factions.Length; i++)
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

			var allianceDoorEntry1 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive04);
			var allianceDoorEntry2 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive02_2);
			var allianceDoorEntry3 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive01_2);

			var hordeDoor1 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor01);
			var hordeDoor2 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor02);

			_allianceDoor1 = allianceDoorEntry1.FirstTemplate.Spawn(this);
			_allianceDoor2 = allianceDoorEntry2.FirstTemplate.Spawn(this);
			_allianceDoor3 = allianceDoorEntry3.FirstTemplate.Spawn(this);

			_hordeDoor1 = hordeDoor1.FirstTemplate.Spawn(this);
			_hordeDoor2 = hordeDoor2.FirstTemplate.Spawn(this);

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
				CallDelayed(5f, () =>
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
			var allianceDoor1 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive04);
			var allianceDoor2 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive02_2);
			var allianceDoor3 = GOMgr.GetEntry(GOEntryId.Doodad_PortcullisActive01_2);

			var hordeDoor1 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor01);
			var hordeDoor2 = GOMgr.GetEntry(GOEntryId.Doodad_RazorfenDoor02);


			// Manually fixing each entry's template. (should be replaced by DB values)
			allianceDoor1.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceDoor1.FirstTemplate.Pos = new Vector3(1471.555f, 1458.778f, 362.6332f);
			allianceDoor1.FirstTemplate.Orientation = 3.115414f;
			allianceDoor1.FirstTemplate.Scale = 1.5f;
			allianceDoor1.FirstTemplate.Rotations = new float[] { 3.115414f, 0, 0, 0.9999143f, 0.01308903f };
			allianceDoor1.FirstTemplate.State = GameObjectState.Enabled; // Spawn the door closed
			allianceDoor1.FirstTemplate.AutoSpawn = false;
			allianceDoor1.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			allianceDoor2.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceDoor2.FirstTemplate.Pos = new Vector3(1492.478f, 1457.912f, 342.9689f);
			allianceDoor2.FirstTemplate.Orientation = 3.115414f;
			allianceDoor2.FirstTemplate.Scale = 2.5f;
			allianceDoor2.FirstTemplate.Rotations = new float[] { 0, 0, 0.9999143f, 0.01308903f };
			allianceDoor2.FirstTemplate.State = GameObjectState.Enabled; // Spawn the door closed
			allianceDoor2.FirstTemplate.AutoSpawn = false;
			allianceDoor2.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			allianceDoor3.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceDoor3.FirstTemplate.Pos = new Vector3(1503.335f, 1493.466f, 352.1888f);
			allianceDoor3.FirstTemplate.Orientation = 3.115414f;
			allianceDoor3.FirstTemplate.Scale = 2f;
			allianceDoor3.FirstTemplate.Rotations = new float[] { 0, 0, 0.9999143f, 0.01308903f };
			allianceDoor3.FirstTemplate.State = GameObjectState.Enabled; // Spawn the door closed
			allianceDoor3.FirstTemplate.AutoSpawn = false;
			allianceDoor3.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			hordeDoor1.FirstTemplate.MapId = MapId.WarsongGulch;
			hordeDoor1.FirstTemplate.Pos = new Vector3(949.1663f, 1423.772f, 345.6241f);
			hordeDoor1.FirstTemplate.Orientation = -0.5756807f;
			hordeDoor1.FirstTemplate.Rotations = new float[] { -0.01673368f, -0.004956111f, -0.2839723f, 0.9586737f };
			hordeDoor1.FirstTemplate.State = GameObjectState.Enabled; // Spawn the door closed
			hordeDoor1.FirstTemplate.AutoSpawn = false;
			hordeDoor1.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			hordeDoor2.FirstTemplate.MapId = MapId.WarsongGulch;
			hordeDoor2.FirstTemplate.Pos = new Vector3(953.0507f, 1459.842f, 340.6526f);
			hordeDoor2.FirstTemplate.Orientation = -1.99662f;
			hordeDoor2.FirstTemplate.Rotations = new float[] { -0.1971825f, 0.1575096f, -0.8239487f, 0.5073641f };
			hordeDoor2.FirstTemplate.State = GameObjectState.Enabled; // Spawn the door closed
			hordeDoor2.FirstTemplate.AutoSpawn = false;
			hordeDoor2.Flags |= GameObjectFlags.DoesNotDespawn | GameObjectFlags.InUse;

			var allianceFlag = GOMgr.GetEntry(GOEntryId.SilverwingFlag_2); // The flagstand
			var hordeFlag = GOMgr.GetEntry(GOEntryId.WarsongFlag_2); // The flagstand.

			allianceFlag.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceFlag.FirstTemplate.Pos = new Vector3(1540.423f, 1481.325f, 351.8284f);
			allianceFlag.FirstTemplate.Orientation = 3.089233f;
			allianceFlag.FirstTemplate.Scale = 2f;
			allianceFlag.FirstTemplate.Rotations = new float[] { 0, 0, 0.9996573f, 0.02617699f };
			allianceFlag.FirstTemplate.AutoSpawn = false;

			hordeFlag.FirstTemplate.MapId = MapId.WarsongGulch;
			hordeFlag.FirstTemplate.Pos = new Vector3(916.0226f, 1434.405f, 345.413f);
			hordeFlag.FirstTemplate.Orientation = 0.01745329f;
			hordeFlag.FirstTemplate.Scale = 2f;
			hordeFlag.FirstTemplate.Rotations = new float[] { 0, 0, 0.008726535f, 0.9999619f };
			hordeFlag.FirstTemplate.AutoSpawn = false;

			var allianceSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff_2);
			var allianceBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff_2);
			var allianceFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff_2);

			var hordeSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff);
			var hordeBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff);
			var hordeFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff);

			allianceBerserkerBuff.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceBerserkerBuff.FirstTemplate.Pos = new Vector3(1320.09375f, 1378.78967285156f, 314.753234863281f);
			allianceBerserkerBuff.FirstTemplate.Orientation = 1.18682384490967f;
			allianceBerserkerBuff.FirstTemplate.Rotations = new float[] { 0, 0, 0.559192895889282f, 0.829037606716156f };
			allianceBerserkerBuff.FirstTemplate.Scale = 1f;
			allianceBerserkerBuff.FirstTemplate.AutoSpawn = false;

			allianceFoodBuff.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceFoodBuff.FirstTemplate.Pos = new Vector3(1317.50573730469f, 1550.85070800781f, 313.234375f);
			allianceFoodBuff.FirstTemplate.Orientation = -0.26179963350296f;
			allianceFoodBuff.FirstTemplate.Rotations = new float[] { 0, 0, 0.130526319146156f, -0.991444826126099f };
			allianceFoodBuff.FirstTemplate.Scale = 1f;
			allianceFoodBuff.FirstTemplate.AutoSpawn = false;

			allianceSpeedBuff.FirstTemplate.MapId = MapId.WarsongGulch;
			allianceSpeedBuff.FirstTemplate.Pos = new Vector3(1449.9296875f, 1470.70971679688f, 342.634552001953f);
			allianceSpeedBuff.FirstTemplate.Orientation = -1.64060950279236f;
			allianceSpeedBuff.FirstTemplate.Rotations = new float[] { 0, 0, 0.73135370016098f, -0.681998312473297f };
			allianceSpeedBuff.FirstTemplate.Scale = 1f;
			allianceSpeedBuff.FirstTemplate.AutoSpawn = false;


			hordeSpeedBuff.FirstTemplate.MapId = MapId.WarsongGulch;
			hordeSpeedBuff.FirstTemplate.Pos = new Vector3(1005.17071533203f, 1447.94567871094f, 335.903228759766f);
			hordeSpeedBuff.FirstTemplate.Orientation = 1.64060950279236f;
			hordeSpeedBuff.FirstTemplate.Rotations = new float[] { 0, 0, 0.73135370016098f, 0.681998372077942f };
			hordeSpeedBuff.FirstTemplate.Scale = 1f;
			hordeSpeedBuff.FirstTemplate.AutoSpawn = false;

			hordeBerserkerBuff.FirstTemplate.MapId = MapId.WarsongGulch;
			hordeBerserkerBuff.FirstTemplate.Pos = new Vector3(1139.68774414063f, 1560.28771972656f, 306.843170166016f);
			hordeBerserkerBuff.FirstTemplate.Orientation = -2.4434609413147f;
			hordeBerserkerBuff.FirstTemplate.Rotations = new float[] { 0, 0, 0.939692616462708f, -0.342020124197006f };
			hordeBerserkerBuff.FirstTemplate.Scale = 1f;
			hordeBerserkerBuff.FirstTemplate.AutoSpawn = false;

			hordeFoodBuff.FirstTemplate.MapId = MapId.WarsongGulch;
			hordeFoodBuff.FirstTemplate.Pos = new Vector3(1110.45129394531f, 1353.65563964844f, 316.518096923828f);
			hordeFoodBuff.FirstTemplate.Orientation = -0.68067866563797f;
			hordeFoodBuff.FirstTemplate.Rotations = new float[] { 0, 0, 0.333806991577148f, -0.94264143705368f };
			hordeFoodBuff.FirstTemplate.Scale = 1f;
			hordeFoodBuff.FirstTemplate.AutoSpawn = false;
		}

		[Initialization(InitializationPass.Second)]
		public static void AddFlagEffectHandler()
		{
			var hordeFlagSpell = SpellHandler.Get(SpellId.WarsongFlag);
			var effect = hordeFlagSpell.AddEffect(SpellEffectType.ApplyAura);
			effect.AuraType = AuraType.Dummy;
			effect.ImplicitTargetA = ImplicitTargetType.Duel;
			effect.AuraEffectHandlerCreator = () => new WarsongFlagsHandler();

			var allianceFlagSpell = SpellHandler.Get(SpellId.SilverwingFlag);
			var efct = allianceFlagSpell.AddEffect(SpellEffectType.ApplyAura);
			efct.AuraType = AuraType.Dummy;
			efct.ImplicitTargetA = ImplicitTargetType.Duel;
			efct.AuraEffectHandlerCreator = () => new WarsongFlagsHandler();

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
			var standEntry = ((GOFlagStandEntry)GOMgr.GetEntry(SilverwingFlagStandId));
			standEntry.Side = BattlegroundSide.Alliance;
			var droppedEntry = ((GOFlagDropEntry)GOMgr.GetEntry(SilverwingFlagId));
			droppedEntry.Side = BattlegroundSide.Alliance;

			standEntry = ((GOFlagStandEntry)GOMgr.GetEntry(WarsongClanFlagStandId));
			standEntry.Side = BattlegroundSide.Horde;
			droppedEntry = ((GOFlagDropEntry)GOMgr.GetEntry(WarsongFlagId));
			droppedEntry.Side = BattlegroundSide.Horde;

			// register AreaTrigger capture events
			var hordeFlagAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeFlagSpawn);
			var allianceFlagAT = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceFlagSpawn);

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
			var team = user.Battlegrounds.Team;
			var wsg = flag.Region as WarsongGulch;
			if (wsg != null && team != null)
			{
				var entry = flag.Entry as GOFlagEntry;

				if (entry != null)
				{
					if (entry.Side != team.Side)
					{
						// using opponent's flag
						var flagFaction = wsg.GetFaction(entry.Side);
						if (flagFaction.CanPickupFlag(user))
						{
							flagFaction.PickupFlag(user);
						}
						return true;
					}
					else if (mayReturn)
					{
						// using own flag
						var userFaction = wsg.GetFaction(team.Side);
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
				CallDelayed(PowerUpRespawnTime, respawnCallback);
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
			var team = chr.Battlegrounds.Team;
			var wsg = chr.Region as WarsongGulch;
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
			var team = chr.Battlegrounds.Team;
			var wsg = chr.Region as WarsongGulch;
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
			var allianceBerserker =
				AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceElexirOfBerserkSpawn);
			var allianceFood =
				AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceElexirOfRegenerationSpawn);
			var allianceSpeed = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchAllianceElexirOfSpeedSpawn);

			var hordeBerserker = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeElexirOfBerserkSpawn);
			var hordeFood = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeElexirOfRegenerationSpawn);
			var hordeSpeed = AreaTriggerMgr.GetTrigger(AreaTriggerId.WarsongGulchHordeElexirOfSpeedSpawn);

			allianceBerserker.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _allianceBerserkerBuff, SpawnAllianceBerserkerBuff);
			allianceFood.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _allianceFoodBuff, SpawnAllianceFoodBuff);
			allianceSpeed.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.Speed_5, _allianceSpeedBuff, SpawnAllianceSpeedBuff);

			hordeBerserker.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _hordeBerserkerBuff, SpawnHordeBerserkerBuff);
			hordeFood.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.None, _hordeFoodBuff, SpawnHordeFoodBuff);
			hordeSpeed.Triggered += (at, unit) => HandlePowerUp(unit, SpellId.Speed_5, _hordeSpeedBuff, SpawnHordeSpeedBuff);

		}

		public void SpawnAllianceSpeedBuff()
		{
			var allianceSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff_2);
			_allianceSpeedBuff = allianceSpeedBuff.FirstTemplate.Spawn(this);
		}

		public void SpawnAllianceFoodBuff()
		{
			var allianceFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff_2);
			_allianceFoodBuff = allianceFoodBuff.FirstTemplate.Spawn(this);
		}

		public void SpawnAllianceBerserkerBuff()
		{
			var allianceBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff_2);
			_allianceBerserkerBuff = allianceBerserkerBuff.FirstTemplate.Spawn(this);
		}

		public void SpawnHordeSpeedBuff()
		{
			var hordeSpeedBuff = GOMgr.GetEntry(GOEntryId.SpeedBuff);
			_hordeSpeedBuff = hordeSpeedBuff.FirstTemplate.Spawn(this);
		}

		public void SpawnHordeFoodBuff()
		{
			var hordeFoodBuff = GOMgr.GetEntry(GOEntryId.FoodBuff);
			_hordeFoodBuff = hordeFoodBuff.FirstTemplate.Spawn(this);
		}

		public void SpawnHordeBerserkerBuff()
		{
			var hordeBerserkerBuff = GOMgr.GetEntry(GOEntryId.BerserkBuff);
			_hordeBerserkerBuff = hordeBerserkerBuff.FirstTemplate.Spawn(this);
		}

		#endregion
	}

	public delegate void FlagActionHandler(Character chr);
}