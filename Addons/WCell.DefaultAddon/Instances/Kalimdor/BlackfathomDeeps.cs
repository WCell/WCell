using WCell.Constants.Misc;
using WCell.Constants.NPCs;
using WCell.Core.Initialization;
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
using System;
using WCell.Util.Graphics;


namespace WCell.Addons.Default.Instances
{
	public class BlackfathomDeeps : RaidInstance
	{
		#region General (Content)
		private static NPCEntry LadySarevessEntry;
		private static NPCEntry GelihastEntry;
		private static NPCEntry LorgusJettEntry;
		private static NPCEntry LordKelrisEntry;
		private static NPCEntry AkuMaiEntry;

		[Initialization]
		[DependentInitialization(typeof(NPCMgr))]
		public static void InitNPCs()
		{
			LadySarevessEntry = NPCMgr.GetEntry(NPCId.LadySarevess);
			LadySarevessEntry.Activated += LadySarevess =>
			{
				((BaseBrain)LadySarevess.Brain).DefaultCombatAction.Strategy = new SarevessAttackAction(LadySarevess);
			};

			GelihastEntry = NPCMgr.GetEntry(NPCId.Gelihast);
			GelihastEntry.Activated += Gelihast =>
			{
				((BaseBrain)Gelihast.Brain).DefaultCombatAction.Strategy = new GelihastAttackAction(Gelihast);
			};

			LorgusJettEntry = NPCMgr.GetEntry(NPCId.LorgusJett);
			LorgusJettEntry.Activated += LorgusJett =>
			{
				((BaseBrain)LorgusJett.Brain).DefaultCombatAction.Strategy = new LorgusJettAttackAction(LorgusJett);
			};

			LordKelrisEntry = NPCMgr.GetEntry(NPCId.TwilightLordKelris);
			LordKelrisEntry.Activated += LordKelris =>
			{
				((BaseBrain)LordKelris.Brain).DefaultCombatAction.Strategy = new LordKelrisAttackAction(LordKelris);
			};

			AkuMaiEntry = NPCMgr.GetEntry(NPCId.AkuMai);
			AkuMaiEntry.Activated += AkuMai =>
			{
				((BaseBrain)AkuMai.Brain).DefaultCombatAction.Strategy = new AkuMaiAttackAction(AkuMai);
			};
		}
		#endregion

		#region LadySarevess
		public class SarevessAttackAction : AIAttackAction
		{
			[Initialization(InitializationPass.Second)]
			public static void InitLadySpells()
			{
				var forkedLightning = SpellHandler.Get(SpellId.ForkedLightningRank1);
				forkedLightning.AISettings.SetValues(8000);

				// is this correct? shouldn't it be AoE? that spell only does 5 damage
				// in that case it should not have a custom target definition, but stick with the default one
				forkedLightning.OverrideCustomTargetDefinitions(DefaultTargetAdders.AddRandomPlayer, DefaultTargetFilters.IsHostile);

				var slow = SpellHandler.Get(SpellId.SlowRank1);
				slow.AISettings.SetValues(13000);
				slow.OverrideCustomTargetDefinitions(DefaultTargetAdders.AddRandomPlayer, DefaultTargetFilters.IsHostile);


				var frostNova = SpellHandler.Get(SpellId.ClassSkillFrostNovaRank2);
				frostNova.AISettings.SetCooldown(20000);
				frostNova.AddTextAndSoundEvent(NPCAiTextMgr.GetFirstTextByEnglishPrefix("You should not be here! Slay them!"));
			}

			public SarevessAttackAction(NPC LadySarevess)
				: base(LadySarevess)
			{
				LadySarevess.Spells.AddSpell(SpellId.ForkedLightningRank1, SpellId.SlowRank1, SpellId.ClassSkillFrostNovaRank2);
			}

		}
		#endregion

		#region Gelihast
		public class GelihastAttackAction : AIAttackAction
		{
			[Initialization(InitializationPass.Second)]
			public static void InitGelihast()
			{
				var net = SpellHandler.Get(SpellId.Net);
				net.AISettings.SetValues(3000);
				net.OverrideCustomTargetDefinitions(DefaultTargetAdders.AddRandomPlayer, DefaultTargetFilters.IsHostile);
			}

			public GelihastAttackAction(NPC Gelihast)
				: base(Gelihast)
			{
				Gelihast.Spells.AddSpell(SpellId.Net);
			}

		}
		#endregion

		#region LorgusJett
		public class LorgusJettAttackAction : AIAttackAction
		{
			internal static Spell LightningBolt, LightningShield;

			private DateTime timeSinceLastInterval;
			private const int interVal = 1;
			private int LightningBoltTick;
			private int LightningShieldTick;

			[Initialization(InitializationPass.Second)]
			public static void InitLorgusJett()
			{
				LightningBolt = SpellHandler.Get(12167);
				LightningShield = SpellHandler.Get(12550);
			}

			public LorgusJettAttackAction(NPC LorgusJett)
				: base(LorgusJett)
			{
			}
			/*public override void OnEnterCombat()
			{
				m_owner.SpellCast.Start(LightningShield, false, m_owner);
			}*/

			public override void Start()
			{
				timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				var timeNow = DateTime.Now;
				var timeBetween = timeNow - timeSinceLastInterval;

				if (timeBetween.TotalSeconds >= interVal)
				{
					timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				LightningBoltTick++;
				LightningShieldTick++;

				if (LightningBoltTick >= 4)
				{
					var chr = m_owner.GetNearbyRandomHostileCharacter();
					if (chr != null)
					{
						LightningBoltTick = 0;
						m_owner.SpellCast.Start(LightningBolt, false, chr);
					}
					return true;
				}
				/* if (LightningShieldTick >= 2)
				 {
					 if (m_owner!= null)
					 {
						 LightningShieldTick = 0;
						 m_owner.SpellCast.Start(LightningShield, false, m_owner);
					 }
					 return true;
				 }*/
				return false;
			}

		}
		#endregion

		#region LordKelris
		public class LordKelrisAttackAction : AIAttackAction
		{
			[Initialization(InitializationPass.Second)]
			public static void InitLordSpells()
			{
				// Even use mindblast if there are one or more sleeping targets nearby? -> AI code should prevent targeting of enemies with debuffs that might get cancelled
				var mindblast = SpellHandler.Get(SpellId.MindBlast_2);
				mindblast.AISettings.SetCooldown(3000);

				// What to do if no one is in range for Sleep? -> It won't cast it
				var sleep = SpellHandler.Get(SpellId.Sleep);
				sleep.AISettings.SetCooldown(7000);
				sleep.AddTextAndSoundEvent(NPCAiTextMgr.GetFirstTextByEnglishPrefix("Sleep..."));
			}

			public LordKelrisAttackAction(NPC LordKelris)
				: base(LordKelris)
			{
				LordKelris.Spells.AddSpell(SpellId.Sleep, SpellId.MindBlast_2);
			}

			/*public override void OnEnterCombat()
			{
				var Texts = NPCAiTextMgr.GetEntry("Who dares disturb my meditation?!");
				var CurrentNpcText = Texts[0];
				m_owner.PlaySound(CurrentNpcText.Sound); (m_owner as NPC).Yell(CurrentNpcText.Texts); 
			}*/

		}
		#endregion

		#region Aku'mai
		public class AkuMaiAttackAction : AIAttackAction
		{
			internal static Spell FrenziedRage, PoisonCloud;

			private DateTime timeSinceLastInterval;
			private const int interVal = 1;
			private int PoisonCloudTick;

			[Initialization(InitializationPass.Second)]
			static void InitAkuMai()
			{
				FrenziedRage = SpellHandler.Get(3490);
				PoisonCloud = SpellHandler.Get(3815);
			}

			public AkuMaiAttackAction(NPC AkuMai)
				: base(AkuMai)
			{
			}

			public override void Start()
			{
				timeSinceLastInterval = DateTime.Now;
				base.Start();
			}

			public override void Update()
			{
				var timeNow = DateTime.Now;
				var timeBetween = timeNow - timeSinceLastInterval;

				if (timeBetween.TotalSeconds >= interVal)
				{
					timeSinceLastInterval = timeNow;
					if (CheckSpellCast())
					{
						// idle a little after casting a spell
						m_owner.Idle(1000);
						return;
					}
				}

				base.Update();
			}

			private bool CheckSpellCast()
			{
				PoisonCloudTick++;

				var hpPct = m_owner.HealthPct;
				if (hpPct <= 55)
				{
					if (m_owner != null)
					{
						m_owner.SpellCast.Start(FrenziedRage, false, m_owner);
					}
					return true;
				}
				if (PoisonCloudTick >= 3)
				{
					var chr = m_owner.GetNearbyRandomHostileCharacter();
					if (chr != null)
					{
						PoisonCloudTick = 0;
						m_owner.SpellCast.Start(PoisonCloud, false, chr);
					}
					return true;
				}
				return false;
			}

		}
		#endregion

	}
}